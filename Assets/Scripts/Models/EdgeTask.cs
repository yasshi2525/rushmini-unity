using System;
using System.Collections.Generic;
using UnityEngine;

public class EdgeTask : LineTask
{
  public RailEdge edge;

  public EdgeTask(ModelStorage db, ModelListener lis, RailLine l, RailEdge re, LineTask lt) : base(db, lis, l, lt)
  {
    edge = re;
    db.Add(this);
    listener.Fire(EventType.CREATED, this);
  }

  public override RailNode Departure { get { return edge.from; } }
  public override RailNode Destination { get { return edge.to; } }
  public override float SignedAngle(RailEdge re)
  {
    if (!IsNeighbor(re))
    {
      throw new ArgumentException("ould not calculate angle to un-neighbored edge");
    }
    return Vector3.SignedAngle(-edge.arrow, re.arrow, Vector3.forward);
  }
  public override float Length { get { return edge.arrow.magnitude; } }
  public override bool IsNeighbor(RailEdge re) { return edge.to = re.from; }
  public override void InsertEdge(RailEdge re)
  {
    var nx = next;
    var inbound = CreateTask(edge);
    inbound.next = nx;
    nx.prev = inbound;
  }
  public override void InsertPlatform(Platform platform)
  {
    if (edge.to != platform.on)
    {
      throw new ArgumentException("try to insert non-neighbored platform");
    }
    var obj = next;
    var dept = new DeptTask(storage, listener, parent, platform, this);
    dept.next = obj;
  }
}