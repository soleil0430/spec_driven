using System.Reflection;
using UnityEngine;

namespace Codenoob.Util
{
    public class BehaviourBase : MonoBehaviour
    {
#if UNITY_EDITOR
        [ContextMenu("Bind Serialized Field")]
        protected void BindSerializedField()
        {
            UnityEditor.Undo.RecordObject(this, "Bind Serialized Field");
            OnBindSerializedField();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Bind Serialized Field Childrens")]
        protected void BindSerializedFieldChildrens()
        {
            BindSerializedField();

            var childBehaviours = GetComponentsInChildren<BehaviourBase>(true);
            
            for (int i = 0; i < childBehaviours.Length; i++)
            {
                var childBehaviour = childBehaviours[i];
                if (childBehaviour == this) continue; // 자기 자신은 제외
                
                UnityEditor.Undo.RecordObject(childBehaviour, "Bind Serialized Field Childrens");
                childBehaviour.OnBindSerializedField();
                UnityEditor.EditorUtility.SetDirty(childBehaviour);
            }
        }

        protected virtual void OnBindSerializedField() { }
        public virtual void OnInspectorGUI() { }
        public virtual void OnSceneGUI() { }
#endif // UNITY_EDITOR
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(BehaviourBase), true)]
    [UnityEditor.CanEditMultipleObjects]
    [ExecuteInEditMode]
    public class BehaviourBaseEditor : UnityEditor.Editor
    {
        public static readonly MethodInfo BIND_SERIALIZED_FIELD = typeof(BehaviourBase).GetMethod("BindSerializedField", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly MethodInfo BIND_SERIALIZED_FIELD_CHILDRENS = typeof(BehaviourBase).GetMethod("BindSerializedFieldChildrens", BindingFlags.NonPublic | BindingFlags.Instance);



        public override void OnInspectorGUI()
        {
            var targetList = serializedObject.targetObjects;
            if (null == targetList)
                return;

            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Bind Serialized Field"))
            {
                for (int n = 0, cnt = targetList.Length; n < cnt; ++n)
                {
                    var target = targetList[n] as BehaviourBase;
                    if (null == target)
                        continue;

                    BIND_SERIALIZED_FIELD.Invoke(target, null);
                }
            }

            if (GUILayout.Button("Bind Serialized Field Childrens"))
            {
                for (int n = 0, cnt = targetList.Length; n < cnt; ++n)
                {
                    var target = targetList[n] as BehaviourBase;
                    if (null == target)
                        continue;

                    BIND_SERIALIZED_FIELD.Invoke(target, null);
                    BIND_SERIALIZED_FIELD_CHILDRENS.Invoke(target, null);
                }
            }
            
            GUILayout.EndHorizontal();



            CustomInspectorGUI();

            for (int n = 0, cnt = targetList.Length; n < cnt; ++n)
            {
                var target = targetList[n] as BehaviourBase;
                if (null == target)
                    continue;

                target.OnInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 유니티에서 기본적으로 제공되는 Inspector GUI부분을 커스텀하는 메서드
        /// </summary>
        protected virtual void CustomInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        void OnSceneGUI() 
        {
            if (null == target)
                return;

            var bb = target as BehaviourBase;
            bb.OnSceneGUI();
        }
    }
#endif // UNITY_EDITOR
}
