using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DS
{
    public class DialogueSystemInspector : MonoBehaviour
    {
        [SerializeField] DialogueSystemContainerScriptableObject dialogueData;
        [SerializeField] TMP_Text dialogueText;
        [SerializeField] List<TMP_Text> choiceTextList;
        [SerializeField] float toNextWaitTime;

        public Action TextStartEvent { get; set; }
        public Action TextEndtEvent { get; set; }
        public Action<int> RefreshChoiceWitchCountEvent { get; set; }
        public string CurrentNodeID { get; private set; }
        public Dictionary<string, string> DialogueContentDictionary { get; set; }

        bool isInited = false;
        Dictionary<string, DialogueSystemNodeSaveData> nodeDictionary;

        public void Initialize()
        {
            if (dialogueData == null)
            {
                Debug.LogError("Dialogue data is null!");
                return;
            }
            nodeDictionary = new Dictionary<string, DialogueSystemNodeSaveData>();
            DialogueContentDictionary = new Dictionary<string, string>();
            foreach (var node in dialogueData.NodeList)
            {
                if (nodeDictionary.ContainsKey(node.Id) == false)
                {
                    nodeDictionary.Add(node.Id, node);
                }
                if (DialogueContentDictionary.ContainsKey(node.TextKey) == false)
                {
                    DialogueContentDictionary.Add(node.TextKey, string.Empty);
                }

                foreach (var choice in node.ChoiceList)
                {
                    if (string.IsNullOrEmpty(choice.TextKey) == false)
                    {
                        if (DialogueContentDictionary.ContainsKey(choice.TextKey) == false)
                        {
                            DialogueContentDictionary.Add(choice.TextKey, string.Empty);
                        }
                    }
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
            }
            else
            {
                TextStartEvent?.Invoke();
                dialogueText.text = "";
                yield return new WaitForSeconds(0.1f);
                dialogueText.text = DialogueContentDictionary[nodeData.TextKey];
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
                        choiceTextList[i].text = DialogueContentDictionary[choices[i].TextKey];
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
