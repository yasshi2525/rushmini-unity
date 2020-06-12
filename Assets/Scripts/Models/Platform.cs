using UnityEngine;
using System.Collections.Generic;

public class Platform : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;

  private Platform template;
  private bool isTemplate = true;

  [System.NonSerialized] public RailNode On;
  [System.NonSerialized] public Station BelongsTo;
  [System.NonSerialized] public List<DeptTask> Depts;

  private void Awake()
  {
    Depts = new List<DeptTask>();
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Platform>(EventType.CREATED, st => storage.Add(st));
      listener.Add<Platform>(EventType.DELETED, st => storage.Remove(st));
    }
  }

  public Platform NewInstance(RailNode on, Station st)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.On = on;
    on.StandsOver = obj;
    obj.transform.position = on.transform.position;
    st.AddPlatform(obj);
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    On.StandsOver = null;
    listener.Fire(EventType.MODIFIED, On);
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }
}