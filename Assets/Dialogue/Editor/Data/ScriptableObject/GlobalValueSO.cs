using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    [CreateAssetMenu(fileName = "GlobalValueSO", menuName = "Dialogue/GlobalValueSO")]
    public class GlobalValueSO : ScriptableObject
    {
        public List<GlobalValueInt> IntValues;
        public List<GlobalValueFloat> FloatValues;
        public List<GlobalValueBool> BoolValues;
        public List<GlobalValueString> StringValues;

        public void LoadValues()
        {
            // This method can be expanded to load values from a file or database if needed.
            // Currently, it does nothing as values are stored directly in the ScriptableObject.
        }



    }
}
