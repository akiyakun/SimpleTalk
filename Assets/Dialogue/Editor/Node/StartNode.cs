using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class StartNode : BaseNode
    {
        public string StartId { get; set; }
        private TextField idField;

        public StartNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;
            title = "Dialogue Start";
            SetPosition(new Rect(_position, defualtNodeSize));
            NodeGuid = System.Guid.NewGuid().ToString();

            AddOutputPort("Output", Port.Capacity.Single);
            GenerateBetterTitle("Dialogue Start", "");

            idField = new TextField("Start ID"); 
            idField.RegisterValueChangedCallback(value =>
            {
                StartId = value.newValue;
            });
            idField.SetValueWithoutNotify(StartId);
            idField.AddToClassList("unity-text-element");
            mainContainer.Add(idField);

            elementTypeColor = new Color(0.2f, 0.6f, 0.3f, 0.2f);

            RefreshExpandedState();
            RefreshPorts();
            AddValidationContainer();
        }

        public override void LoadValueInToField()
        {
            idField.SetValueWithoutNotify(StartId);
        }

        public override void SetValidation()
        {
            // Reset validation lists for errors and warnings.
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            // Error: Check if the output port is connected to another node.
            Port port = outputContainer.Query<Port>().First(); // Retrieve the output port.
            if (port.connected == false)
            {
                error.Add("Output does not lead to any node"); // Add error if no connection exists.
            }
            // Assign validation results to the node's error and warning lists.
            ErrorList = error;
            WarningList = warning;
        }

        public static StartNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            StartNode node = new StartNode(_position, _editorWindow, _graphView);
            node.name = "Start";
            return node;
        }

        // Create Node from Save data
        public static StartNode GenerateNode(StartNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            StartNode newNode = StartNode.CreateNewGraphNode(data.Position, editor, graph);
            newNode.NodeGuid = data.NodeGuid;
            newNode.StartId = data.StartId;
            newNode.LoadValueInToField();
            return newNode;
        }

        public StartNodeData SaveNodeData()
        {
            StartNodeData nodeData = new StartNodeData
            {
                NodeGuid = NodeGuid,
                Position = GetPosition().position,
                StartId = StartId,
            };
            return nodeData;
        }
    }
}
