using UnityEngine;
public class Train : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  private Train template;
  private bool isTemplate = true;

  private void Awake()
  {
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Train>(EventType.CREATED, t => storage.Find<Train>().Add(t));
      listener.Add<Train>(EventType.DELETED, t => storage.Find<Train>().Remove(t));
    }
  }

  public Train NewInstance(Vector3 pos)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.GetComponent<SpriteRenderer>().enabled = true;
    obj.transform.position = pos;
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }
}