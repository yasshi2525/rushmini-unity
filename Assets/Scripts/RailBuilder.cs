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
      userResource.Extend(translateMousePoint(Input.mousePosition));
    }
  }

  private Vector3 translateMousePoint(Vector3 input)
  {
    return Camera.main.ScreenToWorldPoint(new Vector3(input.x, input.y, Camera.main.nearClipPlane + 1.0f));
  }
}