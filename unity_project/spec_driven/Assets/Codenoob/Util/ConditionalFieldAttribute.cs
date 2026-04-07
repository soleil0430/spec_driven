using UnityEngine;

namespace Codenoob.Util
{
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public readonly string ConditionalFieldName;
        public readonly bool ShowIfTrue;

        public ConditionalFieldAttribute(string conditionalFieldName, bool showIfTrue = true)
        {
            ConditionalFieldName = conditionalFieldName;
            ShowIfTrue = showIfTrue;
        }
    }
}
