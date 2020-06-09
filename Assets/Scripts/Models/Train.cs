using UnityEngine;
public class Train : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  private Train template;
  private bool isTemplate = true;

  private void Start()
  {
    if (isTemplate)
    {
      template = this;
      listener.Find<Train>(EventType.CREATED).AddListener(t => storage.Find<Train>().Add(t));
      listener.Find<Train>(EventType.DELETED).AddListener(t => storage.Find<Train>().Remove(t));
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
}