using System.Collections.Generic;
using UnityEngine;

public class Residence : MonoBehaviour
{
  public ModelStorage storage;
  public ModelListener listener;
  private Residence template;
  private bool isTemplate = true;
  public ModelFactory factory;
  /**
   * 会社の魅力度に応じて住民をスポーンするため、
   * 魅力度の数だけ同じ会社を行き先に設定する
   */
  [System.NonSerialized] public List<Company> Destinations;

  public float IntervalSec = 0.5f;

  private float remainTime;

  private void AddDestination(Company c)
  {
    for (int i = 0; i < c.Attractiveness; i++)
    {
      Destinations.Add(c);
    }
  }

  private void DeleteDestination(Company c)
  {
    Destinations.RemoveAll(oth => oth == c);
  }
  private void Awake()
  {
    if (isTemplate) template = this;
  }

  // Start is called before the first frame update
  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Residence>(EventType.CREATED, r => storage.Add(r));
      listener.Add<Residence>(EventType.DELETED, r => storage.Remove(r));
    }
    Destinations = new List<Company>();
    if (!isTemplate)
    {
      storage.List<Company>().ForEach(c => AddDestination(c));
      listener.Add<Company>(EventType.CREATED, c => AddDestination(c));
      listener.Add<Company>(EventType.DELETED, c => DeleteDestination(c));
    }
  }

  // Update is called once per frame
  private void Update()
  {
    remainTime -= Time.deltaTime;
    if (remainTime < 0)
    {
      if (Destinations.Count > 0)
      {
        var c = Destinations[0];
        factory.NewHuman(this, c);
        Destinations.Add(c);
      }
      remainTime += IntervalSec;
    }
  }

  public Residence NewInstance(Vector3 pos)
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
