using System;
using System.Collections;
using System.Collections.Generic;
using Codenoob.Util;
using UnityEditor;
using UnityEngine;

namespace Codenoob.SimpleUI
{
    [RequireComponent(typeof(Animator), typeof(CanvasGroup), typeof(RectTransform))]
    public class VisibilityUIAnimator : BehaviourBase 
    { 
        //------------------------------------------------------------------------------
        // Constants
        //------------------------------------------------------------------------------
        static readonly Dictionary<EState, int> _hashDict = new Dictionary<EState, int>() 
        {
            { EState.Showing, Animator.StringToHash("Showing") },
            { EState.Shown, Animator.StringToHash("Shown") },
            { EState.Hiding, Animator.StringToHash("Hiding") },
            { EState.Hidden, Animator.StringToHash("Hidden") }
        };

        //------------------------------------------------------------------------------
        // Serialized Variables
        //------------------------------------------------------------------------------
        [Header("# Components")]
        [SerializeField, BindFromScript] Animator _animator;
        [SerializeField, BindFromScript] CanvasGroup _canvasGroup;

        //------------------------------------------------------------------------------
        // Variables
        //------------------------------------------------------------------------------
        EState _nowState = EState.Hidden;

        Coroutine _coRoutine;

        bool _interactable;
        bool _blockRaycasts;

        //------------------------------------------------------------------------------
        // Properties
        //------------------------------------------------------------------------------
        public EState NowState => _nowState;

        public bool IsVisible => _nowState != EState.Hidden;

        public bool IsShow => _nowState == EState.Shown || _nowState == EState.Showing;
        public bool IsHide => _nowState == EState.Hiding || _nowState == EState.Hidden;
        public bool IsPlaying => _nowState == EState.Showing || _nowState == EState.Hiding;

        //------------------------------------------------------------------------------
        // Methods
        //------------------------------------------------------------------------------
        void Awake()
        {
            _interactable = _canvasGroup.interactable;
            _blockRaycasts = _canvasGroup.blocksRaycasts;
        }

        void OnDisable()
        {
            _nowState = EState.Hidden;

            _animator.Rebind();

            this.ReleaseCoroutine(ref _coRoutine);
        }

        int GetHash(EState state)
        {
            if (!_hashDict.TryGetValue(state, out var hash))
            {
                Debug.LogError($"[VisibilityUIAnimator.GetHash] {state}의 해시값을 찾을 수 없습니다.");
                return 0;
            }

            return hash;
        }

        public void Show() => Show(null);
        public void Show(Action doneCallback)
        {
            if (!gameObject.activeInHierarchy || !_animator.enabled)
            {
                Debug.Log($"[VisibilityUIAnimator.Show] GameObject가 비활성화 되어있거나, Animator가 비활성화 되어있습니다.");
                return;
            }
            
            PreShow();
            _nowState = EState.Showing;

            this.SwapCoroutine(ref _coRoutine, Co_Play(GetHash(EState.Showing), () => {
                _nowState = EState.Shown;
                PostShow();
                doneCallback?.Invoke();
            }));
        }

        public void Hide() => Hide(null);
        public void Hide(Action doneCallback)
        {
            if (!gameObject.activeInHierarchy || !_animator.enabled)
            {
                Debug.Log($"[VisibilityUIAnimator.Hide] GameObject가 비활성화 되어있거나, Animator가 비활성화 되어있습니다.");
                return;
            }
            
            PreHide();
            _nowState = EState.Hiding;

            this.SwapCoroutine(ref _coRoutine, Co_Play(GetHash(EState.Hiding), () => {
                _nowState = EState.Hidden;
                PostHide();
                doneCallback?.Invoke();
            }));
        }

