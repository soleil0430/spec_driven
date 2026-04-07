# FileBookmark - 기술 스펙

## 시스템 아키텍처

### 개요

FileBookmark는 MVC 패턴을 기반으로 한 Unity 에디터 확장 도구입니다.

### 컴포넌트 다이어그램

```
FileBookmarkWindow (Controller/View)
  ├─ UI 레이아웃 관리
  ├─ 이벤트 처리 (드래그 앤 드롭)
  ├─ 설정 로드/저장
  └─ 프리셋 관리
        ↓
  ┌─────────────────────┬─────────────────────┐
  ↓                     ↓                     ↓
FileBookmarkTreeView   FileBookmarkSettings (Model)
  ├─ 트리 구조 렌더링     ├─ 프리셋 리스트
  ├─ 아이콘 표시         └─ 현재 프리셋 인덱스
  ├─ 인터랙션 처리             ↓
  └─ 드래그 재정렬       FileBookmarkPreset
                         ├─ 프리셋 이름
                         └─ 아이템 리스트
                               ↓
                         FileBookmarkItem
                           ├─ GUID (영구 식별자)
                           ├─ 경로/이름 캐시
                           ├─ 폴더 여부
                           └─ 자식 아이템 (계층 구조)
```

### 폴더 구조

```
Assets/FileBookmark/
├── Editor/
│   ├── FileBookmark.Editor.asmdef
│   ├── FileBookmarkData.cs
│   ├── FileBookmarkWindow.cs
│   ├── FileBookmarkTreeView.cs
│   └── FileBookmarkSettings.txt (런타임 생성)
└── Spec/
    ├── README.md
    └── SPEC.md
```

---

## 데이터 모델

### 1. FileBookmarkSettings

전체 설정을 담는 루트 데이터 구조입니다.

```csharp
[Serializable]
public class FileBookmarkSettings
{
    public List<FileBookmarkPreset> presets;
    public int currentPresetIndex;
    
    // 생성 시 기본 프리셋 자동 생성
    public FileBookmarkSettings()
    {
        presets.Add(new FileBookmarkPreset("Default"));
    }
}
```

**필드**:
- `presets`: 모든 프리셋의 리스트
- `currentPresetIndex`: 현재 선택된 프리셋의 인덱스 (0-based)

### 2. FileBookmarkPreset

개별 프리셋을 나타내는 데이터 구조입니다.

```csharp
[Serializable]
public class FileBookmarkPreset
{
    public string name;
    public List<FileBookmarkItem> items;
}
```

**필드**:
- `name`: 프리셋 이름 (사용자 정의)
- `items`: 이 프리셋에 속한 루트 레벨 아이템들

### 3. FileBookmarkItem

개별 에셋 정보를 담는 데이터 구조입니다.

```csharp
[Serializable]
public class FileBookmarkItem
{
    public string guid;              // Unity Asset GUID (영구)
    public string cachedPath;        // 캐시된 경로 (변경 가능)
    public string cachedName;        // 캐시된 이름 (변경 가능)
    public bool isFolder;            // 폴더 여부
    public List<FileBookmarkItem> children;  // 하위 아이템
    
    [NonSerialized]
    public bool isMissing;           // 런타임 상태 (직렬화 안됨)
}
```

**주요 메서드**:

- `ValidateAndUpdateCache()`: 에셋 유효성 검증 및 캐시 업데이트
- `ValidateRecursive()`: 재귀적으로 모든 자식 검증
- `RemoveMissingRecursive()`: Missing 상태인 아이템 재귀 제거
- `ContainsGUID(string)`: 중복 체크용 GUID 검색

---

## 핵심 클래스

### FileBookmarkWindow

메인 에디터 윈도우 클래스 (`EditorWindow` 상속)

**주요 책임**:
- Unity 에디터 메뉴 통합 (`Tools > File Bookmark`)
- UI 레이아웃 및 렌더링
- 프리셋 관리 (생성, 삭제, 전환, 이름 변경)
- 드래그 앤 드롭 이벤트 처리
- 설정 파일 로드/저장 (JSON)
- Missing 항목 카운트 및 제거

**주요 메서드**:
```csharp
[MenuItem("Tools/File Bookmark")]
public static void OpenWindow()

private void HandleDragAndDrop()  // 드래그 앤 드롭 처리
private void AddItemFromPath(string path)  // 경로에서 아이템 추가
private void LoadFolderContents(FileBookmarkItem parentItem, string folderPath)  // 재귀 로드
private void SaveSettings()  // JSON 저장
private void LoadSettings()  // JSON 로드
```

### FileBookmarkTreeView

트리 뷰 렌더링 및 인터랙션 클래스 (`TreeView` 상속)

**주요 책임**:
- Unity TreeView를 활용한 계층 구조 표시
- 아이콘 및 텍스트 렌더링
- Missing 항목 시각적 표시
- 드래그 앤 드롭으로 재정렬 (루트 레벨만)
- 싱글/더블 클릭 처리
- 컨텍스트 메뉴

