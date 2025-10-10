using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    /// <summary>
    /// 外部制御なしでも自動的に対話を表示・進行するダイアログシステムコンポーネント
    /// </summary>
    public class DialogueSystemSingleViewGroupAuto : MonoBehaviour
    {
        [SerializeField] DialogueSystemContainerScriptableObject dialogueData;
        [SerializeField] DialogueSystemView dialogueView;
        [SerializeReference, SerializeInterface] IDialogueContent dialogueContentProvider;
        [SerializeField] float toNextWaitTime = 0.2f;
        [SerializeField] float autoAdvanceWaitTime = 2.0f;
        [SerializeField] bool playOnStart = true;
        [SerializeField] bool loop = false;

        Coroutine currentCoroutine;
        Dictionary<string, DialogueSystemNodeSaveData> nodeDictionary;

        public string CurrentNodeID { get; private set; }
        public string CurrentCharacterId { get; private set; }
        public int CurrentTextBoxIndex { get; private set; }
        public bool IsPlaying { get; private set; }

        #region Unity ライフサイクル
        private void Start()
        {
            if (playOnStart)
            {
                AutoInitializeAndPlay();
            }
        }

        private void OnDisable()
        {
            StopDialogue();
        }
        #endregion

        #region 制御メソッド
        public void AutoInitializeAndPlay()
        {
            if (CheckData() == false)
            {
                return;
            }

            CreateNodeDictionary();
            if (dialogueContentProvider == null)
            {
                Debug.LogWarning("DialogueSystemSingleViewGroupAuto: Dialogue content provider is null. Creating default provider.");
                CreateDefaultContentProvider();
            }
            StartAutoDialogue();
        }

        public void StopDialogue()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }
            IsPlaying = false;
        }
        #endregion

        #region 内部メソッド
        bool CheckData()
        {
            if (dialogueData == null)
            {
                Debug.LogError("DialogueSystemSingleViewGroupAuto: Dialogue data is null!");
                return false;
            }

            if (dialogueView == null)
            {
                Debug.LogError("DialogueSystemSingleViewGroupAuto: Dialogue view is null!");
                return false;
            }

            if (dialogueData.NodeList.Count == 0)
            {
                Debug.LogWarning("DialogueSystemSingleViewGroupAuto: Dialogue data contains no nodes.");
                return false;
            }

            return true;
        }

        void CreateNodeDictionary()
        {
            nodeDictionary = new Dictionary<string, DialogueSystemNodeSaveData>();

            foreach (var node in dialogueData.NodeList)
            {
                if (!nodeDictionary.ContainsKey(node.Id))
                {
                    nodeDictionary.Add(node.Id, node);
                }
            }
        }

        void CreateDefaultContentProvider()
        {
            var contentDictionary = new Dictionary<string, string>();

            foreach (var node in dialogueData.NodeList)
            {
                AddTextKeyToDictionary(contentDictionary, node.TextKey);

                foreach (var choice in node.ChoiceList)
                {
                    if (!string.IsNullOrEmpty(choice.TextKey))
                    {
                        AddTextKeyToDictionary(contentDictionary, choice.TextKey);
                    }
                }
            }

            dialogueContentProvider = new DialogueContentProvider(contentDictionary);
        }

        void AddTextKeyToDictionary(Dictionary<string, string> dictionary, string key)
        {
            if (string.IsNullOrEmpty(key) == false && dictionary.ContainsKey(key) == false)
            {
                dictionary.Add(key, string.Empty);
            }
        }

        DialogueSystemNodeSaveData GetStartingNode()
        {
            foreach (var node in dialogueData.NodeList)
            {
                if (node.IsStartingDialogue)
                {
                    return node;
                }
            }

            return dialogueData.NodeList[0];
        }

        void StartAutoDialogue()
        {
            StopDialogue();
            currentCoroutine = StartCoroutine(AutoPlayDialogue());
            IsPlaying = true;
        }

        IEnumerator AutoPlayDialogue()
        {
            var startingNode = GetStartingNode();
            CurrentNodeID = startingNode.Id;
            CurrentCharacterId = startingNode.CharacterId;
            CurrentTextBoxIndex = startingNode.TextBoxIndex;

            string startingNodeId = CurrentNodeID;
            bool continueDialogue = true;

            while (continueDialogue)
            {
                var currentNode = nodeDictionary[CurrentNodeID];
                if (currentNode.NodeType == DialogueSystemNodeType.MultipleChoice)
                {
                    Debug.LogError($"DialogueSystemSingleViewGroupAuto: Node '{CurrentNodeID}' is MultipleChoice type which is not supported in auto mode.");
                    yield break;
                }

                yield return StartCoroutine(dialogueView.ShowDialogueText(
                    nodeData: currentNode,
                    content: dialogueContentProvider,
                    startEvent: null,
                    endEvent: null,
                    refreshChoiceEvent: null,
                    showTextTime: toNextWaitTime));

                yield return new WaitForSeconds(autoAdvanceWaitTime);

                if (currentNode.ChoiceList.Count > 0 && !string.IsNullOrEmpty(currentNode.ChoiceList[0].NextNodeId))
                {
                    CurrentNodeID = currentNode.ChoiceList[0].NextNodeId;

                    if (nodeDictionary.TryGetValue(CurrentNodeID, out var nextNode))
                    {
                        CurrentCharacterId = nextNode.CharacterId;
                    }
                    else
                    {
                        Debug.LogError($"DialogueSystemSingleViewGroupAuto: Next node '{CurrentNodeID}' not found.");
                        continueDialogue = false;
                    }
                }
                else
                {
                    if (loop)
                    {
                        CurrentNodeID = startingNodeId;
                        if (nodeDictionary.TryGetValue(CurrentNodeID, out var loopNode))
                        {
                            CurrentCharacterId = loopNode.CharacterId;
                        }
                    }
                    else
                    {
                        continueDialogue = false;
                    }
                }
            }

            IsPlaying = false;
        }

        #endregion
    }
}