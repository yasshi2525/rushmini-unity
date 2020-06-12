using System;
using UnityEngine;

public class MoveTask : TrainTask
{
  protected EdgeTask org;

  public MoveTask(Train t, EdgeTask edge, OnComplete fn) : base(t, fn)
  {
    org = edge;
    org.Trains.Add(t);
  }

  public override Vector3 Position
  {
    get
    {
      var from = org.Departure.transform.position;
      var to = org.Destination.transform.position;
      return Vector3.Lerp(from, to, progress);
    }
  }

  protected override bool IsCompleted()
  {
    return progress >= 1;
  }

  public override LineTask Origin { get { return org; } }

  protected override float Estimate()
  {
    return (1 - progress) * org.Length / train.Speed;
  }

  protected override float OnFullConsume(float available)
  {
    var cost = Estimate();
    progress = 1;
    return available - cost;
  }

  protected override float OnPartialConsume(float available)
  {
    progress += available * train.Speed / Origin.Length;
    return 0;
  }

  protected override void HandleOnInited() { }

  protected override void HandleOnConsumed(float available)
  {

  }
}