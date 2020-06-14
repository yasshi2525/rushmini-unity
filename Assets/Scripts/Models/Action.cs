using System.Collections.Generic;
using UnityEngine;

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