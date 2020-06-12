using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public class RailLine
{
  protected ModelListener listener;
  protected ModelStorage storage;
  public DeptTask top;

  public RailLine(ModelStorage db, ModelListener lis)
  {
    storage = db;
    listener = lis;
    storage.Add(this);
    listener.Fire(EventType.CREATED, this);
  }

  public void Remove()
  {
    storage.Remove(this);
    listener.Fire<RailLine>(EventType.DELETED, this);
  }

  public void StartLine(Platform p)
  {
    if (top != null)
    {
      throw new ArgumentException("try to start already constructed line");
    }
    top = new DeptTask(storage, listener, this, p);
  }

  public delegate bool Cond(LineTask lt);

  /**
   * 指定された条件を満たすタスクを絞り込みます
   */
  public List<LineTask> Filter(Cond cond)
  {
    if (top == null) return new List<LineTask>();
    var result = new List<LineTask>();
    LineTask current = top;
    do
    {
      if (cond(current))
      {
        result.Add(current);
      }
      current = current.next;
    } while (current != top);
    return result;
  }

  /**
   * 指定された線路の始点を終点とする隣接タスクを返します
   */
  private List<LineTask> FilterNeighbors(RailEdge re)
  {
    // 隣接していないタスクはスキップ
    // 駅に到着するタスクはスキップ。発車タスクの後に挿入する
    return Filter((lt) => lt.IsNeighbor(re) && lt.next is EdgeTask);
  }

  /**
    * 候補が複数ある場合、距離0の移動タスクは角度の計算ができないのでスキップ
    */
  private List<LineTask> FilterOutUnangled(List<LineTask> neighbors)
  {
    return (neighbors.Count == 1) ? neighbors : neighbors.FindAll(lt => lt is DeptTask || lt.Length > 0);
  }

  /**
    * 次のタスクへの回転角が最も大きいものを返す
    */
  private LineTask FindLargestAngle(List<LineTask> list, RailEdge edge)
  {
    list.Sort((lt1, lt2) =>
    {
      if (lt1.SignedAngle(edge) > lt2.SignedAngle(edge)) return 1;
      else if (lt1.SignedAngle(edge) < lt2.SignedAngle(edge)) return -1;
      return 0;
    });
    return list[0];
  }

  /**
    * 指定された線路と隣接するタスクの内、右向き正とした角度がもっとも大きいタスクを返します
    * これは線路を分岐させたとき、どの分岐先を選べばよいか判定するためのものです
    */
  private LineTask FindFarLeft(RailEdge re)
  {
    if (top == null) return null;
    // セルフループの場合自身を返す
    if (top.next == top)
    {
      if (!top.IsNeighbor(re))
      {
        throw new ArgumentException("top is not neighbored edge");
      }
      return top;
    }
    // 隣接するタスクを絞り込む
    var neighbors = FilterNeighbors(re);
    if (neighbors.Count == 0)
    {
      throw new ArgumentException("edge is not any neighbored");
    }
    // 候補が複数ある場合、距離0の移動タスクは角度の計算ができないのでスキップ
    var candidates = FilterOutUnangled(neighbors);
    // 次のタスクへの回転角が最も大きいものを返す
    return FindLargestAngle(candidates, re);
  }

  public (LineTask, LineTask) InsertEdge(RailEdge re)
  {
    var pivot = FindFarLeft(re);
    var prevNext = pivot.next;
    pivot.InsertEdge(re);
    return (pivot, prevNext);
  }

  /**
   * 指定された駅を自路線に組み込みます
   */
  public void InsertPlatform(Platform platform)
  {
    Filter(lt => lt.Destination == platform.on).ForEach(lt => lt.InsertPlatform(platform));
  }

  public void RemovePlatform(Platform platform)
  {
    Filter(lt => lt is DeptTask && (lt as DeptTask).stay == platform).ForEach(dept => dept.prev.Shrink(dept.next));
  }

  public float Length
  {
    get
    {
      return Filter((_) => true).Aggregate(0f, (prev, current) =>
        prev + current.Length
      );
    }
  }
}