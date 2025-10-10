using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class DialogueSystemViewGroup : MonoBehaviour
    {
        [SerializeField] DialogueSystemContainerScriptableObject dialogueData;
        [SerializeField] List<DialogueSystemView> dialogueViewList;
        [SerializeField] float toNextWaitTime;

        Coroutine currentCoroutine;
        Dictionary<string, DialogueSystemNodeSaveData> nodeDictionary;

        public System.Action OnDialogueTextViewBeginEvent { get; set; }
        public System.Action OnDialogueTextViewEndEvent { get; set; }
        public System.Action<int> OnChangeTextViewWithIndexEvent { get; set; }
        public System.Action<int> OnRefreshChoiceWithCountEvent { get; set; }
        public IDialogueContent DialogueContentProvider { get; private set; }
        public string CurrentNodeID { get; private set; }
        public string CurrentCharacterId { get; private set; }
        public int CurrentTextBoxIndex { get; private set; }
        public bool IsInited { get; private set; }

        #region Unity ライフサイクル
        void OnDisable()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
        }
        #endregion

        #region 初期化
        public void Initialize(IDialogueContent dialogueContent)
        {
            IsInited = false;
            Debug.Assert(dialogueData != null, "Dialogue data is null!");
            Debug.Assert(dialogueViewList != null || dialogueViewList.Count > 0, "Dialogue view list is null or empty!");

            DialogueContentProvider = dialogueContent;

            if (DialogueContentProvider == null)
            {
                Debug.LogWarning("Dialogue content provider is null, creating default one.");
                DialogueContentProvider = CreateDefaultContentProvider();
            }

            CreateNodeDictionary();

            if (toNextWaitTime <= 0)
            {
                Debug.LogWarning("toNextWaitTime is 0, setting to default value 0.2f");
                toNextWaitTime = 0.2f;
            }

            IsInited = true;
        }

        IDialogueContent CreateDefaultContentProvider()
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

            return new DialogueContentProvider(contentDictionary);
        }

        void AddTextKeyToDictionary(Dictionary<string, string> dictionary, string key)
        {
            if (string.IsNullOrEmpty(key) == false && dictionary.ContainsKey(key) == false)
            {
                dictionary.Add(key, string.Empty);
            }
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
        #endregion

        #region 制御メソッド
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
                var startingNode = GetStartingNode();
                SetCurrentDataFromNodeData(startingNode);
            }
            else
            {
                Debug.LogWarning("Dialogue data contains no nodes.");
                return;
            }

            ShowCurrentNodeText();
        }

        public void StartDialogueFromBeginAuto(float waitTime, bool loop)
        {
            StopCurrentCoroutine();
            currentCoroutine = StartCoroutine(ShowCurrentNodeTextAuto(waitTime, loop));
        }

        IEnumerator ShowCurrentNodeTextAuto(float waitTime, bool loop)
        {
            Debug.Assert(IsInited, "Dialogue system is not initialized.");

            if (dialogueData == null)
            {
                Debug.LogError("Dialogue data is null!");
                yield return null;
            }

            var startingNodeId = string.Empty;
            if (dialogueData.NodeList.Count > 0)
            {
                var startingNode = GetStartingNode();
                SetCurrentDataFromNodeData(startingNode);
                startingNodeId = CurrentNodeID;
            }
            else
            {
                Debug.LogWarning("Dialogue data contains no nodes.");
                yield break;
            }

            var nextIds = GetNextNodeIds();
            while (nextIds.Count > 0)
            {
                ShowCurrentNodeText();
                yield return new WaitForSeconds(waitTime);

                nextIds = GetNextNodeIds();
                if (nextIds.Count > 0)
                {
                    CurrentNodeID = nextIds[0];
                }
                else if (loop)
                {
                    CurrentNodeID = startingNodeId;
                    nextIds = GetNextNodeIds();
                }
            }
        }

        public void StartDialogue(string id)
        {
            Debug.Assert(IsInited, "Dialogue system is not initialized.");
            var node = GetNodeById(id);
            if (node == null)
            {
                Debug.LogError("Node with ID " + id + " not found.");
                return;
            }
            SetCurrentDataFromNodeData(node);
            ShowCurrentNodeText();
        }

        public void StartDialogueWithChoice(int choiceIndex)
        {
            Debug.Assert(IsInited, "Dialogue system is not initialized.");
            if (choiceIndex < 0 || choiceIndex >= nodeDictionary[CurrentNodeID].ChoiceList.Count)
            {
                Debug.LogError("Choice index out of range: " + choiceIndex);
                return;
            }

            var currentNode = GetNodeById(CurrentNodeID);
            string nextNodeId = currentNode.ChoiceList[choiceIndex].NextNodeId;
            if (string.IsNullOrEmpty(nextNodeId))
            {
                Debug.LogWarning($"Next node ID is empty for choice {choiceIndex}");
                return;
            }

            var nextNode = GetNodeById(nextNodeId);
            if (nextNode == null)
            {
                Debug.LogError($"Next node with ID '{nextNodeId}' not found for choice {choiceIndex}");
                return;
            }

            SetCurrentDataFromNodeData(nextNode);
            ShowCurrentNodeText();
        }

        public List<string> GetNextNodeIds()
        {
            Debug.Assert(IsInited, "Dialogue system is not initialized.");

            var ids = new List<string>();
            var currentNode = GetNodeById(CurrentNodeID);
            if (currentNode == null)
            {
                Debug.LogError("Current node is null for ID: " + CurrentNodeID);
                return ids;
            }

            foreach (var choice in currentNode.ChoiceList)
            {
                if (!string.IsNullOrEmpty(choice.TextKey))
                {
                    ids.Add(choice.NextNodeId);
                }
            }

            return ids;
        }

        void ShowCurrentNodeText()
        {
            var currentNode = GetNodeById(CurrentNodeID);
            if (currentNode == null)
            {
                Debug.LogError("Current node is null for ID: " + CurrentNodeID);
                return;
            }

            var nextTextBoxIndex = GetTextBoxIndex(currentNode.TextBoxIndex);
            StopCurrentCoroutine();

            CurrentTextBoxIndex = nextTextBoxIndex;
            OnChangeTextViewWithIndexEvent?.Invoke(CurrentTextBoxIndex);

            currentCoroutine = StartCoroutine(dialogueViewList[CurrentTextBoxIndex].ShowDialogueText(
                nodeData: nodeDictionary[CurrentNodeID],
                content: DialogueContentProvider,
                startEvent: OnDialogueTextViewBeginEvent,
                endEvent: OnDialogueTextViewEndEvent,
                refreshChoiceEvent: OnRefreshChoiceWithCountEvent,
                showTextTime: toNextWaitTime));
        }

        int GetTextBoxIndex(int index)
        {
            if (index >= dialogueViewList.Count)
            {
                Debug.LogWarning($"TextBoxIndex out of range: Node ID {CurrentNodeID}, index {index}. Using last view.");
                return dialogueViewList.Count - 1;
            }
            else if (index < 0)
            {
                Debug.LogWarning($"TextBoxIndex is negative: Node ID {CurrentNodeID}, index {index}. Using first view.");
                return 0;
            }

            return index;
        }

        public Transform GetTextBoxransform(int index)
        {
            if (index >= dialogueViewList.Count || index < 0)
            {
                Debug.LogWarning($"TextBoxIndex out of range: index {index}. Using first view.");
                return null;
            }

            return dialogueViewList[index].transform;
        }
        #endregion

        #region ノードデータ取得
        DialogueSystemNodeSaveData GetStartingNode()
        {
            foreach (var node in dialogueData.NodeList)
            {
                if (node.IsStartingDialogue)
                {
                    return node;
                }
            }
            return dialogueData.NodeList.Count > 0 ? dialogueData.NodeList[0] : null;
        }

        DialogueSystemNodeSaveData GetNodeById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Node ID is null or empty");
                return null;
            }

            if (nodeDictionary.TryGetValue(id, out var node))
            {
                return node;
            }

            Debug.LogError($"Node with ID '{id}' not found");
            return null;
        }
        #endregion

        #region 通用メソッド
        void SetCurrentDataFromNodeData(DialogueSystemNodeSaveData node)
        {
            if (node == null)
            {
                Debug.LogError("Node data is null!");
                return;
            }

            CurrentNodeID = node.Id;
            CurrentTextBoxIndex = node.TextBoxIndex;
            CurrentCharacterId = node.CharacterId;
        }

        void StopCurrentCoroutine()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }
        }
        #endregion
    }
}
