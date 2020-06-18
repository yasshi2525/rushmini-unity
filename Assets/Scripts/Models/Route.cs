using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{
  [System.NonSerialized] public List<PathFinder> Finders;

  public int FinderIdx;
  public int PlatformIdx;
  public int DeptTaskIdx;
  public ModelListener listener;
  public ModelStorage storage;
  public UserResource resource;
  public Transport trans;
  public bool IsWaiting = true;

  public bool IsFixed { get { return !IsWaiting && FinderIdx == Finders.Count; } }

  private void Awake()
  {
    Finders = new List<PathFinder>();
  }

  private void Start()
  {
    listener.Add<Residence>(EventType.CREATED, (r) => Add(r));
    listener.Add<Company>(EventType.CREATED, (c) => Add(c));
    listener.Add<Gate>(EventType.CREATED, (g) => Add(g));
    listener.Add<Platform>(EventType.CREATED, (p) => Add(p));
    listener.Add<DeptTask>(EventType.CREATED, (dept) => Add(dept));
    listener.Add<Residence>(EventType.DELETED, (r) => Remove(r));
    listener.Add<Company>(EventType.DELETED, (c) => Remove(c));
    listener.Add<Gate>(EventType.DELETED, (g) => Remove(g));
    listener.Add<Platform>(EventType.DELETED, (p) => Remove(p));
    listener.Add<DeptTask>(EventType.DELETED, (dept) => Remove(dept));
  }

  private void Update()
  {
    if (!trans.IsWaiting && !trans.IsFixed) return;
    if (resource.State == UserResource.StateType.STARTED) return;
    if (IsFixed) return;
    if (IsWaiting)
    {
      // 以前の経路探索結果 Dept <=> P を削除
      Finders.ForEach(f =>
      {
        storage.List<DeptTask>().ForEach(dept =>
        {
          f.Unnode(dept);
          f.Node(dept);
          f.Edge(dept.Stay, dept, 0);
        });
      });
      IsWaiting = false;
      return;
    }
    if (!trans.IsWaiting && !trans.IsFixed) return;
    if (PlatformIdx < storage.List<Platform>().Count)
    {
      CopyTransport();
      return;
    }

    RouteHuman();
    Finders[FinderIdx].Execute();
    storage.List<Human>().ForEach(h => h.Reroute());
    FinderIdx++;
    PlatformIdx = 0;
  }

  public void Reset()
  {
    FinderIdx = 0;
    PlatformIdx = 0;
    DeptTaskIdx = 0;
    IsWaiting = true;
  }

  private void Add(Residence r)
  {
    Finders.ForEach(f =>
    {
      f.Node(r);
      // R => one C for each goal
      f.Edge(r, f.Goal.Origin, Vector3.Distance(r.transform.position, (f.Goal.Origin as Company).transform.position));

      // R => all G for each goal
      storage.List<Gate>().ForEach(g => f.Edge(r, g, Vector3.Distance(r.transform.position, g.transform.position)));
    });
    Reset();
  }

  private void Add(Company c)
  {
    var f = new PathFinder(c);

    storage.List<Residence>().ForEach(r =>
    {
      // all R => one C for the goal
      f.Edge(r, c, Vector3.Distance(r.transform.position, c.transform.position));

      // all R => all G for the goal
      storage.List<Gate>().ForEach(g => f.Edge(r, g, Vector3.Distance(r.transform.position, g.transform.position)));
    });

    storage.List<Gate>().ForEach(g =>
    {
      // all G => one C for the goal
      f.Edge(g, c, Vector3.Distance(g.transform.position, c.transform.position));
      // all G <=> G
      storage.List<Gate>().Where(oth => g != oth).ToList().ForEach(oth =>
      {
        f.Edge(g, oth, Vector3.Distance(g.transform.position, oth.transform.position));
      });
      // all [G <=> P] for the goal
      g.BelongsTo.Platforms.ForEach(p =>
      {
        f.Edge(p, g, Vector3.Distance(p.transform.position, g.transform.position));
        f.Edge(g, p, Vector3.Distance(g.transform.position, p.transform.position));
      });
    });

    storage.List<DeptTask>().ForEach(dept =>
    {
      // all P => one lt for the goal
      f.Edge(dept.Stay, dept, 0);
    });

    // lt => P
    storage.List<Platform>().ForEach(dest =>
    {
      storage.List<DeptTask>().ForEach(dept =>
      {
        IRoutable prev = dept;
        do
        {
          var next = prev.Route.NextFor(dest);
          if (next != null)
          {
            f.Edge(prev, next, prev.Route.DistanceFor(next), prev.Route.PaymentFor(next));
          }
          prev = next;
        } while (prev != null && prev != dept);
      });
    });

    Finders.Add(f);
    Reset();
  }

  private void Add(Gate g)
  {
    Finders.ForEach(f =>
    {
      f.Node(g);

      // all R => G for each goal
      storage.List<Residence>().ForEach(r => f.Edge(r, g, Vector3.Distance(r.transform.position, g.transform.position)));

      // G => one C for each goal
      f.Edge(g, f.Goal.Origin, Vector3.Distance(g.transform.position, (f.Goal.Origin as Company).transform.position));

      // all G <=> G for each goal
      storage.List<Gate>().Where(oth => g != oth).ToList().ForEach(oth =>
      {
        f.Edge(g, oth, Vector3.Distance(g.transform.position, oth.transform.position));
        f.Edge(oth, g, Vector3.Distance(oth.transform.position, g.transform.position));
      });

      // G <=> P for each goal
      g.BelongsTo.Platforms.ForEach(p =>
      {
        f.Edge(p, g, Vector3.Distance(p.transform.position, g.transform.position));
        f.Edge(g, p, Vector3.Distance(g.transform.position, p.transform.position));
      });
    });
    Reset();
  }

  private void Add(Platform p)
  {
    Finders.ForEach(f =>
    {
      f.Node(p);
      // G <=> P for each goal
      f.Edge(p, p.BelongsTo.Under, Vector3.Distance(p.transform.position, p.BelongsTo.Under.transform.position));
      f.Edge(p.BelongsTo.Under, p, Vector3.Distance(p.BelongsTo.Under.transform.position, p.transform.position));
    });
    Reset();
  }

  private void Add(DeptTask dept)
  {
    Finders.ForEach(f => f.Node(dept));
    Reset();
  }

  private void Remove(Residence r)
  {
    Finders.ForEach(f => f.Unnode(r));
  }

  private void Remove(Company c)
  {
    Finders.RemoveAll(f => f.Goal.Origin == c as IRoutable);
  }

  private void Remove(Gate g)
  {
    Finders.ForEach(f => f.Unnode(g));
  }

  private void Remove(Platform p)
  {
    Finders.ForEach(f => f.Unnode(p));
  }

  private void Remove(DeptTask dept)
  {
    Finders.ForEach(f => f.Unnode(dept));
  }

  private void CopyTransport()
  {
    var f = Finders[FinderIdx];
    var dest = storage.List<Platform>()[PlatformIdx];
    var dept = storage.List<DeptTask>()[DeptTaskIdx];

    IRoutable prev = dept;
    do
    {
      var next = prev.Route.NextFor(dest);
      if (next != null)
      {
        f.Edge(prev, next, prev.Route.DistanceFor(next), prev.Route.PaymentFor(next));
      }
      prev = next;
    } while (prev != null && prev != dest as IRoutable);

    DeptTaskIdx++;
    if (DeptTaskIdx == storage.List<DeptTask>().Count)
    {
      DeptTaskIdx = 0;
      PlatformIdx++;
    }
  }

  /**
   * 改札内にいるため、改札(出場)かホームへのみ移動可能
   */
  private bool TryLinkGate(PathFinder f, Human h)
  {
    if (h.OnGate)
    {
      f.Edge(h, h.OnGate, Vector3.Distance(h.transform.position, h.OnGate.transform.position));
      h.OnGate.BelongsTo.Platforms.ForEach(p => f.Edge(h, p, Vector3.Distance(h.transform.position, p.transform.position)));
      return true;
    }
    return false;
  }

  /**
   * ホーム内にいるため、ホームか、改札へのみ移動可能
   */
  private bool TryLinkPlatform(PathFinder f, Human h)
  {
    if (h.OnPlatform)
    {
      f.Edge(h, h.OnPlatform, Vector3.Distance(h.transform.position, h.OnPlatform.transform.position));
      f.Edge(h, h.OnPlatform.BelongsTo.Under, Vector3.Distance(h.transform.position, h.OnPlatform.BelongsTo.Under.transform.position));
      return true;
    }
    return false;
  }

  /**
   * 乗車列にいる場合、乗車列か改札へのみ移動可能
   */
  private bool TryLinkDeptTask(PathFinder f, Human h)
  {
    if (h.OnDeptTask != null)
    {
      f.Edge(h, h.OnDeptTask, Vector3.Distance(h.transform.position, h.OnDeptTask.Stay.transform.position));
      f.Edge(h, h.OnDeptTask.Stay, Vector3.Distance(h.transform.position, h.OnDeptTask.Stay.transform.position));
      return true;
    }
    return false;
  }

  /**
   * 車内にいる場合は、電車が経路探索結果を持っているため、それに接続する
   */
  private bool TryLinkTrain(PathFinder f, Human h)
  {
    if (h.OnTrain)
    {
      storage.List<Platform>().ForEach(dest =>
      {
        if (h.OnTrain.Route.NextFor(dest) != null)
        {
          f.Edge(h, dest, h.OnTrain.Route.DistanceFor(dest), h.OnTrain.Route.PaymentFor(dest));
        }
      });
      return true;
    }
    return false;
  }

  /**
   * 地面にいる場合、改札か会社に移動可能
   */
  private bool TryLinkGround(PathFinder f, Human h)
  {
    storage.List<Gate>().ForEach(g =>
    {
      f.Edge(h, g, Vector3.Distance(h.transform.position, g.transform.position));
    });
    f.Edge(h, f.Goal.Origin, Vector3.Distance(h.transform.position, (f.Goal.Origin as Company).transform.position));
    return true;
  }

  private void RouteHuman()
  {
    var f = Finders[FinderIdx];
    storage.List<Human>().Where(h => h.Destination as IRoutable == f.Goal.Origin).ToList().ForEach(h =>
    {
      f.Unnode(h);
      f.Node(h);
      if (!TryLinkGate(f, h))
        if (!TryLinkPlatform(f, h))
          if (!TryLinkDeptTask(f, h))
            if (!TryLinkTrain(f, h))
              TryLinkGround(f, h);
    });
  }
}