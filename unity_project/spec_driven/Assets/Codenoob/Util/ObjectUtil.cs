using UnityEngine;

namespace Codenoob.Util
{
    public static class ObjectUtil
    {
        public static void SafeDestroy<T>(this T t) where T : MonoBehaviour
        {
            if (null != t && null != t.gameObject)
                UnityEngine.Object.Destroy(t.gameObject);
        }

        //------------------------------------------------------------------------------
        // GameObject
        //------------------------------------------------------------------------------
        public static GameObject Instantiate(GameObject prefab, Transform parent)
        {
            return Instantiate(prefab, parent, Vector3.zero, Vector3.one, Quaternion.Euler(0f, 0f, 0f));
        }

        public static GameObject Instantiate(GameObject prefab, Transform parent, Vector3 pos)
        {
            return Instantiate(prefab, parent, pos, Vector3.one, Quaternion.Euler(0f, 0f, 0f));
        }

        public static GameObject Instantiate(GameObject prefab, Transform parent, Vector3 pos, Vector3 scale)
        {
            return Instantiate(prefab, parent, pos, scale, Quaternion.Euler(0f, 0f, 0f));
        }

        public static GameObject Instantiate(GameObject prefab, Transform parent, Vector3 pos, Vector3 scale, Quaternion rot)
        {
            if (null == prefab)
            {
                Debug.LogError("[ObjectUtil.Instantiate] 프리팹을 찾을 수 없습니다.");
                return null;
            }

            var go = UnityEngine.Object.Instantiate(prefab);
            if (null == go)
            {
                Debug.LogError("[ObjectUtil.Instantiate] 오브젝트 생성에 실패했습니다.");
                return null;
            }

            if (null != parent)
                go.transform.SetParent(parent);

            go.transform.localPosition = pos;
            go.transform.localScale    = scale;
            go.transform.localRotation = rot;

            return go;
        }

        //------------------------------------------------------------------------------
        // Load and instantiate
        //------------------------------------------------------------------------------
        public static T LoadAndInstantiate<T>(string path, Transform parent) where T : Object
        {
            return LoadAndInstantiate<T>(path, parent, Vector3.zero, Vector3.one, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T LoadAndInstantiate<T>(string path, Transform parent, Vector3 pos) where T : Object
        {
            return LoadAndInstantiate<T>(path, parent, pos, Vector3.one, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T LoadAndInstantiate<T>(string path, Transform parent, Vector3 pos, Vector3 scale) where T : Object
        {
            return LoadAndInstantiate<T>(path, parent, pos, scale, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T LoadAndInstantiate<T>(string path, Transform parent, Vector3 pos, Vector3 scale, Quaternion rot) where T : Object
        {
            var prefab = Resources.Load<GameObject>(path);
            if (null == prefab)
                return null;

            return Instantiate<T>(prefab, parent, pos, scale, rot);
        }

        public static T LoadAndInstantiate<T>(int key, Transform parent) where T : MonoBehaviour
        {
            return LoadAndInstantiate<T>(key, parent, Vector3.zero, Vector3.one, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T LoadAndInstantiate<T>(int key, Transform parent, Vector3 pos) where T : MonoBehaviour
        {
            return LoadAndInstantiate<T>(key, parent, pos, Vector3.one, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T LoadAndInstantiate<T>(int key, Transform parent, Vector3 pos, Vector3 scale) where T : MonoBehaviour
        {
            return LoadAndInstantiate<T>(key, parent, pos, scale, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T LoadAndInstantiate<T>(int key, Transform parent, Vector3 pos, Vector3 scale, Quaternion rot) where T : MonoBehaviour
        {
            var prefab = PrefabLoader.Instance.GetPrefab<T>(key);
            if (null == prefab)
                return null;

            return Instantiate<T>(prefab, parent, pos, scale, rot);
        }

        //------------------------------------------------------------------------------
        // Generic
        //------------------------------------------------------------------------------
        public static T Instantiate<T>(GameObject prefab, Transform parent) where T : Object
        {
            return Instantiate<T>(prefab, parent, Vector3.zero, Vector3.one, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T Instantiate<T>(GameObject prefab, Transform parent, Vector3 pos) where T : Object
        {
            return Instantiate<T>(prefab, parent, pos, Vector3.one, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T Instantiate<T>(GameObject prefab, Transform parent, Vector3 pos, Vector3 scale) where T : Object
        {
            return Instantiate<T>(prefab, parent, pos, scale, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T Instantiate<T>(GameObject prefab, Transform parent, Vector3 pos, Vector3 scale, Quaternion rot) where T : Object
        {
            var go = Instantiate(prefab, parent, pos, scale, rot);
            if (null == go)
            {
                Debug.LogError($"[ObjectUtil.Instantiate] 오브젝트 생성에 실패했습니다. type: {typeof(T)}");
                return null;
            }

            var t = go.GetComponent<T>();
            if (null == t)
            {
                Debug.LogError($"[ObjectUtil.Instantiate] 해당 컴포넌트를 찾을 수 없습니다. type: {typeof(T)}");
                return null;
            }

            return t;
        }

        //------------------------------------------------------------------------------
        // MonoBehaviour
        //------------------------------------------------------------------------------
        public static T Instantiate<T>(MonoBehaviour prefab, Transform parent) where T : Object
        {
            return Instantiate<T>(prefab, parent, Vector3.zero, Vector3.one, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T Instantiate<T>(MonoBehaviour prefab, Transform parent, Vector3 pos) where T : Object
        {
            return Instantiate<T>(prefab, parent, pos, Vector3.one, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T Instantiate<T>(MonoBehaviour prefab, Transform parent, Vector3 pos, Vector3 scale) where T : Object
        {
            return Instantiate<T>(prefab, parent, pos, scale, Quaternion.Euler(0f, 0f, 0f));
        }

        public static T Instantiate<T>(MonoBehaviour prefab, Transform parent, Vector3 pos, Vector3 scale, Quaternion rot) where T : Object
        {
            var go = Instantiate(prefab.gameObject, parent, pos, scale, rot);
            if (null == go)
            {
                Debug.LogError($"[ObjectUtil.Instantiate] 오브젝트 생성에 실패했습니다. type: {typeof(T)}");
                return null;
            }

            var t = go.GetComponent<T>();
            if (null == t)
            {
                Debug.LogError($"[ObjectUtil.Instantiate] 해당 컴포넌트를 찾을 수 없습니다. type: {typeof(T)}");
                return null;
            }

            return t;
        }

        //------------------------------------------------------------------------------
        // layer
        //------------------------------------------------------------------------------
        public static void ChangeLayerWithAllChilds(GameObject go, int layer)
        {
            if (null == go)
                return;

            go.layer = layer;

            foreach (Transform child in go.transform)
                ChangeLayerWithAllChilds(child.gameObject, layer);
        }
    }
}