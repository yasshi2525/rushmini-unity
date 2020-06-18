using System;
using UnityEngine;
public class Human : MonoBehaviour, IRoutable
{
  public ModelListener listener;
  public ModelStorage storage;
  private Human template;
  private bool isTemplate = true;
  [System.NonSerialized] public Residence Departure;
  [System.NonSerialized] public Company Destination;
  public float Speed = 0.5f;
  public int DespawnScore = -10;
  /**
   * 何秒間歩き続けたらゲームから除外されるか
   */
  public float LifeSpan = 8.5f;
  /**
  * 歩いていない状態（ホーム、電車の中にいるなど）の場合、
  * 歩く場合の何倍の体力を消費するか
  */
  public float StayBuff = 0.15f;
  public float Rand = 0.25f;
  /**
   * 残り体力。0-1
   */
  private float stamina;
  /**
   * 次に向かう経由点
   */
  [System.NonSerialized] public IRoutable Next;
  private DeptTask rideFrom;
  /**
   * 改札を出るときに支払う運賃
   */
  private float payment;
  [System.NonSerialized] public Gate OnGate;
  [System.NonSerialized] public Platform OnPlatform;
  [System.NonSerialized] public DeptTask OnDeptTask;
  /**
   * 電車と紐付ける。死んだことを電車に通知するため。
   * 電車乗車中は nextが Platformのため、電車側が分からない
   */
  [System.NonSerialized] public Train OnTrain;
  private float rideTime;
  private WaitEvent wait;
  private StateType state;

  public enum StateType
  {
    SPAWNED,
    MOVE,
    WAIT_ENTER_GATE,
    WAIT_ENTER_PLATFORM,
    WAIT_ENTER_DEPTQUEUE,
    WAIT_TRAIN_ARRIVAL,
    WAIT_ENTER_TRAIN,
    ON_TRAIN,
    WAIT_EXIT_TRAIN,
    WAIT_EXIT_PLATFORM,
    WAIT_EXIT_GATE,
    ARCHIVED,
    DIED,
    WAIT_REROUTING,
  }

  private Router router;

  private void Awake()
  {
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Human>(EventType.CREATED, h => storage.Add(h));
      listener.Add<Human>(EventType.DELETED, h => storage.Remove(h));
    }
  }

  private void Update()
  {
    if (!isTemplate)
    {
      wait.Wait();
      if (state == StateType.ON_TRAIN) rideTime += Time.deltaTime;
      Next?.Route.Handle(this);
      DamageByStay(Time.deltaTime);
      // 疲れ果てて死んだ
      if (stamina < 0 && state != StateType.DIED) Dead();
    }
  }

  public Human NewInstance(Residence r, Company c)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.router = new RouterImpl();
    obj.state = StateType.SPAWNED;
    obj.wait = new WaitEvent(listener, obj.state);
    obj.stamina = 1.0f;
    obj.payment = 0f;
    obj.rideTime = 0f;
    obj.Departure = r;
    obj.Destination = c;
    obj.Next = r.Route.NextFor(c);
    obj.GetComponent<SpriteRenderer>().enabled = true;
    obj.transform.position = r.transform.position;
    obj.Randomize();
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }

  public StateType State
  {
    get { return state; }
    set
    {
      wait.fire();
      state = value;
      wait = new WaitEvent(listener, state);
    }
  }

  /**
   * 指定した地点へワープする。
   * @param arg1
   * @param arg2
   */
  public float Warp(Vector3 pos)
  {
    var prev = transform.position;
    transform.position = pos;
    var dist = Vector3.Distance(pos, prev);
    if (dist > 0f)
    {
      listener.Fire(EventType.MODIFIED, this);
    }
    return dist;
  }

  private void DamageByWalk(float dist)
  {
    var time = dist / Speed;
    // stay の分まで引かないようにする
    stamina -= ((1 - StayBuff) * time) / LifeSpan;
  }

  private void DamageByStay(float time)
  {
    stamina -= StayBuff * time / LifeSpan;
  }

  /**
   * 指定した地点へ徒歩で移動する。戻り地は到達したかどうか
   */
  public bool Seek(Vector3 goal)
  {
    var remain = goal - transform.position;
    float available = Speed * Time.deltaTime;
    if (available >= remain.magnitude)
    {
      // オーバーランを防ぐ
      DamageByWalk(Warp(goal));
      return true;
    }
    else
    {
      float angle = Vector3.SignedAngle(Vector3.right, remain, Vector3.forward) / 180 * Mathf.PI;
      DamageByWalk(Warp(
        transform.position + available * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle))
      ));
      return false;
    }
  }

  public void Randomize()
  {
    var len = UnityEngine.Random.Range(0f, Rand);
    var theta = UnityEngine.Random.Range(0f, Mathf.PI * 2);
    Warp(transform.position + len * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta)));
  }

  public void Reroute()
  {
    Next = router.NextFor(Destination);
  }

  /**
   * 死亡状態にし、関連タスクから自身を除外する。
   */
  private void Dead()
  {
    listener.Fire(EventType.CREATED, new DieEvent(state));
    state = StateType.DIED;
    Next?.Route.Discard(this);
    OnTrain?.Route.Discard(this);
    listener.Fire(EventType.CREATED, new ScoreEvent(DespawnScore, this));
    Remove();
  }

  public void Ride(DeptTask dept)
  {
    rideFrom = dept;
  }

  public void GetOff(Platform p)
  {
    payment += rideFrom.Route.PaymentFor(p);
  }

  public void Pay()
  {
    listener.Fire(EventType.CREATED, new ScoreEvent(payment, this));
  }

  public void Complete()
  {
    var prev = Next;
    Next = Next.Route.NextFor(Destination);
    // 会社到着
    if (Next == null)
    {
      if (rideTime > 0) listener.Fire(EventType.CREATED, new CommuteEvent(rideTime));
      listener.Fire(EventType.CREATED, new DieEvent(state));
      Remove();
    }
  }

  public Router Route { get { return router; } }

  private class RouterImpl : Router
  {
    public override void Handle(Human subject)
    {
      throw new InvalidOperationException();
    }
    public override void Discard(Human subject)
    {
      throw new InvalidOperationException();
    }
  }

}