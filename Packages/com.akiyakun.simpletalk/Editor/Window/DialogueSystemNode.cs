using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS
{
    public class DialogueSystemNode : Node
    {
        public string Id { get; set; }
        public string DialogueName { get; set; }
        public string TextKey { get; set; }
        public string Text { get; set; }
        public List<DialogueSystemChoiceSaveData> ChoiceList { get; set; }
        public DialogueSystemNodeType NodeType { get; set; }

        protected DialogueSystemGraphView graphView;

        Label textLabel;
        Color defaultBackgroundColor;
        Color errorBackgroundColor;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
            base.BuildContextualMenu(evt);
        }

        public virtual void Initialize(DialogueSystemGraphView dsGraphView, Vector2 position)
        {
            Id = Guid.NewGuid().ToString();
            DialogueName = "New Name";
            Text = "New Text";
            ChoiceList = new List<DialogueSystemChoiceSaveData>();
            NodeType = DialogueSystemNodeType.SingleChoice;
            graphView = dsGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);
            errorBackgroundColor = new Color(65f / 255f, 50f / 255f, 50f / 255f);
            SetPosition(new Rect(position, Vector2.zero));
            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public virtual void Draw()
        {
            // Title
            var titleLabel = DialogueSystemElementUtility.CreateLabel(DialogueName);
            titleLabel.AddToClassList("ds-node__label-text");
            titleContainer.Insert(0, titleLabel);
            // Button
            var previewButton = DialogueSystemElementUtility.CreateButton("Preview", () => TextPreview());
            //previewButton.AddToClassList("ds-node__button");
            titleButtonContainer.Insert(0, previewButton);
            // Input Port
            var inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Dialogue Connetion";
            inputContainer.Add(inputPort);
            // Custom Element
            var customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");
            var textFoldout = DialogueSystemElementUtility.CreateFoldout("Dialogue Text");
            var textField = DialogueSystemElementUtility.CreateTextField(TextKey, "", (callback) =>
            {
                TextKey = callback.newValue;
            });
            textField.AddToClassList("ds-node__text-field");
            textFoldout.Add(textField);
            customDataContainer.Add(textField);
            textLabel = DialogueSystemElementUtility.CreateLabel(Text);
            textLabel.AddToClassList("ds-node__label-contents-text");
            textFoldout.Add(textLabel);
            customDataContainer.Add(textLabel);
            extensionContainer.Add(customDataContainer);
            //RefreshExpandedState();
        }

        public virtual void TextPreview()
        {
            if (string.IsNullOrEmpty(DialogueSystemSaveUtility.TxtDataPath) == false)
            {
                var txtfile = AssetDatabase.LoadAssetAtPath<TextDataScriptableObject>(DialogueSystemSaveUtility.ConvertPathToRelative(DialogueSystemSaveUtility.TxtDataPath));
                if (txtfile != null)
                {
                    textLabel.text = txtfile.Get(TextKey).Text;
                }
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();
            return !inputPort.connected;
        }

        public void SetErrorColorStyle()
        {
            mainContainer.style.backgroundColor = errorBackgroundColor;
        }

        public void ResetColorStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

    }
}
