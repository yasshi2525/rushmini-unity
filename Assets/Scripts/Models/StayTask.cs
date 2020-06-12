using UnityEngine;
public class StayTask : TrainTask
{
  protected DeptTask org;
  /**
   * 後、何秒待機すれば次の乗降を許可するか。
   * 移動タスクを秒単位で消費していくため、frameと同期がとれない。そのため秒単位で管理している
   */
  protected float wait;

  public StayTask(Train t, DeptTask dept, OnComplete fn) : base(t, fn)
  {
    org = dept;
    onComplete = () => { fn(); };
  }

  public override Vector3 Position { get { return org.Departure.transform.position; } }

  protected override bool IsCompleted()
  {
    return progress >= 1;
  }

  protected override float Estimate()
  {
    return Mathf.Max((1 - progress) * train.Stay, 0);
  }

  protected override float OnFullConsume(float available)
  {
    var cost = Estimate();
    progress = 1;
    return available - cost;
  }

  protected override float OnPartialConsume(float available)
  {
    progress += available / train.Stay;
    return 0;
  }

  /**
   * 電車が駅に到着した際、乗車客、降車客を確定させます。
   * 到着した瞬間にホームにいた客が対象
   */
  protected override void HandleOnInited()
  {

  }

  protected override void HandleOnConsumed(float available)
  {
    wait = Mathf.Max(wait - available, 0);
  }

  public override LineTask Origin { get { return org; } }
}