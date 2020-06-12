using System;
using System.Collections.Generic;
using UnityEngine;

public class EdgeTask : MonoBehaviour, ILineTask
{
  public ModelListener listener;
  public ModelStorage storage;
  public ModelFactory factory;
  private EdgeTask template;
  private bool isTemplate = true;
  [System.NonSerialized] public RailLine parent;
  [System.NonSerialized] public ILineTask prev;
  [System.NonSerialized] public ILineTask next;
  [System.NonSerialized] public RailEdge edge;
  [System.NonSerialized] public List<Train> trains;

  private void Awake()
  {
    trains = new List<Train>();
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<RailLine>(EventType.CREATED, l => storage.Find<RailLine>().Add(l));
      listener.Add<RailLine>(EventType.DELETED, l => storage.Find<RailLine>().Remove(l));
    }
  }

  public EdgeTask NewInstance(RailLine line, RailEdge re, ILineTask prev)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.parent = line;
    obj.edge = re;
    obj.prev = prev;
    prev.Next = obj;
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  RailLine ILineTask.Parent { get { return parent; } }
  ILineTask ILineTask.Prev { get { return prev; } set { prev = value; } }
  ILineTask ILineTask.Next { get { return next; } set { next = value; } }
  List<Train> ILineTask.Trains { get { return trains; } }
  RailNode ILineTask.Departure() { return edge.from; }
  RailNode ILineTask.Destination() { return edge.to; }
  float ILineTask.SignedAngle(RailEdge re)
  {
    if (!(this as ILineTask).IsNeighbor(edge))
    {
      throw new ArgumentException("ould not calculate angle to un-neighbored edge");
    }

    return Vector3.SignedAngle(-edge.arrow, re.arrow, Vector3.forward);
  }
  float ILineTask.Length { get { return edge.arrow.magnitude; } }
  bool ILineTask.IsNeighbor(RailEdge re)
  {
    return edge.to = re.from;
  }
  void ILineTask.InsertEdge(RailEdge re)
  {
    if (!(this as ILineTask).IsNeighbor(edge))
    {
      throw new ArgumentException("try to insert non-neighbored edge");
    }
    var nx = next;
    var outBound = factory.NewEdgeTask(parent, re, this);
    EdgeTask inbound;
    if (!re.to.platform)
    {
      inbound = factory.NewEdgeTask(parent, re.reverse, outBound);
    }
    else
    {
      inbound = factory.NewEdgeTask(parent, re.reverse, factory.NewDeptTask(parent, re.to.platform, outBound));
    }
    inbound.next = nx;
    nx.Prev = inbound;
  }
  void ILineTask.InsertPlatform(Platform platform)
  {
    if (edge.to != platform.on)
    {
      throw new ArgumentException("try to insert non-neighbored platform");
    }
    var obj = next;
    var dept = factory.NewDeptTask(parent, platform, this);
    dept.next = obj;
  }
  void ILineTask.Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }

  /**
   * 現在のタスクの次を指定のタスクに設定します。
   * 今ある中間タスクはすべて削除されます
   */
  void ILineTask.Shrink(ILineTask to)
  {
    var obj = next;
    while (obj != to)
    {
      obj.Trains.ForEach(t => t.Skip(to));
      obj.Remove();
      obj = obj.Next;
    }
    next = to;
    to.Prev = this;
    listener.Fire(EventType.MODIFIED, this);
    listener.Fire(EventType.MODIFIED, to);
  }
}