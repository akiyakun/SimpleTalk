using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace DS
{
    public class DialogueSystemEditorWindow : EditorWindow
    {
        readonly string DefaultFileName = "DialoguesFileName";

        [SerializeField] StyleSheet m_StyleSheet = default;

        DialogueSystemGraphView graphView;
        Label filePathLabel;
        Label txtFileLabel;
        Button loadTxtDataButton;
        Button saveButton;
        Button loadButton;
        Button clearButton;

        [MenuItem("Onihime/DialogueSystemEditorWindow")]
        public static void ShowExample()
        {
            DialogueSystemEditorWindow wnd = GetWindow<DialogueSystemEditorWindow>("Dialogue Window");
        }

        void OnEnable()
        {
            AddGraphView();
            AddToolbar();
            AddStyles();
        }

        void AddGraphView()
        {
            graphView = new DialogueSystemGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }
        void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();
            filePathLabel = DialogueSystemElementUtility.CreateLabel("File Path: Null  ");
            txtFileLabel = DialogueSystemElementUtility.CreateLabel("Txt File: Null  ");
            loadTxtDataButton = DialogueSystemElementUtility.CreateButton("Load Txt Data", () => LoadTxtData());
            saveButton = DialogueSystemElementUtility.CreateButton("Save", () => Save());
            loadButton = DialogueSystemElementUtility.CreateButton("Load", () => Load());
            clearButton = DialogueSystemElementUtility.CreateButton("Clear", () => Clear());

            toolbar.Add(filePathLabel);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(loadTxtDataButton);
            toolbar.Add(txtFileLabel);

            toolbar.AddStyleSheets("Assets/DialogueSystem/Editor/StyleSheet/DialogueSystemToolbar.uss");

            rootVisualElement.Add(toolbar);
        }

        void AddStyles()
        {
            var styleSheet = (StyleSheet)EditorGUIUtility.Load("Assets/DialogueSystem/Editor/StyleSheet/DialogueSystemVariables.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        void LoadTxtData()
        {
            string filePath = EditorUtility.OpenFilePanel("Text Data", "Assets", "asset");

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            DialogueSystemSaveUtility.SetTxtPath(filePath);
            txtFileLabel.text = $"Txt File: {Path.GetFileName(filePath)}";
        }

        void Save()
        {
            var selectedPath = EditorUtility.SaveFilePanelInProject("Save Graphs", "Dialogue", "asset", "Please enter a file name to save the asset to");
            if (string.IsNullOrEmpty(selectedPath))
            {
                EditorUtility.DisplayDialog(
                    "Could not Save the file!",
                    $"The file at the following path could not be saved:\n\n{selectedPath}.",
                    "OK"
                );
                return;
            }

            var fileName = Path.GetFileNameWithoutExtension(selectedPath);
            var folderPath = selectedPath.Replace($"/{fileName}.asset", "");
            DialogueSystemSaveUtility.Initialize(graphView, folderPath, fileName);
            DialogueSystemSaveUtility.Save(folderPath, fileName);
            filePathLabel.text = $"File Path: {DialogueSystemSaveUtility.ConvertPathToRelative(selectedPath)}  ";
        }

        void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "", "asset");

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            Clear();

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var folderPath = filePath.Replace($"/{fileName}.asset", "");
            DialogueSystemSaveUtility.Initialize(graphView, folderPath, fileName);
            DialogueSystemSaveUtility.Load(folderPath, fileName);
            filePathLabel.text = $"File Path: {DialogueSystemSaveUtility.ConvertPathToRelative(filePath)}  ";
            txtFileLabel.text = $"Txt File: {Path.GetFileName(DialogueSystemSaveUtility.TxtDataPath)}";
        }

        void Clear()
        {
            graphView.ClearGraph();
            DialogueSystemSaveUtility.SetTxtPath(string.Empty);
            filePathLabel.text = "File Path: Null  ";
            txtFileLabel.text = "Txt File: Null ";
        }

    }
}

