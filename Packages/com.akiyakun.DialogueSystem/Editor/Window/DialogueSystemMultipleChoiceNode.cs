using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS
{
    public class DialogueSystemMultipleChoiceNode : DialogueSystemNode
    {
        public override void Initialize(DialogueSystemGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(dsGraphView, position);
            DialogueName = "Multiple Choice";
            NodeType = DialogueSystemNodeType.MultipleChoice;
            ChoiceList.Add(new DialogueSystemChoiceSaveData() { TextKey = "New Choice Key", Text = "New Choice Text" });
        }

        public override void Draw()
        {
            base.Draw();
            Button addButton = DialogueSystemElementUtility.CreateButton("Add Choice", () =>
            {
                var data = new DialogueSystemChoiceSaveData() { TextKey = "New Choice Key", Text = "New Choice Text" };
                var port = CreateChoicePort(data);
                ChoiceList.Add(data);
                outputContainer.Add(port);
            });
            addButton.AddToClassList("ds-node__button");
            mainContainer.Insert(1, addButton);

            foreach (var choice in ChoiceList)
            {
                var choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }

        Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();
            choicePort.userData = userData;
            var choiceData = (DialogueSystemChoiceSaveData)userData;
            Button deleteButton = DialogueSystemElementUtility.CreateButton("X", () =>
            {
                if (ChoiceList.Count == 1)
                {
                    return;
                }

                if (choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }

                ChoiceList.Remove(choiceData);
                graphView.RemoveElement(choicePort);
            });
            deleteButton.AddToClassList("ds-node__button");
            TextField choiceTextField = DialogueSystemElementUtility.CreateTextField(choiceData.TextKey, "", (callback) =>
            {
                choiceData.TextKey = callback.newValue;
            });
            choiceTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__choice-text-field"
            );
            var choiceTextLabel = DialogueSystemElementUtility.CreateLabel(choiceData.Text);
            choiceTextLabel.AddToClassList("ds-node__label-text");
            choicePort.Add(choiceTextLabel);
            choicePort.Add(choiceTextField);
            choicePort.Add(deleteButton);
            return choicePort;
        }

    }
}
