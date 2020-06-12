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
  public float railInterval = 0.1f;
  public float shortThreshold = 1f;
  public float stationInterval = 2.5f;
  public int trainInterval = 2;
  public float terminalInterval = 0.95f;
  [System.NonSerialized] public Action action;
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
    action = new Action(storage, listener, factory);
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
      action.StartRail(pos);
      action.BuildStation();
      action.CreateLine();
      action.StartLine();
      action.DeployTrain(action.tailLine.top);
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
        action.tailNode.GetComponent<Transform>().position,
        tailPosition) < railInterval)
      {
        return;
      }
      var dist = action.ExtendRail(pos);
      InterviseStation(dist);
      action.InsertEdge();
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
    if (distRail > stationInterval)
    {
      action.BuildStation();
      distRail -= stationInterval;
      distTrain++;
    }
  }

  private void InterviseTrain()
  {
    if (distTrain >= trainInterval)
    {
      action.tailLine.Filter(lt => lt.Departure == action.tailNode).ForEach(lt => action.DeployTrain(lt));
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
    var dist = Vector3.Distance(tailPosition, action.tailNode.transform.position);
    var tail = action.tailNode;
    while (tail.platform != action.tailPlatform)
    {
      var outEdge = tail.inEdge.Find(re => re.isOutbound);
      dist += outEdge.arrow.magnitude;
      tail = outEdge.from;
    }

    if (dist < terminalInterval)
    {
      while (action.tailNode.platform != action.tailPlatform)
      {
        action.actions.Last.Value.Rollback();
        action.actions.RemoveLast();
      }
    }
    else if (Vector3.Distance(tailPosition, action.tailNode.transform.position) > 0)
    {
      // 建設抑止していた場合、最後にクリックした地点まで延伸する
      action.ExtendRail(tailPosition);
      action.InsertEdge();
    }

    // 終点には駅があるようする
    if (action.tailNode.platform == null)
    {
      action.BuildStation();
      action.InsertPlatform();
    }
    // 終点にはかならず電車を配置する
    if (action.tailPlatform.depts.Count == 0)
    {
      Debug.LogWarning("no dept");
      return;
    }
    var dept = action.tailPlatform.depts[0];
    if (dept.trains.Count == 0)
    {
      action.DeployTrain(dept);
    }
    else if (!(dept.next is DeptTask))
    {
      // 終駅がある状態で end に入ると、すでに2台おかれている(deptとdept.nextに)。1台を撤去する
      // 1点nodeのときは撤去しない
      var lastTrainAction = this.action.actions.Last(a => a is DeployTrainAction && dept.next.trains.Contains((a as DeployTrainAction).train));
      // branch時は電車挿入場所のため、lastTrainActionがみつからない
      if (lastTrainAction != null)
      {
        lastTrainAction.Rollback();
        this.action.actions.Remove(lastTrainAction);
      }
    }
  }
}