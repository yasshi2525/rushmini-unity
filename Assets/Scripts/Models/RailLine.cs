using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public class RailLine : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  public ModelFactory factory;
  private RailLine template;
  private bool isTemplate = true;
  [System.NonSerialized] public DeptTask top;

  private void Awake()
  {
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

  public RailLine NewInstance()
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    listener.Fire<RailLine>(EventType.DELETED, this);
    Destroy(gameObject);
  }

  public void StartLine(Platform p)
  {
    if (top)
    {
      throw new ArgumentException("try to start already constructed line");
    }
    top = factory.NewDeptTask(this, p);
  }

  public delegate bool Cond(ILineTask lt);

  /**
   * 指定された条件を満たすタスクを絞り込みます
   */
  public List<ILineTask> Filter(Cond cond)
  {
    if (top == null) return new List<ILineTask>();
    var result = new List<ILineTask>();
    ILineTask current = top;
    do
    {
      if (cond(current))
      {
        result.Add(current);
      }
      current = current.Next;
    } while (current != top as ILineTask);
    return result;
  }

  /**
   * 指定された線路の始点を終点とする隣接タスクを返します
   */
  private List<ILineTask> FilterNeighbors(RailEdge re)
  {
    // 隣接していないタスクはスキップ
    // 駅に到着するタスクはスキップ。発車タスクの後に挿入する
    return Filter((lt) => lt.IsNeighbor(re) && lt.Next is EdgeTask);
  }

  /**
    * 候補が複数ある場合、距離0の移動タスクは角度の計算ができないのでスキップ
    */
  private List<ILineTask> FilterOutUnangled(List<ILineTask> neighbors)
  {
    return (neighbors.Count == 1) ? neighbors : neighbors.FindAll(lt => lt is DeptTask || lt.Length > 0);
  }

  /**
    * 次のタスクへの回転角が最も大きいものを返す
    */
  private ILineTask FindLargestAngle(List<ILineTask> list, RailEdge edge)
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
  private ILineTask FindFarLeft(RailEdge re)
  {
    if (top == null) return null;
    // セルフループの場合自身を返す
    if (top.next == top as ILineTask)
    {
      if (!(top as ILineTask).IsNeighbor(re))
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

  public (ILineTask, ILineTask) InsertEdge(RailEdge re)
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
    Filter(lt => lt.Destination() == platform.on).ForEach(lt => lt.InsertPlatform(platform));
  }

  public void RemovePlatform(Platform platform)
  {
    Filter(lt => lt is DeptTask && (lt as DeptTask).stay == platform).ForEach(dept => dept.Prev.Shrink(dept.Next));
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