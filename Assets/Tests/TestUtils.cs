using UnityEngine;

public class TestUtils
{
  public float DELTA = 0.0001f;
  public ModelStorage storage;
  public ModelListener listener;
  public ModelFactory factory;
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
  public UserResource ures;
  public Transport trans;
  public TestUtils()
  {
    storage = new GameObject().AddComponent<ModelStorage>();
    listener = new GameObject().AddComponent<ModelListener>();
    factory = new GameObject().AddComponent<ModelFactory>();

    var co = new GameObject();
    co.AddComponent<SpriteRenderer>();
    factory.c = c = co.AddComponent<Company>();

    var ro = new GameObject();
    ro.AddComponent<SpriteRenderer>();
    factory.r = r = ro.AddComponent<Residence>();

    factory.rn = rn = new GameObject().AddComponent<RailNode>();

    factory.re = re = new GameObject().AddComponent<RailEdge>();

    var rpo = new GameObject();
    rpo.AddComponent<MeshRenderer>();
    factory.rp = rp = rpo.AddComponent<RailPart>();

    var sto = new GameObject();
    sto.AddComponent<SpriteRenderer>();
    factory.st = st = sto.AddComponent<Station>();

    factory.p = p = new GameObject().AddComponent<Platform>();

    factory.g = g = new GameObject().AddComponent<Gate>();

    var to = new GameObject();
    to.AddComponent<SpriteRenderer>();
    factory.t = t = to.AddComponent<Train>();

    var ho = new GameObject();
    ho.AddComponent<SpriteRenderer>();
    factory.h = h = ho.AddComponent<Human>();

    var ureso = new GameObject();
    ures = ureso.AddComponent<UserResource>();

    var transo = new GameObject();
    trans = transo.AddComponent<Transport>();
    trans.resource = ures;

    c.listener = r.listener = rn.listener = re.listener = rp.listener = st.listener = p.listener = g.listener = t.listener = h.listener = ures.listener = trans.listener = listener;
    c.storage = r.storage = rn.storage = re.storage = rp.storage = st.storage = p.storage = g.storage = t.storage = h.storage = ures.storage = trans.storage = storage;
    r.factory = rn.factory = re.factory = st.factory = ures.factory = factory;
  }
}
