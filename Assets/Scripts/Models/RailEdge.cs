using System;
using System.Collections.Generic;
using UnityEngine;

public class RailEdge : MonoBehaviour
{
  public ModelStorage storage;
  public ModelListener listener;
  public ModelFactory factory;
  private RailEdge template;

  private bool isTemplate = true;
  public bool isView = false;

  [System.NonSerialized] public bool isOutbound;
  [System.NonSerialized] public RailNode from;
  [System.NonSerialized] public RailNode to;
  [System.NonSerialized] public RailEdge reverse;
  /**
   * 始点から終点に向かうベクトル
   */
  [System.NonSerialized] public Vector3 arrow;
  [System.NonSerialized] public RailPart forwardPart;
  [System.NonSerialized] public RailPart backPart;

  private void Awake()
  {
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<RailEdge>(EventType.CREATED, re => storage.Add(re));
      listener.Add<RailEdge>(EventType.DELETED, re => storage.Remove(re));
    }
    else
    {
      listener.Add<RailEdge>(EventType.MODIFIED, this, (_) =>
      {
        listener.Fire(EventType.MODIFIED, forwardPart);
        listener.Fire(EventType.MODIFIED, backPart);
      });
    }
  }

  public RailEdge NewInstance(RailNode from, RailNode to, bool isOutbound)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.isOutbound = isOutbound;
    obj.from = from;
    obj.to = to;
    from.outEdge.Add(obj);
    to.inEdge.Add(obj);
    obj.arrow = to.transform.position - from.transform.position;
    obj.transform.position = Vector3.Lerp(from.transform.position, to.transform.position, 0.5f);

    if (isView)
    {
      obj.GetComponent<MeshRenderer>().enabled = true;
      obj.GetComponent<MeshRenderer>().material.color = (isOutbound) ? Color.black : Color.gray;
      obj.transform.localScale = new Vector3(0.02f, obj.arrow.magnitude / 2, 0.02f);
      obj.transform.localRotation = Quaternion.Euler(0f, 0f, Vector3.SignedAngle(Vector3.up, obj.arrow, Vector3.forward));
    }

    obj.forwardPart = factory.NewRailPart(obj, true);
    obj.backPart = factory.NewRailPart(obj, false);
    listener.Fire(EventType.CREATED, obj);
    List<RailEdge>[] adj = { from.inEdge, from.outEdge, to.inEdge, to.outEdge };
    Array.ForEach(adj, list => list
      .FindAll(re => re != this)
      .ForEach(re => listener.Fire(EventType.MODIFIED, re)));
    return obj;
  }

  public void Remove()
  {
    from.outEdge.Remove(this);
    to.inEdge.Remove(this);
    backPart.Remove();
    forwardPart.Remove();
    listener.Fire(EventType.MODIFIED, from);
    listener.Fire(EventType.MODIFIED, to);
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }
}