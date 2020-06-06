using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class CityResource : MonoBehaviour
{
  public List<Company> cs;
  public Company c;

  private void Awake()
  {
    cs = new List<Company>();
  }

  // Start is called before the first frame update
  private void Start()
  {
    var newC = Instantiate(c);
    newC.transform.position = new Vector3(0.5f, -0.5f, 0.0f);
    cs.Add(newC);
  }

  // Update is called once per frame
  private void Update()
  {

  }
}
