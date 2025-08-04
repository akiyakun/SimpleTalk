using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class DialogueSystemEditorScriptableObject : ScriptableObject
    {
        [SerializeField] public string FileName;
        [SerializeField] public string TxtDataPath;
        [SerializeField] public List<DialogueSystemNodeEditorSaveData> NodeList;

        public void Initialize(string fileName, string txtDataPath)
        {
            FileName = fileName;
            TxtDataPath = txtDataPath;
            NodeList = new List<DialogueSystemNodeEditorSaveData>();
        }

    }
}