**주요 메서드**:
```csharp
protected override TreeViewItem BuildRoot()  // 트리 구조 생성
private TreeViewItem BuildTreeRecursive(FileBookmarkItem item, ...)  // 재귀 빌드
protected override void RowGUI(RowGUIArgs args)  // 각 행 렌더링
private Texture2D GetIcon(FileBookmarkItem item)  // 아이콘 선택
protected override void SingleClickedItem(int id)  // 클릭 시 선택 및 Ping
protected override void DoubleClickedItem(int id)  // 더블 클릭 시 열기
```

---

## 데이터 흐름

### 사용자 액션 → 데이터 변경 흐름

```
1. 사용자 액션 (드래그/클릭/버튼)
   ↓
2. FileBookmarkWindow (이벤트 처리)
   ↓
3. FileBookmarkSettings 수정 (데이터 변경)
   ↓
4. SaveSettings() (JSON 파일 저장)
   ↓
5. FileBookmarkTreeView.Reload() (UI 갱신)
```

### 초기화 흐름

```
1. OnEnable()
   ↓
2. LoadSettings() (JSON → FileBookmarkSettings)
   ↓
3. ValidateAllItems() (모든 GUID 검증)
   ↓
4. InitializeTreeView()
```

---

## 핵심 기능 구현

### 1. GUID 기반 추적 시스템

**목적**: 파일 이름이나 경로가 변경되어도 에셋을 추적

**구현**:
```csharp
// 에셋 추가 시
string guid = AssetDatabase.AssetPathToGUID(path);
var item = new FileBookmarkItem(guid, path, name, isFolder);

// 검증 시
string currentPath = AssetDatabase.GUIDToAssetPath(item.guid);
if (string.IsNullOrEmpty(currentPath))
{
    item.isMissing = true;  // 에셋이 삭제됨
}
else if (currentPath != item.cachedPath)
{
    item.cachedPath = currentPath;  // 경로 업데이트
}
```

**장점**:
- 파일 이름 변경에 강함
- 폴더 이동에도 추적 유지
- Unity의 Asset Database와 통합

### 2. 폴더 재귀 로드

**구현 전략**:
1. `System.IO`를 사용하여 파일 시스템 탐색
2. 하위 디렉토리 먼저 로드 (폴더가 위에 표시)
3. 파일 나중에 로드
4. `.meta` 파일 자동 제외
5. GUID가 없는 항목 제외

```csharp
private void LoadFolderContents(FileBookmarkItem parentItem, string folderPath)
{
    // Assets 상대 경로 → 절대 경로 변환
    string fullPath = Path.Combine(Application.dataPath, 
                                    folderPath.Substring("Assets/".Length));
    
    // 하위 디렉토리 로드
    foreach (string dirPath in Directory.GetDirectories(fullPath))
    {
        // GUID 얻기 → 아이템 생성 → 재귀 호출
        var childItem = new FileBookmarkItem(...);
        LoadFolderContents(childItem, relativePath);  // 재귀
        parentItem.children.Add(childItem);
    }
    
    // 하위 파일 로드 (.meta 제외)
    foreach (string filePath in Directory.GetFiles(fullPath))
    {
        if (!filePath.EndsWith(".meta"))
        {
            var childItem = new FileBookmarkItem(...);
            parentItem.children.Add(childItem);
        }
    }
}
```

### 3. Missing 항목 관리

**감지**:
```csharp
public string ValidateAndUpdateCache()
{
    string currentPath = AssetDatabase.GUIDToAssetPath(guid);
    
    if (string.IsNullOrEmpty(currentPath))
    {
        isMissing = true;  // 에셋 없음
        return null;
    }
    
    isMissing = false;
    // 경로 캐시 업데이트
    return currentPath;
}
```

**표시**:
- 흐린 붉은색 텍스트 (`GUI.contentColor`)
- 경고 아이콘 (`console.warnicon.sml`)
- 툴팁에 마지막 경로 표시

**제거**:
```csharp
// 재귀적으로 Missing 아이템 제거
public void RemoveMissingRecursive()
{
    children.RemoveAll(item => item.isMissing);
    foreach (var child in children)
    {
        child.RemoveMissingRecursive();
    }
}
```

### 4. 드래그 앤 드롭 재정렬

**제약**: 루트 레벨 항목만 재정렬 가능 (폴더 내부는 파일 시스템 구조 유지)

**구현**:
```csharp
protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
{
    DragAndDrop.SetGenericData("FileBookmarkDraggedItems", draggedItems);
    DragAndDrop.StartDrag("FileBookmark Reorder");
}

protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
{
    // 루트 레벨에서만 순서 변경
    if (args.parentItem == rootItem)
    {
        ReorderItemInPreset(draggedItem.data, insertIndex);
    }
    return DragAndDropVisualMode.Move;
}
```

