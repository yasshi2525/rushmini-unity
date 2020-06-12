using UnityEngine;
public class Human : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  private Human template;
  private bool isTemplate = true;
  [System.NonSerialized] public Residence Departure;
  [System.NonSerialized] public Company Destination;
  public float Rand = 0.1f;

  private void Awake()
  {
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Human>(EventType.CREATED, h => storage.Add(h));
      listener.Add<Human>(EventType.DELETED, h => storage.Remove(h));
    }
  }

  public Human NewInstance(Residence r, Company c)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.Departure = r;
    obj.Destination = c;
    obj.GetComponent<SpriteRenderer>().enabled = true;
    var len = Random.Range(0f, obj.Rand);
    var theta = Random.Range(0f, Mathf.PI * 2);

    var rLoc = r.GetComponent<SpriteRenderer>().transform.position;
    obj.transform.position = rLoc + new Vector3(
      len * Mathf.Cos(theta),
      len * Mathf.Sin(theta),
      0
    );
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }
}