using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace DialogueSystem
{
    public class EndNode : BaseNode
    {
        public EndNodeType EndNodeType { get; set; }
        public string StartID { get; set; }
        public DialogueContainerSO dialogueSO;

        // Node Fields
        private EnumField enumField;
        private TextField idField;
        private Label dialogLabel;
        private ObjectField dialogField;

        public EndNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;
            title = "Dialogue End";

            SetPosition(new Rect(_position, defualtNodeSize));
            NodeGuid = System.Guid.NewGuid().ToString();
            GenerateBetterTitle("Dialogue End", "");
            AddInputPort("Input", Port.Capacity.Multi);
            // Label
            Label enumLabel = new Label("Dialogue End Type");
            enumLabel.AddToClassList("label_name"); 
            enumLabel.AddToClassList("Label"); 
            extensionContainer.Add(enumLabel);
            // Enum Field
            enumField = new EnumField() { value = EndNodeType };
            enumField.Init(EndNodeType);
            enumField.RegisterValueChangedCallback((value) =>
            {
                EndNodeType = (EndNodeType)value.newValue;
                UpdateFieldDisplay();
            });
            enumField.SetValueWithoutNotify(EndNodeType); 
            mainContainer.Add(enumField);
            // Dialogue Label
            dialogLabel = new Label("Next Dialogue");
            dialogLabel.AddToClassList("label_name");
            dialogLabel.AddToClassList("Label");
            // DialogField
            dialogField = new ObjectField()
            {
                objectType = typeof(DialogueContainerSO),
                allowSceneObjects = false,
                value = null,
            };
            dialogField.RegisterValueChangedCallback(value =>
            {
                dialogueSO = value.newValue as DialogueContainerSO;
            });
            dialogField.SetValueWithoutNotify(dialogueSO);
            dialogField.AddToClassList("EndDialogue");
            mainContainer.Add(dialogField);
            // Start ID Field
            idField = new TextField("Start ID");
            idField.RegisterValueChangedCallback(value =>
            {
                StartID = value.newValue;
            });
            idField.SetValueWithoutNotify(StartID);
            idField.AddToClassList("canCollapse");
            mainContainer.Add(idField);

            RefreshExpandedState();
            RefreshPorts();
            AddValidationContainer();
            UpdateFieldDisplay();
        }

        public override void LoadValueInToField()
        {
            enumField.SetValueWithoutNotify(EndNodeType);
            dialogField.SetValueWithoutNotify(dialogueSO);
            idField.SetValueWithoutNotify(StartID);

            UpdateFieldDisplay();
        }

        public override void SetValidation()
        {
            // Reset validation lists for errors and warnings.
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            // Warning: Check if the output port is connected to another node.
            Port input = inputContainer.Query<Port>().First();
            if (input.connected == false)
            {
                warning.Add("Node cannot be called");
            }

            // Error: Nie przypisane Dialogue Container SO
            if (EndNodeType == EndNodeType.NextDialogue && dialogueSO == null)
            {
                error.Add("Dialogue SO is Empty!");
            }

            // Assign validation results to the node's error and warning lists.
            ErrorList = error;
            WarningList = warning;
        }

        // Create from Save Data
        public static EndNode GenerateNode(EndNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            EndNode newNode = EndNode.CreateNewGraphNode(data.Position, editor, graph);
            newNode.NodeGuid = data.NodeGuid;
            newNode.EndNodeType = data.EndNodeType;
            newNode.StartID = data.StartId;
            newNode.dialogueSO = data.Dialogue;
            newNode.LoadValueInToField();
            return newNode;
        }


        public static EndNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            EndNode node = new EndNode(_position, _editorWindow, _graphView);
            node.name = "End";
            return node;
        }

        public EndNodeData SaveNodeData()
        {
            EndNodeData nodeData = new EndNodeData
            {
                NodeGuid = NodeGuid,
                Position = GetPosition().position,
                EndNodeType = EndNodeType,
                StartId = StartID,
                Dialogue = dialogueSO
            };
            return nodeData;
        }

        void UpdateFieldDisplay()
        {
            if (EndNodeType == EndNodeType.ReturnToStart)
            {
                idField.style.display = DisplayStyle.Flex;
                dialogField.style.display = DisplayStyle.None;
            }
            else if (EndNodeType == EndNodeType.NextDialogue)
            {
                idField.style.display = DisplayStyle.Flex;
                dialogField.style.display = DisplayStyle.Flex;
            }
            else
            {
                idField.style.display = DisplayStyle.None;
                dialogField.style.display = DisplayStyle.None;
            }
        }
    }
}
