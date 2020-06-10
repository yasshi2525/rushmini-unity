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

  [System.NonSerialized] public List<Platform> platforms;
  [System.NonSerialized] public Gate gate;

  private void Awake()
  {
    platforms = new List<Platform>();
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Station>(EventType.CREATED, st => storage.Find<Station>().Add(st));
      listener.Add<Station>(EventType.DELETED, st => storage.Find<Station>().Remove(st));
    }
  }

  public Station NewInstance()
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.GetComponent<SpriteRenderer>().enabled = true;
    obj.gate = factory.NewGate(this);
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
    platforms.Add(p);
    transform.position = platforms.Aggregate(new Vector3(), (prev, cur) =>
    {
      prev += cur.transform.position / platforms.Count;
      return prev;
    });
    gate.transform.position = transform.position;
  }
}