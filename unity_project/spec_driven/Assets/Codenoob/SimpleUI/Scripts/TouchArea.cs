using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Codenoob.SimpleUI
{
    public class TouchArea : Text
    {
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            EditorOnly_ClearFields();
        }
        public void EditorOnly_ClearFields()
        {
            font            = null;
            supportRichText = false;
            maskable        = false;
            text            = string.Empty;
        }
#endif // UNITY_EDITOR
    }


#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TouchArea))]
    public class TouchAreaEditor : Editor
    {
        enum State
        {
            Off = 0,
            Mix = 1, 
            On  = 2,
        }

        public override void OnInspectorGUI()
        {
            var list = targets;
            if (null == list)       return;
            if (list.Length <= 0)   return;

            var first = list[0] as TouchArea;
            if (null == first)
                return;

            var firstValue  = first.raycastTarget;
            var state       = firstValue ? State.On : State.Off;
            for (int n = 1, cnt = list.Length; n < cnt; ++n)
            {
                var ta = list[n] as TouchArea;
                if (null == ta)
                    continue;

                if (firstValue == ta.raycastTarget)
                    continue;
                
                state = State.Mix;
                break;
            }

            EditorGUI.showMixedValue = state == State.Mix;
            var value = EditorGUILayout.Toggle("Raycast Target", EditorGUI.showMixedValue ? false : firstValue);
            EditorGUI.showMixedValue = false;

            if (state == State.Mix)
            {
                if (!value)
                    return;
            }   
            else
            {
                if (firstValue == value)            return;
                if (state == State.On && value)     return;
                if (state == State.Off && !value)   return;
            }

            for (int n = 0, cnt = list.Length; n < cnt; ++n)
            {
                var ta = list[n] as TouchArea;
                if (null == ta)                 continue;
                if (ta.raycastTarget == value)  continue;

                ta.raycastTarget = value;
                ta.EditorOnly_ClearFields();
                EditorUtility.SetDirty(ta);
            }
        }
    }
#endif // UNITY_EDITOR
}