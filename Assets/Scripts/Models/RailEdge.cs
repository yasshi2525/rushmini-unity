using System.Collections.Generic;
using UnityEngine;

public class RailEdge : MonoBehaviour
{
  public ModelStorage storage;
  public ModelListener listener;
  private RailEdge template;

  private bool isTemplate = true;

  [System.NonSerialized] public bool isOutbound;
  [System.NonSerialized] public RailNode from;
  [System.NonSerialized] public RailNode to;
  [System.NonSerialized] public RailEdge reverse;
  /**
   * 始点から終点に向かうベクトル
   */
  [System.NonSerialized] public Vector3 arrow;
  public Material material;
  public Color color = Color.white;
  [Range(0, 1)] public float band = 0.1f;

  private void Awake()
  {
    var liner = GetComponent<LineRenderer>();
    liner.positionCount = 0;
    liner.material = material;
    liner.material.color = color;
    liner.startWidth = band;
    liner.endWidth = band;
  }

  private void Start()
  {
    if (isTemplate)
    {
      template = this;
      listener.Find<RailEdge>(EventType.CREATED).AddListener(re => storage.Find<RailEdge>().Add(re));
      listener.Find<RailEdge>(EventType.DELETED).AddListener(re => storage.Find<RailEdge>().Remove(re));
    }
    else
    {
      var liner = GetComponent<LineRenderer>();
      liner.positionCount = 2;
      liner.SetPosition(0, from.GetComponent<Transform>().transform.position);
      liner.SetPosition(1, to.GetComponent<Transform>().transform.position);
    }
  }


  public RailEdge NewInstance(RailNode from, RailNode to, bool isOutbound)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.GetComponent<LineRenderer>().enabled = true;
    obj.isOutbound = isOutbound;
    obj.from = from;
    obj.to = to;
    from.outEdge.Add(obj);
    to.inEdge.Add(obj);
    obj.arrow = to.GetComponent<Transform>().position - from.GetComponent<Transform>().position;



    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    from.outEdge.Remove(this);
    to.inEdge.Remove(this);
    listener.Fire(EventType.DELETED, this);
  }
}