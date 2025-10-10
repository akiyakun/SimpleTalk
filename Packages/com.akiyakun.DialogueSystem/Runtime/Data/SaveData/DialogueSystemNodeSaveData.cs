using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    [Serializable]
    public class DialogueSystemNodeSaveData
    {
        [SerializeField] public string Id;
        [SerializeField] public string CharacterId;
        [SerializeField] public int TextBoxIndex;
        [SerializeField] public string TextKey;
        [SerializeField] public bool IsStartingDialogue;
        [SerializeField] public DialogueSystemNodeType NodeType;
        [SerializeField] public List<DialogueSystemChoiceSaveData> ChoiceList;

        public void Initialize(
            string id,
            string characterId,
            int textBoxIndex,
            string textKey,
            bool isStartingDialogue,
            DialogueSystemNodeType nodeType,
            List<DialogueSystemChoiceSaveData> choices)
        {
            Id = id;
            CharacterId = characterId;
            TextBoxIndex = textBoxIndex;
            TextKey = textKey;
            ChoiceList = choices;
            NodeType = nodeType;
            IsStartingDialogue = isStartingDialogue;
        }
    }
}
