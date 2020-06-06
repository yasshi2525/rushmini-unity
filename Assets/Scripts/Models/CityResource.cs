using System.Collections.Generic;
using UnityEngine;

public class CityResource : MonoBehaviour
{
  public List<Company> cs;
  public List<Residence> rs;
  public Company c;
  public Residence r;
  public Human h;

  private void Awake()
  {
    cs = new List<Company>();
    rs = new List<Residence>();
  }

  // Start is called before the first frame update
  private void Start()
  {
    Vector3[] poses = { new Vector3(0.5f, -0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f) };
    foreach (Vector3 pos in poses)
    {
      var newC = Instantiate(c);
      newC.GetComponent<SpriteRenderer>().enabled = true;
      newC.transform.position = pos;
      cs.Add(newC);
    }
    var newR = Instantiate(r);
    newR.GetComponent<SpriteRenderer>().enabled = true;
    newR.transform.position = new Vector3(0f, 0f, 0f);
    rs.Add(newR);
  }

  // Update is called once per frame
  private void Update()
  {

  }
}
