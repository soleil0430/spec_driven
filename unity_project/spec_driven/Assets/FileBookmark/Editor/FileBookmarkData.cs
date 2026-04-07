using System;
using System.Collections.Generic;
using UnityEngine;

namespace FileBookmark.Editor
{
    public enum BookmarkItemType
    {
        File,       // 실제 파일
        Folder,     // 실제 폴더
        Group       // 가상 그룹
    }

    [Serializable]
    public class FileBookmarkSettings
    {
        public List<FileBookmarkPreset> presets = new List<FileBookmarkPreset>();
        public int currentPresetIndex = 0;

        public FileBookmarkSettings()
        {
            presets.Add(new FileBookmarkPreset("기본"));
        }
    }

    [Serializable]
    public class FileBookmarkPreset
    {
        public string name;
        public List<FileBookmarkItem> items = new List<FileBookmarkItem>();

        public FileBookmarkPreset(string name)
        {
            this.name = name;
        }
    }

    [Serializable]
    public class FileBookmarkItem
    {
        public string guid;
        public string cachedPath;
        public string cachedName;
        public bool isFolder;  // 하위 호환성을 위해 유지
        public BookmarkItemType itemType;
        public string nickname;  // 사용자 정의 별명 (선택적)
        public List<FileBookmarkItem> children = new List<FileBookmarkItem>();

        [NonSerialized]
        public bool isMissing = false;

        public FileBookmarkItem(string guid, string path, string name, bool isFolder)
        {
            this.guid = guid;
            this.cachedPath = path;
            this.cachedName = name;
            this.isFolder = isFolder;
            this.itemType = isFolder ? BookmarkItemType.Folder : BookmarkItemType.File;
        }

        // 그룹용 생성자
        public FileBookmarkItem(string groupName)
        {
            this.itemType = BookmarkItemType.Group;
            this.cachedName = groupName;
            this.guid = string.Empty;
            this.cachedPath = string.Empty;
            this.isFolder = false;
        }

        // 구버전 데이터 마이그레이션
        public void MigrateFromOldFormat()
        {
            if (itemType == default(BookmarkItemType))
            {
                if (string.IsNullOrEmpty(guid))
                    itemType = BookmarkItemType.Group;
                else if (isFolder)
                    itemType = BookmarkItemType.Folder;
                else
                    itemType = BookmarkItemType.File;
            }
        }

        /// <summary>
        /// 아이템이 유효한지 검증하고 캐시를 업데이트합니다.
        /// </summary>
        /// <returns>현재 유효한 경로, 없으면 null</returns>
        public string ValidateAndUpdateCache()
        {
            // 그룹은 검증 불필요
            if (itemType == BookmarkItemType.Group)
            {
                isMissing = false;
                return cachedName;
            }

#if UNITY_EDITOR
            string currentPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            
            if (string.IsNullOrEmpty(currentPath))
            {
                isMissing = true;
                return null;
            }

            isMissing = false;
            
            // 경로가 변경되었으면 캐시 업데이트
            if (currentPath != cachedPath)
            {
                cachedPath = currentPath;
                cachedName = System.IO.Path.GetFileName(currentPath);
            }

            return currentPath;
#else
            return null;
#endif
        }

        /// <summary>
        /// 재귀적으로 모든 자식 아이템도 검증합니다.
        /// </summary>
        public void ValidateRecursive()
        {
            ValidateAndUpdateCache();
            foreach (var child in children)
            {
                child.ValidateRecursive();
            }
        }

        /// <summary>
        /// Missing 상태인 아이템을 재귀적으로 제거합니다.
        /// </summary>
        public void RemoveMissingRecursive()
        {
            children.RemoveAll(item => item.isMissing);
            foreach (var child in children)
            {
                child.RemoveMissingRecursive();
            }
        }

