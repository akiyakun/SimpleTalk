using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    [Serializable]
    public class DialogueSystemNodeEditorSaveData
    {
        [HideInInspector][SerializeField] public string Id;
        [HideInInspector][SerializeField] public string Name;
        [SerializeField] public string CharacterId;
        [SerializeField] public int TextBoxIndex;
        [SerializeField] public string TextKey;
        [SerializeField] public string Text;
        [SerializeField] public bool IsStartingNode;
        [SerializeField] public DialogueSystemNodeType NodeType;
        [HideInInspector][SerializeField] public Vector2 Position;
        [SerializeField] public List<DialogueSystemChoiceSaveData> ChoiceList;
    }
}