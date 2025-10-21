using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    [System.Serializable]
    public enum EndNodeType
    {
        End,
        ReturnToStart,
        NextDialogue
    }

    [System.Serializable]
    public enum GlobalValueType
    {
        Int,
        Float,
        Bool,
        String
    }

    [System.Serializable]
    public enum GlobalValueIFOperations
    {
        Equal,
        Lesser,
        Greater,
        LesserOrEqual,
        GreaterOrEqual,
        NoEqual
    }

    [System.Serializable]
    public class BaseNodeData
    {
        public string NodeGuid;
        public Vector2 Position;
    }

    [System.Serializable]
    public class StartNodeData : BaseNodeData
    {
        public string StartId;
    }

    [System.Serializable]
    public class EndNodeData : BaseNodeData
    {
        public EndNodeType EndNodeType;
        public DialogueContainerSO Dialogue;
        public string StartId;
    }

    [System.Serializable]
    public class DialogueNodeData : BaseNodeData
    {
        public string CharacterId;
        public string Motion;
        public string DialogueTextId;
        public float Duration;
    }

    [System.Serializable]
    public class DialogueNodePort
    {
        public Object PortObj;
        public string PortGuid;
        public string InputGuid;
        public string OutputGuid;
        public string TextId;
    }

    [System.Serializable]
    public class DialogueChoiceNodeData : BaseNodeData
    {
        public string CharacterId;
        public string Motion;
        public string DialogueTextId;
        public float Duration;
        public List<string> ChoiceTextIdList;
        public List<DialogueNodePort> DialogueNodePorts;
    }

    [System.Serializable]
    public class IfNodeData : BaseNodeData
    {
        public string ValueName;
        public GlobalValueIFOperations Operations;
        public string OperationValue;
        public string TrueGuid;
        public string FalseGuid;
    }

    [System.Serializable]
    public class GlobalValueInt
    {
        public string ValueName;
        public int Value;
    }

    [System.Serializable]
    public class GlobalValueFloat
    {
        public string ValueName;
        public float Value;
    }

    [System.Serializable]
    public class GlobalValueBool
    {
        public string ValueName;
        public bool Value;
    }

    [System.Serializable]
    public class GlobalValueString
    {
        public string ValueName;
        public string Value;
    }
}
