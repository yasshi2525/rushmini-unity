using UnityEngine;

public class RailPart : MonoBehaviour
{
  public ModelStorage storage;
  public ModelListener listener;
  private RailPart template;
  private bool isTemplate = true;
  [System.NonSerialized] public RailEdge Parent;
  [System.NonSerialized] public bool IsForwardPart;

  public Material material;
  public Color color = Color.white;
  [Range(0, 1)] public float Band = 0.1f;
  [Range(0, 1)] public float Slide = 0.1f;

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
      listener.Add<RailPart>(EventType.CREATED, rp => storage.Add(rp));
      listener.Add<RailPart>(EventType.DELETED, rp => storage.Remove(rp));
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
    obj.Parent = parent;
    obj.IsForwardPart = isForward;
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
    var rn = IsForwardPart ? Parent.To : Parent.From;
    var w = Parent.Arrow.magnitude / 4 + (Parent.IsOutbound ? rn.Left(Slide) : rn.Right(Slide)) / 2;
    return Mathf.Max(w, 0);
  }

  private delegate float PosFn(float n);

  private float Offset(PosFn fn)
  {
    var angle = Vector3.SignedAngle(Vector3.left, Parent.Arrow, Vector3.forward);
    return Slide * fn((angle + 90) / 180 * Mathf.PI) - (IsForwardPart ? 1 : -1) * width * fn(angle / 180 * Mathf.PI);
  }

  private float OffsetX()
  {
    return Offset((v) => Mathf.Cos(v));
  }

  private float OffsetY()
  {
    return Offset((v) => Mathf.Sin(v));
  }

  private void SetTransform()
  {
    width = Width();
    transform.position = Parent.transform.position + new Vector3(OffsetX(), OffsetY(), Band / 2);
    transform.localScale = new Vector3(Band, width, Band);
    transform.localRotation = Quaternion.Euler(0f, 0f, Vector3.SignedAngle(Vector3.up, Parent.Arrow, Vector3.forward));
  }
}