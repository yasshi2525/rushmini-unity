using System;
using UnityEngine;

public class TrainExecutor
{
  protected ModelListener listener;
  protected Train train;
  public TrainTask Current;

  public TrainExecutor(ModelListener lis, Train t, LineTask lt)
  {
    listener = lis;
    train = t;
    Current = Generate(lt, () => Next());
  }

  protected TrainTask Generate(LineTask lt, TrainTask.OnComplete fn)
  {
    if (lt is DeptTask) return new StayTask(listener, train, lt as DeptTask, fn);
    if (lt is EdgeTask) return new MoveTask(train, lt as EdgeTask, fn);
    throw new ArgumentException("invalid type" + lt);
  }

  public Vector3 Position { get { return Current.Position; } }

  public void Update()
  {
    var remain = Time.deltaTime;
    while (remain > 0)
    {
      remain = Current.Execute(remain);
    }
  }

  public void Skip(LineTask to)
  {
    Current = Generate(to, () => Next());
  }

  public void Discard(Human subject)
  {
    Current.Discard(subject);
  }

  protected void Next()
  {
    Current.Origin.Trains.Remove(train);
    Current = Generate(Current.Origin.Next, () => Next());
  }
}