using System.Collections.Generic;

namespace DS
{
    public class DialogueContentProvider : IDialogueContent
    {
        readonly Dictionary<string, string> dataDictionary;

        public DialogueContentProvider(Dictionary<string, string> dict)
        {
            dataDictionary = dict;
        }

        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }
            return dataDictionary.TryGetValue(key, out var value) ? value : string.Empty;
        }

    }
}
