using System.Collections.Generic;
using UnityEngine;

public class CityResource : MonoBehaviour
{
  public ModelFactory factory;
  private bool isInited;

  // Start is called before the first frame update
  private void Start()
  {
  }

  // Update is called once per frame
  private void Update()
  {
    if (!isInited)
    {
      for (int i = 0; i < 2; i++)
      {
        var c = factory.NewCompany(i + 1, new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f));
      }
      var r = factory.NewResidence(new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f));
    }
    isInited = true;
  }
}
