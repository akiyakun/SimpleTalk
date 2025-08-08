using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DS
{
    public class DialogueSystemInspector : MonoBehaviour
    {
        [SerializeField] DialogueSystemContainerScriptableObject dialogueData;
        [SerializeField] TMP_Text dialogueText;
        [SerializeField] List<TMP_Text> choiceTextList;
        [SerializeField] float toNextWaitTime;

        public IDialogueContent DialogueContentProvider { get; set; }
        public Action TextStartEvent { get; set; }
        public Action TextEndtEvent { get; set; }
        public Action<int> RefreshChoiceWitchCountEvent { get; set; }
        public string CurrentNodeID { get; private set; }

        bool isInited = false;
        Dictionary<string, DialogueSystemNodeSaveData> nodeDictionary;

        public void Initialize()
        {
            if (dialogueData == null)
            {
                Debug.LogError("Dialogue data is null!");
                return;
            }

            if (DialogueContentProvider == null)
            {
                var contentDictionary = new Dictionary<string, string>();
                foreach (var node in dialogueData.NodeList)
                {
                    if (contentDictionary.ContainsKey(node.TextKey) == false)
                    {
                        contentDictionary.Add(node.TextKey, string.Empty);
                    }

                    foreach (var choice in node.ChoiceList)
                    {
                        if (string.IsNullOrEmpty(choice.TextKey) == false)
                        {
                            if (contentDictionary.ContainsKey(choice.TextKey) == false)
                            {
                                contentDictionary.Add(choice.TextKey, string.Empty);
                            }
                        }
                    }
                }
                DialogueContentProvider = new DialogueContentProvider(contentDictionary);
            }

            nodeDictionary = new Dictionary<string, DialogueSystemNodeSaveData>();
            foreach (var node in dialogueData.NodeList)
            {
                if (nodeDictionary.ContainsKey(node.Id) == false)
                {
                    nodeDictionary.Add(node.Id, node);
                }
            }

            if (toNextWaitTime == 0)
            {
                toNextWaitTime = 0.5f;
            }

            isInited = true;
        }

        public List<string> GetNextNodeId()
        {
            var ids = new List<string>();
            if (nodeDictionary[CurrentNodeID].ChoiceList.Count > 0)
            {
                foreach (var choice in nodeDictionary[CurrentNodeID].ChoiceList)
                {
                    if (string.IsNullOrEmpty(choice.TextKey) == false)
                    {
                        ids.Add(choice.TextKey);
                    }
                }
            }

            return ids;
        }

        public void StartDialogue()
        {
            if (isInited == false)
            {
                Initialize();
            }
            if (dialogueData == null)
            {
                Debug.LogError("Dialogue data is null!");
                return;
            }
            if (dialogueData.NodeList.Count > 0)
            {
                CurrentNodeID = dialogueData.NodeList[0].Id;
            }

            ShowCurrentNode();
        }

        public void StartDialogue(string id)
        {
            if (isInited == false)
            {
                Initialize();
            }
            CurrentNodeID = id;
            ShowCurrentNode();
        }

        void ShowCurrentNode()
        {
            if (isInited == false)
            {
                Initialize();
            }

            if (nodeDictionary.Count > 0)
            {
                StartCoroutine(TypeText(nodeDictionary[CurrentNodeID]));
            }

        }

        IEnumerator TypeText(DialogueSystemNodeSaveData nodeData)
        {
            if (dialogueText == null)
            {
                Debug.LogError("Dialogue Text(TMP_Text) is null!");
                yield break;
            }
            else
            {
                TextStartEvent?.Invoke();
                dialogueText.text = "";
                yield return new WaitForSeconds(0.1f);
                dialogueText.text = DialogueContentProvider.GetText(nodeData.TextKey);
                yield return new WaitForSeconds(toNextWaitTime);
                TextEndtEvent?.Invoke();
            }

            ShowChoices(nodeData.ChoiceList);
        }

        void ShowChoices(List<DialogueSystemChoiceSaveData> choices)
        {
            RefreshChoiceWitchCountEvent?.Invoke(choices.Count);
            if (choices.Count > 0)
            {
                for (int i = 0; i < choiceTextList.Count; i++)
                {
                    if (i < choices.Count)
                    {
                        choiceTextList[i].text = DialogueContentProvider.GetText(choices[i].TextKey);
                    }
                    else
                    {
                        choiceTextList[i].text = "";
                    }
                }
            }

        }

    }
}
