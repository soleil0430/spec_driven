using System.Collections.Generic;
using Codenoob.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Codenoob.SimpleUI
{
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
    public class SimpleCanvas : BehaviourBase
    {
        //------------------------------------------------------------------------------
        // Serialize Fields
        //------------------------------------------------------------------------------
        [Header("# Components")]
        [SerializeField, BindFromScript] RectTransform _rectTransform;
        [SerializeField, BindFromScript] Canvas _canvas;
        [SerializeField, BindFromScript] CanvasScaler _canvasScaler;
        [SerializeField, BindableFromScript] GraphicRaycaster _graphicRaycaster;

        [Header("# Configs")]
        [SerializeField] ECanvasSortingGroup _sortGroupType;

        //------------------------------------------------------------------------------
        // Variables
        //------------------------------------------------------------------------------
        bool _isRegisted;

        //------------------------------------------------------------------------------
        // Methods
        //------------------------------------------------------------------------------
        void Awake()
        {
            if (_rectTransform == null) _rectTransform = this.GetComponent<RectTransform>();
            if (_canvas == null) _canvas = this.GetComponent<Canvas>();
            if (_graphicRaycaster == null) _graphicRaycaster = this.GetComponent<GraphicRaycaster>();
            if (_canvasScaler == null) _canvasScaler = this.GetComponent<CanvasScaler>();

            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = UICamera.Camera;

            // _canvas.sortingLayerName = "Default";
            // _canvas.sortingOrder = 0;
            _canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 |
                                               AdditionalCanvasShaderChannels.Normal |
                                               AdditionalCanvasShaderChannels.Tangent;

            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.referenceResolution = new Vector2(720, 1280);
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            _canvasScaler.referencePixelsPerUnit = 100;

            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.ignoreReversedGraphics = true;
                _graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
                _graphicRaycaster.blockingMask = -1;
            }

            gameObject.SetLayerWithChildren("UI");

            _canvas.sortingOrder = (int)_sortGroupType;
        }

        public void SetSortingGroupType(ECanvasSortingGroup sortGroupType)
        {
            _sortGroupType = sortGroupType;

            _canvas.sortingOrder = (int)sortGroupType;
        }

        public Vector3 ScreenToCanvasPosition(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle
            (
                _rectTransform,
                screenPosition,
                _canvas.worldCamera,
                out Vector3 canvasPosition
            );

            return canvasPosition;
        }
        
        public void SetSortingOrder(int order) 
        { 
            _canvas.sortingOrder = order; 
        }

#if UNITY_EDITOR
        void Reset()
        {
            OnBindSerializedField();            
            
            _canvas.worldCamera = UICamera.Camera;
        }

        protected override void OnBindSerializedField()
        {
            base.OnBindSerializedField();

            _rectTransform = this.GetComponent<RectTransform>();
            _canvas = this.GetComponent<Canvas>();
            _graphicRaycaster = this.GetComponent<GraphicRaycaster>();
            _canvasScaler = this.GetComponent<CanvasScaler>();
        }
#endif //UNITY_EDITOR
    }
}