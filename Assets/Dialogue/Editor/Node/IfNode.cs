using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class IfNode : BaseNode
    {
        public GlobalValueIFOperations Operations { get; set; }
        public string OperationValue { get; set; }
        public string ValueName { get; set; }

        DropdownField valueNameField;
        EnumField operationField;
        TextField valueField;

        public IfNode(Vector2 _position, DialogueEditorWindow _editorWindow, DialogueGraphView _graphView)
        {
            editorWindow = _editorWindow;
            graphView = _graphView;

            title = "Branch";
            SetPosition(new Rect(_position, defualtNodeSize));
            NodeGuid = System.Guid.NewGuid().ToString();

            GenerateBetterTitle("Branch");
            AddInputPort("Input ", Port.Capacity.Multi);
            AddOutputPort("True", "True", Port.Capacity.Single);
            AddOutputPort("False", "False", Port.Capacity.Single);
            AddValidationContainer();

            GlobalValueSO valueSO = AssetDatabase.LoadAssetAtPath<GlobalValueSO>("Assets/Dialogue/GlobalValue.asset");
            valueSO.LoadValues();

            List<string> valueNames = new List<string>();
            valueNames.AddRange(valueSO.IntValues.Select(intValue => intValue.ValueName));
            valueNames.AddRange(valueSO.FloatValues.Select(floatValue => floatValue.ValueName));
            valueNames.AddRange(valueSO.BoolValues.Select(boolValue => boolValue.ValueName));
            // Value Name Field
            valueNameField = new DropdownField(label: "Global Value", choices: (valueNames.Distinct().ToList()), defaultIndex: 0);
            valueNameField.RegisterValueChangedCallback(value =>
            {
                ValueName = value.newValue;
                bool isValueNameInIntValues = valueSO.IntValues.Any(intValue => intValue.ValueName == ValueName);
                bool isValueNameInFloatValues = valueSO.FloatValues.Any(floatValue => floatValue.ValueName == ValueName);
                operationField.style.display = isValueNameInIntValues || isValueNameInFloatValues ? DisplayStyle.Flex : DisplayStyle.None;
                valueField.style.display = isValueNameInIntValues || isValueNameInFloatValues ? DisplayStyle.Flex : DisplayStyle.None;
            });
            valueNameField.SetValueWithoutNotify(ValueName);
            mainContainer.Add(valueNameField);
            // Operation Enum Field
            operationField = new EnumField("Operation", Operations);
            operationField.RegisterValueChangedCallback(value =>
            {
                Operations = (GlobalValueIFOperations)value.newValue;
            });
            operationField.SetValueWithoutNotify(Operations);
            mainContainer.Add(operationField);
            // Value Field
            valueField = new TextField("Value");
            valueField.RegisterValueChangedCallback(value =>
            {
                OperationValue = value.newValue;
            });
            valueField.SetValueWithoutNotify(OperationValue);
            valueField.multiline = true;
            mainContainer.Add(valueField);
        }

        public override void LoadValueInToField()
        {
            GlobalValueSO valueSO = AssetDatabase.LoadAssetAtPath<GlobalValueSO>("Dialogue/GlobalValue");
            valueSO.LoadValues();

            bool isValueNameInIntValues = valueSO.IntValues.Any(intValue => intValue.ValueName == ValueName);
            bool isValueNameInFloatValues = valueSO.FloatValues.Any(floatValue => floatValue.ValueName == ValueName);

            operationField.style.display = isValueNameInIntValues || isValueNameInFloatValues ? DisplayStyle.Flex : DisplayStyle.None;
            valueField.style.display = isValueNameInIntValues || isValueNameInFloatValues ? DisplayStyle.Flex : DisplayStyle.None;

            operationField.SetValueWithoutNotify(Operations);
            valueField.SetValueWithoutNotify(OperationValue);
            valueNameField.SetValueWithoutNotify(ValueName);
        }

        public static IfNode CreateNewGraphNode(Vector2 position, DialogueEditorWindow editorWindow, DialogueGraphView graphView)
        {
            IfNode node = new IfNode(position, editorWindow, graphView);
            node.name = "IF";
            return node;
        }

        public static IfNode GenerateNode(IfNodeData data, DialogueEditorWindow editor, DialogueGraphView graph)
        {
            IfNode newNode = CreateNewGraphNode(data.Position, editor, graph);
            newNode.NodeGuid = data.NodeGuid;
            newNode.ValueName = data.ValueName;
            newNode.Operations = data.Operations;
            newNode.OperationValue = data.OperationValue;
            newNode.LoadValueInToField();
            return newNode;
        }

        public IfNodeData SaveNodeData(List<Edge> edges)
        {
            IfNodeData nodeData = new IfNodeData
            {
                NodeGuid = NodeGuid,
                Position = GetPosition().position,
                ValueName = ValueName,
                Operations = Operations,
                OperationValue = OperationValue
            };

            List<string> nodeGuidList = new List<string>();

            foreach (Edge edge in edges)
            {
                if ((edge.output.node as BaseNode).NodeGuid == NodeGuid)
                {
                    nodeGuidList.Add((edge.input.node as BaseNode).NodeGuid);
                }
            }

            if (nodeGuidList.Count > 0)
            {
                nodeData.TrueGuid = nodeGuidList[0];
            }
            if (nodeGuidList.Count > 1)
            {
                nodeData.FalseGuid = nodeGuidList[1];
            }

            return nodeData;
        }

        public override void SetValidation()
        {
            List<string> error = new List<string>();
            List<string> warning = new List<string>();

            Port input = inputContainer.Query<Port>().First();
            if (input.connected == false)
            {
                warning.Add("Node cannot be called");
            }

            if (outputContainer.Query<Port>().AtIndex(0).connected == false)
            {
                error.Add("True does not lead to any node");
            }
            if (!outputContainer.Query<Port>().AtIndex(1).connected)
            {
                error.Add("False does not lead to any node");
            }

            ErrorList = error;
            WarningList = warning;
        }
    }
}
