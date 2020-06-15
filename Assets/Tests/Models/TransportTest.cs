using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class TransportTest
  {
    private TestUtils utils;

    [SetUp]
    public void Init()
    {
      utils = new TestUtils();
    }

    [UnityTest]
    public IEnumerator Step()
    {
      yield return null;
      var a = utils.ures.Proxy;
      var tr = utils.trans;
      Assert.True(tr.IsWaiting);
      a.StartRail(new Vector3(0f, 0f));
      yield return null;
      a.BuildStation();
      yield return null;
      a.CreateLine();
      a.StartLine();
      a.DeployTrain(a.TailLine.Top);
      yield return null;
      a.ExtendRail(new Vector3(1f, 1f));
      yield return null;
      a.InsertEdge();
      a.ExtendRail(new Vector3(2f, 2f));
      yield return null;
      a.BuildStation();
      yield return null;
      a.InsertEdge();
      utils.ures.State = UserResource.StateType.FIXED;
      yield return null;  // unedge
      Assert.AreEqual(0, tr.FinderIdx);
      Assert.AreEqual(0, tr.TrainIdx);
      Assert.AreEqual(0, tr.RailLineIdx);
      Assert.AreEqual(0, tr.DeptTaskIdx);
      Assert.False(tr.IsFixed);
      Assert.False(tr.IsWaiting);

      yield return null;
      Assert.AreEqual(0, tr.FinderIdx);
      Assert.AreEqual(1, tr.TrainIdx);
      Assert.AreEqual(0, tr.RailLineIdx);
      Assert.AreEqual(0, tr.DeptTaskIdx);
      Assert.False(tr.IsFixed);
      Assert.False(tr.IsWaiting);

      yield return null;
      Assert.AreEqual(0, tr.FinderIdx);
      Assert.AreEqual(1, tr.TrainIdx);
      Assert.AreEqual(0, tr.RailLineIdx);
      Assert.AreEqual(1, tr.DeptTaskIdx);
      Assert.False(tr.IsFixed);
      Assert.False(tr.IsWaiting);

      yield return null;
      Assert.AreEqual(0, tr.FinderIdx);
      Assert.AreEqual(1, tr.TrainIdx);
      Assert.AreEqual(1, tr.RailLineIdx);
      Assert.AreEqual(0, tr.DeptTaskIdx);
      Assert.False(tr.IsFixed);
      Assert.False(tr.IsWaiting);

      yield return null;
      Assert.AreEqual(1, tr.FinderIdx);
      Assert.AreEqual(0, tr.TrainIdx);
      Assert.AreEqual(0, tr.RailLineIdx);
      Assert.AreEqual(0, tr.DeptTaskIdx);
      Assert.False(tr.IsFixed);
      Assert.False(tr.IsWaiting);

      yield return null;
      Assert.AreEqual(1, tr.FinderIdx);
      Assert.AreEqual(1, tr.TrainIdx);
      Assert.AreEqual(0, tr.RailLineIdx);
      Assert.AreEqual(0, tr.DeptTaskIdx);
      Assert.False(tr.IsFixed);
      Assert.False(tr.IsWaiting);

      yield return null;
      Assert.AreEqual(1, tr.FinderIdx);
      Assert.AreEqual(1, tr.TrainIdx);
      Assert.AreEqual(0, tr.RailLineIdx);
      Assert.AreEqual(1, tr.DeptTaskIdx);
      Assert.False(tr.IsFixed);
      Assert.False(tr.IsWaiting);

      yield return null;
      Assert.AreEqual(1, tr.FinderIdx);
      Assert.AreEqual(1, tr.TrainIdx);
      Assert.AreEqual(1, tr.RailLineIdx);
      Assert.AreEqual(0, tr.DeptTaskIdx);
      Assert.False(tr.IsFixed);
      Assert.False(tr.IsWaiting);

      yield return null;
      Assert.AreEqual(2, tr.FinderIdx);
      Assert.AreEqual(0, tr.TrainIdx);
      Assert.AreEqual(0, tr.RailLineIdx);
      Assert.AreEqual(0, tr.DeptTaskIdx);
      Assert.True(tr.IsFixed);
      Assert.False(tr.IsWaiting);

      a.Rollback();
      Assert.False(tr.IsFixed);
      Assert.True(tr.IsWaiting);
    }

    [UnityTest]
    public IEnumerator Single()
    {
      yield return null;
      var a = utils.ures.Proxy;
      var tr = utils.trans;
      a.StartRail(new Vector3(0f, 0f));
      yield return null;
      a.BuildStation();
      yield return null;
      a.CreateLine();
      a.StartLine();
      a.ExtendRail(new Vector3(3f, 4f));
      yield return null;
      a.BuildStation();
      yield return null;
      a.InsertEdge();
      utils.ures.State = UserResource.StateType.FIXED;

      var dept1 = a.TailLine.Top;
      var p1 = dept1.Departure.StandsOver;
      var dept2 = dept1.Next.Next as DeptTask;
      var p2 = dept2.Departure.StandsOver;
      var cost = 5 / Mathf.Sqrt(10) * 4;

      Assert.False(tr.IsFixed);
      Assert.True(tr.IsWaiting);

      while (!tr.IsFixed) yield return null;

      Assert.True(tr.IsFixed);
      Assert.False(tr.IsWaiting);

      Assert.AreSame(dept1, p1.Route.NextFor(p1));
      Assert.AreEqual(0f, p1.Route.DistanceFor(p1), utils.DELTA);
      Assert.AreEqual(0f, p1.Route.PaymentFor(p1), utils.DELTA);
      Assert.AreSame(dept1, p1.Route.NextFor(p2));
      Assert.AreEqual(1.5f, p1.Route.DistanceFor(p2), utils.DELTA);
      Assert.AreEqual(cost, p1.Route.PaymentFor(p2), utils.DELTA);

      Assert.AreSame(p1, dept1.Route.NextFor(p1));
      Assert.AreEqual(0f, dept1.Route.DistanceFor(p1), utils.DELTA);
      Assert.AreEqual(0f, dept1.Route.PaymentFor(p1), utils.DELTA);
      Assert.AreSame(p2, dept1.Route.NextFor(p2));
      Assert.AreEqual(1.5f, dept1.Route.DistanceFor(p2), utils.DELTA); // 乗車 +1
      Assert.AreEqual(cost, dept1.Route.PaymentFor(p2), utils.DELTA);

      Assert.AreSame(dept2, p2.Route.NextFor(p1));
      Assert.AreEqual(1.5f, p2.Route.DistanceFor(p1), utils.DELTA); // 乗車 +1
      Assert.AreEqual(cost, p2.Route.PaymentFor(p1), utils.DELTA);
      Assert.AreSame(dept2, p2.Route.NextFor(p2));
      Assert.AreEqual(0f, p2.Route.DistanceFor(p2), utils.DELTA);
      Assert.AreEqual(0f, p2.Route.PaymentFor(p2), utils.DELTA);

      Assert.AreSame(p1, dept2.Route.NextFor(p1));
      Assert.AreEqual(1.5f, dept2.Route.DistanceFor(p1), utils.DELTA); // 乗車 +1
      Assert.AreEqual(cost, dept2.Route.PaymentFor(p1), utils.DELTA);
      Assert.AreSame(p2, dept2.Route.NextFor(p2));
      Assert.AreEqual(0f, dept2.Route.DistanceFor(p2), utils.DELTA);
      Assert.AreEqual(0f, dept2.Route.PaymentFor(p2), utils.DELTA);
    }
  }
}
