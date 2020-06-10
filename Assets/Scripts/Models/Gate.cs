using UnityEngine;

public class Gate : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;

  private Gate template;
  private bool isTemplate = true;

  [System.NonSerialized] public Station station;

  private void Awake()
  {
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Gate>(EventType.CREATED, st => storage.Find<Gate>().Add(st));
      listener.Add<Gate>(EventType.DELETED, st => storage.Find<Gate>().Remove(st));
    }
  }

  public Gate NewInstance(Station st)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.station = st;
    obj.transform.position = st.transform.position;
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(this);
  }
}