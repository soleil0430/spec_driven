using System.Collections.Generic;
using UnityEngine;

namespace Codenoob.Util
{
    public static class HierarchyExtensions
    {
        #region Layer
        public static bool HasLayer(this LayerMask mask, int layer)
        {
            var layerValue = 1 << layer;
            return 0 != (layerValue & mask);
        }

        public static void SetLayerWithChildren(this GameObject gobj, string layerName)
        {
            if (null == gobj) { Debug.LogError("[SetLayerWithChildren] This object is null !"); return; }

            SetLayerWithChildren(gobj.transform, LayerMask.NameToLayer(layerName));
        }
        public static void SetLayerWithChildren(this GameObject gobj, int layer)
        {
            if (null == gobj) { Debug.LogError("[SetLayerWithChildren] This object is null !"); return; }

            SetLayerWithChildren(gobj.transform, layer);
        }
        public static void SetLayerWithChildren(this Component gobj, string layerName)
        {
            if (null == gobj) { Debug.LogError("[SetLayerWithChildren] This object is null !"); return; }

            SetLayerWithChildren(gobj.transform, LayerMask.NameToLayer(layerName));
        }
        public static void SetLayerWithChildren(this Component gobj, int layer)
        {
            if (null == gobj) { Debug.LogError("[SetLayerWithChildren] This object is null !"); return; }

            SetLayerWithChildren(gobj.transform, layer);
        }
        static void SetLayerWithChildren(Transform gt, int layer)
        {
            if (null == gt)
                return;

            gt.gameObject.layer = layer;

            var childCount = gt.childCount;
            if (childCount <= 0)
                return;

            for (int n = 0, cnt = gt.childCount; n < cnt; ++n)
            {
                var child = gt.GetChild(n);
                if (null != child)
                    SetLayerWithChildren(child, layer);
            }
        }
        #endregion// Layer


        #region GetChild
        public static Transform GetChildWith(this Transform gobj, string name)
        {
            return ShearchChildWith(gobj.transform, name);
        }
        public static Transform ShearchChildWith(Transform gT, string name)
        {
            if (gT.name == name) return gT;
            foreach (Transform t in gT)
            {
                if (t.name == name) return t;
                var ut = ShearchChildWith(t, name);
                if (ut != null) return ut;
            }
            return null;
        }

        public static string[] GetChildNames(this Transform gobj, string name, bool isActiveOnly = false)
        {
            var result = new List<string>();
            ShearchChildNamesWith(ref result, gobj, name, isActiveOnly);
            return result.ToArray();
        }
        public static void ShearchChildNamesWith(ref List<string> list, Transform gT, string name, bool isActiveOnly = false)
        {
            if (isActiveOnly && gT.gameObject.activeSelf == false)
                return;

            if (gT.name.Contains(name))
            {
                var key = gT.name.Replace(name, "");
                if (!list.Contains(key))
                    list.Add(key);
            }

            foreach (Transform t in gT)
                ShearchChildNamesWith(ref list, t, name, isActiveOnly);
        }

        public static Transform[] GetChildsWithName(this Transform gobj, string name, bool isActiveOnly = false)
        {
            var result = new List<Transform>();
            ShearchChildsWithName(result, gobj, name, isActiveOnly);
            return result.ToArray();
        }
        public static void ShearchChildsWithName(List<Transform> list, Transform gT, string name, bool isActiveOnly = false)
        {
            if (isActiveOnly && gT.gameObject.activeSelf == false)
                return;

            if (gT.name == name)
                list.Add(gT);

            foreach (Transform t in gT)
                ShearchChildsWithName(list, t, name, isActiveOnly);
        }
        #endregion GetChild


        #region Find
        public static Transform FindTransform(this Component component, string path)    => component == null ? null : FindTransform(component.transform, path);
        public static Transform FindTransform(this GameObject go, string path)          => go == null ? null : FindTransform(go.transform, path);
        static Transform FindTransform(Transform trans, string path)                    => trans == null ? null : trans.Find(path);


        public static Transform FindRecursive(this Transform trans, string name)
        {
            if (null == trans)
                return null;

            foreach (Transform child in trans)
            {
                if (null == child)
                    continue;

                if (child.name.Equals(name))
                    return child;

                var find = child.FindRecursive(name);
                if (null != find)
                    return find;
            }

            return null;
        }

        public static GameObject FindGameObject(this GameObject go, string path) 
        {
            var transform = go.FindComponent<Transform>(path);
            return transform == null ? null : transform.gameObject;
        }
        public static GameObject FindGameObject(this Component co, string path) 
        {
            var transform = co.FindComponent<Transform>(path);
            return transform == null ? null : transform.gameObject;
        }

        public static T FindComponent<T>(this GameObject go, string path) where T : Component
        {
            return go == null ? null : FindComponent<T>(go.transform, path);
        }
        public static T FindComponent<T>(this Component co, string path) where T : Component
        {
            return co == null ? null : FindComponent<T>(co.transform, path);
        }
        static T FindComponent<T>(Transform tf, string path) where T : Component
        {
            if (null == tf)
                return null;

            var target = FindTransform(tf, path);
            return target == null ? null : target.GetComponent<T>();
        }
        #endregion// Find
    }
}