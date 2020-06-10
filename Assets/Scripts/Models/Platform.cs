using UnityEngine;

public class Platform : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;

  private Platform template;
  private bool isTemplate = true;

  [System.NonSerialized] public RailNode on;
  [System.NonSerialized] public Station station;

  private void Awake()
  {
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Platform>(EventType.CREATED, st => storage.Find<Platform>().Add(st));
      listener.Add<Platform>(EventType.DELETED, st => storage.Find<Platform>().Remove(st));
    }
  }

  public Platform NewInstance(RailNode on, Station st)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.on = on;
    on.platform = obj;
    obj.transform.position = on.transform.position;
    st.AddPlatform(obj);
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    on.platform = null;
    listener.Fire(EventType.MODIFIED, on);
    listener.Fire(EventType.DELETED, this);
    Destroy(this);
  }
}