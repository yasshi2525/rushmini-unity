using System;
using System.Collections.Generic;
using UnityEngine;

public class RailNode : MonoBehaviour
{
  public ModelStorage storage;
  public ModelListener listener;
  public ModelFactory factory;
  private RailNode template;

  private bool isTemplate = true;
  public bool isView = false;

  [System.NonSerialized] public List<RailEdge> outEdge;
  [System.NonSerialized] public List<RailEdge> inEdge;
  [System.NonSerialized] public Platform platform;

  private void Awake()
  {
    outEdge = new List<RailEdge>();
    inEdge = new List<RailEdge>();
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<RailNode>(EventType.CREATED, rn => storage.Find<RailNode>().Add(rn));
      listener.Add<RailNode>(EventType.DELETED, rn => storage.Find<RailNode>().Remove(rn));
    }
  }

  public RailNode NewInstance(Vector3 pos)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.transform.position = pos;
    if (isView)
    {
      obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
      obj.GetComponent<MeshRenderer>().enabled = true;
      obj.GetComponent<MeshRenderer>().material.color = Color.green;
    }
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public RailEdge Extend(Vector3 pos)
  {
    var tail = factory.NewRailNode(pos);
    var outE = factory.NewRailEdge(this, tail, true);
    var inE = factory.NewRailEdge(tail, this, false);
    outE.reverse = inE;
    inE.reverse = outE;
    return outE;
  }

  public Platform BuildStation()
  {
    if (platform)
    {
      throw new ArgumentException("try to build station on already deployed");
    }
    return factory.NewPlatform(this, factory.NewStation());
  }

  public void Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }

  /**
  * from から to へ ratio([0, 1]) 進んだ点を返す
  */
  private float Div(float from, float to, float ratio)
  {
    return from * (1 - ratio) + to * ratio;
  }

  /**
   * 成す角度により、どの程度線を伸ばすと、カーブがつながって見えるか返す
   * (実測値。計算式不明)
   */
  private float CurveRatio(float degree)
  {
    if (degree > 180) degree = 360 - degree;

    float result = 3.35f;

    if (degree <= 10) result = 0.44f;
    else if (degree <= 15) result = Div(0.44f, 0.48f, (degree - 10) / 5);
    else if (degree <= 30) result = Div(0.48f, 0.52f, (degree - 15) / 15);
    else if (degree <= 45) result = Div(0.52f, 0.57f, (degree - 30) / 15);
    else if (degree <= 60) result = Div(0.57f, 0.66f, (degree - 45) / 15);
    else if (degree <= 90) result = Div(0.66f, 1.0f, (degree - 60) / 30);
    else if (degree <= 120) result = Div(1.0f, 2.0f, (degree - 90) / 30);
    else if (degree <= 135) result = Div(2.0f, 3.35f, (degree - 120) / 45);
    return result;
  }

  private float Slide(RailEdge prev, RailEdge next, float slide)
  {
    var angle = Vector3.SignedAngle(prev.arrow, next.arrow, Vector3.forward);
    if (angle < 0) angle = 360 + angle;
    return slide * Mathf.Sin(angle / 180 * Mathf.PI) * CurveRatio(angle);
  }

  /**
    * この地点を目的地/出発地とする上りRailEdgeが引き伸ばす距離を返します
    */
  public float Left(float slide)
  {
    if (inEdge.Count == 2 && outEdge.Count == 2)
    {
      // 最初に到達したのが前の上り
      // 最後に出発したのが次の上り
      return Slide(inEdge[0], outEdge[1], slide);
    }
    return 0;
  }

  /**
   * この地点を目的地/出発地とする下りRailEdgeが引き伸ばす距離を返します
   */
  public float Right(float slide)
  {
    if (inEdge.Count == 2 && outEdge.Count == 2)
    {
      // 最後に到達したのが前の下り
      // 最初に出発したのが次の上り
      return Slide(inEdge[1], outEdge[0], slide);
    }
    return 0;
  }
}