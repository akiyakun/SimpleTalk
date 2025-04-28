using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

[Description("EditorTests")]
public class EditorTests
{
    [UnityTest]
    public IEnumerator Test()
    {
        Assert.AreEqual(1, 1);
        yield return null;
    }
}
