using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour, IRoutable
{
  public ModelListener listener;
  public ModelStorage storage;

  private Gate template;
  private bool isTemplate = true;
  private Router router;
  /**
   * 1秒間に通過できる人数
   */
  public float Mobility = 5f;
  /**
   * ホームへの入場待ち者が入れる最大数
   */
  public int Capacity = 5;
  /**
   * 後、どれくらい経過すれば、人が1人通れるか
   */
  private float waitTime;

  [System.NonSerialized] public Station BelongsTo;
  /**
   * 改札内に入りたい人たち
   */
  [System.NonSerialized] public LinkedList<Human> InQueue;
  /**
   * プラットフォームへの入場待機者
   * デッドロックを防ぐため、出場者はホームから容量無制限の outQueue に移動させる
   */
  [System.NonSerialized] public LinkedList<Human> Concourse;
  /**
   * コンコースから改札外に出たい人達
   */
  [System.NonSerialized] public LinkedList<Human> OutQueue;


  private void Awake()
  {
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Gate>(EventType.CREATED, st => storage.Add(st));
      listener.Add<Gate>(EventType.DELETED, st => storage.Remove(st));
    }
  }

  /**
   * 入出場待ちがいる場合、改札を通して移動させます。
   * プラットフォーム移動待ちが満杯の場合、入場規制します
   * 人を移動させた場合、ペナルティとして待機時間を増やします。
   */
  private void Update()
  {
    if (!isTemplate)
    {
      waitTime = Mathf.Max(waitTime - Time.deltaTime, 0f);
      if (waitTime == 0f)
      {
        if (!TryExit()) TryEnter();
      }
    }
  }

  public Gate NewInstance(Station st)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.BelongsTo = st;
    obj.router = new RouterImpl(listener, obj);
    obj.InQueue = new LinkedList<Human>();
    obj.Concourse = new LinkedList<Human>();
    obj.OutQueue = new LinkedList<Human>();
    obj.waitTime = 0f;
    obj.transform.position = st.transform.position;
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }

  /**
    * 入場待ちをプラットフォーム移動待ちにさせます
    * 一人移動できたなら、trueを返します
    */
  protected bool TryEnter()
  {
    // 入場規制
    if (Concourse.Count >= Capacity)
    {
      return false;
    }
    while (InQueue.Count > 0)
    {
      var h = InQueue.First.Value;
      InQueue.RemoveFirst();
      listener.Fire(EventType.MODIFIED, BelongsTo);
      // 途中で再経路探索され、目的地が変わった場合は無視
      // => 必ず h.Update()でコンコースに
      if (h.Next == this as IRoutable && !OutQueue.Contains(h))
      {
        h.Complete();
        Concourse.AddLast(h);
        h.State = Human.StateType.WAIT_ENTER_PLATFORM;
        h.OnGate = this;
        waitTime += Time.deltaTime / Mobility;
        return true;
      }
      else
      {
        h.State = Human.StateType.MOVE;
      }
    }
    return false;
  }

  /**
    * 出場待ちを改札外に移動させます。
    * 一人移動できたなら、trueを返します
    */
  protected bool TryExit()
  {
    int ignore = 0;
    while (OutQueue.Count > 0 && OutQueue.Count > ignore)
    {
      var h = OutQueue.First.Value;
      OutQueue.RemoveFirst();
      if (h.Next == this as IRoutable)
      {
        h.OnGate = null;
        h.Complete();
        h.Pay();
        h.Randomize();
        h.State = Human.StateType.MOVE;
        waitTime += Time.deltaTime / Mobility;
        return true;
      }
      else
      {
        // 途中で再経路探索され、目的地が変わった場合は無視
        OutQueue.AddLast(h);
        ignore++;
      }
    }
    return false;
  }

  public Router Route { get { return router; } }

  private class RouterImpl : Router
  {
    protected ModelListener listener;
    protected Gate parent;
    public RouterImpl(ModelListener lis, Gate g)
    {
      listener = lis;
      parent = g;
    }
    /**
      * 自身を目的地とされた場合、移動者に対して指示を出します。
      * 外にいる場合、自身まで移動させます。到着した場合、入場列に並ばせます。
      * プラットフォームから出たい移動者は _step() で処理します (改札数のキャパシティ制約を受けるため)
      */
    public override void Handle(Human subject)
    {
      // コンコースに入ったが、目的地が変わり出場することになった
      if (parent.Concourse.Contains(subject))
      {
        parent.Concourse.Remove(subject);
        parent.OutQueue.AddLast(subject);
        subject.State = Human.StateType.WAIT_EXIT_GATE;
        return;
      }

      // 待機列にいるならば人を待たせる
      if (parent.OutQueue.Contains(subject) || parent.InQueue.Contains(subject))
      {
        return;
      }

      // 地面を歩いているならば、自身に向かって移動させる
      if (subject.Seek(parent.transform.position))
      {
        // 到着したならば、入場待機列に移動させる
        parent.InQueue.AddLast(subject);
        listener.Fire(EventType.MODIFIED, parent.BelongsTo);
        subject.State = Human.StateType.WAIT_ENTER_GATE;
      }
      else
      {
        subject.State = Human.StateType.MOVE;
      }
    }

    public override void Discard(Human subject)
    {
      // 改札に入りたかった人を取り除く。改札へ移動中の人の場合何もしない
      parent.InQueue.Remove(subject);
      listener.Fire(EventType.MODIFIED, parent.BelongsTo);
      // 改札を出たかった人を取り除く
      parent.OutQueue.Remove(subject);
    }
  }
}