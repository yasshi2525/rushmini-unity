using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Station : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  public ModelFactory factory;

  private Station template;
  private bool isTemplate = true;

  [System.NonSerialized] public List<Platform> Platforms;
  [System.NonSerialized] public Gate Under;

  private void Awake()
  {
    Platforms = new List<Platform>();
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Station>(EventType.CREATED, st => storage.Add(st));
      listener.Add<Station>(EventType.DELETED, st => storage.Remove(st));
    }
  }

  public Station NewInstance()
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.GetComponent<SpriteRenderer>().enabled = true;
    obj.Under = factory.NewGate(this);
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }

  public void AddPlatform(Platform p)
  {
    Platforms.Add(p);
    transform.position = Platforms.Aggregate(new Vector3(), (prev, cur) =>
    {
      prev += cur.transform.position / Platforms.Count;
      return prev;
    });
    Under.transform.position = transform.position;
  }
}