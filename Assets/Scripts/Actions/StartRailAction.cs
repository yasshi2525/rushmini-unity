using UnityEngine;

public class StartRailAction : Transactional
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

  public void Rollback()
  {
    rollback(prevTail);
    rn.Remove();
  }
}
