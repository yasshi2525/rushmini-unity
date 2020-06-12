using System;
using System.Collections.Generic;
using UnityEngine;

public class DeptTask : MonoBehaviour, ILineTask
{
  public ModelListener listener;
  public ModelStorage storage;
  public ModelFactory factory;
  private DeptTask template;
  private bool isTemplate = true;
  [System.NonSerialized] public RailLine parent;
  [System.NonSerialized] public ILineTask prev;
  [System.NonSerialized] public ILineTask next;
  [System.NonSerialized] public List<Train> trains;
  [System.NonSerialized] public Platform stay;

  private void Awake()
  {
    trains = new List<Train>();
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<DeptTask>(EventType.CREATED, dept => storage.Find<DeptTask>().Add(dept));
      listener.Add<DeptTask>(EventType.DELETED, dept => storage.Find<DeptTask>().Remove(dept));
    }
  }

  public DeptTask NewInstance(RailLine l, Platform stay)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.parent = l;
    obj.prev = obj;
    obj.next = obj;
    obj.stay = stay;
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public DeptTask NewInstance(RailLine l, Platform stay, ILineTask prev)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.parent = l;
    obj.prev = prev;
    prev.Next = obj;
    obj.stay = stay;
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  RailLine ILineTask.Parent
  {
    get { return parent; }
  }

  ILineTask ILineTask.Prev
  {
    get { return prev; }
    set { prev = value; }
  }

  ILineTask ILineTask.Next
  {
    get { return next; }
    set { next = value; }
  }

  List<Train> ILineTask.Trains
  {
    get { return trains; }
  }

  RailNode ILineTask.Departure()
  {
    return stay.on;
  }

  RailNode ILineTask.Destination()
  {
    return stay.on;
  }

  bool ILineTask.IsNeighbor(RailEdge edge)
  {
    return stay.on == edge.from;
  }

  float ILineTask.SignedAngle(RailEdge edge)
  {
    if (!(this as ILineTask).IsNeighbor(edge))
    {
      throw new ArgumentException("ould not calculate angle to un-neighbored edge");
    }

    var obj = prev;
    while (obj != this as ILineTask)
    {
      if (obj.Length > 0)
      {
        return obj.SignedAngle(edge);
      }
      obj = obj.Prev;
    }
    throw new ArgumentException("line has no edge task");
  }

  float ILineTask.Length { get { return 0; } }


  /**
   * 現在地点で路線を分断し、指定された往復路を路線タスクに挿入します
   * Before (a) = (a) -> (b)
   * After  (a) = (a) -> (X) -> (a) -> (a) -> (b)
   * * edge : (a) -> (X)
   */
  void ILineTask.InsertEdge(RailEdge edge)
  {
    if (!(this as ILineTask).IsNeighbor(edge))
    {
      throw new ArgumentException("try to insert non-neighbored edge");
    }
    var nx = next;
    var outBound = factory.NewEdgeTask(parent, edge, this);
    EdgeTask inbound;
    if (!edge.to.platform)
    {
      inbound = factory.NewEdgeTask(parent, edge.reverse, outBound);
    }
    else
    {
      inbound = factory.NewEdgeTask(parent, edge.reverse, factory.NewDeptTask(parent, edge.to.platform, outBound));
    }

    if (this as ILineTask != nx)
    {
      // 自身が発車タスクなので、復路の後の発車タスクを追加する
      var dept = factory.NewDeptTask(parent, stay, inbound);
      dept.next = nx;
      nx.Prev = dept;
    }
    else
    {
      // 単体dept(セルフループ)の場合は例外で発車タスクをつけない
      inbound.next = nx;
      nx.Prev = inbound;
    }
  }

  void ILineTask.InsertPlatform(Platform platform)
  {
    throw new ArgumentException("try to insert platform to DeptTask");
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