using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue")]
    [System.Serializable]
    public class DialogueContainerSO : ScriptableObject
    {
        public List<StartNodeData> StartNodeDatas = new List<StartNodeData>();
        public List<EndNodeData> EndNodeDatas = new List<EndNodeData>();
        public List<DialogueNodeData> DialogueNodeDatas = new List<DialogueNodeData>();
        public List<DialogueChoiceNodeData> DialogueChoiceNodeDatas = new List<DialogueChoiceNodeData>();
        public List<IfNodeData> IfNodeDatas = new List<IfNodeData>();
    }
}
