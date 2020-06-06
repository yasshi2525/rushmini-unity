using UnityEngine;
public class Human : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  public bool isTemplate = true;
  public Residence departure;
  public Company destination;
  public float rand = 0.1f;

  private void Start()
  {
    if (isTemplate)
    {
      listener.Find<Human>(EventType.CREATED).AddListener(h => storage.Find<Human>().Add(h));
      listener.Find<Human>(EventType.DELETED).AddListener(h => storage.Find<Human>().Remove(h));
    }
  }
}