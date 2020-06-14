public class StartLineAction : Transactional
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

  public void Rollback()
  {
    line.Top.Remove();
    line.Top = null;
  }
}
