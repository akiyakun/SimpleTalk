using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class DialogueNode : BaseNode
    {
        public string CharacterId { get; set; }
        public string TextId { get; set; }
        public string Motion { get; set; }
        public float Duration { get; set; }

        TextField characterIdTextField;
        TextField motionTextField;
        TextField textIdTextField;
        ObjectField textObjField;

        public DialogueNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Dialogue";
            SetPosition(new Rect(_position, defualtNodeSize));
            NodeGuid = System.Guid.NewGuid().ToString();

            // Add Better Title
            GenerateBetterTitle("Dialogue", " ");
            AddInputPort("Input", Port.Capacity.Multi);
            AddOutputPort("Output", Port.Capacity.Single);
            // Character ID
            characterIdTextField = new TextField("Character ID");
            characterIdTextField.RegisterValueChangedCallback(value =>
            {
                CharacterId = value.newValue;
            });
            characterIdTextField.SetValueWithoutNotify(CharacterId);
            mainContainer.Add(characterIdTextField);
            // Motion
            motionTextField = new TextField("Motion");
            motionTextField.RegisterValueChangedCallback(value =>
            {
                Motion = value.newValue;
            });
            motionTextField.SetValueWithoutNotify(Motion);
            mainContainer.Add(motionTextField);
            // Text ID
            textIdTextField = new TextField("Dialogue Text ID");
            textIdTextField.RegisterValueChangedCallback(value =>
            {
                TextId = value.newValue;
                //var entry = LocalizationSettings.StringDatabase.GetTableEntry("Test", TextId).Entry;
                //Debug.Log(entry.Value);
            });
            textIdTextField.SetValueWithoutNotify(TextId);
            mainContainer.Add(textIdTextField);

            RefreshExpandedState();         
            RefreshPorts();                 
            AddValidationContainer();       
        }

        public override void LoadValueInToField()
        {
            characterIdTextField.SetValueWithoutNotify(CharacterId);
            motionTextField.SetValueWithoutNotify(Motion);
            textIdTextField.SetValueWithoutNotify(TextId);
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected)
            {
                warning.Add("Node cannot be called");
            }

            Port output = outputContainer.Query<Port>().First();
            if (!output.connected)
            {
                error.Add("Output does not lead to any node");
            }

            ErrorList = error;
            WarningList = warning;
        }

        public DialogueNodeData SaveNodeData()
        {
            DialogueNodeData nodeData = new DialogueNodeData
            {
                NodeGuid = NodeGuid,
                Position = GetPosition().position,
                CharacterId = CharacterId,
                DialogueTextId = TextId,
                Motion = Motion,
                Duration = Duration
            };
            return nodeData;
        }

        // Create Node from Save data
        public static DialogueNode GenerateNode(DialogueNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            DialogueNode newNode = DialogueNode.CreateNewGraphNode(data.Position, editor, graph);
            newNode.NodeGuid = data.NodeGuid;
            newNode.CharacterId = data.CharacterId;
            newNode.Motion = data.Motion;
            newNode.TextId = data.DialogueTextId;
            newNode.Duration = data.Duration;
            newNode.LoadValueInToField();
            return newNode;
        }

        public static DialogueNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            DialogueNode node = new DialogueNode(_position, _editorWindow, _graphView);
            node.name = "Dialog";
            return node;
        }

    }
}
