using UnityEngine;
public class Train : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  public bool isTemplate = true;

  private void Start()
  {
    if (isTemplate)
    {
      listener.Find<Train>(EventType.CREATED).AddListener(t => storage.Find<Train>().Add(t));
      listener.Find<Train>(EventType.DELETED).AddListener(t => storage.Find<Train>().Remove(t));
    }
  }
}