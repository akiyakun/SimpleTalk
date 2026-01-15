using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SampleSingleUI : MonoBehaviour
{
    [SerializeField] DS.DialogueSystemViewGroup viewGroup;
    [SerializeField] TextDataScriptableObject dialogueContentProvider;
    [SerializeField] List<Button> choiceButtonList;

    void Start()
    {
        if (viewGroup == null || dialogueContentProvider == null)
        {
            return;
        }
        viewGroup.Initialize(dialogueContentProvider);
        viewGroup.OnRefreshChoiceWithCountEvent += RefreshChoice;
        InitButtonOnClick();
        RefreshChoice(0);
        viewGroup.StartDialogueFromBegin();
    }

    void OnDestroy()
    {
        if (viewGroup != null)
        {
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

    void RefreshChoice(int count)
    {
        for (int i = 0; i < choiceButtonList.Count; i++)
        {
            choiceButtonList[i].gameObject.SetActive(i < count);
        }
    }

}
