using System.Collections.Generic;
using UnityEditor;

namespace FileBookmark.Editor
{
    /// <summary>
    /// AssetDatabase 변경사항을 감지하여 FileBookmark에 알립니다.
    /// </summary>
    public class FileBookmarkAssetWatcher : AssetPostprocessor
    {
        public delegate void AssetChangedDelegate(string[] changedPaths);
        public static event AssetChangedDelegate OnAssetChanged;

        private static readonly HashSet<string> _pendingPaths = new HashSet<string>();
        private static bool _isUpdateScheduled = false;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            // 변경된 모든 경로 수집
            var allChangedPaths = new HashSet<string>();
            
            foreach (var path in importedAssets)
                allChangedPaths.Add(path);
            
            foreach (var path in deletedAssets)
                allChangedPaths.Add(path);
            
            foreach (var path in movedAssets)
                allChangedPaths.Add(path);
            
            foreach (var path in movedFromAssetPaths)
                allChangedPaths.Add(path);

            if (allChangedPaths.Count == 0)
                return;

            // 변경된 경로들을 pending 리스트에 추가
            foreach (var path in allChangedPaths)
            {
                _pendingPaths.Add(path);
            }

            // debounce: 짧은 시간에 여러 변경이 있을 경우 한 번만 알림
            if (!_isUpdateScheduled)
            {
                _isUpdateScheduled = true;
                EditorApplication.delayCall += NotifyChanges;
            }
        }

        private static void NotifyChanges()
        {
            _isUpdateScheduled = false;

            if (_pendingPaths.Count > 0)
            {
                string[] paths = new string[_pendingPaths.Count];
                _pendingPaths.CopyTo(paths);
                _pendingPaths.Clear();

                OnAssetChanged?.Invoke(paths);
            }
        }
    }
}

