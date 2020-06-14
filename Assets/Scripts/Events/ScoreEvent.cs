using UnityEngine;

public class ScoreEvent
{
  public float value;
  public Human src;
  public ScoreEvent(float v, Human h)
  {
    value = v;
    src = h;
  }
}