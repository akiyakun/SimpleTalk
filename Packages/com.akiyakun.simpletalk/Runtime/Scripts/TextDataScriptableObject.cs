using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    [CreateAssetMenu(fileName = "TextDataScriptableObject", menuName = "Scriptable Objects/TextDataScriptableObject")]
    public class TextDataScriptableObject : ScriptableObject
    {
        [System.Serializable]
        public class Data
        {
            public string Key;
            public string Text;
        }

        [SerializeField] SerializableDictionary<string, Data> entityDictionary = new SerializableDictionary<string, Data>();

        public Data Get(string key)
        {
            if (entityDictionary == null)
            {
                entityDictionary = new SerializableDictionary<string, Data>();
            }
            if (entityDictionary.ContainsKey(key) == false)
            {
                return new Data { Text = key };
            }
            return entityDictionary[key];
        }
    }
}
