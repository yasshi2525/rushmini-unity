using System;
using UnityEngine;

public class Company : MonoBehaviour, IRoutable
{
  public ModelListener listener;
  public ModelStorage storage;

  private Company template;
  private bool isTemplate = true;
  /**
   * 住民がこの会社を行き先として選ぶ度合い 自身/全会社の合計 の割合で行き先が選ばれる
   */
  public int Attractiveness;
  private Router router;

  private void Awake()
  {
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Company>(EventType.CREATED, c => storage.Add(c));
      listener.Add<Company>(EventType.DELETED, c => storage.Remove(c));
    }
  }

  private void Update()
  {

  }

  public Company NewInstance(int attractiveness, Vector3 pos)
  {
    if (attractiveness <= 0)
    {
      throw new ArgumentException("attractiveness must be >0");
    }
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.router = new RouterImpl(obj);
    obj.Attractiveness = attractiveness;
    obj.GetComponent<SpriteRenderer>().enabled = true;
    obj.transform.position = pos;
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public Router Route { get { return router; } }

  public void Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }

  private class RouterImpl : Router
  {
    protected Company parent;

    public RouterImpl(Company c)
    {
      parent = c;
    }

    public override void Handle(Human subject)
    {
      if (subject.Seek(parent.transform.position))
      {
        subject.State = Human.StateType.ARCHIVED;
        subject.Complete();
      }
      else
      {
        subject.State = Human.StateType.MOVE;
      }
    }

    public override void Discard(Human subject) { }
  }
}
