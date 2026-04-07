using System;
using Codenoob.Util;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Codenoob.SimpleUI
{
    public class JoypadView : BehaviourBase, IPointerDownHandler, IPointerUpHandler
    {
        //------------------------------------------------------------------------------
        // Serialize Fields
        //------------------------------------------------------------------------------
        [Header("# Childs")]
        [SerializeField, BindFromScript] CanvasGroup _cvgFrame;
        [SerializeField, BindFromScript] RectTransform _rtfFrame;
        [SerializeField, BindFromScript] RectTransform _rtfStick;

        [Header("# Configs")]
        [SerializeField] bool _isVisible = true;
        [SerializeField] float _deadRatio = 0.05f;
        [Space]
        [SerializeField] bool _useTracking = false;
        [SerializeField] float _trackingSpeed = 2f;

        //------------------------------------------------------------------------------
        // Variables
        //------------------------------------------------------------------------------
        bool _isInited;
        bool _isPressing;

        bool _isActivated;

        Vector3 _beginPosition;

        //------------------------------------------------------------------------------
        // Properties
        //------------------------------------------------------------------------------
        public event Action OnBegin;
        public event Action<float, float> OnDrag;
        public event Action OnFinish;

        //------------------------------------------------------------------------------
        // Methods
        //------------------------------------------------------------------------------
        public void Init() 
        { 
            if (_isInited)
                return;

            _cvgFrame.alpha = 0f;

            _isInited = true;
            
            gameObject.SetActive(false);
        }
        public void Release() 
        { 
            if (_isInited == false)
                return;
            
            //TODO : 해제
            if (_isPressing)
                FinishInput();

            OnBegin = null;
            OnDrag = null;
            OnFinish = null;

            _isInited = false;

            gameObject.SetActive(false);
        }

        public void Activate() 
        { 
            if (_isActivated)
               return;

            //NOTE : 활성화할 때 처리

            _isActivated = true;

            gameObject.SetActive(true);
        }
        public void Deactivate() 
        { 
            if (_isActivated == false)
                return;

            //NOTE : 비활성화할 때 처리
            if (_isPressing)
                FinishInput();

            _isActivated = false;

            gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData) 
        { 
            if (_isInited == false)
                return;

            if (_isPressing)
                return;

            BeginInput();
        }
        public void OnPointerUp(PointerEventData eventData) 
        { 
            if (_isInited == false)
                return;

            if (_isPressing == false)
                return;

            FinishInput();
        }

        void Update()
        {
            if (_isInited == false)
                return;

            if (_isPressing == false)
                return;

            DragInput();
        }

        void BeginInput() 
        { 
            _beginPosition = Input.mousePosition;

            _rtfFrame.position = CanvasHelper.ScreenToCanvasPosition(_beginPosition);

            if (_isVisible)
                _cvgFrame.alpha = 1f;
            else
                _cvgFrame.alpha = 0f;

            OnBegin?.Invoke();

            _isPressing = true;
        }
        void DragInput() 
        { 
            var mousePosition = Input.mousePosition;
            var frameRadius = _rtfFrame.sizeDelta.x * 0.5f;
            var toMouse = mousePosition - _beginPosition;

            //Tracking
            if (_useTracking)
            {
                var distance = toMouse.magnitude;
                if (frameRadius < distance)
                {
                    _beginPosition = Vector3.Lerp(_beginPosition, mousePosition, _trackingSpeed * Time.deltaTime);

                    _rtfFrame.position = CanvasHelper.ScreenToCanvasPosition(_beginPosition);
                }
            }

            var d = toMouse.normalized;
            var m = toMouse.magnitude;

            var i = Mathf.Clamp01(m / frameRadius);

            var inputPower = (m < _deadRatio * frameRadius) ? 0f : i;
            var radian = Mathf.Atan2(toMouse.y, toMouse.x);

            _rtfStick.anchoredPosition = d * i * frameRadius;

            OnDrag?.Invoke(radian, inputPower);
        }
        void FinishInput() 
        { 
            _cvgFrame.alpha = 0f;

            _isPressing = false;

            OnFinish?.Invoke();
        }

#if UNITY_EDITOR
        protected override void OnBindSerializedField()
        {
            base.OnBindSerializedField();

            //Childs
            _rtfFrame = this.FindComponent<RectTransform>("JoypadFrame");
            _cvgFrame = this.FindComponent<CanvasGroup>("JoypadFrame");
            _rtfStick = this.FindComponent<RectTransform>("JoypadFrame/Contents/JoypadStick");
        }
#endif //UNITY_EDITOR
    }
}