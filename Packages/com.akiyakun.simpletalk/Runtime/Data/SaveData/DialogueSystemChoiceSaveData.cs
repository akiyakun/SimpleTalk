using System;
using UnityEngine;

namespace DS
{
    [Serializable]
    public class DialogueSystemChoiceSaveData
    {
        [SerializeField] public string NextNodeId;
        [SerializeField] public string TextKey;
        [SerializeField] public string Text;

        public void Initialize(string NextId, string textKey)
        {
            NextNodeId = NextId;
            TextKey = textKey;
        }
    }
}