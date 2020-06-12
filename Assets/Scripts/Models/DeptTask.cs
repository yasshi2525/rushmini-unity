using System;
using System.Collections.Generic;
using UnityEngine;

public class DeptTask : LineTask
{
  public Platform stay;

  public DeptTask(ModelStorage db, ModelListener lis, RailLine l, Platform p) : base(db, lis, l)
  {
    stay = p;
    stay.depts.Add(this);
    db.Add(this);
    listener.Fire(EventType.CREATED, this);
  }
  public DeptTask(ModelStorage db, ModelListener lis, RailLine l, Platform p, LineTask lt) : base(db, lis, l, lt)
  {
    stay = p;
    stay.depts.Add(this);
    db.Add(this);
    listener.Fire(EventType.CREATED, this);
  }

  public override RailNode Departure { get { return stay.on; } }

  public override RailNode Destination { get { return stay.on; } }

  public override bool IsNeighbor(RailEdge edge) { return stay.on == edge.from; }

  public override float SignedAngle(RailEdge edge)
  {
    if (!IsNeighbor(edge))
    {
      throw new ArgumentException("ould not calculate angle to un-neighbored edge");
    }

    var obj = prev;
    while (obj != this)
    {
      if (obj.Length > 0)
      {
        return obj.SignedAngle(edge);
      }
      obj = obj.prev;
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
    var nx = next;
    var inbound = CreateTask(edge);

    if (this != nx)
    {
      // 自身が発車タスクなので、復路の後の発車タスクを追加する
      var dept = new DeptTask(storage, listener, parent, stay, inbound);
      dept.next = nx;
      nx.prev = dept;
    }
    else
    {
      // 単体dept(セルフループ)の場合は例外で発車タスクをつけない
      inbound.next = nx;
      nx.prev = inbound;
    }
  }

  public override void InsertPlatform(Platform platform)
  {
    throw new ArgumentException("try to insert platform to DeptTask");
  }
}