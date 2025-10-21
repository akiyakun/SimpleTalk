using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class DialogueChoiceNode : BaseNode
    {
        public string TextId { get; set; }
        public string CharacterId { get; set; }
        public string Motion { get; set; }
        public float DurationShow { get; set; }

        public List<string> ChoiceTextIdList { get; set; } = new List<string>();
        public List<DialogueNodePort> DialogueNodePortList { get; set; } = new List<DialogueNodePort>();

        TextField textIdTextField;
        TextField characterIdTextField;
        TextField motionTextField;
        FloatField durationField;
        Button addChoiceButton;

        public DialogueChoiceNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;
            title = "Dialogue Choice";
            NodeGuid = Guid.NewGuid().ToString();

            AddInputPort("Input ", Port.Capacity.Multi);
            GenerateBetterTitle("Dialogue Choice");
            SetPosition(new Rect(_position, defualtNodeSize));
            AddValidationContainer();

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
            mainContainer.Add(motionTextField);
            // Text ID
            textIdTextField = new TextField("Dialogue Text ID");
            textIdTextField.RegisterValueChangedCallback(value =>
            {
                TextId = value.newValue;
            });
            textIdTextField.SetValueWithoutNotify(TextId);
            mainContainer.Add(textIdTextField);
            // Duration
            durationField = new FloatField("Display Time");
            durationField.RegisterValueChangedCallback((EventCallback<ChangeEvent<float>>)(value =>
            {
                DurationShow = value.newValue;
            }));
            durationField.SetValueWithoutNotify(DurationShow);
            durationField.AddToClassList("TextDuration");
            mainContainer.Add(durationField);
            // Add Choice Button
            addChoiceButton = new Button()
            {
                text = "+ Add Choice Option"
            };
            addChoiceButton.clicked += () =>
            {
                AddChoicePort(this);
            };
            NewTitleBox.Add(addChoiceButton);

            UpdatePortraits();
        }

        public override void LoadValueInToField()
        {
            UpdatePortraits();
            textIdTextField.SetValueWithoutNotify(TextId);
            durationField.SetValueWithoutNotify(DurationShow);
            characterIdTextField.SetValueWithoutNotify(CharacterId);
            motionTextField.SetValueWithoutNotify(Motion);
        }

        public Port AddChoicePort(BaseNode node, DialogueNodePort nodePort = null)
        {
            Port port = GetPortInstance(Direction.Output, Port.Capacity.Single, typeof(float));
            string outputPortName = "";
            int outputPortCount = node.outputContainer.Query("connector").ToList().Count();
            if (outputPortCount < 9)
            {
                outputPortName = $"Choice 0{outputPortCount + 1}";
            }
            else
            {
                outputPortName = $"Choice {outputPortCount + 1}";
            }

            DialogueNodePort dialogueNodePort = new DialogueNodePort();
            dialogueNodePort.PortGuid = Guid.NewGuid().ToString();

            if (nodePort != null)
            {
                dialogueNodePort.InputGuid = nodePort.InputGuid;
                dialogueNodePort.OutputGuid = nodePort.OutputGuid;

                if (nodePort.PortGuid == "")
                {
                    nodePort.PortGuid = Guid.NewGuid().ToString();
                }
                dialogueNodePort.PortGuid = nodePort.PortGuid;

            }

            TextField choiceTextField = new TextField("Choice Text Id");
            choiceTextField.RegisterValueChangedCallback(value =>
            {
                dialogueNodePort.TextId = value.newValue;
            });

            port.contentContainer.Add(choiceTextField);

            Button deleteButton = new Button(() => DeleteButton(node, port))
            {
                text = "X"
            };
            port.contentContainer.Add(deleteButton);

            port.portName = "";
            DialogueNodePortList.Add(dialogueNodePort);
            ChoiceTextIdList.Add("");
            node.outputContainer.Add(port);

            node.RefreshPorts();
            node.RefreshExpandedState();
            addChoiceButton.SetEnabled(true);
            addChoiceButton.text = UpdateButtonText();

            return port;
        }

        private void DeleteButton(BaseNode node, Port port)
        {
            IEnumerable<Edge> portEdge = graphView.edges.ToList().Where(edge => edge.output == port);

            if (portEdge.Any())
            {
                Edge edge = portEdge.First();
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }

            node.outputContainer.Remove(port);
            node.RefreshPorts();
            node.RefreshExpandedState();

            addChoiceButton.SetEnabled(true);
            addChoiceButton.text = UpdateButtonText();
        }

        public static DialogueChoiceNode CreateNewGraphNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            DialogueChoiceNode node = new DialogueChoiceNode(_position, _editorWindow, _graphView); // Create the StartNode instance.
            node.name = "Choice";
            return node;
        }

        public static DialogueChoiceNode GenerateNode(DialogueChoiceNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            DialogueChoiceNode node = DialogueChoiceNode.CreateNewGraphNode(data.Position, editor, graph);
            node.NodeGuid = data.NodeGuid;

            foreach (DialogueNodePort nodePort in data.DialogueNodePorts)
            {
                node.AddChoicePort(node, nodePort);
            }

            node.CharacterId = data.CharacterId;
            node.TextId = data.DialogueTextId;
            node.Motion = data.Motion;
            node.DurationShow = data.Duration;
            node.ChoiceTextIdList = data.ChoiceTextIdList;
            node.LoadValueInToField();

            return node;

        }
        public DialogueChoiceNodeData SaveNodeData(List<Edge> edges)
        {
            DialogueChoiceNodeData dialogueNodeData = new DialogueChoiceNodeData
            {
                NodeGuid = NodeGuid,
                Position = GetPosition().position,
                CharacterId = CharacterId,
                DialogueTextId = TextId,
                Motion = Motion,
                ChoiceTextIdList = ChoiceTextIdList,
                Duration = DurationShow,
                DialogueNodePorts = DialogueNodePortList
            };

            foreach (DialogueNodePort nodePort in dialogueNodeData.DialogueNodePorts)
            {
                nodePort.OutputGuid = string.Empty;
                nodePort.InputGuid = string.Empty;
                foreach (Edge edge in edges)
                {
                    //if (edge.output == nodePort.MyPort)
                    //{
                    //    nodePort.OutputGuid = (edge.output.node as BaseNode).NodeGuid;
                    //    nodePort.InputGuid = (edge.input.node as BaseNode).NodeGuid;
                    //}
                }
            }

            return dialogueNodeData;
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (!input.connected) warning.Add("Node cannot be called");
            if (DialogueNodePortList.Count < 1) error.Add("You need to add more Choice");
            else
            {
                for (int i = 0; i < DialogueNodePortList.Count; i++)
                {
                    //if (!DialogueNodePortList[i].MyPort.connected)
                    //{
                    //    error.Add($"Choice ID:{i} does not lead to any node");
                    //}
                        
                }
            }
            for (int i = 0; i < ChoiceTextIdList.Count; i++)
            {
                // todo
            }

            ErrorList = error;
            WarningList = warning;
        }

        public void UpdatePortraits()
        {

        }

        string UpdateButtonText()
        {
            return "+ Add New Choice";
        }

        public override void UpdateNodeUI()
        {
            addChoiceButton.text = UpdateButtonText();
        }
    }
}