using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DS
{
    public interface IDialogueSystemUI
    {
        void ShowDialogueText(string text);
        void ShowChoices(List<string> choiceTexts);

    }
}

