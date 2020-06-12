using UnityEngine;

public class ModelFactory : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  public Company c;
  public Residence r;
  public RailNode rn;
  public RailEdge re;
  public RailPart rp;
  public Station st;
  public Platform p;
  public Gate g;
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

  public RailPart NewRailPart(RailEdge parent, bool isForward)
  {
    return rp.NewInstance(parent, isForward);
  }

  public Station NewStation()
  {
    return st.NewInstance();
  }

  public Gate NewGate(Station s)
  {
    return g.NewInstance(s);
  }

  public Platform NewPlatform(RailNode on, Station s)
  {
    return p.NewInstance(on, s);
  }

  public Train NewTrain(LineTask lt)
  {
    return t.NewInstance(lt);
  }

  public Human NewHuman(Residence r, Company c)
  {
    return h.NewInstance(r, c);
  }
}