using System.Collections.Generic;
using UnityEngine;

public class Residence : MonoBehaviour
{
  public Human h;
  /**
   * 会社の魅力度に応じて住民をスポーンするため、
   * 魅力度の数だけ同じ会社を行き先に設定する
   */
  public List<Company> destinations;

  public float intervalSec = 0.5f;

  private float remainTime;

  // Start is called before the first frame update
  private void Start()
  {

  }

  // Update is called once per frame
  private void Update()
  {
    remainTime -= Time.deltaTime;
    if (remainTime < 0)
    {
      var newR = Instantiate(h);
      newR.GetComponent<SpriteRenderer>().enabled = true;
      newR.transform.position = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
      remainTime += intervalSec;
    }
  }
}