        public void ShowImmediately() 
        { 
            if (!gameObject.activeInHierarchy || !_animator.enabled)
            {
                // Debug.Log($"[VisibilityUIAnimator.ShowImmediately] GameObject가 비활성화 되어있거나, Animator가 비활성화 되어있습니다.");
                return;
            }

            this.ReleaseCoroutine(ref _coRoutine);
            
            _nowState = EState.Shown;

            _animator.Rebind();
            _animator.Play(GetHash(EState.Shown));
            _animator.Update(0);

            PostShow();
        }
        public void HideImmediately() 
        { 
            if (!gameObject.activeInHierarchy || !_animator.enabled)
            {
                Debug.Log($"[VisibilityUIAnimator.HideImmediately] GameObject가 비활성화 되어있거나, Animator가 비활성화 되어있습니다.");
                return;
            }

            this.ReleaseCoroutine(ref _coRoutine);
            
            _nowState = EState.Hidden;

            _animator.Rebind();
            _animator.Play(GetHash(EState.Hidden));
            _animator.Update(0);

            PostHide();
        }

        public void TryShow() => TryShow(null);
        public void TryShow(Action doneCallback) 
        {
            if (!gameObject.activeInHierarchy || !_animator.enabled)
            {
                Debug.Log($"[VisibilityUIAnimator.TryShow] GameObject가 비활성화 되어있거나, Animator가 비활성화 되어있습니다.");
                doneCallback?.Invoke();
                return;
            }

            if (_nowState == EState.Showing || _nowState == EState.Shown)
            {
                doneCallback?.Invoke();
                return;
            }
            
            Show(doneCallback);
        }

        public void TryShowImmediately()
        {
            if (!gameObject.activeInHierarchy || !_animator.enabled)
            {
                Debug.Log($"[VisibilityUIAnimator.TryShowImmediately] GameObject가 비활성화 되어있거나, Animator가 비활성화 되어있습니다.");
                return;
            }

            if (_nowState == EState.Showing || _nowState == EState.Shown)
            {
                return;
            }

            ShowImmediately();
        }


        public void TryHide() => TryHide(null);
        public void TryHide(Action doneCallback) 
        {
            if (!gameObject.activeInHierarchy || !_animator.enabled)
            {
                Debug.Log($"[VisibilityUIAnimator.TryHide] GameObject가 비활성화 되어있거나, Animator가 비활성화 되어있습니다.");
                doneCallback?.Invoke();
                return;
            }

            if (_nowState == EState.Hiding || _nowState == EState.Hidden)
            {
                doneCallback?.Invoke();
                return;
            }

            Hide(doneCallback);
        }

        public void TryHideImmediately()
        {
            if (!gameObject.activeInHierarchy || !_animator.enabled)
            {
                Debug.Log($"[VisibilityUIAnimator.TryHideImmediately] GameObject가 비활성화 되어있거나, Animator가 비활성화 되어있습니다.");
                return;
            }

            if (_nowState == EState.Hiding || _nowState == EState.Hidden)
            {
                return;
            }

            HideImmediately();
        }

        void PreShow() 
        { 
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = _blockRaycasts;
        }
        void PostShow() 
        { 
            _canvasGroup.interactable = _interactable;
            _canvasGroup.blocksRaycasts = _blockRaycasts;
        }

        void PreHide() 
        { 
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = _blockRaycasts;
        }
        void PostHide() 
        { 
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        IEnumerator Co_Play(int hash, Action doneCallback)
        {
            _animator.Rebind();
            _animator.Play(hash);
            _animator.Update(0);

            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            var length    = stateInfo.length;
            yield return CoroutineUtil.WaitForSeconds(length);

            _coRoutine = null;

            doneCallback?.Invoke();
        }
        
        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;

            switch (_nowState)
            {
                case EState.Hidden: PostHide(); break;
                case EState.Showing: PreShow(); break;
                case EState.Shown: PostShow(); break;
                case EState.Hiding: PreHide(); break;
            }
        }
        public void SetBlockRaycasts(bool blockRaycasts)
        {
            _blockRaycasts = blockRaycasts;
            
            switch (_nowState)
            {
                case EState.Hidden: PostHide(); break;
                case EState.Showing: PreShow(); break;
                case EState.Shown: PostShow(); break;
                case EState.Hiding: PreHide(); break;
            }
        }

