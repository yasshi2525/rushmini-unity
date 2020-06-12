using UnityEngine;

public abstract class TrainTask
{
  protected bool isFirstExecute = true;
  protected float progress;
  protected Train train;
  public delegate void OnComplete();
  protected OnComplete onComplete;

  public static float DELTA = 0.0001f;

  public TrainTask(Train t, OnComplete fn)
  {
    train = t;
    onComplete = fn;
  }

  public abstract Vector3 Position { get; }
  public abstract LineTask Origin { get; }
  /**
  * タスクが開始されるときに実行されるハンドラ
  */
  protected abstract void HandleOnInited();
  /**
   * 時間を消費したとき実行されるハンドラ
   */
  protected abstract void HandleOnConsumed(float available);
  /**
   * このタスクの完了までにかかる時間を計算します
   */
  protected abstract float Estimate();
  /**
   * 達成度を1にできる時間があるとき実施する処理
   */
  protected abstract float OnFullConsume(float available);
  /**
   * 達成度を部分的に上げる時間があるとき実施する処理
   */
  protected abstract float OnPartialConsume(float available);
  /**
   * 実際にタスクを消化し、残りの時間を返します
   */
  protected float Consume(float available)
  {
    var strictRequired = Estimate();
    // 浮動小数点の計算誤差があるため、極めて小さな値が残っていた場合、完了とする
    var remain = (available > strictRequired - DELTA) ? OnFullConsume(available) : OnPartialConsume(available);
    HandleOnConsumed(available);
    return remain;
  }

  protected abstract bool IsCompleted();

  /**
   * 指定された時間内でタスクを消化し、残った時間を返します
   * 単位は秒
   */
  public float Execute(float available)
  {
    if (isFirstExecute)
    {
      HandleOnInited();
      isFirstExecute = false;
    }
    var remain = Consume(available);
    if (IsCompleted()) onComplete();
    return remain;
  }
}