using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DS
{
    public class DialogueSystemView : MonoBehaviour
    {
        [SerializeField] TMP_Text dialogueText;
        [SerializeField] List<TMP_Text> choiceTextList;

        public string CurrentNodeID { get; private set; }
        public string CurrentCharacterId { get; private set; }
        public bool IsInited { get; private set; } = false;

        public IEnumerator ShowDialogueText(DialogueSystemNodeSaveData nodeData, IDialogueContent content,
            System.Action startEvent = null, System.Action endEvent = null, System.Action<int> refreshChoiceEvent = null,
            float showTextTime = 0.2f)
        {
            if (dialogueText == null)
            {
                Debug.LogError("Dialogue Text(TMP_Text) is null!");
                yield break;
            }
            else
            {
                startEvent?.Invoke();
                dialogueText.text = "";
                yield return new WaitForSeconds(0.1f);
                dialogueText.text = content.GetText(nodeData.TextKey);
                yield return new WaitForSeconds(showTextTime);
                endEvent?.Invoke();
            }

            ShowChoices(nodeData.ChoiceList, content, refreshChoiceEvent);
        }

        void ShowChoices(List<DialogueSystemChoiceSaveData> choices, IDialogueContent content, System.Action<int> refreshChoiceEvent)
        {
            int choiceCount = 0;
            for (int i = 0; i < choices.Count; i++)
            {
                if (string.IsNullOrEmpty(choices[i].NextNodeId) == false)
                {
                    choiceCount++;
                }
            }

            refreshChoiceEvent?.Invoke(choiceCount);
            if (choiceCount > 0)
            {
                for (int i = 0; i < choiceTextList.Count; i++)
                {
                    if (i < choices.Count)
                    {
                        choiceTextList[i].text = content.GetText(choices[i].TextKey);
                    }
                    else
                    {
                        choiceTextList[i].text = "";
                    }
                }
            }

        }

    }
}
