using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UserResource : MonoBehaviour
{
  public enum State
  {
    INITED, STARTED, FIXED
  }

  public ModelFactory factory;
  public ModelStorage storage;
  public ModelListener listener;

  /**
   * 最低この距離離れないと、RailEdgeを作成しない (じぐざぐ防止)
   */
  public float RailInterval = 0.1f;
  public float ShortThreshold = 1f;
  public float StationInterval = 2.5f;
  public int TrainInterval = 2;
  public float TerminalInterval = 0.95f;
  [System.NonSerialized] public Action Proxy;
  /**
   * end() 時に、このポイントまで伸ばす
   */
  private Vector3 tailPosition;
  /**
   * 駅を一定間隔で設置するため、最後に駅を作ってからextendした距離を保持するカウンター
   */
  private float distRail;
  /**
  * 電車の配置をスキップした駅の数
  */
  private int distTrain;

  [System.NonSerialized] public State state;
  private State committedState;
  private float committedLength;

  private IDictionary<State, UnityEvent> stateListeners;

  /**
   * 駅を一定間隔で設置するため、最後に駅を作ってからextendした距離を保持するカウンター
   */
  private float railLength;
  /**
   * 電車の配置をスキップした駅の数
   */
  private float skipStation;

  private void Awake()
  {
    state = State.INITED;
    stateListeners = new Dictionary<State, UnityEvent>();
  }

  private void Start()
  {
    Proxy = new Action(storage, listener, factory);
  }

  public UnityEvent FindListener(State ev)
  {
    if (!stateListeners.ContainsKey(ev))
    {
      stateListeners[ev] = new UnityEvent();
    }
    return stateListeners[ev];
  }

  private void SetState(State ev)
  {
    state = ev;
    FindListener(ev).Invoke();
  }

  public void StartRail(Vector3 pos)
  {
    if (state == State.INITED)
    {
      Proxy.StartRail(pos);
      Proxy.BuildStation();
      Proxy.CreateLine();
      Proxy.StartLine();
      Proxy.DeployTrain(Proxy.TailLine.Top);
      SetState(State.STARTED);
    }
  }

  public void ExtendRail(Vector3 pos)
  {
    if (state == State.STARTED)
    {
      // 近い距離でつくってしまうとじぐざぐするのでスキップする
      tailPosition = pos;
      if (Vector3.Distance(
        Proxy.TailNode.GetComponent<Transform>().position,
        tailPosition) < RailInterval)
      {
        return;
      }
      var dist = Proxy.ExtendRail(pos);
      InterviseStation(dist);
      Proxy.InsertEdge();
      InterviseTrain();
    }
  }

  public void EndRail()
  {
    if (state == State.STARTED)
    {
      InsertTerminal();
      SetState(State.FIXED);
    }
  }

  /**
    * 一定間隔で駅を作成する
    */
  private void InterviseStation(float dist)
  {
    distRail += dist;
    if (distRail > StationInterval)
    {
      Proxy.BuildStation();
      distRail -= StationInterval;
      distTrain++;
    }
  }

  private void InterviseTrain()
  {
    if (distTrain >= TrainInterval)
    {
      Proxy.TailLine.Filter(lt => lt.Departure == Proxy.TailNode).ForEach(lt => Proxy.DeployTrain(lt));
      distTrain = 0;
    }
  }

  private void InsertTerminal()
  {
    if (tailPosition == null)
    {
      // extendしていない場合は何もしない
      return;
    }

    // 最後に駅を作ってからカーソルまで距離が短い場合、
    // 駅作成までロールバックする
    var dist = Vector3.Distance(tailPosition, Proxy.TailNode.transform.position);
    var tail = Proxy.TailNode;
    while (tail.StandsOver != Proxy.TailPlatform)
    {
      var outEdge = tail.InEdge.Find(re => re.IsOutbound);
      dist += outEdge.Arrow.magnitude;
      tail = outEdge.From;
    }

    if (dist < TerminalInterval)
    {
      while (Proxy.TailNode.StandsOver != Proxy.TailPlatform)
      {
        Proxy.Actions.Last.Value.Rollback();
        Proxy.Actions.RemoveLast();
      }
    }
    else if (Vector3.Distance(tailPosition, Proxy.TailNode.transform.position) > 0)
    {
      // 建設抑止していた場合、最後にクリックした地点まで延伸する
      Proxy.ExtendRail(tailPosition);
      Proxy.InsertEdge();
    }

    // 終点には駅があるようする
    if (Proxy.TailNode.StandsOver == null)
    {
      Proxy.BuildStation();
      Proxy.InsertPlatform();
    }
    // 終点にはかならず電車を配置する
    if (Proxy.TailPlatform.Depts.Count == 0)
    {
      Debug.LogWarning("no dept");
      return;
    }
    var dept = Proxy.TailPlatform.Depts.First.Value;
    if (dept.Trains.Count == 0)
    {
      Proxy.DeployTrain(dept);
    }
    else if (!(dept.Next is DeptTask))
    {
      // 終駅がある状態で end に入ると、すでに2台おかれている(deptとdept.nextに)。1台を撤去する
      // 1点nodeのときは撤去しない
      var lastTrainAction = this.Proxy.Actions.LastOrDefault(a => a is DeployTrainAction && dept.Next.Trains.Contains((a as DeployTrainAction).train));
      // branch時は電車挿入場所のため、lastTrainActionがみつからない
      if (lastTrainAction != null)
      {
        lastTrainAction.Rollback();
        this.Proxy.Actions.Remove(lastTrainAction);
      }
    }
  }
}