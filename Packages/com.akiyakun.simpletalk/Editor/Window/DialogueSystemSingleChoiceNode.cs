using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace DS
{
    public class DialogueSystemSingleChoiceNode : DialogueSystemNode
    {
        public override void Initialize(DialogueSystemGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(dsGraphView, position);
            DialogueName = "Single Choice";
            NodeType = DialogueSystemNodeType.SingleChoice;
            ChoiceList.Add((new DialogueSystemChoiceSaveData() { TextKey = "Next Dialogue" }));
        }

        public override void Draw()
        {
            base.Draw();
            foreach (var choice in ChoiceList)
            {
                Port choicePort = this.CreatePort(choice.TextKey);
                choicePort.userData = choice;
                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }

    }
}
