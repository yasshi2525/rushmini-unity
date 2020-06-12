using System;
using UnityEngine;

public class TrainExecutor
{
  protected Train train;
  public TrainTask Current;

  public TrainExecutor(Train t, LineTask lt)
  {
    train = t;
    Current = Generate(lt, () => Next());
  }

  protected TrainTask Generate(LineTask lt, TrainTask.OnComplete fn)
  {
    if (lt is DeptTask) return new StayTask(train, lt as DeptTask, fn);
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

  protected void Next()
  {
    Current.Origin.Trains.Remove(train);
    Current = Generate(Current.Origin.Next, () => Next());
  }
}