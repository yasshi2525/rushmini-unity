using System;
using System.Collections.Generic;
using UnityEngine;

public class DeptTask : LineTask, IRoutable
{
  public Platform Stay;
  private Router router;
  public LinkedList<Human> Queue;

  public DeptTask(ModelStorage db, ModelListener lis, RailLine l, Platform p) : base(db, lis, l)
  {
    Stay = p;
    Stay.Depts.AddLast(this);
    db.Add(this);
    Queue = new LinkedList<Human>();
    router = new RouterImpl(this);
    listener.Fire(EventType.CREATED, this);
  }
  public DeptTask(ModelStorage db, ModelListener lis, RailLine l, Platform p, LineTask lt) : base(db, lis, l, lt)
  {
    Stay = p;
    Stay.Depts.AddLast(this);
    db.Add(this);
    Queue = new LinkedList<Human>();
    router = new RouterImpl(this);
    listener.Fire(EventType.CREATED, this);
  }

  public override RailNode Departure { get { return Stay.On; } }

  public override RailNode Destination { get { return Stay.On; } }

  public override bool IsNeighbor(RailEdge edge) { return Stay.On == edge.From; }

  public override float SignedAngle(RailEdge edge)
  {
    if (!IsNeighbor(edge))
    {
      throw new ArgumentException("ould not calculate angle to un-neighbored edge");
    }

    var obj = Prev;
    while (obj != this)
    {
      if (obj.Length > 0)
      {
        return obj.SignedAngle(edge);
      }
      obj = obj.Prev;
    }
    throw new ArgumentException("line has no edge task");
  }

  public override float Length { get { return 0; } }


  /**
   * 現在地点で路線を分断し、指定された往復路を路線タスクに挿入します
   * Before (a) = (a) -> (b)
   * After  (a) = (a) -> (X) -> (a) -> (a) -> (b)
   * * edge : (a) -> (X)
   */
  public override void InsertEdge(RailEdge edge)
  {
    var nx = Next;
    var inbound = CreateTask(edge);

    if (this != nx)
    {
      // 自身が発車タスクなので、復路の後の発車タスクを追加する
      var dept = new DeptTask(storage, listener, Parent, Stay, inbound);
      dept.Next = nx;
      nx.Prev = dept;
    }
    else
    {
      // 単体dept(セルフループ)の場合は例外で発車タスクをつけない
      inbound.Next = nx;
      nx.Prev = inbound;
    }
  }

  public override void InsertPlatform(Platform platform)
  {
    throw new ArgumentException("try to insert platform to DeptTask");
  }

  public override void Remove()
  {
    Stay.Depts.Remove(this);
    base.Remove();
  }

  public Router Route { get { return router; } }

  private class RouterImpl : Router
  {
    protected DeptTask parent;
    public RouterImpl(DeptTask dept)
    {
      parent = dept;
    }
    /**
      * プラットフォームで電車を待っているならば、乗車待ちリストに登録します
      */
    public override void Handle(Human subject)
    {
      if (!parent.Queue.Contains(subject))
      {
        subject.State = Human.StateType.WAIT_TRAIN_ARRIVAL;
        subject.OnPlatform = null;
        subject.OnDeptTask = parent;
        parent.Stay.InQueue.Remove(subject);
        parent.Queue.AddLast(subject);
      }
    }

    /**
      * ホームにおり、発車待機列に並ぶのを待っていた人を取り除く
      * 次のframeで _fireがコールされる人が該当
      * 電車の待機者を取り除く
      */
    public override void Discard(Human subject)
    {
      parent.Stay.InQueue.Remove(subject);
      parent.Queue.Remove(subject);
    }
  }
}