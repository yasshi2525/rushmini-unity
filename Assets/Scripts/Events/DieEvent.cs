using UnityEngine;

public class DieEvent
{
  public Human.StateType cause;
  public DieEvent(Human.StateType s)
  {
    cause = s;
  }
}