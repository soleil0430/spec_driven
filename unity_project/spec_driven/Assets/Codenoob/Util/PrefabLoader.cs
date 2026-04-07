using System;
using System.Collections.Generic;
using UnityEngine;

namespace Codenoob.Util
{
    public class PrefabLoader
    {
        //------------------------------------------------------------------------------
        // singleton
        //------------------------------------------------------------------------------
        private PrefabLoader() {}
        private static PrefabLoader _instance = null;
        public  static PrefabLoader Instance  { get { return _instance ?? (_instance = new PrefabLoader()); } }

        //------------------------------------------------------------------------------
        // variables
        //------------------------------------------------------------------------------
        private Dictionary<Type, Dictionary<int, MonoBehaviour>> _resourceSetSet = new Dictionary<Type, Dictionary<int, MonoBehaviour>>();

        public Dictionary<Type, string> _resourcePathSet = new Dictionary<Type, string>();

        //------------------------------------------------------------------------------
        // functions
        //------------------------------------------------------------------------------
        public void AddResourcePathes(Type[] types, string[] pathFormats)
        {
            if (null == types
                || null == pathFormats
                || 0 == types.Length
                || 0 == pathFormats.Length
                || types.Length != pathFormats.Length)
            {
                Debug.LogError("[PrefabLoader.AddResourcePathes] 매개변수가 정상적이지 않습니다.");
                return;
            }

            for (int i = 0, size = types.Length; i < size; ++i)
                _resourcePathSet[types[i]] = pathFormats[i];
        }

        public T GetPrefab<T>(int id) where T : MonoBehaviour
        {
            var type = typeof(T);

            if (!_resourceSetSet.TryGetValue(type, out var set))
            {
                set = new Dictionary<int, MonoBehaviour>();
                _resourceSetSet.Add(type, set);
            }

            if (set.TryGetValue(id, out var c))
                return (T)c;

            if (!_resourcePathSet.TryGetValue(type, out var path))
            {
                Debug.LogError($"[PrefabLoader.GetPrefab] pathFormat 이 지정되지 않은 리소스를 로드하려 합니다. type: {type.Name}");
                return null;
            }

            var p = string.Format(path, id);
            var t = Resources.Load<T>(p);
            if (null == t)
            {
                Debug.LogError($"[PrefabLoader.GetPrefab] 프리팹 로드에 실패했습니다. type: {type.Name},  path: {p}");
                return null;
            }

            set.Add(id, t);
            return t;
        }

        public void ClearAllPrefabs<T>()
        {
            if (_resourceSetSet.TryGetValue(typeof(T), out var set))
                set.Clear();
        }
    }
}