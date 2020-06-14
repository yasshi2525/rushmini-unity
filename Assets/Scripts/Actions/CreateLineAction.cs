public class CreateLineAction : Transactional
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

  public void Rollback()
  {
    if (rollback != null)
    {
      rollback(prevLine);
    }
    l.Remove();
  }
}