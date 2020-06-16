using System.Collections.Generic;
using UnityEngine;

public class Transport : MonoBehaviour
{
  /**
   * 徒歩に比べて鉄道の移動がどれほど優位か
   */
  public float DistRatio = 0.1f;
  /**
   * 乗車コスト
   */
  public float RideCost = 1f;
  /**
   * 総延長に対する移動距離の割合に対して乗ずる料金
   */
  public float PayRatio = 4f;

  public ModelListener listener;
  public ModelStorage storage;
  public UserResource resource;

  [System.NonSerialized] public List<PathFinder> Finders;

  public int FinderIdx;
  public int RailLineIdx;
  public int DeptTaskIdx;
  public int TrainIdx;
  public bool IsWaiting = true;

  private void Awake()
  {
    Finders = new List<PathFinder>();
  }

  private void Start()
  {
    listener.Add<Platform>(EventType.CREATED, (p) => Add(p));
    listener.Add<DeptTask>(EventType.CREATED, (dept) => Add(dept));
    listener.Add<Train>(EventType.CREATED, (t) => Add(t));
    listener.Add<Platform>(EventType.DELETED, (p) => Remove(p));
    listener.Add<DeptTask>(EventType.DELETED, (dept) => Remove(dept));
    listener.Add<Train>(EventType.DELETED, (t) => Remove(t));
  }

  private void Update()
  {
    if (resource.State != UserResource.StateType.FIXED) return;
    if (IsFixed) return;
    if (IsWaiting)
    {
      Finders.ForEach(f => f.UnedgeAll());
      IsWaiting = false;
      return;
    }
    if (TrainIdx < storage.List<Train>().Count)
    {
      RouteTrain();
      return;
    }
    if (RailLineIdx < storage.List<RailLine>().Count)
    {
      Scan();
      return;
    }
    Finders[FinderIdx].Execute();
    FinderIdx++;
    TrainIdx = 0;
    RailLineIdx = 0;
  }

  public bool IsFixed { get { return !IsWaiting && FinderIdx == Finders.Count; } }

  public void Reset()
  {
    FinderIdx = 0;
    RailLineIdx = 0;
    DeptTaskIdx = 0;
    TrainIdx = 0;
    IsWaiting = true;
  }

  private void Add(Platform p)
  {
    Finders.ForEach(f => f.Node(p));
    Finders.Add(new PathFinder(p));
    Reset();
  }

  private void Add(DeptTask dept)
  {
    Finders.ForEach(f => f.Node(dept));
    Reset();
  }

  private void Add(Train t)
  {
    Finders.ForEach(f => f.Node(t));
  }

  private void Remove(Platform p)
  {
    Finders.ForEach(f => f.Unnode(p));
    Finders.RemoveAll(f => f.Goal.Origin == p as IRoutable);
    Reset();
  }

  private void Remove(DeptTask dept)
  {
    Finders.ForEach(f => f.Unnode(dept));
    Reset();
  }

  private void Remove(Train t)
  {
    Finders.ForEach(f => f.Unnode(t));
  }

  private float Payment(float length, RailLine l)
  {
    return length / Mathf.Sqrt(l.Length) * PayRatio;
  }
  /**
   * 電車の現在地点から各駅へのedgeを貼る。
   * これにより、電車が到達可能な駅に対して距離を設定できる
   */
  private void RouteTrain()
  {
    var f = Finders[FinderIdx];
    var t = storage.List<Train>()[TrainIdx];
    var current = t.Current;
    var length = current.Length;
    do
    {
      if (current is DeptTask)
      {
        f.Edge(t, (current as DeptTask).Stay, length * DistRatio, Payment(length, current.Parent));
      }
      current = current.Next;
      length += current.Length;
    } while (t.Current != current);
    TrainIdx++;
  }
  /**
   * 前の駅から次の駅までの距離をタスク距離合計とする
   * 乗車プラットフォーム => 発車タスク => 到着プラットフォームとする
   */
  private void Scan()
  {
    var f = Finders[FinderIdx];
    var l = storage.List<RailLine>()[RailLineIdx];
    // 各発車タスクを始発とし、電車で到達可能なプラットフォームを登録する
    var deptList = l.Filter(lt => lt is DeptTask);
    var dept = deptList[DeptTaskIdx] as DeptTask;

    // 乗車タスクとホームは相互移動可能
    f.Edge(dept, dept.Stay, 0);
    f.Edge(dept.Stay, dept, 0);

    LineTask current = dept;
    var length = current.Length;
    do
    {
      current = current.Next;
      length += current.Length;
      if (current is DeptTask)
      {
        // Dept -> P のみ登録する
        // P -> P 接続にしてしまうと乗り換えが必要かどうか分からなくなるため
        f.Edge(dept, (current as DeptTask).Stay, length * DistRatio + RideCost, Payment(length, l));
      }
    } while (dept != current);

    DeptTaskIdx++;
    if (DeptTaskIdx == deptList.Count)
    {
      DeptTaskIdx = 0;
      RailLineIdx++;
    }
  }
}