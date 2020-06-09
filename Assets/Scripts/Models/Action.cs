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


public class Action : MonoBehaviour
{
  public ModelFactory factory;

  [System.NonSerialized] public List<Transactional> actions;
  [System.NonSerialized] public RailNode tailNode;
  [System.NonSerialized] public RailEdge tailEdge;

  public Action()
  {
    actions = new List<Transactional>();
  }

  public void StartRail(Vector3 pos)
  {
    var action = new StartRailAction(factory, tailNode, (prev) => { tailNode = prev; });
    tailNode = action.Act(pos);
    actions.Add(action);
  }

  public float ExtendRail(Vector3 pos)
  {
    var action = new ExtendRailAction(tailNode, (prev) => { tailNode = prev; });
    tailEdge = action.Act(pos);
    tailNode = tailEdge.to;
    actions.Add(action);
    return Vector3.Magnitude(tailEdge.arrow);
  }
}