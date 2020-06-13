using System;
using System.Collections.Generic;
using UnityEngine;

public class EdgeTask : LineTask
{
  public RailEdge Edge;

  public EdgeTask(ModelStorage db, ModelListener lis, RailLine l, RailEdge re, LineTask lt) : base(db, lis, l, lt)
  {
    Edge = re;
    db.Add(this);
    listener.Fire(EventType.CREATED, this);
  }

  public override RailNode Departure { get { return Edge.From; } }
  public override RailNode Destination { get { return Edge.To; } }
  public override float SignedAngle(RailEdge re)
  {
    if (!IsNeighbor(re))
    {
      throw new ArgumentException("could not calculate angle to un-neighbored edge");
    }
    return Vector3.SignedAngle(-Edge.Arrow, re.Arrow, Vector3.forward);
  }
  public override float Length { get { return Edge.Arrow.magnitude; } }
  public override bool IsNeighbor(RailEdge re) { return Edge.To == re.From; }
  public override void InsertEdge(RailEdge re)
  {
    var nx = Next;
    var inbound = CreateTask(re);
    inbound.Next = nx;
    nx.Prev = inbound;
  }
  public override void InsertPlatform(Platform platform)
  {
    if (Edge.To != platform.On)
    {
      throw new ArgumentException("try to insert non-neighbored platform");
    }
    var obj = Next;
    var dept = new DeptTask(storage, listener, Parent, platform, this);
    dept.Next = obj;
    obj.Prev = dept;
  }
}