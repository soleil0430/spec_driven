using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Codenoob.SimpleUI
{
    public static class SimpleCanvasMenu
    {
        // Simple UI 그룹에 Simple Canvas 메뉴 항목 추가
        [MenuItem("GameObject/Simple UI/Simple Canvas", false, 10)]
        private static void CreateSimpleCanvas()
        {
            CreateSimpleCanvasInternal(true);
        }

        // Simple UI 그룹에 GraphicRaycaster가 없는 Simple Canvas 메뉴 항목 추가
        [MenuItem("GameObject/Simple UI/Simple Canvas(No Graphic Raycaster)", false, 11)]
        private static void CreateSimpleCanvasWithoutRaycaster()
        {
            CreateSimpleCanvasInternal(false);
        }

        private static void CreateSimpleCanvasInternal(bool includeGraphicRaycaster)
        {
            // 부모 오브젝트 (현재 선택된 GameObject) 가져오기
            GameObject parent = Selection.activeGameObject;
            
            // 새 캔버스 오브젝트 생성
            GameObject canvasObj = includeGraphicRaycaster
                ? new GameObject("SimpleCanvas", typeof(RectTransform), typeof(Canvas), 
                               typeof(CanvasScaler), typeof(GraphicRaycaster))
                : new GameObject("SimpleCanvas", typeof(RectTransform), typeof(Canvas), 
                               typeof(CanvasScaler));
            
            // 부모 설정
            if (parent != null)
            {
                canvasObj.transform.SetParent(parent.transform, false);
            }
            
            // RectTransform 설정
            RectTransform rectTransform = canvasObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localPosition = Vector3.zero;
            
            // Canvas 설정
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 |
                                             AdditionalCanvasShaderChannels.Normal |
                                             AdditionalCanvasShaderChannels.Tangent;
            
            // CanvasScaler 설정
            CanvasScaler canvasScaler = canvasObj.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(720, 1280);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasScaler.referencePixelsPerUnit = 100;
            
            // GraphicRaycaster 설정
            if (includeGraphicRaycaster)
            {
                GraphicRaycaster graphicRaycaster = canvasObj.GetComponent<GraphicRaycaster>();
                graphicRaycaster.ignoreReversedGraphics = true;
                graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
                graphicRaycaster.blockingMask = -1;
            }
            
            // SimpleCanvas 컴포넌트 추가
            canvasObj.AddComponent<SimpleCanvas>();
            
            // EventSystem이 씬에 존재하는지 확인하고 없으면 생성
            CheckAndCreateEventSystem();
            
            // 레이어 설정
            SetLayerRecursively(canvasObj, LayerMask.NameToLayer("UI"));
            
            // 생성된 오브젝트 선택
            Selection.activeGameObject = canvasObj;
            
            // Undo 등록
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create SimpleCanvas");
        }
        
        // EventSystem을 확인하고 없으면 생성하는 메서드
        private static void CheckAndCreateEventSystem()
        {
            // 씬에 EventSystem이 이미 존재하는지 확인
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                // EventSystem이 없으면 생성
                GameObject eventSystemObj = new GameObject("EventSystem", 
                    typeof(EventSystem), 
                    typeof(StandaloneInputModule));
                
                // Undo 등록
                Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");
                
                Debug.Log("EventSystem이 생성되었습니다.");
            }
        }
        
        // 오브젝트와 모든 자식 오브젝트의 레이어를 설정하는 유틸리티 메서드
        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
} 