using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace FileBookmark.Editor
{
    public class FileBookmarkWindow : EditorWindow
    {
        private FileBookmarkSettings settings;
        private FileBookmarkTreeView treeView;
        private TreeViewState treeViewState;
        private Vector2 presetScrollPosition;
        private string settingsPath;

        private const float PresetAreaHeight = 80f;    // 프리셋 영역 높이 (2줄)
        private const float ToolbarHeight = 25f;
        private const float MinWindowWidth = 300f;
        private const float MinWindowHeight = 400f;

        [MenuItem("Tools/File Bookmark")]
        public static void OpenWindow()
        {
            var window = GetWindow<FileBookmarkWindow>("File Bookmark");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void OnEnable()
        {
            // 모듈 폴더 내에 로컬 전용으로 저장 (.txt 확장자로 Unity Import 오류 방지)
            settingsPath = Path.Combine(Application.dataPath, "FileBookmark", "Editor", "FileBookmarkSettings.txt");
            LoadSettings();
            InitializeTreeView();
            
            // Asset 변경 감지 이벤트 구독
            FileBookmarkAssetWatcher.OnAssetChanged += OnAssetChangedHandler;
        }

        private void OnDisable()
        {
            SaveSettings();
            
            // 이벤트 구독 해제
            FileBookmarkAssetWatcher.OnAssetChanged -= OnAssetChangedHandler;
        }

        private void InitializeTreeView()
        {
            if (treeViewState == null)
                treeViewState = new TreeViewState();

            if (settings != null && settings.presets.Count > 0 && settings.currentPresetIndex >= 0 && settings.currentPresetIndex < settings.presets.Count)
            {
                treeView = new FileBookmarkTreeView(treeViewState, settings.presets[settings.currentPresetIndex], OnItemsChanged);
            }
        }

        private void OnItemsChanged()
        {
            SaveSettings();
        }

        private void OnGUI()
        {
            if (settings == null)
            {
                EditorGUILayout.HelpBox("설정을 불러올 수 없습니다.", MessageType.Error);
                return;
            }

            // 드래그 앤 드롭을 먼저 처리 (TreeView가 이벤트를 소비하기 전에)
            HandleDragAndDrop();
            
            DrawPresetButtons();
            DrawToolbar();
            DrawTreeView();
        }

        private void DrawPresetButtons()
        {
            EditorGUILayout.BeginVertical(GUILayout.Height(PresetAreaHeight), GUILayout.ExpandWidth(true));
            
            // 첫 번째 줄: 프리셋 버튼들 (정확히 40px)
            Rect firstLineRect = EditorGUILayout.GetControlRect(false, 40f);
            
            // 스크롤 영역 수동 그리기 (가로 스크롤만)
            Rect scrollViewRect = new Rect(firstLineRect.x, firstLineRect.y, firstLineRect.width, 40f);
            
            presetScrollPosition = GUI.BeginScrollView(scrollViewRect, presetScrollPosition, 
                new Rect(0, 0, settings.presets.Count * 110, 40f), 
                GUI.skin.horizontalScrollbar, GUIStyle.none);
            
            float currentX = 0;
            
            // 프리셋 버튼들
            for (int i = 0; i < settings.presets.Count; i++)
            {
                bool isSelected = i == settings.currentPresetIndex;
                var originalColor = GUI.backgroundColor;
                
                if (isSelected)
                    GUI.backgroundColor = new Color(0.5f, 0.7f, 1f);

                Rect buttonRect = new Rect(currentX, 5f, 100f, 20f);
                if (GUI.Button(buttonRect, settings.presets[i].name))
                {
                    SwitchPreset(i);
                }

                GUI.backgroundColor = originalColor;
                currentX += 105f;
            }
            
            GUI.EndScrollView();
            
            // 두 번째 줄: 관리 버튼들 (정확히 40px)
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(40f));
            
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                CreateNewPreset();
            }

            EditorGUI.BeginDisabledGroup(settings.presets.Count <= 1);
            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                DeleteCurrentPreset();
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Edit", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                RenameCurrentPreset();
            }

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(45)))
            {
                ClearCurrentPreset();
            }
            
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(ToolbarHeight));

            EditorGUILayout.LabelField("파일/폴더를 여기에 드래그하세요", EditorStyles.miniLabel);

            GUILayout.FlexibleSpace();

            // 그룹 추가 버튼
            if (GUILayout.Button("그룹 추가", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                AddGroupItem();
            }

            // Missing 항목 카운트 및 제거 버튼
            int missingCount = CountMissingItems();
            if (missingCount > 0)
            {
                EditorGUILayout.LabelField($"누락: {missingCount}", EditorStyles.miniLabel, GUILayout.Width(70));
                
                if (GUILayout.Button("모두 제거", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    RemoveAllMissingItems();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTreeView()
        {
            if (treeView == null)
            {
                EditorGUILayout.HelpBox("프리셋이 선택되지 않았습니다.", MessageType.Info);
                return;
            }

            var currentPreset = GetCurrentPreset();
            Rect treeRect = EditorGUILayout.GetControlRect(false, position.height - PresetAreaHeight - ToolbarHeight - 10);

            if (currentPreset == null || currentPreset.items.Count == 0)
            {
                EditorGUI.HelpBox(treeRect, "빈 프리셋입니다.\n프로젝트 창에서 파일이나 폴더를 여기에 드래그하여 추가하세요.", MessageType.Info);
                return;
            }

            treeView.OnGUI(treeRect);

            // 드래그 중일 때 시각적 피드백
            if (Event.current.type == EventType.DragUpdated && treeRect.Contains(Event.current.mousePosition))
            {
                var originalColor = GUI.color;
                GUI.color = new Color(0.5f, 0.7f, 1f, 0.3f);
                EditorGUI.DrawRect(treeRect, new Color(0.5f, 0.7f, 1f, 0.1f));
                GUI.color = originalColor;
                Repaint();
            }
        }

        private void HandleDragAndDrop()
        {
            Event evt = Event.current;
            Rect dropArea = new Rect(0, PresetAreaHeight + ToolbarHeight, position.width, position.height - PresetAreaHeight - ToolbarHeight);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        break;

                    // 드래그 중인 객체가 Asset인 경우에만 허용
                    bool hasValidAssets = false;
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        string path = AssetDatabase.GetAssetPath(obj);
                        if (!string.IsNullOrEmpty(path))
                        {
                            hasValidAssets = true;
                            break;
                        }
                    }

                    if (!hasValidAssets)
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            string path = AssetDatabase.GetAssetPath(obj);
                            if (!string.IsNullOrEmpty(path))
                            {
                                AddItemFromPath(path);
                            }
                        }

                        SaveSettings();
                        RefreshTreeView();
                    }

                    // 이벤트를 명시적으로 소비하여 TreeView가 처리하지 않도록 함
                    Event.current.Use();
                    break;
            }
        }

        private void AddItemFromPath(string path)
        {
            var currentPreset = GetCurrentPreset();
            if (currentPreset == null)
                return;

            string guid = AssetDatabase.AssetPathToGUID(path);
            string name = Path.GetFileName(path);
            bool isFolder = AssetDatabase.IsValidFolder(path);

            var newItem = new FileBookmarkItem(guid, path, name, isFolder);
            
            // 폴더일 경우 하위 내용 재귀적으로 로드
            if (isFolder)
            {
                LoadFolderContents(newItem, path);
            }
            
            currentPreset.items.Add(newItem);
        }

        private void AddGroupItem()
        {
            var currentPreset = GetCurrentPreset();
            if (currentPreset == null)
                return;

            string groupName = EditorInputDialog.Show("그룹 추가", "그룹 이름을 입력하세요:", "새 그룹");
            if (!string.IsNullOrEmpty(groupName))
            {
                var newGroup = new FileBookmarkItem(groupName);
                currentPreset.items.Add(newGroup);
                RefreshTreeView();
                SaveSettings();
            }
        }

        private void OnAssetChangedHandler(string[] changedPaths)
        {
            // 현재 프리셋의 폴더 중 영향받는 것이 있는지 확인하고 갱신
            bool needsRefresh = false;
            var currentPreset = GetCurrentPreset();
            
            if (currentPreset == null)
                return;

            foreach (var path in changedPaths)
            {
                if (RefreshFoldersContainingPath(currentPreset, path))
                {
                    needsRefresh = true;
                }
            }

            if (needsRefresh)
            {
                RefreshTreeView();
                SaveSettings();
                Repaint();
            }
        }

        private bool RefreshFoldersContainingPath(FileBookmarkPreset preset, string changedPath)
        {
            bool refreshed = false;

            foreach (var item in preset.items)
            {
                if (RefreshFolderRecursive(item, changedPath))
                {
                    refreshed = true;
                }
            }

            return refreshed;
        }

        private bool RefreshFolderRecursive(FileBookmarkItem item, string changedPath)
        {
            bool refreshed = false;

            // 이 아이템이 폴더이고 변경된 경로가 이 폴더 하위인 경우
            if (item.isFolder && item.IsPathUnderThisFolder(changedPath))
            {
                item.RefreshFolderContents();
                refreshed = true;
            }

            // 자식들도 재귀적으로 확인
            foreach (var child in item.children)
            {
                if (RefreshFolderRecursive(child, changedPath))
                {
                    refreshed = true;
                }
            }

            return refreshed;
        }

        private void LoadFolderContents(FileBookmarkItem parentItem, string folderPath)
        {
            // Assets 폴더를 기준으로 절대 경로 생성
            string fullPath = Path.Combine(Application.dataPath, folderPath.Substring("Assets/".Length));
            
            if (!Directory.Exists(fullPath))
                return;

            // 하위 디렉토리 로드
            string[] subDirectories = Directory.GetDirectories(fullPath);
            foreach (string dirPath in subDirectories)
            {
                // Assets 폴더 기준 상대 경로로 변환
                string relativePath = "Assets" + dirPath.Substring(Application.dataPath.Length).Replace('\\', '/');
                
                string guid = AssetDatabase.AssetPathToGUID(relativePath);
                if (string.IsNullOrEmpty(guid))
                    continue;

                string dirName = Path.GetFileName(dirPath);
                var childItem = new FileBookmarkItem(guid, relativePath, dirName, true);
                
                // 재귀적으로 하위 폴더 로드
                LoadFolderContents(childItem, relativePath);
                
                parentItem.children.Add(childItem);
            }

            // 하위 파일 로드
            string[] files = Directory.GetFiles(fullPath);
            foreach (string filePath in files)
            {
                // .meta 파일 제외
                if (filePath.EndsWith(".meta"))
                    continue;

                // Assets 폴더 기준 상대 경로로 변환
                string relativePath = "Assets" + filePath.Substring(Application.dataPath.Length).Replace('\\', '/');
                
                string guid = AssetDatabase.AssetPathToGUID(relativePath);
                if (string.IsNullOrEmpty(guid))
                    continue;

                string fileName = Path.GetFileName(filePath);
                var childItem = new FileBookmarkItem(guid, relativePath, fileName, false);
                
                parentItem.children.Add(childItem);
            }
        }

        private void SwitchPreset(int index)
        {
            if (index < 0 || index >= settings.presets.Count)
                return;

            settings.currentPresetIndex = index;
            ValidateAllItems();
            RefreshTreeView();
            SaveSettings();
        }

        private void CreateNewPreset()
        {
            string presetName = "새 프리셋";
            int counter = 1;
            
            while (settings.presets.Exists(p => p.name == presetName))
            {
                presetName = $"새 프리셋 {counter}";
                counter++;
            }

            settings.presets.Add(new FileBookmarkPreset(presetName));
            settings.currentPresetIndex = settings.presets.Count - 1;
            RefreshTreeView();
            SaveSettings();
        }

        private void DeleteCurrentPreset()
        {
            if (settings.presets.Count <= 1)
            {
                EditorUtility.DisplayDialog("삭제 불가", "마지막 프리셋은 삭제할 수 없습니다.", "확인");
                return;
            }

            var currentPreset = GetCurrentPreset();
            if (currentPreset == null)
                return;

            // 프리셋이 비어있으면 확인 없이 바로 삭제
            bool isEmpty = currentPreset.items == null || currentPreset.items.Count == 0;
            bool shouldDelete = isEmpty || EditorUtility.DisplayDialog("프리셋 삭제", 
                $"정말로 프리셋 '{currentPreset.name}'을(를) 삭제하시겠습니까?", 
                "삭제", "취소");

            if (shouldDelete)
            {
                settings.presets.RemoveAt(settings.currentPresetIndex);
                settings.currentPresetIndex = Mathf.Clamp(settings.currentPresetIndex, 0, settings.presets.Count - 1);
                RefreshTreeView();
                SaveSettings();
            }
        }

        private void RenameCurrentPreset()
        {
            var currentPreset = GetCurrentPreset();
            if (currentPreset == null)
                return;

            string newName = EditorInputDialog.Show("프리셋 이름 변경", "새 프리셋 이름을 입력하세요:", currentPreset.name);
            if (!string.IsNullOrEmpty(newName) && newName != currentPreset.name)
            {
                currentPreset.name = newName;
                SaveSettings();
                Repaint();
            }
        }

        private void ClearCurrentPreset()
        {
            var currentPreset = GetCurrentPreset();
            if (currentPreset == null)
                return;

            if (EditorUtility.DisplayDialog("프리셋 비우기", 
                $"정말로 프리셋 '{currentPreset.name}'의 모든 항목을 삭제하시겠습니까?\n\n이 작업은 되돌릴 수 없습니다.", 
                "비우기", "취소"))
            {
                currentPreset.items.Clear();
                RefreshTreeView();
                SaveSettings();
            }
        }

        private int CountMissingItems()
        {
            var currentPreset = GetCurrentPreset();
            if (currentPreset == null)
                return 0;

            int count = 0;
            foreach (var item in currentPreset.items)
            {
                CountMissingRecursive(item, ref count);
            }
            return count;
        }

        private void CountMissingRecursive(FileBookmarkItem item, ref int count)
        {
            if (item.isMissing)
                count++;

            foreach (var child in item.children)
            {
                CountMissingRecursive(child, ref count);
            }
        }

        private void RemoveAllMissingItems()
        {
            var currentPreset = GetCurrentPreset();
            if (currentPreset == null)
                return;

            currentPreset.items.RemoveAll(item => item.isMissing);
            foreach (var item in currentPreset.items)
            {
                item.RemoveMissingRecursive();
            }

            RefreshTreeView();
            SaveSettings();
        }

        private void ValidateAllItems()
        {
            var currentPreset = GetCurrentPreset();
            if (currentPreset == null)
                return;

            foreach (var item in currentPreset.items)
            {
                item.ValidateRecursive();
            }
        }

        private void RefreshTreeView()
        {
            var currentPreset = GetCurrentPreset();
            if (currentPreset != null)
            {
                if (treeView == null)
                {
                    InitializeTreeView();
                }
                else
                {
                    treeView.UpdatePreset(currentPreset);
                }
            }
        }

        private FileBookmarkPreset GetCurrentPreset()
        {
            if (settings == null || settings.presets.Count == 0)
                return null;

            if (settings.currentPresetIndex < 0 || settings.currentPresetIndex >= settings.presets.Count)
                settings.currentPresetIndex = 0;

            return settings.presets[settings.currentPresetIndex];
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(settingsPath);
                    settings = JsonUtility.FromJson<FileBookmarkSettings>(json);
                    
                    if (settings.presets == null || settings.presets.Count == 0)
                    {
                        settings = new FileBookmarkSettings();
                    }

                    // 구버전 데이터 마이그레이션
                    foreach (var preset in settings.presets)
                    {
                        foreach (var item in preset.items)
                        {
                            MigrateItemRecursive(item);
                        }
                    }

                    ValidateAllItems();
                }
                catch (Exception e)
                {
                    Debug.LogError($"파일 북마크 설정을 불러올 수 없습니다: {e.Message}");
                    settings = new FileBookmarkSettings();
                }
            }
            else
            {
                settings = new FileBookmarkSettings();
                SaveSettings();
            }
        }

        private void MigrateItemRecursive(FileBookmarkItem item)
        {
            item.MigrateFromOldFormat();
            foreach (var child in item.children)
            {
                MigrateItemRecursive(child);
            }
        }

        private void SaveSettings()
        {
            if (settings == null)
                return;

            try
            {
                string directory = Path.GetDirectoryName(settingsPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonUtility.ToJson(settings, true);
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"파일 북마크 설정을 저장할 수 없습니다: {e.Message}");
            }
        }
    }

    // 간단한 입력 다이얼로그 헬퍼
    public class EditorInputDialog : EditorWindow
    {
        private string inputText;
        private string message;
        private Action<string> onConfirm;
        private bool shouldClose = false;

        public static string Show(string title, string message, string defaultValue)
        {
            // Unity의 EditorWindow를 사용한 간단한 입력 다이얼로그
            var window = CreateInstance<EditorInputDialog>();
            window.titleContent = new GUIContent(title);
            window.message = message;
            window.inputText = defaultValue;
            window.minSize = new Vector2(300, 100);
            window.maxSize = new Vector2(300, 100);
            window.ShowModal();
            return window.inputText;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(message);
            EditorGUILayout.Space(5);
            
            GUI.SetNextControlName("InputField");
            inputText = EditorGUILayout.TextField(inputText);
            
            if (Event.current.type == EventType.Repaint && !shouldClose)
            {
                EditorGUI.FocusTextInControl("InputField");
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("확인", GUILayout.Width(80)) || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                shouldClose = true;
                Close();
            }
            
            if (GUILayout.Button("취소", GUILayout.Width(80)) || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape))
            {
                inputText = null;
                shouldClose = true;
                Close();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}


