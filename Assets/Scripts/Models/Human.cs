using UnityEngine;
public class Human : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  private Human template;
  private bool isTemplate = true;
  [System.NonSerialized] public Residence departure;
  [System.NonSerialized] public Company destination;
  public float rand = 0.1f;

  private void Start()
  {
    if (isTemplate)
    {
      template = this;
      listener.Find<Human>(EventType.CREATED).AddListener(h => storage.Find<Human>().Add(h));
      listener.Find<Human>(EventType.DELETED).AddListener(h => storage.Find<Human>().Remove(h));
    }
  }

  public Human NewInstance(Residence r, Company c)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.departure = r;
    obj.destination = c;
    obj.GetComponent<SpriteRenderer>().enabled = true;
    var len = Random.Range(0f, obj.rand);
    var theta = Random.Range(0f, Mathf.PI * 2);

    var rLoc = r.GetComponent<SpriteRenderer>().transform.position;
    obj.transform.position = new Vector3(
      rLoc.x + len * Mathf.Cos(theta),
      rLoc.y + len * Mathf.Sin(theta),
      rLoc.z
    );
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }
}