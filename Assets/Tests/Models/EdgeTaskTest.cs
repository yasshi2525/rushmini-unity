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
  }
}
