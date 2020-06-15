using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public class StayTask : TrainTask
{
  protected DeptTask org;
  /**
   * 後、何秒待機すれば次の乗降を許可するか。
   * 移動タスクを秒単位で消費していくため、frameと同期がとれない。そのため秒単位で管理している
   */
  protected float wait;
  /**
   * 降車待ちの人
   */
  protected LinkedList<Human> OutQueue;
  /**
   * 乗車待ちの人
   */
  protected LinkedList<Human> InQueue;

  protected ModelListener listener;

  public StayTask(ModelListener lis, Train t, DeptTask dept, OnComplete fn) : base(t, fn)
  {
    org = dept;
    listener = lis;
    onComplete = () =>
    {
      // 積み残しがあるまま発車
      InQueue.ToList().ForEach(h =>
      {
        h.State = Human.StateType.WAIT_TRAIN_ARRIVAL;
        h.OnTrain = null;
      });
      fn();
    };
    org = dept;
    OutQueue = new LinkedList<Human>();
    InQueue = new LinkedList<Human>();
    org.Trains.Add(train);
  }

  public override Vector3 Position { get { return org.Departure.transform.position; } }

  /**
   * 乗降客がまだプラットフォーム、車内に残っているか。
   * 残っている間は発車できない
   */
  protected bool IsHumanRemained()
  {
    return (
      train.Passengers.Count < train.Capacity &&
      InQueue.Count + OutQueue.Count > 0
    );
  }

  protected override bool IsCompleted()
  {
    return progress >= 1 && !this.IsHumanRemained();
  }

  protected override float Estimate()
  {
    return Mathf.Max((1 - progress) * train.Stay, 0);
  }

  protected override float OnFullConsume(float available)
  {
    var cost = Estimate();
    progress = 1;
    if (IsHumanRemained())
    {
      // 発車抑止
      return 0;
    }
    return available - cost;
  }

  protected override float OnPartialConsume(float available)
  {
    progress += available / train.Stay;
    return 0;
  }

  /**
   * 電車が駅に到着した際、乗車客、降車客を確定させます。
   * 到着した瞬間にホームにいた客が対象
   */
  protected override void HandleOnInited()
  {
    train.Passengers.FindAll(h => h.Next == org.Stay as IRoutable).ForEach(h =>
    {
      h.State = Human.StateType.WAIT_EXIT_TRAIN;
      OutQueue.AddLast(h);
    });
    org.Queue.Where(h => h.Next == org).ToList().ForEach(h =>
    {
      h.State = Human.StateType.WAIT_ENTER_TRAIN;
      InQueue.AddLast(h);
    });
  }

  protected bool TryRide()
  {
    // 満員
    if (train.Passengers.Count >= train.Capacity)
    {
      return false;
    }
    while (InQueue.Count > 0)
    {
      var h = InQueue.First.Value;
      InQueue.RemoveFirst();
      if (h.Next == org)
      {
        // ホームの利用客を電車に乗せる
        h.Complete();
        h.State = Human.StateType.ON_TRAIN;
        org.Queue.Remove(h);
        h.OnDeptTask = null;
        h.Ride(org);
        train.Passengers.Add(h);
        // 電車が満員になったら通知
        if (train.Passengers.Count == train.Capacity)
        {
          listener.Fire(EventType.MODIFIED, train);
        }
        wait += 1 / train.Mobility;
        listener.Fire(EventType.RIDDEN, train);
        return true;
      }
      // 上記が else になるのは、発車待ち時に経路が変わったとき。
      // このとき Platform が Human#deptTask を参照しキューの入れ替えを行っている
      // そのためここでは 何もしない
    }
    return false;
  }

  protected bool TryGetOff()
  {
    while (OutQueue.Count > 0)
    {
      // 乗車している利用客をホームに移動させる
      var h = OutQueue.First.Value;
      OutQueue.RemoveFirst();
      if (h.Next == org.Stay as IRoutable)
      {
        h.State = Human.StateType.WAIT_EXIT_PLATFORM;
        h.OnTrain = null;
        org.Stay.OutQueue.AddLast(h);
        h.OnPlatform = org.Stay;
        h.GetOff(org.Stay);
        train.Passengers.Remove(h);
        wait += 1 / train.Mobility;
        listener.Fire(EventType.RIDDEN, train);
        return true;
      }
      else
      {
        // 降車待ち中に経路再探索がされ、目的地がかわり
        // 引き続きの乗車が決まった場合
        h.State = Human.StateType.ON_TRAIN;
      }
    }
    return false;
  }

  protected override void HandleOnConsumed(float available)
  {
    wait = Mathf.Max(wait - available, 0);
    if (wait < 1 / train.Mobility)
    {
      if (!TryGetOff()) TryRide();
    }
  }

  public override void Discard(Human subject)
  {
    // 乗車待ちの人を削除する
    InQueue.Remove(subject);
    // 電車乗降中はpassengerにおらず、taskのqueueにいる
    train.Passengers.Remove(subject);
    // 降車待ちの人を削除する
    OutQueue.Remove(subject);
  }

  public override LineTask Origin { get { return org; } }
}