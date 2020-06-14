public class InsertPlatformAction : Transactional
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

  public void Rollback()
  {
    line.RemovePlatform(platform);
  }
}
