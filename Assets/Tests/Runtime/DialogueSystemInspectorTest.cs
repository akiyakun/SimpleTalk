using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using NUnit.Framework;

namespace DS.Test
{
    [TestFixture]
    public class DialogueSystemInspectorTest
    {
        GameObject canvas;
        GameObject obj;
        DialogueSystemViewGroup viewGroup;

        [SetUp]
        public void Setup()
        {
            // CanvasとUIの生成
            canvas = new GameObject("Canvas", typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            obj = new GameObject("DialogueSystem", typeof(DialogueSystemViewGroup));
            obj.transform.SetParent(canvas.transform);
            viewGroup = obj.GetComponent<DialogueSystemViewGroup>();

            // TMP_Text初期化
            var dialogueTextObj = new GameObject("DialogueText");
            var dialogueTMP = dialogueTextObj.AddComponent<TextMeshProUGUI>();
            dialogueTMP.text = "";
            dialogueTextObj.transform.SetParent(viewGroup.transform);
            typeof(DialogueSystemView)
                .GetField("dialogueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(viewGroup, dialogueTMP);

            // ChoiceTextList初期化
            var choiceList = new List<TMP_Text>();
            for (int i = 0; i < 3; i++)
            {
                var choice = new GameObject("ChoiceText" + i).AddComponent<TextMeshProUGUI>();
                choice.text = "";
                choice.transform.SetParent(viewGroup.transform);
                choiceList.Add(choice);
            }
            typeof(DialogueSystemView)
                .GetField("choiceTextList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(viewGroup, choiceList);

            // WaitTime
            typeof(DialogueSystemView)
                .GetField("toNextWaitTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(viewGroup, 0.1f);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(obj);
            Object.DestroyImmediate(canvas);
        }

        [UnityTest]
        public IEnumerator DialogueTextの初期化()
        {
            // ScriptableObject生成
            var scriptable = ScriptableObject.CreateInstance<DialogueSystemContainerScriptableObject>();
            var node = new DialogueSystemNodeSaveData
            {
                Id = "node1",
                TextKey = "mainText",
                ChoiceList = new List<DialogueSystemChoiceSaveData>
            {
                new DialogueSystemChoiceSaveData { TextKey = "choice1" },
                new DialogueSystemChoiceSaveData { TextKey = "choice2" },
            }
            };
            scriptable.NodeList = new List<DialogueSystemNodeSaveData> { node };

            // ScriptableObject
            typeof(DialogueSystemView)
                .GetField("dialogueData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(viewGroup, scriptable);

            var contentProvider = new DialogueContentProvider(new Dictionary<string, string>
            {
                { "mainText", "Hello, this is a test dialogue!" },
                { "choice1", "Option A" },
                { "choice2", "Option B" }
            });

            viewGroup.Initialize(contentProvider);
            yield return new WaitForSeconds(2f);

            Assert.IsTrue(viewGroup.IsInited);
            Assert.IsTrue(viewGroup.GetComponentsInChildren<TextMeshProUGUI>().Length > 0);
        }

        [UnityTest]
        public IEnumerator DialogueTextのイベントの実行()
        {
            // ScriptableObject生成
            var scriptable = ScriptableObject.CreateInstance<DialogueSystemContainerScriptableObject>();
            var node = new DialogueSystemNodeSaveData
            {
                Id = "node1",
                TextKey = "mainText",
                ChoiceList = new List<DialogueSystemChoiceSaveData>
            {
                new DialogueSystemChoiceSaveData { TextKey = "choice1" },
                new DialogueSystemChoiceSaveData { TextKey = "choice2" },
            }
            };
            scriptable.NodeList = new List<DialogueSystemNodeSaveData> { node };

            // ScriptableObject
            typeof(DialogueSystemView)
                .GetField("dialogueData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(viewGroup, scriptable);

            var contentProvider = new DialogueContentProvider(new Dictionary<string, string>
            {
                { "mainText", "Hello, this is a test dialogue!" },
                { "choice1", "Option A" },
                { "choice2", "Option B" }
            });

            // Event
            bool startEventFired = false;
            bool endEventFired = false;
            bool refreshEventFired = false;
            //viewGroup.TextStartEvent = () => startEventFired = true;
            //viewGroup.TextEndEvent = () => endEventFired = true;
            //viewGroup.RefreshChoiceWitchCountEvent = (count) => refreshEventFired = true;

            viewGroup.Initialize(contentProvider);
            viewGroup.StartDialogueFromBegin();
            yield return new WaitForSeconds(2f);

            Assert.IsTrue(startEventFired);
            Assert.IsTrue(endEventFired);
            Assert.IsTrue(refreshEventFired);
        }

        //[UnityTest]
        public IEnumerator DialogueTextAndChoicesAllTest()
        {
            // ScriptableObject生成
            var scriptable = ScriptableObject.CreateInstance<DialogueSystemContainerScriptableObject>();
            var node = new DialogueSystemNodeSaveData
            {
                Id = "node1",
                TextKey = "mainText",
                ChoiceList = new List<DialogueSystemChoiceSaveData>
            {
                new DialogueSystemChoiceSaveData { TextKey = "choice1" },
                new DialogueSystemChoiceSaveData { TextKey = "choice2" },
            }
            };
            scriptable.NodeList = new List<DialogueSystemNodeSaveData> { node };

            // ScriptableObject
            typeof(DialogueSystemView)
                .GetField("dialogueData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(viewGroup, scriptable);

            var contentProvider = new DialogueContentProvider(new Dictionary<string, string>
            {
                { "mainText", "Hello, this is a test dialogue!" },
                { "choice1", "Option A" },
                { "choice2", "Option B" }
            });
            viewGroup.Initialize(contentProvider);

            // Event
            bool startEventFired = false;
            bool endEventFired = false;
            //viewGroup.TextStartEvent = () => startEventFired = true;
            //viewGroup.TextEndEvent = () => endEventFired = true;

            // Act
            viewGroup.StartDialogueFromBegin();
            yield return new WaitForSeconds(2f);

            // Assert
            Assert.IsTrue(startEventFired);
            Assert.IsTrue(endEventFired);
            Assert.IsTrue(viewGroup.GetComponentsInChildren<TextMeshProUGUI>().Length > 0);

            var choices = (List<TMP_Text>)typeof(DialogueSystemView)
                .GetField("choiceTextList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(viewGroup);

            Assert.AreEqual("Option A", choices[0].text);
            Assert.AreEqual("Option B", choices[1].text);
            Assert.AreEqual("", choices[2].text);
        }
    }
}