        /// <summary>
        /// 특정 GUID가 이 아이템 또는 자식에 존재하는지 확인합니다.
        /// </summary>
        public bool ContainsGUID(string targetGuid)
        {
            if (guid == targetGuid)
                return true;

            foreach (var child in children)
            {
                if (child.ContainsGUID(targetGuid))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 특정 경로가 이 폴더 하위에 있는지 확인합니다.
        /// </summary>
        public bool IsPathUnderThisFolder(string path)
        {
            // 그룹은 실제 폴더가 아니므로 false
            if (itemType == BookmarkItemType.Group)
                return false;

            if (itemType != BookmarkItemType.Folder)
                return false;

            string currentPath = ValidateAndUpdateCache();
            if (string.IsNullOrEmpty(currentPath))
                return false;

            // 경로 정규화 (끝에 / 추가)
            string normalizedFolderPath = currentPath.TrimEnd('/') + "/";
            string normalizedTargetPath = path.Replace('\\', '/');

            return normalizedTargetPath.StartsWith(normalizedFolderPath);
        }

        /// <summary>
        /// 폴더의 하위 내용을 다시 로드합니다.
        /// 기존 children과 병합하여 확장 상태를 유지합니다.
        /// </summary>
        public void RefreshFolderContents()
        {
#if UNITY_EDITOR
            // 그룹은 실제 폴더가 아니므로 새로고침 불필요
            if (itemType == BookmarkItemType.Group)
                return;

            if (itemType != BookmarkItemType.Folder)
                return;

            string currentPath = ValidateAndUpdateCache();
            if (string.IsNullOrEmpty(currentPath))
                return;

            // 기존 children의 GUID를 맵핑
            var existingItems = new Dictionary<string, FileBookmarkItem>();
            foreach (var child in children)
            {
                existingItems[child.guid] = child;
            }

            // 새로운 children 리스트
            var newChildren = new List<FileBookmarkItem>();

            // Assets 폴더를 기준으로 절대 경로 생성
            string fullPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, 
                currentPath.Substring("Assets/".Length));

            if (!System.IO.Directory.Exists(fullPath))
                return;

            // 하위 디렉토리 로드
            string[] subDirectories = System.IO.Directory.GetDirectories(fullPath);
            foreach (string dirPath in subDirectories)
            {
                // Assets 폴더 기준 상대 경로로 변환
                string relativePath = "Assets" + dirPath.Substring(UnityEngine.Application.dataPath.Length).Replace('\\', '/');
                
                string childGuid = UnityEditor.AssetDatabase.AssetPathToGUID(relativePath);
                if (string.IsNullOrEmpty(childGuid))
                    continue;

                // 기존 아이템이 있으면 재사용 (확장 상태 유지)
                if (existingItems.TryGetValue(childGuid, out var existingItem))
                {
                    existingItem.ValidateAndUpdateCache();
                    newChildren.Add(existingItem);
                }
                else
                {
                    // 새로운 아이템 생성
                    string dirName = System.IO.Path.GetFileName(dirPath);
                    var newItem = new FileBookmarkItem(childGuid, relativePath, dirName, true);
                    LoadFolderContentsRecursive(newItem, relativePath);
                    newChildren.Add(newItem);
                }
            }

            // 하위 파일 로드
            string[] files = System.IO.Directory.GetFiles(fullPath);
            foreach (string filePath in files)
            {
                // .meta 파일 제외
                if (filePath.EndsWith(".meta"))
                    continue;

                // Assets 폴더 기준 상대 경로로 변환
                string relativePath = "Assets" + filePath.Substring(UnityEngine.Application.dataPath.Length).Replace('\\', '/');
                
                string childGuid = UnityEditor.AssetDatabase.AssetPathToGUID(relativePath);
                if (string.IsNullOrEmpty(childGuid))
                    continue;

                // 기존 아이템이 있으면 재사용
                if (existingItems.TryGetValue(childGuid, out var existingItem))
                {
                    existingItem.ValidateAndUpdateCache();
                    newChildren.Add(existingItem);
                }
                else
                {
                    // 새로운 아이템 생성
                    string fileName = System.IO.Path.GetFileName(filePath);
                    var newItem = new FileBookmarkItem(childGuid, relativePath, fileName, false);
                    newChildren.Add(newItem);
                }
            }

            // children 업데이트
            children = newChildren;
#endif
        }

        private static void LoadFolderContentsRecursive(FileBookmarkItem parentItem, string folderPath)
        {
#if UNITY_EDITOR
            // Assets 폴더를 기준으로 절대 경로 생성
            string fullPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, 
                folderPath.Substring("Assets/".Length));
            
            if (!System.IO.Directory.Exists(fullPath))
                return;

            // 하위 디렉토리 로드
            string[] subDirectories = System.IO.Directory.GetDirectories(fullPath);
            foreach (string dirPath in subDirectories)
            {
                // Assets 폴더 기준 상대 경로로 변환
                string relativePath = "Assets" + dirPath.Substring(UnityEngine.Application.dataPath.Length).Replace('\\', '/');
                
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(relativePath);
                if (string.IsNullOrEmpty(guid))
                    continue;

                string dirName = System.IO.Path.GetFileName(dirPath);
                var childItem = new FileBookmarkItem(guid, relativePath, dirName, true);
                
                // 재귀적으로 하위 폴더 로드
                LoadFolderContentsRecursive(childItem, relativePath);
                
                parentItem.children.Add(childItem);
            }

            // 하위 파일 로드
            string[] files = System.IO.Directory.GetFiles(fullPath);
            foreach (string filePath in files)
            {
                // .meta 파일 제외
                if (filePath.EndsWith(".meta"))
                    continue;

                // Assets 폴더 기준 상대 경로로 변환
                string relativePath = "Assets" + filePath.Substring(UnityEngine.Application.dataPath.Length).Replace('\\', '/');
                
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(relativePath);
                if (string.IsNullOrEmpty(guid))
                    continue;

                string fileName = System.IO.Path.GetFileName(filePath);
                var childItem = new FileBookmarkItem(guid, relativePath, fileName, false);
                
                parentItem.children.Add(childItem);
            }
#endif
        }
    }
}

