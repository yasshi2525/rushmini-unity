using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class DeptTaskTest
  {
    private TestUtils utils;
    [SetUp]
    public void Init()
    {
      utils = new TestUtils();
    }

    [UnityTest]
    public IEnumerator FirstDeparture()
    {
      yield return null;
      var rn = utils.factory.NewRailNode(new Vector3(1f, 2f, 3f));
      yield return null;
      var p = rn.BuildStation();
      yield return null;
      var l = new RailLine(utils.storage, utils.listener);
      var dept = new DeptTask(utils.storage, utils.listener, l, p);
      Assert.AreSame(p, dept.Stay);
      Assert.AreSame(l, dept.Parent);
      Assert.AreSame(dept, dept.Prev);
      Assert.AreSame(dept, dept.Next);
      Assert.AreSame(rn, dept.Departure);
      Assert.AreSame(rn, dept.Destination);
      Assert.AreEqual(0, dept.Length);
    }

    [UnityTest]
    public IEnumerator AngleErrorWhenAllTaskZeroLength()
    {
      yield return null;
      var rn = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var p1 = rn.BuildStation();
      yield return null;
      var e12 = rn.Extend(new Vector3(0f, 0f));
      yield return null;
      var rn2 = e12.To;
      var p2 = rn2.BuildStation();
      yield return null;
      var e23 = rn2.Extend(new Vector3(0f, 0f));
      yield return null;
      var l = new RailLine(utils.storage, utils.listener);
      var dept1 = new DeptTask(utils.storage, utils.listener, l, p1);
      dept1.InsertEdge(e12);
      var dept2 = dept1.Next.Next;
      Assert.Throws<ArgumentException>(() => dept2.SignedAngle(e23));
    }

    [UnityTest]
    public IEnumerator AngleErrorNonNeighbor()
    {
      yield return null;
      var rn = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var p = rn.BuildStation();
      yield return null;
      var l = new RailLine(utils.storage, utils.listener);
      var dept = new DeptTask(utils.storage, utils.listener, l, p);
      var rnX = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var eX = rnX.Extend(new Vector3(1f, 1f));
      Assert.Throws<ArgumentException>(() => dept.SignedAngle(eX));
    }

    [UnityTest]
    public IEnumerator AngleErrorOnlyDeptTask()
    {
      yield return null;
      var rn = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var p = rn.BuildStation();
      yield return null;
      var re = rn.Extend(new Vector3(1f, 1f));
      yield return null;
      var l = new RailLine(utils.storage, utils.listener);
      var dept = new DeptTask(utils.storage, utils.listener, l, p);
      Assert.True(dept.IsNeighbor(re));
      Assert.Throws<ArgumentException>(() => dept.SignedAngle(re));
    }

    [UnityTest]
    public IEnumerator InsertEdgeToNonStation()
    {
      yield return null;
      var rn1 = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var p = rn1.BuildStation();
      yield return null;
      var e12 = rn1.Extend(new Vector3(1f, 1f));
      yield return null;
      var e21 = e12.Reverse;
      var rn2 = e12.To;
      var l = new RailLine(utils.storage, utils.listener);
      l.StartLine(p);
      var dept = l.Top;
      // from -> [dept] -> from -> [outbound] -> to -> [inbound] -> from
      dept.InsertEdge(e12);
      var mv12 = dept.Next;
      Assert.IsInstanceOf(typeof(EdgeTask), mv12);
      Assert.AreSame(l, mv12.Parent);
      Assert.AreSame(e12, (mv12 as EdgeTask).Edge);
      Assert.AreSame(rn1, mv12.Departure);
      Assert.AreSame(rn2, mv12.Destination);
      Assert.AreEqual(e12.Arrow.magnitude, mv12.Length, utils.DELTA);
    }
  }
}
