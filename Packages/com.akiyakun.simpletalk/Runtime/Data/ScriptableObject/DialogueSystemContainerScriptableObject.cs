using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class DialogueSystemContainerScriptableObject : ScriptableObject
    {
        [SerializeField] public string FileName;
        [SerializeField] public List<DialogueSystemNodeSaveData> NodeList;

        public void Initialize(string fileName, List<DialogueSystemNodeSaveData> nodeList)
        {
            FileName = fileName;
            NodeList = new List<DialogueSystemNodeSaveData>(nodeList);
        }

    }
}
