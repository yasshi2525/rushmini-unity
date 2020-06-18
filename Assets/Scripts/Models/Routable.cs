using System.Collections.Generic;
using UnityEngine;

public interface IRoutable
{
  Router Route { get; }
}

public abstract class Router
{
  protected class Route
  {
    public IRoutable Goal;
    public IRoutable Next;
    public float Dist;
    public float Payment;
    public Route(IRoutable next, IRoutable goal, float dist, float payment)
    {
      Goal = goal;
      Next = next;
      Dist = dist;
      Payment = payment;
    }
  }

  protected IDictionary<IRoutable, Route> dict;

  public Router()
  {
    dict = new Dictionary<IRoutable, Route>();
  }


  /**
   * 指定された目的地に移動するには、次にどの地点に向かう必要があるか返します
   */
  public IRoutable NextFor(IRoutable goal)
  {
    return dict.ContainsKey(goal) ? dict[goal].Next : null;
  }

  /**
   * 指定された地点までの移動コストを返します
   */
  public float DistanceFor(IRoutable goal)
  {
    return dict.ContainsKey(goal) ? dict[goal].Dist : float.NaN;
  }

  /**
   * 指定された地点までの運賃を返します
   */
  public float PaymentFor(IRoutable goal)
  {
    return dict.ContainsKey(goal) ? dict[goal].Payment : 0;
  }

  /**
   * 指定された目的地に向かうには、どれほどのコストがかかり、次にどこに向かう必要があるか設定します
   */
  public void SetNext(IRoutable next, IRoutable goal, float dist, float payment)
  {
    if (dict.ContainsKey(goal))
    {
      dict[goal].Next = next;
      dict[goal].Dist = dist;
      dict[goal].Payment = payment;
    }
    else
    {
      dict.Add(goal, new Route(next, goal, dist, payment));
    }
  }

  public void SetNext(IRoutable next, IRoutable goal, float dist)
  {
    SetNext(next, goal, dist, 0);
  }

  /**
   * 自身を目的地とする移動者に対し、1フレーム分アクションさせます
   */
  public abstract void Handle(Human subject);
  /**
   * 自身を目的地とする移動者を取り除きます
   */
  public abstract void Discard(Human subject);
}