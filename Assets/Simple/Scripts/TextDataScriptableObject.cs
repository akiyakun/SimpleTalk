using UnityEngine;

[CreateAssetMenu(fileName = "TextDataScriptableObject", menuName = "Scriptable Objects/TextDataScriptableObject")]
public class TextDataScriptableObject : ScriptableObject, DS.IDialogueContent
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

    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(key) == false)
        {
            Data textData = null;
            if (entityDictionary.TryGetValue(key, out textData))
            {
                return textData.Text;
            }

        }
        return key;
    }
}