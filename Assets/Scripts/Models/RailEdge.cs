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
  public bool IsView = false;

  [System.NonSerialized] public bool IsOutbound;
  [System.NonSerialized] public RailNode From;
  [System.NonSerialized] public RailNode To;
  [System.NonSerialized] public RailEdge Reverse;
  /**
   * 始点から終点に向かうベクトル
   */
  [System.NonSerialized] public Vector3 Arrow;
  [System.NonSerialized] public RailPart ForwardPart;
  [System.NonSerialized] public RailPart BackPart;

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
        listener.Fire(EventType.MODIFIED, ForwardPart);
        listener.Fire(EventType.MODIFIED, BackPart);
      });
    }
  }

  public RailEdge NewInstance(RailNode from, RailNode to, bool isOutbound)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.IsOutbound = isOutbound;
    obj.From = from;
    obj.To = to;
    from.OutEdge.Add(obj);
    to.InEdge.Add(obj);
    obj.Arrow = to.transform.position - from.transform.position;
    obj.transform.position = Vector3.Lerp(from.transform.position, to.transform.position, 0.5f);

    if (IsView)
    {
      obj.GetComponent<MeshRenderer>().enabled = true;
      obj.GetComponent<MeshRenderer>().material.color = (isOutbound) ? Color.black : Color.gray;
      obj.transform.localScale = new Vector3(0.02f, obj.Arrow.magnitude / 2, 0.02f);
      obj.transform.localRotation = Quaternion.Euler(0f, 0f, Vector3.SignedAngle(Vector3.up, obj.Arrow, Vector3.forward));
    }

    obj.ForwardPart = factory.NewRailPart(obj, true);
    obj.BackPart = factory.NewRailPart(obj, false);
    listener.Fire(EventType.CREATED, obj);
    List<RailEdge>[] adj = { from.InEdge, from.OutEdge, to.InEdge, to.OutEdge };
    Array.ForEach(adj, list => list
      .FindAll(re => re != this)
      .ForEach(re => listener.Fire(EventType.MODIFIED, re)));
    return obj;
  }

  public void Remove()
  {
    From.OutEdge.Remove(this);
    To.InEdge.Remove(this);
    BackPart.Remove();
    ForwardPart.Remove();
    listener.Fire(EventType.MODIFIED, From);
    listener.Fire(EventType.MODIFIED, To);
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }
}