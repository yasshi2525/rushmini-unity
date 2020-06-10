using UnityEngine;

public class ModelBehaviour<T> : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;

  protected ModelBehaviour<T> template;
  protected bool isTemplate = true;

  protected virtual void Awake()
  {
    if (isTemplate) template = this;
  }

  protected virtual void Start()
  {
    if (isTemplate)
    {
      listener.Add<T>(EventType.CREATED, obj => storage.Find<T>().Add(obj));
      listener.Add<T>(EventType.DELETED, obj => storage.Find<T>().Remove(obj));
    }
  }

  protected virtual ModelBehaviour<T> NewInstance()
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public virtual void Remove()
  {
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }
}