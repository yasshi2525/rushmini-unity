using UnityEngine;
public class Train : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  private Train template;
  private bool isTemplate = true;
  public float Stay = 2f;
  public float Mobility = 6f;
  public float Speed = 1.5f;
  private TrainExecutor executor;

  private void Awake()
  {
    if (isTemplate) template = this;
  }

  private void Start()
  {
    if (isTemplate)
    {
      listener.Add<Train>(EventType.CREATED, t => storage.Add(t));
      listener.Add<Train>(EventType.DELETED, t => storage.Remove(t));
    }
  }

  private void Update()
  {
    if (executor != null)
    {
      var prev = executor.Position;
      executor.Update();
      transform.position = executor.Position;
      if (Vector3.Distance(prev, executor.Position) > 0)
      {
        listener.Fire(EventType.MODIFIED, this);
      }
    }
  }

  public Train NewInstance(LineTask current)
  {
    var obj = Instantiate(template);
    obj.isTemplate = false;
    obj.GetComponent<SpriteRenderer>().enabled = true;
    obj.executor = new TrainExecutor(obj, current);
    obj.transform.position = obj.executor.Position;
    listener.Fire(EventType.CREATED, obj);
    return obj;
  }

  public void Remove()
  {
    executor.Current.Origin.Trains.Remove(this);
    listener.Fire(EventType.DELETED, this);
    Destroy(gameObject);
  }

  public void Skip(LineTask to)
  {
    executor.Skip(to);
    listener.Fire(EventType.MODIFIED, this);
  }

  public LineTask Current { get { return executor.Current.Origin; } }
}