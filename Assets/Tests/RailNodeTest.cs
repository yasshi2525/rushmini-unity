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
      Assert.AreEqual(utils.storage.Find<RailNode>().Count, 1);
      Assert.AreEqual(inst.transform.position, new Vector3(1f, 2f, 3f));
    }

    [UnityTest]
    public IEnumerator ExtendRail()
    {
      yield return null;
      var inst = utils.rn.NewInstance(new Vector3(1f, 2f, 3f));
      yield return null;
      var outE = inst.Extend(new Vector3(4f, 5f, 6f));
      yield return null;
      Assert.AreEqual(utils.storage.Find<RailNode>().Count, 2);
      Assert.AreEqual(inst.inEdge.Count, 1);
      Assert.AreEqual(inst.outEdge.Count, 1);
      Assert.AreEqual(inst.outEdge[0].from, inst);
      Assert.AreEqual(inst.inEdge[0].to, inst);
      Assert.AreEqual(inst.outEdge[0], outE);
      Assert.AreEqual(inst.inEdge[0], outE.reverse);
      Assert.AreEqual(inst.outEdge[0].from.transform.position, new Vector3(1f, 2f, 3f));
      Assert.AreEqual(inst.outEdge[0].to.transform.position, new Vector3(4f, 5f, 6f));
      Assert.AreEqual(inst.inEdge[0].from.transform.position, new Vector3(4f, 5f, 6f));
      Assert.AreEqual(inst.inEdge[0].to.transform.position, new Vector3(1f, 2f, 3f));
    }

    [UnityTest]
    public IEnumerator BuildPlatform()
    {
      yield return null;
      var inst = utils.rn.NewInstance(new Vector3(1f, 2f, 3f));
      var p = inst.BuildStation();
      yield return null;
      Assert.AreEqual(inst.platform, p);
      Assert.AreEqual(p.on, inst);
    }
  }
}
