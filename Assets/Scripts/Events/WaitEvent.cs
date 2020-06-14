using UnityEngine;

public class WaitEvent
{
  public Human.StateType state;
  public float value;
  protected ModelListener listener;
  public WaitEvent(ModelListener lis, Human.StateType s)
  {
    state = s;
    listener = lis;
  }
  public void Wait()
  {
    value += Time.deltaTime;
  }
  public void fire()
  {
    listener.Fire(EventType.CREATED, this);
  }
}