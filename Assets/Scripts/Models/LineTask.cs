using System;
using System.Collections.Generic;

public abstract class LineTask
{
  protected ModelStorage storage;
  protected ModelListener listener;
  public RailLine Parent;
  public LineTask Prev;
  public LineTask Next;
  public List<Train> Trains;

  protected LineTask(ModelStorage db, ModelListener lis)
  {
    Trains = new List<Train>();
    listener = lis;
    storage = db;
  }

  public LineTask(ModelStorage db, ModelListener lis, RailLine line) : this(db, lis)
  {
    Parent = line;
    Prev = this;
    Next = this;
  }

  public LineTask(ModelStorage db, ModelListener lis, RailLine line, LineTask lt) : this(db, lis)
  {
    Parent = line;
    Prev = lt;
    Prev.Next = this;
  }

  public abstract RailNode Departure { get; }
  public abstract RailNode Destination { get; }
  /**
   * 自タスクの終点から何ラジアン回転すれば引数の線路に一致するか返す。(左回り正)
   */
  public abstract float SignedAngle(RailEdge edge);
  public abstract float Length { get; }
  /**
   * 指定された線路と隣接しているか判定します
   */
  public abstract bool IsNeighbor(RailEdge edge);
  /**
   * 現在地点で路線を分断し、指定された往復路を路線タスクに挿入します
   * Before (a) ---------------> (b) -> (c)
   * After  (a) -> (X) -> (a) -> (b) -> (c)
   * * edge : (a) -> (X)
   */
  public abstract void InsertEdge(RailEdge edge);
  public abstract void InsertPlatform(Platform platform);

  /**
    * 現在のタスクに続く RailEdge に沿うタスクを作成します
    * 循環参照によるプロトタイプ生成失敗を防ぐため、別モジュールにしている
  */
  protected LineTask CreateTask(RailEdge edge)
  {
    if (!IsNeighbor(edge))
    {
      throw new ArgumentException("try to insert non-neighbored edge");
    }
    var outBound = new EdgeTask(storage, listener, Parent, edge, this);
    EdgeTask inbound;
    if (!edge.To.StandsOver)
    {
      inbound = new EdgeTask(storage, listener, Parent, edge.Reverse, outBound);
    }
    else
    {
      inbound = new EdgeTask(storage, listener, Parent, edge.Reverse, new DeptTask(storage, listener, Parent, edge.To.StandsOver, outBound));
    }
    return inbound;
  }

  public virtual void Remove()
  {
    storage.Remove(this);
    listener.Fire(EventType.DELETED, this);
  }

  /**
   * 現在のタスクの次を指定のタスクに設定します。
   * 今ある中間タスクはすべて削除されます
   */
  public void Shrink(LineTask to)
  {
    var obj = Next;
    while (obj != to)
    {
      obj.Trains.ForEach(t => t.Skip(to));
      obj.Remove();
      obj = obj.Next;
    }
    Next = to;
    to.Prev = this;
    listener.Fire(EventType.MODIFIED, this);
    listener.Fire(EventType.MODIFIED, to);
  }
}