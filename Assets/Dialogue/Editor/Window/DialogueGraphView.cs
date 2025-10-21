using DS;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    [ExecuteInEditMode]
    public class DialogueGraphView : GraphView
    {
        public DialogueEditorWindow editorWindow;
        public MiniMap minimap;

        enum NodeType
        {
            StartNode,
            DialogueNode,
            DialogueChoiceNode,
            IfNode,
            EndNode,
        }

        public DialogueGraphView(DialogueEditorWindow _editorWindow)
        {
            editorWindow = _editorWindow;

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Dialogue/Editor/StyleSheet/CategoryTheme.uss");
            styleSheets.Add(styleSheet);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddManipulators();
            AddMiniMap();
            AddSettings();

            //UpdateTheme(styleSheet);
            //this.AddManipulator(CreateGroupContextualMenu());
        }

        void AddManipulators()
        {
            this.AddManipulator(CreateNodeContextualMenu(NodeType.StartNode, "Add Start Node"));
            this.AddManipulator(CreateNodeContextualMenu(NodeType.DialogueNode, "Add Dialogue Node"));
            this.AddManipulator(CreateNodeContextualMenu(NodeType.DialogueChoiceNode, "Add Dialogue Choice Node"));
            this.AddManipulator(CreateNodeContextualMenu(NodeType.IfNode, "Add If Node"));
            this.AddManipulator(CreateNodeContextualMenu(NodeType.EndNode, "Add End Node"));
            //this.AddManipulator(CreateGroupContextualMenu());
        }

        IManipulator CreateNodeContextualMenu(NodeType type, string menuTitle)
        {
            var manipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(menuTitle, action => AddElement(CreateNode(type, action.eventInfo.mousePosition)))
            );
            return manipulator;
        }

        GraphElement CreateNode(NodeType type, Vector2 mousePosition)
        {
            switch (type)
            {
                case NodeType.StartNode:
                    return StartNode.CreateNewGraphNode(mousePosition, editorWindow, this);
                case NodeType.DialogueNode:
                    return DialogueNode.CreateNewGraphNode(mousePosition, editorWindow, this);
                case NodeType.DialogueChoiceNode:
                    return DialogueChoiceNode.CreateNewGraphNode(mousePosition, editorWindow, this);
                case NodeType.IfNode:
                    return IfNode.CreateNewGraphNode(mousePosition, editorWindow, this);
                case NodeType.EndNode:
                    return EndNode.CreateNewGraphNode(mousePosition, editorWindow, this);
                default:
                    break;
            }
            return null;
        }

        void AddMiniMap()
        {
            minimap = new MiniMap()
            {
                anchored = true,
                elementTypeColor = Color.green,
                name = "minimap",
                maxHeight = 100,
                maxWidth = 150
            };
            minimap.SetPosition(new Rect(0, 14, 200, 100));
            Add(minimap);
        }

        void AddSettings()
        {

        }

        void UpdateTheme(StyleSheet sheet)
        {
            List<BaseNode> bases = nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();
            foreach (BaseNode node in bases)
            {
                node.UpdateTheme(sheet);
            }

        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compactiblePorts = new List<Port>();
            Port startPortView = startPort;

            ports.ForEach((port) =>
            {
                Port portView = port;
                if (startPortView != portView &&
                    startPortView.node != portView.node &&
                    startPortView.direction != port.direction &&
                    startPortView.portType == portView.portType)
                {
                    compactiblePorts.Add(port);
                }
            });

            return compactiblePorts;
        }

    }
}