        //------------------------------------------------------------------------------
        // Defines
        //------------------------------------------------------------------------------
        public enum EState { Showing, Shown, Hiding, Hidden }

        
#if UNITY_EDITOR
        enum EValidCheckResult { Success, AnimatorIsNull, AnimatorControllerIsNull, BaseControllerIsEmpty, BaseControllerIsNotOverride, BaseControllerIsNotSimpleAnimator }

        const string BASE_CONTROLLER_RESOURCES_PATH = "VisibilityUIAnimator/VisibilityUIAnimator";

        [SerializeField, HideInInspector] EValidCheckResult _checkResultType;

        void Reset()
        {
            OnBindSerializedField();
        }
        protected override void OnBindSerializedField()
        {
            base.OnBindSerializedField();

            _animator = this.GetComponent<Animator>();
            _canvasGroup = this.GetComponent<CanvasGroup>();
        }
        public override void OnInspectorGUI()
        {
            var checkResultType = CheckVaildAnimatorController(out var msg);

            if (checkResultType != EValidCheckResult.Success)
                EditorGUILayout.HelpBox(msg, MessageType.Error);

            if (checkResultType != _checkResultType)
            {
                if (checkResultType == EValidCheckResult.Success)
                    Debug.Log($"[{gameObject.name}] : {msg}");
                else
                    Debug.LogError($"[{gameObject.name}] : {msg}");
            }

            _checkResultType = checkResultType;

            base.OnInspectorGUI();
        }

        EValidCheckResult CheckVaildAnimatorController(out string msg)
        {
            if (_animator == null)
            {
                msg = "Animator가 null입니다.";
                return EValidCheckResult.AnimatorIsNull;
            }

            if (_animator.runtimeAnimatorController == null)
            {
                msg = "AnimatorController가 없습니다.";
                return EValidCheckResult.AnimatorControllerIsNull;
            }
                
            // 베이스 컨트롤러 로드
            var baseController = Resources.Load<RuntimeAnimatorController>(BASE_CONTROLLER_RESOURCES_PATH);
            if (baseController == null)
            {
                msg = $"Resources/{BASE_CONTROLLER_RESOURCES_PATH}.controller가 없습니다.";
                return EValidCheckResult.BaseControllerIsEmpty;
            }
            
            // 현재 컨트롤러가 오버라이드 컨트롤러인지 확인
            var overrideController = _animator.runtimeAnimatorController as AnimatorOverrideController;
            if (overrideController == null)
            {
                // 오버라이드 컨트롤러가 아니라면 에러 로그
                msg = $"SimpleAnimator는 오버라이드 컨트롤러가 필요합니다. 'Resources/{BASE_CONTROLLER_RESOURCES_PATH}.controller'를 베이스로 사용해주세요.";
                return EValidCheckResult.BaseControllerIsNotOverride;
            }
            
            // 베이스 컨트롤러가 올바른지 확인 (재귀적으로 체크)
            RuntimeAnimatorController currentBase = overrideController;
            bool foundCorrectBase = false;
            
            while (currentBase != null && currentBase is AnimatorOverrideController)
            {
                currentBase = (currentBase as AnimatorOverrideController).runtimeAnimatorController;
                
                if (currentBase == baseController)
                {
                    foundCorrectBase = true;
                    break;
                }
            }
            
            // 기본 컨트롤러가 베이스 컨트롤러와 같은지 확인
            if (currentBase == baseController)
                foundCorrectBase = true;
            
            if (!foundCorrectBase)
            {
                msg = $"올바른 베이스 컨트롤러를 상속받지 않았습니다. '{BASE_CONTROLLER_RESOURCES_PATH}.controller'를 베이스로 사용해주세요.";
                return EValidCheckResult.BaseControllerIsNotSimpleAnimator;
            }

            msg = "바인딩 성공";
            return EValidCheckResult.Success;
        }
#endif
    }
}