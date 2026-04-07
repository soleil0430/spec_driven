using UnityEditor;
using UnityEngine;

namespace Codenoob.Util
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
    public class ConditionalFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var conditionalAttribute = (ConditionalFieldAttribute)attribute;
            
            // 상위 오브젝트의 SerializedProperty를 찾습니다
            string parentPath = property.propertyPath.Contains(".") ? 
                property.propertyPath.Substring(0, property.propertyPath.LastIndexOf(".")) : "";
            
            // 조건부 필드의 SerializedProperty를 찾습니다
            SerializedProperty conditionalProperty = property.serializedObject.FindProperty(
                string.IsNullOrEmpty(parentPath) ? 
                conditionalAttribute.ConditionalFieldName : 
                $"{parentPath}.{conditionalAttribute.ConditionalFieldName}");

            if (conditionalProperty != null && conditionalProperty.propertyType == SerializedPropertyType.Boolean)
            {
                // 조건에 맞을 때만 필드를 그립니다
                if (conditionalProperty.boolValue == conditionalAttribute.ShowIfTrue)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
            else
            {
                // 조건부 필드를 찾을 수 없거나 bool이 아닌 경우 경고를 표시합니다
                string errorMessage = conditionalProperty == null ?
                    $"Could not find conditional field '{conditionalAttribute.ConditionalFieldName}'" :
                    $"Conditional field '{conditionalAttribute.ConditionalFieldName}' must be boolean";
                
                EditorGUI.HelpBox(position, errorMessage, MessageType.Warning);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var conditionalAttribute = (ConditionalFieldAttribute)attribute;
            
            string parentPath = property.propertyPath.Contains(".") ? 
                property.propertyPath.Substring(0, property.propertyPath.LastIndexOf(".")) : "";
            
            SerializedProperty conditionalProperty = property.serializedObject.FindProperty(
                string.IsNullOrEmpty(parentPath) ? 
                conditionalAttribute.ConditionalFieldName : 
                $"{parentPath}.{conditionalAttribute.ConditionalFieldName}");

            if (conditionalProperty != null && 
                conditionalProperty.propertyType == SerializedPropertyType.Boolean &&
                conditionalProperty.boolValue == conditionalAttribute.ShowIfTrue)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            
            return 0;
        }
    }
#endif
}
