using System;
using UnityEngine;

public class TrainExecutor
{
  protected Train train;
  public TrainTask current;

  public TrainExecutor(Train t, LineTask lt)
  {
    train = t;
    current = Generate(lt, () => Next());
  }

  protected TrainTask Generate(LineTask lt, TrainTask.OnComplete fn)
  {
    if (lt is DeptTask) return new StayTask(train, lt as DeptTask, fn);
    if (lt is EdgeTask) return new MoveTask(train, lt as EdgeTask, fn);
    throw new ArgumentException("invalid type" + lt);
  }

  public Vector3 Position { get { return current.Position; } }

  public void Update()
  {
    var remain = Time.deltaTime;
    while (remain > 0)
    {
      remain = current.Execute(remain);
    }
  }

  public void Skip(LineTask to)
  {
    current = Generate(to, () => Next());
  }

  protected void Next()
  {
    current.Origin.trains.Remove(train);
    current = Generate(current.Origin.next, () => Next());
  }
}