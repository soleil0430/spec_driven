using UnityEditor;
using UnityEngine;

namespace Codenoob.Util
{
    [CustomPropertyDrawer(typeof(BindableFromScriptAttribute))]
    public class BindableFromScriptDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var bindFromScriptAttribute = (BindableFromScriptAttribute)attribute;
            var hasIcon = property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null;

            var iconRect = new Rect(position.x, position.y, 20, position.height);
            var propertyRect = hasIcon ? 
                new Rect(position.x + 22, position.y, position.width - 22, position.height) :
                position;

            if (hasIcon)
            {
                var iconContent = new GUIContent(EditorGUIUtility.IconContent("console.infoicon.sml"));
                iconContent.tooltip = bindFromScriptAttribute.Tooltip;
                GUI.Label(iconRect, iconContent);
            }

            GUI.enabled = false;
            EditorGUI.PropertyField(propertyRect, property, label, true);
            GUI.enabled = true;
        }
    }
}
