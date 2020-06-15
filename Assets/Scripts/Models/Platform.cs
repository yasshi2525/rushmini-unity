using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour, IRoutable
{
  public ModelListener listener;
  public ModelStorage storage;

  private Platform template;
  private bool isTemplate = true;
  public int Capacity = 20;
  private Router router;

  [System.NonSerialized] public RailNode On;
  [System.NonSerialized] public Station BelongsTo;
  [System.NonSerialized] public LinkedList<DeptTask> Depts;

  /**
   * プラットフォームで、電車待機列に入るのを待機している人
   */
  [System.NonSerialized] public LinkedList<Human> InQueue;
  /**
   * 電車から降りた人
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
      listener.Add<Platform>(EventType.CREATED, p => storage.Add(p));
      listener.Add<Platform>(EventType.DELETED, p => storage.Remove(p));
    }
  }

  public Platform NewInstance(RailNode on, Station st)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.On = on;
    on.StandsOver = obj;
    obj.transform.position = on.transform.position;
    obj.BelongsTo = st;
    st.AddPlatform(obj);
    obj.Depts = new LinkedList<DeptTask>();
    obj.InQueue = new LinkedList<Human>();
    obj.OutQueue = new LinkedList<Human>();
    obj.router = new RouterImpl(obj);
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    On.StandsOver = null;
    listener.Fire(EventType.MODIFIED, On);
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }

  public int Used { get { return InQueue.Count + Depts.Sum(d => d.Queue.Count); } }

  /**
   * 乗車列にならんでいたが、経路再探索で別のホーム/改札への移動が決まった
   */
  private bool TryLeaveDeptQueue(Human subject)
  {
    if (subject.OnDeptTask != null)
    {
      subject.OnDeptTask.Queue.Remove(subject);
      subject.State = Human.StateType.WAIT_EXIT_PLATFORM;
      subject.OnTrain = null;
      subject.OnDeptTask = null;
      subject.OnPlatform = this;
      OutQueue.AddLast(subject);
      return true;
    }
    return false;
  }

  /**
   * ホームへ移動するときだったが、経路再探索で別のホーム/改札への移動が決まった
   */
  private bool TryLeaveInQueue(Human subject)
  {
    if (InQueue.Contains(subject))
    {
      InQueue.Remove(subject);
      subject.State = Human.StateType.WAIT_EXIT_PLATFORM;
      OutQueue.AddLast(subject);
      return true;
    }
    return false;
  }

  /**
   * 改札に向かっていたが経路再探索でホーム入場にかわった
   */
  private bool TryLeaveOutQueue(Human subject)
  {
    var gate = BelongsTo.Under;
    if (gate.OutQueue.Contains(subject))
    {
      // コンコースに行きたいが満員で移動できない
      if (gate.Concourse.Count >= gate.Capacity)
      {
        return true;
      }
      gate.OutQueue.Remove(subject);
      gate.Concourse.AddLast(subject);
      subject.State = Human.StateType.WAIT_ENTER_PLATFORM;
      subject.OnPlatform = null;
      subject.OnGate = gate;
      return true;
    }
    return false;
  }

  /**
   * 駅入場者をプラットフォーム上にならばせる。
   */
  private bool TryInQueue(Human subject)
  {
    var gate = BelongsTo.Under;
    if (gate.Concourse.Contains(subject) && Used < Capacity)
    {
      gate.Concourse.Remove(subject);
      subject.OnGate = null;
      subject.Complete();
      InQueue.AddLast(subject);
      subject.State = Human.StateType.WAIT_ENTER_DEPTQUEUE;
      subject.OnPlatform = this;
      return true;
    }
    return false;
  }

  /**
   * 到着した人を改札/コンコースに向かわせる
   */
  private bool TryOutQueue(Human subject)
  {
    var gate = BelongsTo.Under;
    if (OutQueue.Contains(subject))
    {
      OutQueue.Remove(subject);
      if (subject.Next.Route.NextFor(subject.Destination) == gate as IRoutable)
      {
        // 到着した人を改札に向かわせる
        subject.Complete();
        subject.State = Human.StateType.WAIT_EXIT_GATE;
        gate.OutQueue.AddLast(subject);
      }
      else
      {
        // 乗り換えの場合、次の次がDeptTask
        subject.State = Human.StateType.WAIT_ENTER_PLATFORM;
        gate.Concourse.AddLast(subject);
      }
      subject.OnPlatform = null;
      subject.OnGate = gate;
      return true;
    }
    return false;
  }

  public Router Route { get { return router; } }

  private class RouterImpl : Router
  {
    private Platform parent;
    public RouterImpl(Platform p)
    {
      parent = p;
    }

    public override void Handle(Human subject)
    {
      if (!parent.TryLeaveDeptQueue(subject))
        if (!parent.TryLeaveInQueue(subject))
          if (!parent.TryLeaveOutQueue(subject))
            if (!parent.TryInQueue(subject))
              parent.TryOutQueue(subject);
    }

    public override void Discard(Human subject)
    {
      // コンコースからホームへの移動待ちの人を取り除く
      parent.BelongsTo.Under.Concourse.Remove(subject);
    }
  }
}