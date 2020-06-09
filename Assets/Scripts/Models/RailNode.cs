using System.Collections.Generic;
using UnityEngine;

public class RailNode : MonoBehaviour
{
  public ModelStorage storage;
  public ModelListener listener;
  public ModelFactory factory;
  private RailNode template;

  private bool isTemplate = true;

  [System.NonSerialized] public List<RailEdge> outEdge;
  [System.NonSerialized] public List<RailEdge> inEdge;

  private void Awake()
  {
    outEdge = new List<RailEdge>();
    inEdge = new List<RailEdge>();
  }

  private void Start()
  {
    if (isTemplate)
    {
      template = this;
      listener.Find<RailNode>(EventType.CREATED).AddListener(rn => storage.Find<RailNode>().Add(rn));
      listener.Find<RailNode>(EventType.DELETED).AddListener(rn => storage.Find<RailNode>().Remove(rn));
    }
  }

  public RailNode NewInstance(Vector3 pos)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.transform.position = pos;
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

  public void Remove()
  {
    listener.Fire(EventType.DELETED, this);
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
}