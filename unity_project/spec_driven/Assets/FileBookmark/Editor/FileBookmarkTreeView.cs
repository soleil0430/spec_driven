using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace FileBookmark.Editor
{
    public class FileBookmarkTreeView : TreeView
    {
        private FileBookmarkPreset currentPreset;
        private System.Action onItemsChanged;

        public FileBookmarkTreeView(TreeViewState state, FileBookmarkPreset preset, System.Action onItemsChanged) 
            : base(state)
        {
            this.currentPreset = preset;
            this.onItemsChanged = onItemsChanged;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            useScrollView = true;
            Reload();
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            var draggedItems = new List<TreeViewItem>();
            foreach (var id in args.draggedItemIDs)
            {
                var item = FindItem(id, rootItem);
                if (item != null)
                    draggedItems.Add(item);
            }
            DragAndDrop.SetGenericData("FileBookmarkDraggedItems", draggedItems);
            DragAndDrop.objectReferences = new UnityEngine.Object[] { };
            DragAndDrop.StartDrag("파일 북마크 재정렬");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var draggedItems = DragAndDrop.GetGenericData("FileBookmarkDraggedItems") as List<TreeViewItem>;
            if (draggedItems == null || draggedItems.Count == 0)
                return DragAndDropVisualMode.None;

            if (args.performDrop)
            {
                var draggedItem = draggedItems[0] as FileBookmarkTreeViewItem;
                if (draggedItem?.data == null)
                    return DragAndDropVisualMode.None;

                FileBookmarkTreeViewItem targetItem = args.parentItem as FileBookmarkTreeViewItem;
                int insertIndex = args.insertAtIndex;

                // 루트 레벨 또는 그룹 안에 드롭 가능
                if (args.parentItem == rootItem)
                {
                    // 루트 레벨에서 재정렬
                    ReorderItemInPreset(draggedItem.data, insertIndex);
                    onItemsChanged?.Invoke();
                    Reload();
                }
                else if (targetItem?.data != null && targetItem.data.itemType == BookmarkItemType.Group)
                {
                    // 그룹 안으로 이동
                    MoveItemToGroup(draggedItem.data, targetItem.data, insertIndex);
                    onItemsChanged?.Invoke();
                    Reload();
                }
            }

            return DragAndDropVisualMode.Move;
        }

        private void MoveItemToGroup(FileBookmarkItem item, FileBookmarkItem targetGroup, int insertIndex)
        {
            if (currentPreset?.items == null || targetGroup == null)
                return;

            // 먼저 원래 위치에서 제거
            RemoveItemFromParent(item);

            // 그룹의 children에 추가
            if (insertIndex < 0 || insertIndex >= targetGroup.children.Count)
                targetGroup.children.Add(item);
            else
                targetGroup.children.Insert(insertIndex, item);
        }

        private void RemoveItemFromParent(FileBookmarkItem item)
        {
            // 루트 레벨에서 제거 시도
            if (currentPreset.items.Remove(item))
                return;

            // 재귀적으로 부모에서 제거
            foreach (var rootItem in currentPreset.items)
            {
                if (RemoveItemFromParentRecursive(rootItem, item))
                    return;
            }
        }

        private bool RemoveItemFromParentRecursive(FileBookmarkItem parent, FileBookmarkItem itemToRemove)
        {
            if (parent.children.Remove(itemToRemove))
                return true;

            foreach (var child in parent.children)
            {
                if (RemoveItemFromParentRecursive(child, itemToRemove))
                    return true;
            }

            return false;
        }

        private void ReorderItemInPreset(FileBookmarkItem item, int newIndex)
        {
            if (currentPreset?.items == null)
                return;

            // 현재 위치 찾기
            int currentIndex = currentPreset.items.IndexOf(item);
            if (currentIndex < 0)
                return;

            // 아이템 제거 후 새 위치에 삽입
            currentPreset.items.RemoveAt(currentIndex);
            
            // 인덱스 조정 (제거로 인한 인덱스 변화 고려)
            if (newIndex > currentIndex)
                newIndex--;
            
            // 범위 체크
            newIndex = Mathf.Clamp(newIndex, 0, currentPreset.items.Count);
            
            currentPreset.items.Insert(newIndex, item);
        }

        public void UpdatePreset(FileBookmarkPreset preset)
        {
            currentPreset = preset;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            if (currentPreset != null && currentPreset.items != null && currentPreset.items.Count > 0)
            {
                int idCounter = 1;
                foreach (var item in currentPreset.items)
                {
                    var treeItem = BuildTreeRecursive(item, root, ref idCounter);
                    root.AddChild(treeItem);
                }
            }
            else
            {
                var emptyItem = new TreeViewItem { id = 1, depth = 0, displayName = "비어있음 - 파일/폴더를 여기에 드래그하세요" };
                root.AddChild(emptyItem);
            }

            return root;
        }

        private TreeViewItem BuildTreeRecursive(FileBookmarkItem item, TreeViewItem parent, ref int idCounter)
        {
            var treeItem = new FileBookmarkTreeViewItem(idCounter++, parent.depth + 1, item);

            if (item.children != null && item.children.Count > 0)
            {
                foreach (var child in item.children)
                {
                    var childTreeItem = BuildTreeRecursive(child, treeItem, ref idCounter);
                    treeItem.AddChild(childTreeItem);
                }
            }

            return treeItem;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as FileBookmarkTreeViewItem;
            if (item?.data == null)
            {
                // Empty 메시지인 경우
                base.RowGUI(args);
                return;
            }

            // 그룹 배경 먼저 그리기 (다른 요소들보다 먼저) - 하얀 배경
            if (item.data.itemType == BookmarkItemType.Group)
            {
                Rect backgroundRect = new Rect(args.rowRect.x, args.rowRect.y, args.rowRect.width, args.rowRect.height);
                
                // 완전 하얀 배경
                Color backgroundColor = new Color(1f, 1f, 1f, 1f);
                EditorGUI.DrawRect(backgroundRect, backgroundColor);
                
                // 상단과 하단에 밝은 회색 구분선 추가
                Color lineColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                
                Rect topLine = new Rect(args.rowRect.x, args.rowRect.y, args.rowRect.width, 1f);
                Rect bottomLine = new Rect(args.rowRect.x, args.rowRect.y + args.rowRect.height - 1f, args.rowRect.width, 1f);
                EditorGUI.DrawRect(topLine, lineColor);
                EditorGUI.DrawRect(bottomLine, lineColor);
            }

            // 색상 처리
            var originalColor = GUI.contentColor;
            
            // 컨텐츠 영역 계산
            float indent = GetContentIndent(item);
            float iconSize = 16f;
            float spacing = 2f;
            float currentX = args.rowRect.x + indent;

            // 아이콘 그리기 (그룹은 아이콘 없음)
            if (item.data.itemType != BookmarkItemType.Group)
            {
                Texture2D icon = GetIcon(item.data);
                Rect iconRect = new Rect(currentX, args.rowRect.y + (args.rowRect.height - iconSize) / 2, iconSize, iconSize);
                if (icon != null)
                {
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                }
                currentX += iconSize + spacing;
            }

            // 경고 아이콘 (Missing인 경우)
            if (item.data.isMissing)
            {
                Rect warningRect = new Rect(currentX, args.rowRect.y + (args.rowRect.height - 12f) / 2, 12f, 12f);
                var warningIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                if (warningIcon != null)
                {
                    GUI.DrawTexture(warningRect, warningIcon, ScaleMode.ScaleToFit);
                }
                currentX += 12f + spacing;
            }

            // 텍스트 그리기
            string tooltip = item.data.isMissing ? $"누락: {item.data.cachedPath}" : 
                            (item.data.itemType == BookmarkItemType.Group ? item.data.cachedName : item.data.cachedPath);
            Rect labelRect = new Rect(currentX, args.rowRect.y, args.rowRect.width - (currentX - args.rowRect.x), args.rowRect.height);
            
            if (item.data.isMissing)
            {
                // Missing 항목은 전체를 빨간색으로
                GUI.contentColor = new Color(1f, 0.5f, 0.5f, 0.7f);
                string displayText = string.IsNullOrEmpty(item.data.nickname) 
                    ? item.data.cachedName 
                    : $"{item.data.nickname} ({item.data.cachedName})";
                EditorGUI.LabelField(labelRect, new GUIContent(displayText, tooltip));
            }
            else if (item.data.itemType == BookmarkItemType.Group)
            {
                // 그룹은 일반 텍스트로 표시 (검은색)
                GUI.contentColor = Color.black;
                EditorGUI.LabelField(labelRect, new GUIContent(item.data.cachedName, tooltip));
            }
            else if (!string.IsNullOrEmpty(item.data.nickname))
            {
                // 별명이 있으면 별명은 밝은 파란색, 원본 이름은 기존 색상으로 분리
                GUIStyle labelStyle = EditorStyles.label;
                
                // 별명 부분 (밝은 파란색)
                GUI.contentColor = new Color(0.4f, 0.7f, 1f);
                GUIContent nicknameContent = new GUIContent(item.data.nickname, tooltip);
                Vector2 nicknameSize = labelStyle.CalcSize(nicknameContent);
                Rect nicknameRect = new Rect(currentX, args.rowRect.y, nicknameSize.x, args.rowRect.height);
                EditorGUI.LabelField(nicknameRect, nicknameContent);
                
                // 원본 이름 부분 (기존 색상)
                GUI.contentColor = originalColor;
                string originalNameText = $" ({item.data.cachedName})";
                GUIContent originalContent = new GUIContent(originalNameText, tooltip);
                Rect originalRect = new Rect(currentX + nicknameSize.x, args.rowRect.y, 
                    args.rowRect.width - (currentX + nicknameSize.x - args.rowRect.x), args.rowRect.height);
                EditorGUI.LabelField(originalRect, originalContent);
            }
            else
            {
                // 별명이 없으면 원본 이름만 표시
                EditorGUI.LabelField(labelRect, new GUIContent(item.data.cachedName, tooltip));
            }

            GUI.contentColor = originalColor;
        }

        private Texture2D GetIcon(FileBookmarkItem item)
        {
            if (item.isMissing)
            {
                return EditorGUIUtility.IconContent("d_winbtn_win_close").image as Texture2D;
            }

            switch (item.itemType)
            {
                case BookmarkItemType.Group:
                    // 그룹은 특별한 아이콘으로 표시 (프리팹 아이콘 또는 파란색 폴더)
                    return EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
                    
                case BookmarkItemType.Folder:
                    return EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
                    
                case BookmarkItemType.File:
                    // 파일 확장자에 따른 아이콘
                    string currentPath = item.ValidateAndUpdateCache();
                    if (!string.IsNullOrEmpty(currentPath))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Object>(currentPath);
                        if (asset != null)
                        {
                            var icon = AssetPreview.GetMiniThumbnail(asset);
                            if (icon != null)
                                return icon;
                        }
                    }
                    return EditorGUIUtility.IconContent("DefaultAsset Icon").image as Texture2D;
            }

            return EditorGUIUtility.IconContent("DefaultAsset Icon").image as Texture2D;
        }

        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);

            var item = FindItem(id, rootItem) as FileBookmarkTreeViewItem;
            
            // 그룹은 클릭해도 Asset 선택 안함
            if (item?.data == null || item.data.itemType == BookmarkItemType.Group || item.data.isMissing)
                return;

            string currentPath = item.data.ValidateAndUpdateCache();
            if (!string.IsNullOrEmpty(currentPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(currentPath);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);

            var item = FindItem(id, rootItem) as FileBookmarkTreeViewItem;
            
            // 그룹은 더블클릭해도 Asset 열지 않음
            if (item?.data == null || item.data.itemType == BookmarkItemType.Group || item.data.isMissing)
                return;

            string currentPath = item.data.ValidateAndUpdateCache();
            if (!string.IsNullOrEmpty(currentPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(currentPath);
                if (asset != null)
                {
                    AssetDatabase.OpenAsset(asset);
                }
            }
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as FileBookmarkTreeViewItem;
            if (item?.data == null)
                return;

            GenericMenu menu = new GenericMenu();
            
            // 그룹 전용 메뉴
            if (item.data.itemType == BookmarkItemType.Group)
            {
                menu.AddItem(new GUIContent("그룹 이름 변경"), false, () =>
                {
                    RenameGroup(item.data);
                });
                
                menu.AddSeparator("");
            }
            else
            {
                // 별명 관련 메뉴 (그룹이 아닐 때만)
                if (string.IsNullOrEmpty(item.data.nickname))
                {
                    menu.AddItem(new GUIContent("별명 설정"), false, () =>
                    {
                        SetNickname(item.data);
                    });
                }
                else
                {
                    menu.AddItem(new GUIContent("별명 수정"), false, () =>
                    {
                        SetNickname(item.data);
                    });
                    
                    menu.AddItem(new GUIContent("별명 제거"), false, () =>
                    {
                        item.data.nickname = null;
                        onItemsChanged?.Invoke();
                        Reload();
                    });
                }
                
                menu.AddSeparator("");
            }
            
            menu.AddItem(new GUIContent("제거"), false, () =>
            {
                RemoveItem(item.data);
                onItemsChanged?.Invoke();
                Reload();
            });

            if (item.data.isMissing)
            {
                menu.AddItem(new GUIContent("제거 (누락)"), false, () =>
                {
                    RemoveItem(item.data);
                    onItemsChanged?.Invoke();
                    Reload();
                });
            }

            menu.ShowAsContext();
        }

        private void RenameGroup(FileBookmarkItem item)
        {
            string newName = EditorInputDialog.Show("그룹 이름 변경", "새 이름을 입력하세요:", item.cachedName);
            if (newName != null && !string.IsNullOrWhiteSpace(newName))
            {
                item.cachedName = newName.Trim();
                onItemsChanged?.Invoke();
                Reload();
            }
        }

        private void SetNickname(FileBookmarkItem item)
        {
            string currentNickname = item.nickname ?? "";
            string title = string.IsNullOrEmpty(currentNickname) ? "별명 설정" : "별명 수정";
            string message = "이 항목의 별명을 입력하세요:";
            
            string newNickname = EditorInputDialog.Show(title, message, currentNickname);
            if (newNickname != null)  // null이면 취소 버튼을 누른 것
            {
                item.nickname = string.IsNullOrWhiteSpace(newNickname) ? null : newNickname.Trim();
                onItemsChanged?.Invoke();
                Reload();
            }
        }

        private void RemoveItem(FileBookmarkItem itemToRemove)
        {
            if (currentPreset?.items == null)
                return;

            // 최상위 레벨에서 제거 시도
            if (currentPreset.items.Remove(itemToRemove))
                return;

            // 재귀적으로 자식에서 제거 시도
            foreach (var item in currentPreset.items)
            {
                RemoveItemRecursive(item, itemToRemove);
            }
        }

        private bool RemoveItemRecursive(FileBookmarkItem parent, FileBookmarkItem itemToRemove)
        {
            if (parent.children.Remove(itemToRemove))
                return true;

            foreach (var child in parent.children)
            {
                if (RemoveItemRecursive(child, itemToRemove))
                    return true;
            }

            return false;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }
    }

    public class FileBookmarkTreeViewItem : TreeViewItem
    {
        public FileBookmarkItem data;

        public FileBookmarkTreeViewItem(int id, int depth, FileBookmarkItem data) : base(id, depth)
        {
            this.data = data;
            displayName = data.cachedName;
        }
    }
}

