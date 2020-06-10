using System.Collections.Generic;
using UnityEngine;

public interface Transactional
{
  void Rollback();
}

class StartRailAction : Transactional
{
  protected ModelFactory factory;

  protected RailNode prevTail;
  public delegate void RollbackFn(RailNode rn);
  protected RollbackFn rollback;
  protected RailNode rn;

  public StartRailAction(ModelFactory f, RailNode prev, RollbackFn fn)
  {
    factory = f;
    prevTail = prev;
    rollback = fn;
  }

  public RailNode Act(Vector3 pos)
  {
    rn = factory.NewRailNode(pos);

    return rn;
  }

  void Transactional.Rollback()
  {
    rollback(prevTail);
    rn.Remove();
  }
}

class ExtendRailAction : Transactional
{
  protected RailNode prevTail;
  public delegate void RollbackFn(RailNode rn);
  protected RollbackFn rollback;
  protected RailEdge re;

  public ExtendRailAction(RailNode prev, RollbackFn fn)
  {
    prevTail = prev;
    rollback = fn;
  }

  public RailEdge Act(Vector3 pos)
  {
    re = prevTail.Extend(pos);
    return re;
  }

  void Transactional.Rollback()
  {
    rollback(prevTail);
    re.to.Remove();
    re.Remove();
    re.reverse.Remove();
  }
}

class BuildStationAction : Transactional
{
  protected RailNode prevTail;
  protected Platform prevPlatform;
  public delegate void RollbackFn(RailNode rn, Platform p);
  protected RollbackFn rollback;
  protected Platform platform;

  public BuildStationAction(RailNode tail, Platform tailP, RollbackFn fn)
  {
    prevTail = tail;
    prevPlatform = tailP;
    rollback = fn;
  }

  public Platform Act()
  {
    platform = prevTail.BuildStation();
    return platform;
  }

  public Platform Act(RailNode rn)
  {
    platform = rn.BuildStation();
    return platform;
  }

  void Transactional.Rollback()
  {
    rollback(prevTail, prevPlatform);
    platform.station.gate.Remove();
    platform.station.Remove();
    platform.Remove();
  }
}

public class Action : MonoBehaviour
{
  public ModelFactory factory;

  [System.NonSerialized] public Stack<Transactional> actions;
  [System.NonSerialized] public RailNode tailNode;
  [System.NonSerialized] public RailEdge tailEdge;
  [System.NonSerialized] public Platform tailPlatform;

  public Action()
  {
    actions = new Stack<Transactional>();
  }

  public void StartRail(Vector3 pos)
  {
    var action = new StartRailAction(factory, tailNode, (prev) => { tailNode = prev; });
    tailNode = action.Act(pos);
    actions.Push(action);
  }

  public float ExtendRail(Vector3 pos)
  {
    var action = new ExtendRailAction(tailNode, (prev) => { tailNode = prev; });
    tailEdge = action.Act(pos);
    tailNode = tailEdge.to;
    actions.Push(action);
    return Vector3.Magnitude(tailEdge.arrow);
  }

  public void BuildStation()
  {
    var action = new BuildStationAction(tailNode, tailPlatform, (prevNode, prevPlatform) =>
    {
      tailNode = prevNode;
      tailPlatform = prevPlatform;
    });
    tailPlatform = action.Act();
    actions.Push(action);
  }

  public void BuildStation(RailNode rn)
  {
    var action = new BuildStationAction(tailNode, tailPlatform, (prevNode, prevPlatform) =>
    {
      tailNode = prevNode;
      tailPlatform = prevPlatform;
    });
    tailPlatform = action.Act(rn);
    tailNode = rn;
    actions.Push(action);
  }
}