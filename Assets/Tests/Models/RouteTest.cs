using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class RouteTest
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
      var ro = utils.route;

      var c = utils.factory.NewCompany(1, new Vector3(9f, 12f));
      yield return null;
      var r = utils.factory.NewResidence(new Vector3(0f, 0f));
      yield return null;

      a.StartRail(new Vector3(3f, 4f));
      yield return null;

      a.BuildStation();
      yield return null;
      a.CreateLine();
      a.StartLine();

      a.ExtendRail(new Vector3(6f, 8f));
      yield return null;
      a.BuildStation();
      yield return null;
      a.InsertEdge();
      utils.ures.State = UserResource.StateType.FIXED;
      while (!tr.IsFixed) yield return null;

      // unregister
      Assert.IsFalse(ro.IsFixed);
      Assert.IsFalse(ro.IsWaiting);
      Assert.AreEqual(0, ro.FinderIdx);
      Assert.AreEqual(0, ro.PlatformIdx);
      Assert.AreEqual(0, ro.DeptTaskIdx);

      yield return null;
      Assert.IsFalse(ro.IsFixed);
      Assert.IsFalse(ro.IsWaiting);
      Assert.AreEqual(0, ro.FinderIdx);
      Assert.AreEqual(0, ro.PlatformIdx);
      Assert.AreEqual(1, ro.DeptTaskIdx);

      yield return null;
      Assert.IsFalse(ro.IsFixed);
      Assert.IsFalse(ro.IsWaiting);
      Assert.AreEqual(0, ro.FinderIdx);
      Assert.AreEqual(1, ro.PlatformIdx);
      Assert.AreEqual(0, ro.DeptTaskIdx);

      yield return null;
      Assert.IsFalse(ro.IsFixed);
      Assert.IsFalse(ro.IsWaiting);
      Assert.AreEqual(0, ro.FinderIdx);
      Assert.AreEqual(1, ro.PlatformIdx);
      Assert.AreEqual(1, ro.DeptTaskIdx);

      yield return null;
      Assert.IsFalse(ro.IsFixed);
      Assert.IsFalse(ro.IsWaiting);
      Assert.AreEqual(0, ro.FinderIdx);
      Assert.AreEqual(2, ro.PlatformIdx);
      Assert.AreEqual(0, ro.DeptTaskIdx);

      yield return null;
      Assert.IsTrue(ro.IsFixed);
      Assert.IsFalse(ro.IsWaiting);
      Assert.AreEqual(1, ro.FinderIdx);
      Assert.AreEqual(0, ro.PlatformIdx);
      Assert.AreEqual(0, ro.DeptTaskIdx);

      var dept1 = utils.ures.Proxy.TailLine.Top;
      var p1 = dept1.Departure.StandsOver;
      var g1 = p1.BelongsTo.Under;
      var dept2 = dept1.Next.Next;
      var p2 = dept2.Departure.StandsOver;
      var g2 = p2.BelongsTo.Under;

      Assert.AreEqual(g1, r.Route.NextFor(c));
      Assert.AreEqual(p1, g1.Route.NextFor(c));
      Assert.AreEqual(dept1, p1.Route.NextFor(c));
      Assert.AreEqual(p2, dept1.Route.NextFor(c));
      Assert.AreEqual(g2, p2.Route.NextFor(c));
      Assert.AreEqual(g2, g2.Route.NextFor(c));
    }
  }
}
