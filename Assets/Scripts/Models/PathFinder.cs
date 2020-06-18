using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : IComparable<PathNode>
{
  public IRoutable Origin;
  /**
  * ゴールに行くまでのコスト
  */
  public float Cost = float.MaxValue;
  /**
   * ゴールに行くまでの運賃
   */
  public float Payment = 0;
  /**
   * この地点に次移動すればゴールにつく
   */
  public PathNode Via;

  public List<PathEdge> In = new List<PathEdge>();
  public List<PathEdge> Out = new List<PathEdge>();

  public PathNode(IRoutable origin)
  {
    Origin = origin;
  }

  /**
   * 自身に隣接する点をコスト昇順で返します
   */
  protected List<PathNode> SortNeighbors()
  {
    var queue = new List<PathNode>();
    In.Select(e =>
    {
      e.From.Cost = e.Cost;
      e.From.Payment = e.Payment;
      e.From.Via = this;
      return e.From;
    }).ToList().ForEach((n) => queue.Add(n));
    queue.Sort();
    return queue;
  }

  /**
   * この地点をゴールとし、連結している各地点の最短経路を求めます
   */
  public void WalkThrough()
  {
    // 隣接点を初期到達点にする
    var queue = SortNeighbors();

    while (queue.Count > 0)
    {
      var x = queue.First();
      queue.Remove(x);
      x.In.ForEach(e =>
      {
        var y = e.From;
        var v = x.Cost + e.Cost;
        // より短い経路がみつかった
        if (v < y.Cost)
        {
          y.Cost = v;
          y.Payment = x.Payment + e.Payment;
          y.Via = x;
          queue.Add(y);
          queue.Sort();
        }
      });
    }
  }

  public void Reset()
  {
    Cost = float.MaxValue;
    Payment = 0;
    Via = null;
  }

  public int CompareTo(PathNode oth)
  {
    return Cost > oth.Cost ? 1 : Cost < oth.Cost ? -1 : 0;
  }
}

public class PathEdge
{
  public PathNode From;
  public PathNode To;
  /**
   * from から to に行くまでのコスト
   */
  public float Cost;
  /**
   * from から to に行くまでの運賃
   */
  public float Payment;

  public PathEdge(PathNode from, PathNode to, float cost, float payment)
  {
    From = from;
    To = to;
    Cost = cost;
    Payment = payment;
    From.Out.Add(this);
    To.In.Add(this);
  }
}

public class PathFinder
{
  public PathNode Goal;
  protected List<PathNode> Nodes;
  protected List<PathEdge> Edges;

  public PathFinder(IRoutable goal)
  {
    Nodes = new List<PathNode>();
    Edges = new List<PathEdge>();
    Goal = Node(goal);
  }

  /**
   * 指定されたオブジェクトをノードとして登録します
   */
  public PathNode Node(IRoutable org)
  {
    if (org == null)
    {
      throw new ArgumentNullException();
    }
    var res = Nodes.Find(n => n.Origin == org);
    if (res == null)
    {
      var n = new PathNode(org);
      Nodes.Add(n);
      return n;
    }
    return res;
  }

  public void Unnode(IRoutable org)
  {
    var res = Nodes.Find(n => n.Origin == org);
    if (res != null)
    {
      var rmList1 = Edges.Where(e => e.From == res).Select(e =>
      {
        e.To.In.Remove(e);
        return e;
      });
      var rmList2 =
        Edges.Where(e => e.To == res).Select(e =>
        {
          e.From.Out.Remove(e);
          return e;
        });
      Edges.RemoveAll(e => rmList1.Contains(e) || rmList2.Contains(e));
      Nodes.Remove(res);
    }
  }
  /**
   * 指定されたオブジェクト同士の連結を登録します。長いパスの場合登録しません
   */
  public PathEdge Edge(IRoutable from, IRoutable to, float cost, float payment)
  {
    if (from == null || to == null)
    {
      throw new ArgumentNullException();
    }
    var res = Edges.Find(e => e.From.Origin == from && e.To.Origin == to);
    if (res != null)
    {
      if (res.Cost >= cost)
      {
        res.Cost = cost;
        res.Payment = payment;
        return res;
      }
      else
      {
        return null;
      }
    }
    else
    {
      var e = new PathEdge(Node(from), Node(to), cost, payment);
      Edges.Add(e);
      return e;
    }
  }
  public PathEdge Edge(IRoutable from, IRoutable to, float cost)
  {
    return Edge(from, to, cost, 0);
  }

  public void Unedge(IRoutable from, IRoutable to)
  {
    var rmList = Edges
      .Where(e => e.From.Origin == from && e.To.Origin == to)
      .Select((e) =>
      {
        e.From.Out.Remove(e);
        e.To.In.Remove(e);
        return e;
      });
    Edges.RemoveAll(e => rmList.Contains(e));
  }

  public void UnedgeAll()
  {
    Edges.ForEach(e =>
    {
      e.From.Out.Remove(e);
      e.To.In.Remove(e);
    });
    Edges.Clear();
  }

  public void Execute()
  {
    Nodes.ForEach(n => n.Reset());
    Goal.WalkThrough();
    Nodes.FindAll(n => n.Via != null).ForEach(n =>
    {
      n.Origin.Route.SetNext(n.Via.Origin, Goal.Origin, n.Cost, n.Payment);
    });
  }
}