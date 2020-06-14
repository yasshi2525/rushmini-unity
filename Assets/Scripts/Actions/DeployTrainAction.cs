public class DeployTrainAction : Transactional
{
  protected ModelFactory factory;
  public Train train;

  public DeployTrainAction(ModelFactory f)
  {
    factory = f;
  }

  public Train Act(LineTask lt)
  {
    train = factory.NewTrain(lt);
    return train;
  }

  public void Rollback()
  {
    train.Remove();
  }
}