using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DS
{
    public class SampleMultipleUI : MonoBehaviour
    {
        [SerializeField] DS.DialogueSystemViewGroup viewGroup;
        [SerializeField] TextDataScriptableObject dialogueContentProvider;
        [SerializeField] RectTransform choiceButtonParent;
        [SerializeField] List<Button> choiceButtonList;

        void Start()
        {
            if (viewGroup == null || dialogueContentProvider == null)
            {
                return;
            }
            viewGroup.Initialize(dialogueContentProvider);
            viewGroup.OnChangeTextViewWithIndexEvent += RefreshChoicePosition;
            viewGroup.OnRefreshChoiceWithCountEvent += RefreshChoice;
            InitButtonOnClick();
            RefreshChoice(0);
            viewGroup.StartDialogueFromBegin();
        }

        void OnDestroy()
        {
            if (viewGroup != null)
            {
                viewGroup.OnChangeTextViewWithIndexEvent -= RefreshChoicePosition;
                viewGroup.OnRefreshChoiceWithCountEvent -= RefreshChoice;
            }
        }

        void InitButtonOnClick()
        {
            for (int i = 0; i < choiceButtonList.Count; i++)
            {
                int index = i;
                choiceButtonList[i].onClick.RemoveAllListeners();
                choiceButtonList[i].onClick.AddListener(() =>
                {
                    RefreshChoice(0);
                    viewGroup.StartDialogueWithChoice(index);
                });
            }

        }

        void RefreshChoicePosition(int textBoxIndex)
        {
            if (choiceButtonParent == null || choiceButtonList.Count == 0)
            {
                return;
            }
            var currentTextBox = viewGroup.GetTextBoxransform(textBoxIndex);
            choiceButtonParent.SetParent(currentTextBox, false);
            choiceButtonParent.localPosition = new Vector3(choiceButtonParent.sizeDelta.x, -choiceButtonParent.sizeDelta.y, 0);
        }

        void RefreshChoice(int count)
        {
            for (int i = 0; i < choiceButtonList.Count; i++)
            {
                choiceButtonList[i].gameObject.SetActive(i < count);
            }
        }
    }
}
