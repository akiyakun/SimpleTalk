using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DS
{
    public static class DialogueSystemSaveUtility
    {
        public static string TxtDataPath { get; private set; }

        static readonly string defaultLoadFolderPath = "Assets/Editor/DialogueSystem/Graphs";

        static DialogueSystemGraphView graphView;
        static string graphFileName;
        static string containerFolderPath;
        static List<DialogueSystemNode> viewNodeList;
        static Dictionary<string, DialogueSystemNodeEditorSaveData> createdNodeDictionary;
        static Dictionary<string, DialogueSystemNode> loadedNodeDictionary;

        public static void Initialize(DialogueSystemGraphView dsGraphView, string path, string graphName)
        {
            graphView = dsGraphView;
            graphFileName = graphName;
            containerFolderPath = path;
            createdNodeDictionary = new Dictionary<string, DialogueSystemNodeEditorSaveData>();
            loadedNodeDictionary = new Dictionary<string, DialogueSystemNode>();
        }

        public static void Save(string path, string fileName)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = defaultLoadFolderPath;
            }
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = graphFileName;
            }

            var oldGraphData = LoadAsset<DialogueSystemEditorScriptableObject>(path, fileName);
            if (oldGraphData != null)
            {
                RemoveAsset(path, fileName);
            }

            var oldRuntimeData = LoadAsset<DialogueSystemContainerScriptableObject>(path, $"{fileName}Runtime");
            if (oldRuntimeData != null)
            {
                RemoveAsset(path, $"{fileName}Runtime");
            }

            viewNodeList = new List<DialogueSystemNode>();
            //CreateDefaultFolders();
            GetElementsFromGraphView();

            var graphData = CreateAsset<DialogueSystemEditorScriptableObject>(path, fileName);
            graphData.Initialize(fileName, TxtDataPath);

            var runtimeData = CreateAsset<DialogueSystemContainerScriptableObject>(path, $"{fileName}Runtime");
            var nodeList = ConvertEditorNodeListToSaveDataNodeList(viewNodeList);
            runtimeData.Initialize($"{fileName}Runtime", nodeList);

            SetGraphViewNodeData(graphData);
            SaveAsset(graphData);
            SaveAsset(runtimeData);
        }

        static List<DialogueSystemNodeSaveData> ConvertEditorNodeListToSaveDataNodeList(List<DialogueSystemNode> nodeList)
        {
            List<DialogueSystemNodeSaveData> nodeSaveDataList = new List<DialogueSystemNodeSaveData>();
            foreach (var node in nodeList)
            {
                var nodeSaveData = new DialogueSystemNodeSaveData();
                nodeSaveData.Initialize(
                    id: node.Id,
                    textKey: node.TextKey,
                    isStartingDialogue: node.IsStartingNode(),
                    nodeType: node.NodeType,
                    choices: node.ChoiceList);
                if (node.IsStartingNode())
                {
                    nodeSaveDataList.Insert(0, nodeSaveData);
                }
                else
                {
                    nodeSaveDataList.Add(nodeSaveData);
                }
            }
            return nodeSaveDataList;
        }

        static void SetGraphViewNodeData(DialogueSystemEditorScriptableObject graphData)
        {
            foreach (var node in viewNodeList)
            {
                SaveNodeToEditorGraph(node, graphData);
                SaveNodeToScriptableObject(node);
            }

            UpdateDialoguesChoicesConnections();
        }

        static void SaveNodeToEditorGraph(DialogueSystemNode node, DialogueSystemEditorScriptableObject graphData)
        {
            List<DialogueSystemChoiceSaveData> choices = node.ChoiceList;
            DialogueSystemNodeEditorSaveData nodeData = new DialogueSystemNodeEditorSaveData()
            {
                Id = node.Id,
                Name = node.DialogueName,
                ChoiceList = choices,
                TextKey = node.TextKey,
                Text = node.Text,
                NodeType = node.NodeType,
                Position = node.GetPosition().position
            };

            graphData.NodeList.Add(nodeData);
        }

        static void SaveNodeToScriptableObject(DialogueSystemNode node)
        {
            var nodeView = new DialogueSystemNodeEditorSaveData();
            nodeView.Id = node.Id;
            nodeView.TextKey = node.TextKey;
            nodeView.Text = node.Text;
            nodeView.NodeType = node.NodeType;
            nodeView.ChoiceList = node.ChoiceList;
            nodeView.TextKey = node.TextKey;
            nodeView.Text = node.Text;
            createdNodeDictionary.Add(node.Id, nodeView);
        }

        static void UpdateDialoguesChoicesConnections()
        {
            foreach (var node in viewNodeList)
            {
                var createdNode = createdNodeDictionary[node.Id];
                for (int choiceIndex = 0; choiceIndex < node.ChoiceList.Count; choiceIndex++)
                {
                    var nodeChoice = node.ChoiceList[choiceIndex];
                    if (string.IsNullOrEmpty(nodeChoice.NextNodeId))
                    {
                        continue;
                    }
                    createdNode.ChoiceList[choiceIndex].NextNodeId = createdNodeDictionary[nodeChoice.NextNodeId].Id;
                }
            }
        }

        public static void Load(string path, string assetName)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = defaultLoadFolderPath;
            }
            var graphData = LoadAsset<DialogueSystemEditorScriptableObject>(path, assetName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    $"The file at the following path could not be found:\n\n{path}/{assetName}",
                    "OK"
                );
                return;
            }

            TxtDataPath = graphData.TxtDataPath;
            LoadNodes(graphData.NodeList);
            LoadNodesConnections();
        }

        static void LoadNodes(List<DialogueSystemNodeEditorSaveData> nodes)
        {
            foreach (var nodeData in nodes)
            {
                List<DialogueSystemChoiceSaveData> choices = nodeData.ChoiceList;
                var node = graphView.CreateNode(nodeData.Name, nodeData.NodeType, nodeData.Position, false);
                node.Id = nodeData.Id;
                node.ChoiceList = choices;
                node.TextKey = nodeData.TextKey;
                node.Text = nodeData.Text;
                node.Draw();
                graphView.AddElement(node);
                loadedNodeDictionary.Add(node.Id, node);

            }
        }

        static void LoadNodesConnections()
        {
            foreach (var loadedNode in loadedNodeDictionary)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    DialogueSystemChoiceSaveData choiceData = (DialogueSystemChoiceSaveData)choicePort.userData;

                    if (string.IsNullOrEmpty(choiceData.NextNodeId))
                    {
                        continue;
                    }

                    DialogueSystemNode nextNode = loadedNodeDictionary[choiceData.NextNodeId];
                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();
                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                    graphView.AddElement(edge);
                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        static void CreateDefaultFolders()
        {
            CreateFolder("Assets/Editor", "DialogueSystem");
            CreateFolder("Assets/Editor/DialogueSystem", "Graphs");

            CreateFolder("Assets", "DialogueSystem");
            CreateFolder("Assets/DialogueSystem", "Dialogues");

            CreateFolder("Assets/DialogueSystem/Dialogues", graphFileName);
            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder($"{containerFolderPath}/Global", "Dialogues");
        }

        static void GetElementsFromGraphView()
        {
            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is DialogueSystemNode node)
                {
                    viewNodeList.Add(node);
                    return;
                }
            });
        }

        public static void CreateFolder(string parentFolderPath, string newFolderName)
        {
            if (AssetDatabase.IsValidFolder($"{parentFolderPath}/{newFolderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(parentFolderPath, newFolderName);
        }

        public static void RemoveFolder(string path)
        {
            FileUtil.DeleteFileOrDirectory($"{path}.meta");
            FileUtil.DeleteFileOrDirectory($"{path}/");
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        public static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            path = ConvertPathToRelative(path);
            string fullPath = $"{path}/{assetName}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        public static void SaveAsset(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                Debug.LogError("Want to save empty asset!");
            }
            else
            {
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        public static void SetTxtPath(string path)
        {
            TxtDataPath = path;
        }

        public static string ConvertPathToRelative(string fullPath)
        {
            string projectPath = Application.dataPath;
            if (fullPath.StartsWith(projectPath))
            {
                return "Assets" + fullPath.Substring(projectPath.Length);
            }

            return fullPath;
        }


    }
}
