using UnityEngine;

public class RailBuilder : MonoBehaviour
{
  public UserResource userResource;

  private void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
      userResource.StartRail(translateMousePoint(Input.mousePosition));
    }
    if (Input.GetMouseButton(0))
    {
      userResource.ExtendRail(translateMousePoint(Input.mousePosition));
    }
    if (Input.GetMouseButtonUp(0))
    {
      userResource.EndRail();
    }
  }

  private Vector3 translateMousePoint(Vector3 input)
  {
    return Camera.main.ScreenToWorldPoint(new Vector3(input.x, input.y, 10.0f));
  }
}