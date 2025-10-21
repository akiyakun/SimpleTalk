using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

namespace DialogueSystem
{
    public class BaseNode : Node
    {
        public string NodeGuid { get; set; }
        public Box NewTitleBox { get; set; }
        public List<string> ErrorList { get; set; } = new();
        public List<string> WarningList { get; set; } = new();

        protected DialogueGraphView graphView;
        protected DialogueEditorWindow editorWindow;
        protected Vector2 defualtNodeSize = new Vector2(200, 300);

        #region Port
        // Node Contextual Menu
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
            base.BuildContextualMenu(evt);
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
                if (port.connected == false)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        // Add Input Port
        public void AddInputPort(string name, Port.Capacity capality)
        {
            AddInputPort(name, "", capality, typeof(float));
        }

        public void AddInputPort(string name, string className, Port.Capacity capality, System.Type portType)
        {
            // Create New Port
            Port inputPort = GetPortInstance(Direction.Input, capality, portType);
            inputPort.portName = name;
            // Add Port Class (USS)
            if (string.IsNullOrEmpty(className) == false)
            {
                inputPort.AddToClassList(className);
            }
            inputContainer.Add(inputPort);
        }

        // Add Output Port
        public void AddOutputPort(string name, Port.Capacity capality)
        {
            AddOutputPort(name, "", capality, typeof(float));
        }

        public void AddOutputPort(string name, string className, Port.Capacity capality)
        {
            AddOutputPort(name, className, capality, typeof(float));
        }

        public void AddOutputPort(string name, string className, Port.Capacity capality, System.Type portType)
        {
            // Create New Port
            Port inputPort = GetPortInstance(Direction.Output, capality, portType);
            inputPort.portName = name;
            // Add Port Class (USS)
            if (string.IsNullOrEmpty(className) == false)
            {
                inputPort.AddToClassList(className);
            }
            outputContainer.Add(inputPort);
        }

        public Port GetPortInstance(Direction nodeDirection, Port.Capacity capacity, System.Type portType)
        {
            return InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, portType);
        }
        #endregion

        public void UpdateTheme(StyleSheet sheet)
        {
            if (styleSheets[styleSheets.count - 1].name != "Node")
            {
                styleSheets.Remove(styleSheets[styleSheets.count - 1]);
            }
            styleSheets.Add(sheet);
        }

        protected void GenerateBetterTitle(string title, string desc = "")
        {
            Box titleBox = new Box();
            titleBox.AddToClassList("MAT_Title");

            Box container = new Box();
            container.style.flexDirection = FlexDirection.Row;
            container.style.backgroundColor = new Color(0, 0, 0, 0);
            container.style.alignItems = Align.Center;

            Box textContainer = new Box();
            textContainer.AddToClassList("TextContainer");

            Label titleField = new Label(title);
            titleField.AddToClassList("TitleText");
            Label descField = new Label(desc);
            descField.AddToClassList("DescText");
            Button button = new Button()
            {
                text = "+ Add Condition"
            };

            textContainer.Add(titleField);
            textContainer.Add(descField);
            container.Add(textContainer);
            titleBox.Add(container);
            mainContainer.Insert(1, titleBox);

            NewTitleBox = titleBox;
        }

        /// <summary>
        /// Adds a validation container to the node editor, including separate indicators 
        /// for errors and warnings, which can be dynamically updated based on validation results.
        /// </summary>
        public void AddValidationContainer()
        {
            // Create the main container for validation elements.
            VisualElement container = new VisualElement();
            container.name = "ValidationContainer";

            // Create the HelpBox for displaying errors.
            HelpBox ErrorContainer = new HelpBox("Empty Error", HelpBoxMessageType.Error);
            ErrorContainer.name = "ErrorContainer";
            ErrorContainer.style.display = DisplayStyle.None; // Initially hide the error container.
            container.Add(ErrorContainer); // Add the error container to the validation container.

            // Create the HelpBox for displaying warnings.
            HelpBox WarningContainer = new HelpBox("Empty Warning", HelpBoxMessageType.Warning);
            WarningContainer.name = "WarningContainer";
            WarningContainer.style.display = DisplayStyle.None; // Initially hide the warning container.
            container.Add(WarningContainer); // Add the warning container to the validation container.

            // Add the validation container to the title container of the node.
            NewTitleBox.Add(container);

            // Set the overflow style for the main container to ensure visibility of all elements.
            mainContainer.style.overflow = Overflow.Visible;
            NewTitleBox.style.overflow = Overflow.Visible;
        }

        public virtual void SetValidation() { }

        public virtual void LoadValueInToField() { }

        public virtual void UpdateNodeUI() { }
    }
}