---

## 저장/로드 시스템

### 파일 위치
```
Assets/FileBookmark/Editor/FileBookmarkSettings.txt
```

### 파일 형식
- **포맷**: JSON
- **확장자**: `.txt` (Unity의 자동 Import 동작 방지)
- **인코딩**: UTF-8
- **직렬화**: `JsonUtility` 사용

### 저장 타이밍
- 프리셋 전환/생성/삭제 시
- 아이템 추가/제거 시
- 창 닫힐 때 (`OnDisable`)

### 구현
```csharp
private void SaveSettings()
{
    string directory = Path.GetDirectoryName(settingsPath);
    if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory);

    string json = JsonUtility.ToJson(settings, true);  // prettyPrint
    File.WriteAllText(settingsPath, json);
}

private void LoadSettings()
{
    if (File.Exists(settingsPath))
    {
        string json = File.ReadAllText(settingsPath);
        settings = JsonUtility.FromJson<FileBookmarkSettings>(json);
        ValidateAllItems();  // 로드 후 검증
    }
    else
    {
        settings = new FileBookmarkSettings();  // 기본값
    }
}
```

---

## 성능 최적화

### 1. 캐싱 전략
- 경로와 이름을 캐시하여 `AssetDatabase` 호출 최소화
- 필요할 때만 `ValidateAndUpdateCache()` 호출

### 2. 지연 렌더링
- Unity TreeView의 기본 가상화 활용
- 보이는 항목만 렌더링 (스크롤 시 동적 생성)

### 3. 직렬화 최적화
- `[NonSerialized]`로 런타임 상태 제외 (`isMissing`)
- `JsonUtility`는 Unity 네이티브로 빠름

### 4. 이벤트 처리 최적화
- `Event.current.Use()`로 불필요한 이벤트 전파 차단
- 필요할 때만 `Repaint()` 호출

---

## 제약 사항

### 기술적 제약

1. **Unity 에셋만 지원**
   - `AssetDatabase`에 등록된 에셋만 추가 가능
   - Hierarchy의 GameObject는 추가 불가

2. **루트 레벨만 재정렬**
   - 폴더 내부는 파일 시스템 구조 유지
   - 사용자 커스텀 순서는 루트 레벨만

3. **.meta 파일 의존성**
   - GUID는 .meta 파일에 저장됨
   - .meta 파일 손실 시 추적 불가능

### 사용자 제약

1. **로컬 전용 설정**
   - 팀 멤버 간 자동 공유 불가
   - 각자 프리셋 구성 필요

2. **대용량 폴더 성능**
   - 수천 개 파일 로드 시 지연 가능
   - 권장: 필요한 하위 폴더만 선택적 추가

---

## 기술 스택

- **Unity Editor API**: `EditorWindow`, `AssetDatabase`, `EditorGUIUtility`
- **Unity TreeView**: `TreeView`, `TreeViewItem`, `TreeViewState`
- **직렬화**: `JsonUtility`
- **파일 시스템**: `System.IO.File`, `System.IO.Directory`
- **C# 기능**: LINQ, Generic Collections, Serialization Attributes

---

## 확장 가능성

### 추가 가능한 기능

1. **검색 기능**
   - 프리셋 내 에셋 검색
   - 파일명, 경로, 타입으로 필터링

2. **Import/Export**
   - 프리셋을 JSON 파일로 내보내기
   - 팀 멤버와 프리셋 공유

3. **태그 시스템**
   - 에셋에 사용자 정의 태그 추가
   - 태그별 필터링

4. **최근 사용 항목**
   - 클릭한 에셋 기록
   - 자동으로 "Recent" 프리셋 생성

5. **즐겨찾기**
   - 자주 사용하는 항목에 별표
   - 프리셋 상단에 고정

6. **키보드 단축키**
   - 숫자 키로 프리셋 빠른 전환
   - 화살표 키로 항목 탐색

### 구현 시 고려사항

- 하위 호환성 유지 (기존 설정 파일 읽기)
- 성능 영향 최소화
- UI/UX 일관성

---

## 버전 관리

### .gitignore 권장 설정

```gitignore
# FileBookmark 로컬 설정
Assets/FileBookmark/Editor/FileBookmarkSettings.txt
Assets/FileBookmark/Editor/FileBookmarkSettings.txt.meta
```

**이유**:
- 개발자마다 다른 작업 환경
- 충돌 방지
- 로컬 전용 설정

---

## 디버깅

### 로그 메시지

- 중복 추가 시도: `Debug.LogWarning("Item already exists in preset: {path}")`
- 설정 로드 실패: `Debug.LogError("Failed to load File Bookmark settings: {e.Message}")`
- 설정 저장 실패: `Debug.LogError("Failed to save File Bookmark settings: {e.Message}")`

### 콘솔 확인 항목

- 설정 파일 로드/저장 실패
- 중복 에셋 추가 시도
- 유효하지 않은 경로

