using System;
using UnityEngine;

public class Company : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;

  private Company template;
  private bool isTemplate = true;
  /**
   * 住民がこの会社を行き先として選ぶ度合い 自身/全会社の合計 の割合で行き先が選ばれる
   */
  public int attractiveness;

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
    obj.attractiveness = attractiveness;
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
