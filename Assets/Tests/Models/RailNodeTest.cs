using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class RailNodeTest
  {
    private TestUtils utils;

    [SetUp]
    public void Init()
    {
      utils = new TestUtils();
    }

    [UnityTest]
    public IEnumerator CreateNewInstance()
    {
      yield return null;
      var inst = utils.rn.NewInstance(new Vector3(1f, 2f, 3f));
      yield return null;
      Assert.AreEqual(1, utils.storage.List<RailNode>().Count);
      Assert.AreEqual(new Vector3(1f, 2f, 3f), inst.transform.position);
    }

    [UnityTest]
    public IEnumerator ExtendRail()
    {
      yield return null;
      var inst = utils.rn.NewInstance(new Vector3(1f, 2f, 3f));
      yield return null;
      var outE = inst.Extend(new Vector3(4f, 5f, 6f));
      yield return null;
      Assert.AreEqual(2, utils.storage.List<RailNode>().Count);
      Assert.AreEqual(1, inst.InEdge.Count);
      Assert.AreEqual(1, inst.OutEdge.Count);
      Assert.AreSame(inst, inst.OutEdge[0].From);
      Assert.AreSame(inst, inst.InEdge[0].To);
      Assert.AreSame(outE, inst.OutEdge[0]);
      Assert.AreSame(outE.Reverse, inst.InEdge[0]);
      Assert.AreEqual(new Vector3(1f, 2f, 3f), inst.OutEdge[0].From.transform.position);
      Assert.AreEqual(new Vector3(4f, 5f, 6f), inst.OutEdge[0].To.transform.position);
      Assert.AreEqual(new Vector3(4f, 5f, 6f), inst.InEdge[0].From.transform.position);
      Assert.AreEqual(new Vector3(1f, 2f, 3f), inst.InEdge[0].To.transform.position);
    }

    [UnityTest]
    public IEnumerator BuildPlatform()
    {
      yield return null;
      var inst = utils.rn.NewInstance(new Vector3(1f, 2f, 3f));
      var p = inst.BuildStation();
      yield return null;
      Assert.AreSame(inst.StandsOver, p);
      Assert.AreSame(p.On, inst);
    }
  }
}
