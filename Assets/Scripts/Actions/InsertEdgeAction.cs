public class InsertEdgeAction : Transactional
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

  public void Rollback()
  {
    pivot.Shrink(prevNext);
  }
}