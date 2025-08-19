using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleUI : MonoBehaviour
{
    [SerializeField] DS.DialogueSystemInspector dialogueSystemInspector;
    [SerializeField] TextDataScriptableObject dialogueContentProvider;
    [SerializeField] List<Button> choiceButtonList;

    void Start()
    {
        if (dialogueSystemInspector == null || dialogueContentProvider == null)
        {
            return;
        }
        dialogueSystemInspector.Initialize(dialogueContentProvider);
        dialogueSystemInspector.RefreshChoiceWitchCountEvent += RefreshChoice;
        InitButtonOnClick();
        RefreshChoice(0);
        dialogueSystemInspector.StartDialogueFromBegin();
    }

    void OnDestroy()
    {
        if (dialogueSystemInspector != null)
        {
            dialogueSystemInspector.RefreshChoiceWitchCountEvent -= RefreshChoice;
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
                dialogueSystemInspector.StartDialogueWithChoice(index);
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
