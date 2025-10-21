using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    [ExecuteInEditMode]
    public class DialogueEditorWindow : EditorWindow
    {
        public DialogueContainerSO currentDialogueContainer;

        DialogueGraphView graphView;
        Toolbar toolbar;

        [OnOpenAsset(1)]
        public static bool Show(int _instanceId, int line)
        {
            UnityEngine.Object item = EditorUtility.InstanceIDToObject(_instanceId);

            if (item is DialogueContainerSO && Application.isPlaying == false)
            {
                DialogueEditorWindow window = (DialogueEditorWindow)GetWindow(typeof(DialogueEditorWindow));
                window.titleContent = new GUIContent("Dialogue Editor");
                window.currentDialogueContainer = item as DialogueContainerSO;
                window.minSize = new Vector2(500, 250);
                window.Load();
            }
            else if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Can't Open a Dialogue", "Dialogue Editor can only be opened when the project is not on!\nTurn off Play Mode to open the Editor", "I understand");
            }

            return false;
        }

        void OnEnable()
        {
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnResize);
            ContructGraphView();
            GenerateToolbar();
            Load();
        }

        void OnDisable()
        {
            if (graphView != null)
            {
                rootVisualElement.Remove(graphView);
            }
        }

        void OnResize(GeometryChangedEvent evt)
        {
            float width = rootVisualElement.resolvedStyle.width;

            if (width < 810)
            {
                toolbar.style.flexDirection = FlexDirection.Column;
                toolbar.RemoveFromClassList("normal");
                toolbar.AddToClassList("compact");
            }
            else
            {
                toolbar.style.flexDirection = FlexDirection.Row;
                toolbar.RemoveFromClassList("compact");
                toolbar.AddToClassList("normal");
            }
        }

        void ContructGraphView()
        {
            graphView = new DialogueGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            //saveAndLoad = new DialogueSaveAndLoad(graphView);
        }

        void GenerateToolbar()
        {
            StyleSheet tmpStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Dialogue/Editor/StyleSheet/CategoryTheme.uss");
            rootVisualElement.styleSheets.Add(tmpStyleSheet);

            // Create Toolbar
            toolbar = new Toolbar();
            VisualElement leftContainer = new VisualElement();
            VisualElement rightContainer = new VisualElement();
            leftContainer.style.flexDirection = FlexDirection.Row;
            leftContainer.style.flexGrow = 1;
            leftContainer.AddToClassList("section");
            rightContainer.style.flexDirection = FlexDirection.Row;
            rightContainer.style.flexGrow = 1;
            rightContainer.AddToClassList("section");
        }

        void Load()
        {

        }

    }

}
