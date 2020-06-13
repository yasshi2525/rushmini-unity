using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class EdgeTaskTest
  {
    private TestUtils utils;

    [SetUp]
    public void Init()
    {
      utils = new TestUtils();
    }

    [UnityTest]
    public IEnumerator Angle()
    {
      yield return null;
      var rn = utils.factory.NewRailNode(new Vector3(-1f, 0f));
      yield return null;
      var p = rn.BuildStation();
      yield return null;
      var e12 = rn.Extend(new Vector3(0f, 0f));
      yield return null;
      var e23 = e12.To.Extend(new Vector3(1, Mathf.Sqrt(3)));
      yield return null;
      var l = new RailLine(utils.storage, utils.listener);
      var dept = new DeptTask(utils.storage, utils.listener, l, p);
      var mv12 = new EdgeTask(utils.storage, utils.listener, l, e12, dept);
      // mv12 (x軸) から e23 (1, √3) は 240° 回転した位置に見える (左向き正)
      Assert.AreEqual(-120f, mv12.SignedAngle(e23), utils.DELTA);
    }

    [UnityTest]
    public IEnumerator AngleZeroWithZeroLengthEdge()
    {
      yield return null;
      var rn = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var p = rn.BuildStation();
      yield return null;
      var e12 = rn.Extend(new Vector3(0f, 0f));
      yield return null;
      var e23 = e12.To.Extend(new Vector3(0f, 0f));
      yield return null;
      var l = new RailLine(utils.storage, utils.listener);
      var dept = new DeptTask(utils.storage, utils.listener, l, p);
      var mv12 = new EdgeTask(utils.storage, utils.listener, l, e12, dept);
      Assert.AreEqual(mv12.SignedAngle(e23), 0f);
    }

    [UnityTest]
    public IEnumerator AngleErrorNonNeighbor()
    {
      yield return null;
      var rn = utils.factory.NewRailNode(new Vector3(0f, 0f));
      var rnX = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var p = rn.BuildStation();
      yield return null;
      var e12 = rn.Extend(new Vector3(1f, 1f));
      var eX = rnX.Extend(new Vector3(1f, 1f));
      yield return null;
      var l = new RailLine(utils.storage, utils.listener);
      var dept = new DeptTask(utils.storage, utils.listener, l, p);
      var move = new EdgeTask(utils.storage, utils.listener, l, e12, dept);
      yield return null;
      Assert.Throws<ArgumentException>(() => move.SignedAngle(eX));
    }

    [UnityTest]
    public IEnumerator InsertEdgeToNonStationEdge()
    {
      yield return null;
      var rn1 = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var p1 = rn1.BuildStation();
      yield return null;
      var e12 = rn1.Extend(new Vector3(1f, 1f));
      yield return null;
      var e21 = e12.Reverse;
      var rn2 = e12.To;
      var e23 = rn2.Extend(new Vector3(2f, 2f));
      yield return null;
      var rn3 = e23.To;
      var e32 = e23.Reverse;
      var l = new RailLine(utils.storage, utils.listener);
      l.StartLine(p1);
      var dept1 = l.Top;
      // 1. rn1 -> [e12] -> rn2
      // 2. rn1 -> [e12] -> rn2 -> [e23] -> rn3
      dept1.InsertEdge(e12);
      var mv12 = dept1.Next;
      mv12.InsertEdge(e23);

      var mv23 = mv12.Next;
      Assert.IsInstanceOf(typeof(EdgeTask), mv23);
      Assert.AreSame(l, mv23.Parent);
      Assert.AreSame(e23, (mv23 as EdgeTask).Edge);
      Assert.AreSame(rn2, mv23.Departure);
      Assert.AreSame(rn3, mv23.Destination);
      Assert.AreEqual(e23.Arrow.magnitude, mv23.Length, utils.DELTA);
      Assert.AreSame(mv23, mv12.Next);
      Assert.AreSame(mv12, mv23.Prev);

      var mv32 = mv23.Next;
      Assert.AreSame(l, mv32.Parent);
      Assert.AreSame(e32, (mv32 as EdgeTask).Edge);
      Assert.AreSame(rn3, mv32.Departure);
      Assert.AreSame(rn2, mv32.Destination);
      Assert.AreSame(mv32, mv23.Next);
      Assert.AreSame(mv23, mv32.Prev);
      Assert.AreSame(mv32, mv32.Next.Prev);
    }

    [UnityTest]
    public IEnumerator InsertEdgeToStationEdge()
    {
      yield return null;
      var rn1 = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var p1 = rn1.BuildStation();
      yield return null;
      var e12 = rn1.Extend(new Vector3(1f, 1f));
      yield return null;
      var e21 = e12.Reverse;
      var rn2 = e12.To;
      var e23 = rn2.Extend(new Vector3(2f, 2f));
      yield return null;
      var rn3 = e23.To;
      var e32 = e23.Reverse;
      var p3 = rn3.BuildStation();
      yield return null;
      var l = new RailLine(utils.storage, utils.listener);
      l.StartLine(p1);
      var dept1 = l.Top;
      // 1. rn1 -> [e12] -> rn2
      // 2. rn1 -> [e12] -> rn2 -> [e23] -> rn3
      dept1.InsertEdge(e12);
      var mv12 = dept1.Next;
      mv12.InsertEdge(e23);

      var mv23 = mv12.Next;
      Assert.IsInstanceOf(typeof(EdgeTask), mv23);
      Assert.AreSame(l, mv23.Parent);
      Assert.AreSame(e23, (mv23 as EdgeTask).Edge);
      Assert.AreSame(rn2, mv23.Departure);
      Assert.AreSame(rn3, mv23.Destination);
      Assert.AreEqual(e23.Arrow.magnitude, mv23.Length, utils.DELTA);
      Assert.AreSame(mv23, mv12.Next);
      Assert.AreSame(mv12, mv23.Prev);

      var dept3 = mv23.Next;
      Assert.IsInstanceOf(typeof(DeptTask), dept3);
      Assert.AreSame(p3, (dept3 as DeptTask).Stay);
      Assert.AreSame(rn3, dept3.Departure);
      Assert.AreSame(rn3, dept3.Destination);
      Assert.AreSame(dept3, mv23.Next);
      Assert.AreSame(mv23, dept3.Prev);

      var mv32 = dept3.Next;
      Assert.AreSame(l, mv32.Parent);
      Assert.AreSame(e32, (mv32 as EdgeTask).Edge);
      Assert.AreSame(rn3, mv32.Departure);
      Assert.AreSame(rn2, mv32.Destination);
      Assert.AreSame(mv32, dept3.Next);
      Assert.AreSame(dept3, mv32.Prev);
      Assert.AreSame(mv32, mv32.Next.Prev);
    }

    [UnityTest]
    public IEnumerator InsertPlatform()
    {
      yield return null;
      var rn1 = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var p1 = rn1.BuildStation();
      yield return null;
      var e12 = rn1.Extend(new Vector3(1f, 1f));
      yield return null;
      var e21 = e12.Reverse;
      var rn2 = e12.To;
      var l = new RailLine(utils.storage, utils.listener);
      l.StartLine(p1);
      var dept1 = l.Top;
      dept1.InsertEdge(e12);
      var mv12 = dept1.Next;
      var mv21 = mv12.Next;
      var p2 = rn2.BuildStation();
      yield return null;
      mv12.InsertPlatform(p2);
      var dept2 = mv12.Next;

      Assert.IsInstanceOf(typeof(DeptTask), dept2);
      Assert.AreSame(p2, (dept2 as DeptTask).Stay);
      Assert.AreSame(rn2, dept2.Departure);
      Assert.AreSame(rn2, dept2.Destination);
      Assert.AreSame(dept2, mv12.Next);
      Assert.AreSame(mv12, dept2.Prev);
      Assert.AreSame(mv21, dept2.Next);
      Assert.AreSame(dept2, mv21.Prev);
    }

    [UnityTest]
    public IEnumerator InsertPlatformErrorUnNeighbored()
    {
      yield return null;
      var rn1 = utils.factory.NewRailNode(new Vector3(0f, 0f));
      var rnX = utils.factory.NewRailNode(new Vector3(0f, 0f));
      yield return null;
      var p1 = rn1.BuildStation();
      var pX = rnX.BuildStation();
      yield return null;
      var e12 = rn1.Extend(new Vector3(1f, 1f));
      yield return null;
      var rn2 = e12.To;
      var l = new RailLine(utils.storage, utils.listener);
      l.StartLine(p1);
      var dept1 = l.Top;
      dept1.InsertEdge(e12);
      var mv12 = dept1.Next;
      Assert.Throws<ArgumentException>(() => mv12.InsertPlatform(pX));
    }
  }
}
