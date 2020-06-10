using UnityEngine;

public class RailPart : MonoBehaviour
{
  public ModelStorage storage;
  public ModelListener listener;
  private RailPart template;
  private bool isTemplate = true;
  [System.NonSerialized] public RailEdge parent;
  [System.NonSerialized] public bool isForwardPart;

  public Material material;
  public Color color = Color.white;
  [Range(0, 1)] public float band = 0.1f;
  [Range(0, 1)] public float slide = 0.1f;

  private float width;

  private void Awake()
  {
    var mesher = GetComponent<MeshRenderer>();
    mesher.material = material;
    mesher.material.color = color;
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<RailPart>(EventType.CREATED, rp => storage.Find<RailPart>().Add(rp));
      listener.Add<RailPart>(EventType.DELETED, rp => storage.Find<RailPart>().Remove(rp));
    }
    else
    {
      listener.Add<RailPart>(EventType.MODIFIED, this, (_) => SetTransform());
    }
  }

  public RailPart NewInstance(RailEdge parent, bool isForward)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.GetComponent<MeshRenderer>().enabled = true;
    obj.parent = parent;
    obj.isForwardPart = isForward;
    obj.SetTransform();
    return obj;
  }

  public void Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }

  private float Width()
  {
    var rn = isForwardPart ? parent.to : parent.from;
    var w = parent.arrow.magnitude / 4 + (parent.isOutbound ? rn.Left(slide) : rn.Right(slide)) / 2;
    return Mathf.Max(w, 0);
  }

  private delegate float PosFn(float n);

  private float Offset(PosFn fn)
  {
    var angle = Vector3.SignedAngle(Vector3.left, parent.arrow, Vector3.forward);
    return slide * fn((angle + 90) / 180 * Mathf.PI) - (isForwardPart ? 1 : -1) * width * fn(angle / 180 * Mathf.PI);
  }

  private float OffsetX()
  {
    return Offset(Mathf.Cos);
  }

  private float OffsetY()
  {
    return Offset(Mathf.Sin);
  }

  private void SetTransform()
  {
    width = Width();
    transform.position = parent.transform.position + new Vector3(OffsetX(), OffsetY(), band / 2);
    transform.localScale = new Vector3(band, width, band);
    transform.localRotation = Quaternion.Euler(0f, 0f, Vector3.SignedAngle(Vector3.up, parent.arrow, Vector3.forward));
  }
}