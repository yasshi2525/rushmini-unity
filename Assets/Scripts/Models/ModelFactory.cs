using UnityEngine;

public class ModelFactory : MonoBehaviour
{
  public ModelStorage storage;
  public Company c;
  public Residence r;
  public Train t;
  public Human h;

  private void SetPosition(Transform trans, float x, float y)
  {
    trans.position = new Vector3(x, y, 0.0f);
  }

  private void EnableRenderer(GameObject obj)
  {
    obj.GetComponent<SpriteRenderer>().enabled = true;
  }

  public Company newCompany(int attractiveness, float x, float y)
  {
    var newC = Instantiate(c);
    newC.attractiveness = attractiveness;
    newC.GetComponent<SpriteRenderer>().enabled = true;
    SetPosition(newC.transform, x, y);
    return newC;
  }

  public Residence newResidence(float x, float y)
  {
    var newR = Instantiate(r);
    newR.isTemplate = false;
    newR.GetComponent<SpriteRenderer>().enabled = true;
    SetPosition(newR.transform, x, y);
    return newR;
  }

  public Train newTrain(float x, float y)
  {
    var newT = Instantiate(t);
    newT.GetComponent<SpriteRenderer>().enabled = true;
    SetPosition(newT.transform, x, y);
    return newT;
  }

  public Human newHuman(Residence r, Company c)
  {
    var newH = Instantiate(h);
    newH.departure = r;
    newH.destination = c;
    newH.GetComponent<SpriteRenderer>().enabled = true;
    var len = Random.Range(0f, newH.rand);
    var theta = Random.Range(0f, Mathf.PI * 2);

    var rLoc = r.GetComponent<SpriteRenderer>().transform.position;
    SetPosition(
      newH.transform,
      rLoc.x + len * Mathf.Cos(theta),
      rLoc.y + len * Mathf.Sin(theta));
    return newH;
  }
}