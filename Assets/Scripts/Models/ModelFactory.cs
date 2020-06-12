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
  public RailLine l;
  public DeptTask dept;
  public EdgeTask edge;
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

  public RailLine NewRailLine()
  {
    return l.NewInstance();
  }

  public DeptTask NewDeptTask(RailLine parent, Platform stay)
  {
    return dept.NewInstance(parent, stay);
  }

  public DeptTask NewDeptTask(RailLine parent, Platform stay, ILineTask prev)
  {
    return dept.NewInstance(parent, stay, prev);
  }

  public EdgeTask NewEdgeTask(RailLine parent, RailEdge re, ILineTask prev)
  {
    return edge.NewInstance(parent, re, prev);
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