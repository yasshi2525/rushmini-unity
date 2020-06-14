public class BuildStationAction : Transactional
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

  public void Rollback()
  {
    rollback(prevTail, prevPlatform);
    platform.BelongsTo.Under.Remove();
    platform.BelongsTo.Remove();
    platform.Remove();
  }
}