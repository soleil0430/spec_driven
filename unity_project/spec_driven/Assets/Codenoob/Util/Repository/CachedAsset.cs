using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Codenoob.Util
{
    public sealed class CachedAsset<T> where T : UnityObject
    {
        static readonly bool isUnloadable = !typeof(Component).IsAssignableFrom(typeof(T))
                                         && !typeof(GameObject).IsAssignableFrom(typeof(T));

        readonly Dictionary<string, T> assets = new Dictionary<string, T>();
        public readonly string PathBase = string.Empty;



        public CachedAsset() { }
        public CachedAsset(string pathBase) { PathBase = pathBase; }

        public bool IsCached(string path) => assets.ContainsKey(path);

        public T Get(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"{nameof(Get)} : {path}가 Empty 입니다.");
                return null;
            }

            if (!assets.TryGetValue(path, out var asset))
            {
                var pathFull = string.IsNullOrEmpty(PathBase)
                             ? path
                             : Path.Combine(PathBase, path);

                asset = Resources.Load<T>(pathFull);
                if (asset == null)
                    Debug.LogError($"{nameof(Get)} : {pathFull}는 존재하지 않는 Asset 입니다.");
                else
                    assets[path] = asset;
            }
            return asset;
        }

        public void Caching(string path) => Get(path);
        public void Caching(params string[] paths)
        {
            if (paths == null)
            {
                Debug.LogError($"{nameof(Caching)} : {nameof(paths)}가 null 입니다.");
                return;
            }

            for (int index = 0; index < paths.Length; ++index)
                Get(paths[index]);
        }



        public bool Remove(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"{nameof(Remove)} : {nameof(path)}가 Empty 입니다.");
                return false;
            }

            if (isUnloadable
             && assets.TryGetValue(path, out var asset))
                Resources.UnloadAsset(asset);
            return assets.Remove(path);
        }

        public void Clear()
        {
            if (isUnloadable)
            {
                foreach (var asset in assets.Values)
                    Resources.UnloadAsset(asset);
            }

            assets.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}