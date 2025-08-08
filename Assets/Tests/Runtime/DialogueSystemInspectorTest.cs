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
        DialogueSystemInspector inspector;

        [SetUp]
        public void Setup()
        {
            // CanvasとUIの生成
            canvas = new GameObject("Canvas", typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            obj = new GameObject("DialogueSystem", typeof(DialogueSystemInspector));
            obj.transform.SetParent(canvas.transform);
            inspector = obj.GetComponent<DialogueSystemInspector>();

            // TMP_Text初期化
            var dialogueTextObj = new GameObject("DialogueText");
            var dialogueTMP = dialogueTextObj.AddComponent<TextMeshProUGUI>();
            dialogueTMP.text = "";
            dialogueTextObj.transform.SetParent(inspector.transform);
            typeof(DialogueSystemInspector)
                .GetField("dialogueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(inspector, dialogueTMP);

            // ChoiceTextList初期化
            var choiceList = new List<TMP_Text>();
            for (int i = 0; i < 3; i++)
            {
                var choice = new GameObject("ChoiceText" + i).AddComponent<TextMeshProUGUI>();
                choice.text = "";
                choice.transform.SetParent(inspector.transform);
                choiceList.Add(choice);
            }
            typeof(DialogueSystemInspector)
                .GetField("choiceTextList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(inspector, choiceList);

            // WaitTime
            typeof(DialogueSystemInspector)
                .GetField("toNextWaitTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(inspector, 0.1f);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(obj);
            Object.DestroyImmediate(canvas);
        }

        [UnityTest]
        public IEnumerator StartShowsDialogueTextAndChoices()
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
            typeof(DialogueSystemInspector)
                .GetField("dialogueData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(inspector, scriptable);

            inspector.DialogueContentProvider = new DialogueContentProvider(new Dictionary<string, string>
            {
                { "mainText", "Hello, this is a test dialogue!" },
                { "choice1", "Option A" },
                { "choice2", "Option B" }
            });

            // Event
            bool startEventFired = false;
            bool endEventFired = false;
            inspector.TextStartEvent = () => startEventFired = true;
            inspector.TextEndtEvent = () => endEventFired = true;

            // Act
            inspector.StartDialogue();
            yield return new WaitForSeconds(2f);

            // Assert
            Assert.IsTrue(startEventFired);
            Assert.IsTrue(endEventFired);
            Assert.IsTrue(inspector.GetComponentsInChildren<TextMeshProUGUI>().Length > 0);

            var choices = (List<TMP_Text>)typeof(DialogueSystemInspector)
                .GetField("choiceTextList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(inspector);

            Assert.AreEqual("Option A", choices[0].text);
            Assert.AreEqual("Option B", choices[1].text);
            Assert.AreEqual("", choices[2].text);
        }
    }
}
