using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace DS
{
    public class DialogueSystemGraphView : GraphView
    {
        DialogueSystemEditorWindow editorWindow;
        SerializableDictionary<string, List<DialogueSystemNode>> nodeDictionary;

        public DialogueSystemGraphView(DialogueSystemEditorWindow dsEditorWindow)
        {
            editorWindow = dsEditorWindow;
            nodeDictionary = new SerializableDictionary<string, List<DialogueSystemNode>>();

            AddManipulators();
            AddGridBackground();

            OnElementsDeleted();
            OnGraphViewChanged();

            AddStyles();
        }

        void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu(DialogueSystemNodeType.SingleChoice, "Add Node (Single Choice)"));
            this.AddManipulator(CreateNodeContextualMenu(DialogueSystemNodeType.MultipleChoice, "Add Node (Multiple Choice)"));

            this.AddManipulator(CreateGroupContextualMenu());
        }

        IManipulator CreateNodeContextualMenu(DialogueSystemNodeType type, string menuTitle)
        {
            var manipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(menuTitle, action => AddElement(CreateNode(type, action.eventInfo.localMousePosition)))
            );
            return manipulator;
        }

        IManipulator CreateGroupContextualMenu()
        {
            var manipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", action => AddElement(CreateGroup("Dialogue Group", action.eventInfo.localMousePosition)))
            );
            return manipulator;
        }

        void AddGridBackground()
        {
            var gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        public DialogueSystemNode CreateNode(string nodeName, DialogueSystemNodeType dialogueType, Vector2 position, bool shouldDraw = true)
        {
            var nodeType = Type.GetType($"DS.DialogueSystem{dialogueType}Node");
            var node = (DialogueSystemNode)Activator.CreateInstance(nodeType);

            node.Initialize(this, position);

            if (shouldDraw)
            {
                node.Draw();
            }

            return node;
        }

        DialogueSystemNode CreateNode(DialogueSystemNodeType type, Vector2 position)
        {
            if (type == DialogueSystemNodeType.SingleChoice)
            {
                var singleNode = new DialogueSystemSingleChoiceNode();
                singleNode.Initialize(this, position);
                singleNode.Draw();
                return singleNode;
            }
            else if (type == DialogueSystemNodeType.MultipleChoice)
            {
                var multipleNode = new DialogueSystemMultipleChoiceNode();
                multipleNode.Initialize(this, position);
                multipleNode.Draw();
                return multipleNode;
            }

            var node = new DialogueSystemNode();
            node.Initialize(this, position);
            node.Draw();

            if (nodeDictionary.ContainsKey(node.Id))
            {
                nodeDictionary[node.Id].Add(node);
                foreach (var item in nodeDictionary[node.Id])
                {
                    item.SetErrorColorStyle();
                }
            }
            else
            {
                nodeDictionary.Add(node.Id, new List<DialogueSystemNode>() { node });
            }

            return node;
        }

        GraphElement CreateGroup(string title, Vector2 localMousePosition)
        {
            Group group = new Group()
            {
                title = title,
            };
            group.SetPosition(new Rect(localMousePosition, Vector2.zero));
            return group;
        }

        void AddStyles()
        {
            this.AddStyleSheets(
                "Assets/DialogueSystem/Editor/StyleSheet/DialogueSystemGraphView.uss",
                "Assets/DialogueSystem/Editor/StyleSheet/DialogueSystemNodeView.uss");
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));
            nodeDictionary.Clear();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter node)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        void OnElementsDeleted()
        {
            deleteSelection = (name, user) =>
            {
                List<DialogueSystemNode> deleteNodes = new List<DialogueSystemNode>();
                List<Edge> deleteEdges = new List<Edge>();

                foreach (var item in selection)
                {
                    if (item is DialogueSystemNode)
                    {
                        deleteNodes.Add((DialogueSystemNode)item);
                        continue;
                    }

                    if (item.GetType() == typeof(Edge))
                    {
                        Edge edge = (Edge)item;
                        deleteEdges.Add(edge);
                        continue;
                    }

                }

                foreach (var node in deleteNodes)
                {
                    if (nodeDictionary.ContainsKey(node.Id))
                    {
                        if (nodeDictionary[node.Id].Count > 1)
                        {
                            nodeDictionary[node.Id].Remove(node);
                            if (nodeDictionary[node.Id].Count == 1)
                            {
                                nodeDictionary[node.Id][0].ResetColorStyle();
                            }
                        }
                        else
                        {
                            nodeDictionary.Remove(node.Id);
                        }
                    }

                    node.DisconnectAllPorts();
                    RemoveElement(node);
                }

                DeleteElements(deleteEdges);

            };
        }

        void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (var edge in changes.edgesToCreate)
                    {
                        var nextNode = (DialogueSystemNode)edge.input.node;
                        var choiceData = (DialogueSystemChoiceSaveData)edge.output.userData;
                        choiceData.NextNodeId = nextNode.Id;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);
                    foreach (var element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }

                        var edge = (Edge)element;
                        var choiceData = (DialogueSystemChoiceSaveData)edge.output.userData;
                        choiceData.NextNodeId = "";
                    }
                }

                return changes;
            };
        }

    }
}
