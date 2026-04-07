using UnityEditor;
using UnityEngine;

namespace Codenoob.Util
{
    [CustomPropertyDrawer(typeof(BindFromScriptAttribute))]
    public class BindFromScriptDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var bindFromScriptAttribute = (BindFromScriptAttribute)attribute;

            var iconRect = new Rect(position.x, position.y, 20, position.height);
            var propertyRect = new Rect(position.x + 22, position.y, position.width - 22, position.height);

            var iconContent = default(GUIContent);

            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                property.objectReferenceValue == null)
            {
                iconContent = new GUIContent(EditorGUIUtility.IconContent("console.erroricon.sml"));
                iconContent.tooltip = bindFromScriptAttribute.Tooltip;
            }
            else
            {
                iconContent = new GUIContent(EditorGUIUtility.IconContent("console.infoicon.sml"));
                iconContent.tooltip = bindFromScriptAttribute.Tooltip;
            }
            
            GUI.Label(iconRect, iconContent);

            GUI.enabled = false;
            EditorGUI.PropertyField(propertyRect, property, label, true);
            GUI.enabled = true;
        }
    }
}
