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
    re.To.Remove();
    re.Remove();
    re.Reverse.Remove();
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
    platform.BelongsTo.Under.Remove();
    platform.BelongsTo.Remove();
    platform.Remove();
  }
}

class CreateLineAction : Transactional
{
  protected ModelStorage storage;
  protected ModelListener listener;
  public delegate void RollbackFn(RailLine l);
  protected RollbackFn rollback;
  protected RailLine prevLine;
  protected RailLine l;

  public CreateLineAction(ModelStorage db, ModelListener lis)
  {
    storage = db;
    listener = lis;
  }

  public CreateLineAction(ModelStorage db, ModelListener lis, RailLine line, RollbackFn fn) : this(db, lis)
  {
    prevLine = line;
    rollback = fn;
  }

  public RailLine Act()
  {
    l = new RailLine(storage, listener);
    return l;
  }

  void Transactional.Rollback()
  {
    if (rollback != null)
    {
      rollback(prevLine);
    }
    l.Remove();
  }
}

class StartLineAction : Transactional
{
  protected RailLine line;

  public StartLineAction(RailLine l)
  {
    line = l;
  }

  public void Act(Platform p)
  {
    line.StartLine(p);
  }

  void Transactional.Rollback()
  {
    line.Top.Remove();
    line.Top = null;
  }
}

class InsertEdgeAction : Transactional
{
  protected RailLine line;

  protected LineTask pivot;
  protected LineTask prevNext;

  public InsertEdgeAction(RailLine l)
  {
    line = l;
  }

  public void Act(RailEdge re)
  {
    (pivot, prevNext) = line.InsertEdge(re);
  }

  void Transactional.Rollback()
  {
    pivot.Shrink(prevNext);
  }
}

class InsertPlatformAction : Transactional
{
  protected RailLine line;
  protected Platform platform;

  public InsertPlatformAction(RailLine l)
  {
    line = l;
  }

  public void Act(Platform p)
  {
    platform = p;
    line.InsertPlatform(p);
  }

  void Transactional.Rollback()
  {
    line.RemovePlatform(platform);
  }
}

public class DeployTrainAction : Transactional
{
  protected ModelFactory factory;
  public Train train;

  public DeployTrainAction(ModelFactory f)
  {
    factory = f;
  }

  public Train Act(LineTask lt)
  {
    train = factory.NewTrain(lt);
    return train;
  }

  void Transactional.Rollback()
  {
    train.Remove();
  }
}

public class Action
{
  protected ModelFactory factory;
  protected ModelStorage storage;
  protected ModelListener listener;

  public LinkedList<Transactional> Actions;
  public RailNode TailNode;
  public RailEdge TailEdge;
  public Platform TailPlatform;
  public RailLine TailLine;

  public Action(ModelStorage db, ModelListener lis, ModelFactory f)
  {
    storage = db;
    listener = lis;
    factory = f;
    Actions = new LinkedList<Transactional>();
  }

  public void StartRail(Vector3 pos)
  {
    var action = new StartRailAction(factory, TailNode, (prev) => { TailNode = prev; });
    TailNode = action.Act(pos);
    Actions.AddLast(action);
  }

  public float ExtendRail(Vector3 pos)
  {
    var action = new ExtendRailAction(TailNode, (prev) => { TailNode = prev; });
    TailEdge = action.Act(pos);
    TailNode = TailEdge.To;
    Actions.AddLast(action);
    return Vector3.Magnitude(TailEdge.Arrow);
  }

  public void BuildStation()
  {
    var action = new BuildStationAction(TailNode, TailPlatform, (prevNode, prevPlatform) =>
    {
      TailNode = prevNode;
      TailPlatform = prevPlatform;
    });
    TailPlatform = action.Act();
    Actions.AddLast(action);
  }

  public void BuildStation(RailNode rn)
  {
    var action = new BuildStationAction(TailNode, TailPlatform, (prevNode, prevPlatform) =>
    {
      TailNode = prevNode;
      TailPlatform = prevPlatform;
    });
    TailPlatform = action.Act(rn);
    TailNode = rn;
    Actions.AddLast(action);
  }

  public void CreateLine()
  {
    var action = new CreateLineAction(storage, listener);
    TailLine = action.Act();
    Actions.AddLast(action);
  }

  public void StartLine()
  {
    var action = new StartLineAction(TailLine);
    action.Act(TailPlatform);
    Actions.AddLast(action);
  }

  public void InsertEdge()
  {
    var action = new InsertEdgeAction(TailLine);
    action.Act(TailEdge);
    Actions.AddLast(action);
  }

  public void InsertPlatform()
  {
    var action = new InsertPlatformAction(TailLine);
    action.Act(TailPlatform);
    Actions.AddLast(action);
  }

  public void InsertPlatform(Platform p)
  {
    var action = new InsertPlatformAction(TailLine);
    action.Act(p);
    Actions.AddLast(action);
  }

  public void DeployTrain(LineTask lt)
  {
    var action = new DeployTrainAction(factory);
    action.Act(lt);
    Actions.AddLast(action);
  }

  public void Commit()
  {
    Actions.Clear();
  }

  public void Rollback()
  {
    while (Actions.Count > 0)
    {
      Actions.Last.Value.Rollback();
      Actions.RemoveLast();
    }
  }
}