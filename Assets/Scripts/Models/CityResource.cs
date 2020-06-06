using System.Collections.Generic;
using UnityEngine;

public class CityResource : MonoBehaviour
{
  public ModelListener listener;
  public ModelFactory factory;

  // Start is called before the first frame update
  private void Start()
  {
    for (int i = 0; i < 2; i++)
    {
      var c = factory.newCompany(i + 1, Random.Range(-5f, 5f), Random.Range(-5f, 5f));
      listener.Find<Company>(EventType.CREATED).Invoke(c);
    }
    var r = factory.newResidence(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
    listener.Find<Residence>(EventType.CREATED).Invoke(r);
  }

  // Update is called once per frame
  private void Update()
  {

  }
}
