using UnityEngine;

public class ModelFactory : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  public Company c;
  public Residence r;
  public RailNode rn;
  public RailEdge re;
  public Train t;
  public Human h;

  public Company NewCompany(int attractiveness, Vector3 pos)
  {
    return c.NewInstance(attractiveness, pos);
  }

  public Residence NewResidence(Vector3 pos)
  {
    return r.NewInstance(pos);
  }

  public RailNode NewRailNode(Vector3 pos)
  {
    return rn.NewInstance(pos);
  }

  public RailEdge NewRailEdge(RailNode from, RailNode to, bool isOutbound)
  {
    return re.NewInstance(from, to, isOutbound);
  }

  public Train NewTrain(Vector3 pos)
  {
    return t.NewInstance(pos);
  }

  public Human NewHuman(Residence r, Company c)
  {
    return h.NewInstance(r, c);
  }
}