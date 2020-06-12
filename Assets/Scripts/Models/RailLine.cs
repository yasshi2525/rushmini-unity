using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public class RailLine
{
  protected ModelListener listener;
  protected ModelStorage storage;
  public DeptTask Top;

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
    if (Top != null)
    {
      throw new ArgumentException("try to start already constructed line");
    }
    Top = new DeptTask(storage, listener, this, p);
  }

  public delegate bool Cond(LineTask lt);

  /**
   * 指定された条件を満たすタスクを絞り込みます
   */
  public List<LineTask> Filter(Cond cond)
  {
    if (Top == null) return new List<LineTask>();
    var result = new List<LineTask>();
    LineTask current = Top;
    do
    {
      if (cond(current))
      {
        result.Add(current);
      }
      current = current.Next;
    } while (current != Top);
    return result;
  }

  /**
   * 指定された線路の始点を終点とする隣接タスクを返します
   */
  private List<LineTask> FilterNeighbors(RailEdge re)
  {
    // 隣接していないタスクはスキップ
    // 駅に到着するタスクはスキップ。発車タスクの後に挿入する
    return Filter((lt) => lt.IsNeighbor(re) && lt.Next is EdgeTask);
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
    if (Top == null) return null;
    // セルフループの場合自身を返す
    if (Top.Next == Top)
    {
      if (!Top.IsNeighbor(re))
      {
        throw new ArgumentException("top is not neighbored edge");
      }
      return Top;
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
    var prevNext = pivot.Next;
    pivot.InsertEdge(re);
    return (pivot, prevNext);
  }

  /**
   * 指定された駅を自路線に組み込みます
   */
  public void InsertPlatform(Platform platform)
  {
    Filter(lt => lt.Destination == platform.On).ForEach(lt => lt.InsertPlatform(platform));
  }

  public void RemovePlatform(Platform platform)
  {
    Filter(lt => lt is DeptTask && (lt as DeptTask).Stay == platform).ForEach(dept => dept.Prev.Shrink(dept.Next));
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