using UnityEngine;
public class ExtendRailAction : Transactional
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

  public void Rollback()
  {
    rollback(prevTail);
    re.To.Remove();
    re.Remove();
    re.Reverse.Remove();
  }
}
