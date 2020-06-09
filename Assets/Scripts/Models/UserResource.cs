using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UserResource : MonoBehaviour
{
  public enum State
  {
    INITED, STARTED, FIXED
  }

  public ModelListener listener;

  /**
   * 最低この距離離れないと、RailEdgeを作成しない (じぐざぐ防止)
   */
  public float railInterval = 1f;
  public float shortThreshold = 10f;
  public float stationInterval = 25f;
  public int trainInterval = 2;
  public float terminalInterval = 9.5f;
  public Action action;
  /**
   * end() 時に、このポイントまで伸ばす
   */
  private Vector3 tailPosition;

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
    if (this.state == State.INITED)
    {
      action.StartRail(pos);
      SetState(State.STARTED);
    }
  }

  public void Extend(Vector3 pos)
  {
    if (this.state == State.STARTED)
    {
      // 近い距離でつくってしまうとじぐざぐするのでスキップする
      tailPosition = pos;
      if (Vector3.Distance(
        action.tailNode.GetComponent<Transform>().position,
        tailPosition) < railInterval)
      {
        return;
      }
      action.ExtendRail(pos);
    }
  }
}