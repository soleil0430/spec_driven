using Codenoob.Util;
using UnityEngine;

namespace Codenoob.SimpleUI
{
    public static class CanvasHelper
    {
        //NOTE : SimpleCanvas는 모두 동일한 카메라를 worldCamera로 삼고있음.
        //NOTE : 따라서 더미 캔버스에 대한 변환값이 모든 캔버스 대상으로 변환한 값과 동일함
        static SimpleCanvas _canvas;

        static void Init()
        {
            if (_canvas != null)
                return;
            
            var goCanvas = GameObject.Find("SimpleCanvasHelper");
            if (goCanvas == null)
            {
                goCanvas = new GameObject("SimpleCanvasHelper");
                GameObject.DontDestroyOnLoad(goCanvas);
            }

            _canvas = goCanvas.GetComponent<SimpleCanvas>();
            if (_canvas == null)
                _canvas = goCanvas.AddComponent<SimpleCanvas>();
        }

        public static Vector2 WorldToScreenPosition(Vector3 worldPosition) => WorldToScreenPosition(Camera.main, worldPosition);
        public static Vector2 WorldToScreenPosition(Camera camera, Vector3 worldPosition)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError($"[CanvasHelper.ToCanvasPosition] 플레이 모드에서만 사용할 수 있습니다.");
                return default;
            }

            Init();

            var viewportPosition = camera.WorldToViewportPoint(worldPosition);

            if (viewportPosition.z < 0f)
            {
                viewportPosition.x = 1.0f - viewportPosition.x;
                viewportPosition.y = 1.0f - viewportPosition.y;
                viewportPosition.z = -viewportPosition.z;
            }

            return camera.ViewportToScreenPoint(viewportPosition);
        }

        public static Vector3 WorldToCanvasPosition(Vector3 worldPosition) => WorldToCanvasPosition(Camera.main, worldPosition, false);
        public static Vector3 WorldToCanvasPosition(Vector3 worldPosition, bool isClamp) => WorldToCanvasPosition(Camera.main, worldPosition, isClamp);
        public static Vector3 WorldToCanvasPosition(Camera camera, Vector3 worldPosition, bool isClamp)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError($"[CanvasHelper.ToCanvasPosition] 플레이 모드에서만 사용할 수 있습니다.");
                return default;
            }

            Init();

            var viewportPosition = camera.WorldToViewportPoint(worldPosition);
            if (viewportPosition.z < 0f)
            {
                viewportPosition.x = 1.0f - viewportPosition.x;
                viewportPosition.y = 1.0f - viewportPosition.y;
                viewportPosition.z = -viewportPosition.z;
            }

            if (!isClamp)
            {
                var screenPosition = camera.ViewportToScreenPoint(viewportPosition);
                return _canvas.ScreenToCanvasPosition(screenPosition);
            }

            viewportPosition.x = Mathf.Clamp01(viewportPosition.x);
            viewportPosition.y = Mathf.Clamp01(viewportPosition.y);
            viewportPosition.z = Mathf.Clamp01(viewportPosition.z);

            var clampedScreenPosition = camera.ViewportToScreenPoint(viewportPosition);
            return _canvas.ScreenToCanvasPosition(clampedScreenPosition);
        }
        static Vector2 GetViewportBorderPoint(Vector2 point)
        {
            var toPoint = point - (Vector2.one * 0.5f); // 중점에서 point로의 방향벡터
            
            // 방향벡터가 0이면 중점 반환
            if (toPoint.x == 0 && toPoint.y == 0)
                return Vector2.one * 0.5f;
            
            // 각 축에서 테두리까지의 스케일링 팩터 계산
            float scaleX = toPoint.x == 0 ? float.MaxValue : 0.5f / Mathf.Abs(toPoint.x);
            float scaleY = toPoint.y == 0 ? float.MaxValue : 0.5f / Mathf.Abs(toPoint.y);
            
            // 더 작은 스케일링 팩터 선택 (먼저 닿는 테두리)
            float scale = Mathf.Min(scaleX, scaleY);
            
            // 중점에서 스케일링된 방향벡터만큼 이동한 지점
            return (Vector2.one * 0.5f) + (toPoint * scale);
        }

        public static Vector3 ScreenToCanvasPosition(Vector2 screenPosition)
        {
            Init();

            return _canvas.ScreenToCanvasPosition(screenPosition);
        }

        public static Vector2 CanvasToScreenPosition(Vector3 canvasPosition) => CanvasToScreenPosition(UICamera.Camera, canvasPosition);
        public static Vector2 CanvasToScreenPosition(Camera renderCamera, Vector3 canvasPosition)
        {
            Init();

            return renderCamera.WorldToScreenPoint(canvasPosition);
        }


        public static bool CheckInScreen(Transform target) => CheckInScreen(Camera.main, target);
        public static bool CheckInScreen(Camera camera, Transform target)
        {
            var viewportPosition = camera.WorldToViewportPoint(target.position);
            return !(viewportPosition.z < 0f ||
                     viewportPosition.x <= 0f || 1.0f <= viewportPosition.x ||
                     viewportPosition.y <= 0f || 1.0f <= viewportPosition.y);
        }
    }
}