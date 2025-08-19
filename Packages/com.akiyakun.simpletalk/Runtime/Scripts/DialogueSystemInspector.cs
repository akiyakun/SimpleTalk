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

        public Action TextStartEvent { get; set; }
        public Action TextEndEvent { get; set; }
        public Action<int> RefreshChoiceWitchCountEvent { get; set; }
        public Action<string> CharacterChangedEvent { get; set; }
        public IDialogueContent DialogueContentProvider { get; private set; }
        public string CurrentNodeID { get; private set; }
        public string CurrentCharacterId { get; private set; }
        public bool IsInited { get; private set; } = false;

        Dictionary<string, DialogueSystemNodeSaveData> nodeDictionary;

        public void Initialize(IDialogueContent dialogueContent)
        {
            Debug.Assert(dialogueData != null, "Dialogue data is null!");

            DialogueContentProvider = dialogueContent;

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
                toNextWaitTime = 0.1f;
            }

            IsInited = true;
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
                        ids.Add(choice.NextNodeId);
                    }
                }
            }

            return ids;
        }

        public void StartDialogueFromBegin()
        {
            Debug.Assert(IsInited, "Dialogue system is not initialized.");

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
            Debug.Assert(IsInited, "Dialogue system is not initialized.");
            CurrentNodeID = id;
            ShowCurrentNode();
        }

        public void StartDialogueWithChoice(int choiceIndex)
        {
            Debug.Assert(IsInited, "Dialogue system is not initialized.");
            if (choiceIndex < 0 || choiceIndex >= nodeDictionary[CurrentNodeID].ChoiceList.Count)
            {
                Debug.LogError("Choice index out of range: " + choiceIndex);
                return;
            }
            CurrentNodeID = nodeDictionary[CurrentNodeID].ChoiceList[choiceIndex].NextNodeId;
            if (string.IsNullOrEmpty(CurrentNodeID) == false)
            {
                ShowCurrentNode();
            }
        }

        void ShowCurrentNode()
        {
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
                if (CurrentCharacterId != nodeData.CharacterId)
                {
                    CurrentCharacterId = nodeData.CharacterId;
                    CharacterChangedEvent?.Invoke(CurrentCharacterId);
                }

                TextStartEvent?.Invoke();
                dialogueText.text = "";
                yield return new WaitForSeconds(0.1f);
                dialogueText.text = DialogueContentProvider.GetText(nodeData.TextKey);
                yield return new WaitForSeconds(toNextWaitTime);
                TextEndEvent?.Invoke();
            }

            ShowChoices(nodeData.ChoiceList);
        }

        void ShowChoices(List<DialogueSystemChoiceSaveData> choices)
        {
            int choiceCount = 0;
            for (int i = 0; i < choices.Count; i++)
            {
                if (string.IsNullOrEmpty(choices[i].NextNodeId) == false)
                {
                    choiceCount++;
                }
            }

            RefreshChoiceWitchCountEvent?.Invoke(choiceCount);
            if (choiceCount > 0)
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
