using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using SimpleTalk;

[Description("RuntimeTests")]
public class RuntimeTests
{
    ITalkFactory talkFactory;
    TalkManager talkManager;

    ITalkWindow talkWindow;
    IChatBubble chatBubble;


    [OneTimeSetUp]
    public void InitializeTests()
    {
        talkFactory = new MockTalkFactory();
        Assert.NotNull(talkFactory);
    }

    [UnityTest]
    // [Order(-9999)]
    public IEnumerator A_TalkManager生成()
    {
        talkManager = new TalkManager(talkFactory);
        Assert.NotNull(talkManager);
        // Assert.That(talkManager, Is.Not.Null);
        yield return null;
    }


    #region ITalkWindow
    [UnityTest]
    // [Order(-8999)]
    public IEnumerator B0_ITalkWindow生成()
    {
        talkWindow = talkManager.CreateTalk<ITalkWindow>(DefaultTalkNames.TalkWindow);
        Assert.NotNull(talkWindow);
        yield return null;
    }

    [UnityTest]
    // [Order(-8998)]
    public IEnumerator B1_ITalkWindow_ShowMessage()
    {
        talkWindow.ShowMessage("Hello, World!");
        yield return null;
    }

    [UnityTest]
    // [Order(-8000)]
    public IEnumerator B2_ITalkWindow_Remove()
    {
        try
        {
            talkManager.RemoveTalk(talkWindow);
            talkWindow = null;
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }

        yield return null;
    }
    #endregion


    #region IChatBubble
    [UnityTest]
    // [Order(-7999)]
    public IEnumerator C0_IChatBubble生成()
    {
        chatBubble = talkManager.CreateTalk<IChatBubble>(DefaultTalkNames.ChatBubble);
        Assert.NotNull(chatBubble);
        yield return null;
    }

    [UnityTest]
    // [Order(-7998)]
    public IEnumerator C1_IChatBubblea_ShowMessage()
    {
        chatBubble.ShowMessage("Hello, World!");
        yield return null;
    }

    [UnityTest]
    // [Order(-7000)]
    public IEnumerator C2_IChatBubblea_Remove()
    {
        try
        {
            talkManager.RemoveTalk(chatBubble);
            chatBubble = null;
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }

        yield return null;
    }
    #endregion

}
