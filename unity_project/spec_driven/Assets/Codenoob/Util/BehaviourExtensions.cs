using System;
using System.Collections;
using UnityEngine;

namespace Codenoob.Util
{
    public static class BehaviourExtensions
    {
        #region Coroutine, Invoke
        public static Coroutine Invoke(this MonoBehaviour src, Action callback) => src.StartCoroutine(WaitForInvoke(null, callback));
        public static Coroutine Invoke(this MonoBehaviour src, IEnumerator enumerator, Action callback) => src.StartCoroutine(WaitForInvoke(enumerator, callback));
        public static Coroutine Invoke(this MonoBehaviour src, YieldInstruction instruction, Action callback) => src.StartCoroutine(WaitForInvoke(instruction, callback));
        static IEnumerator WaitForInvoke(object job, Action callback)
        {
            yield return job;
            callback?.Invoke();
        }


        public static void ReleaseCoroutine(this MonoBehaviour src, ref Coroutine target)
        {
            if (target != null)
                src.StopCoroutine(target);
            target = null;
        }
        public static void SwapCoroutine(this MonoBehaviour src, ref Coroutine target, IEnumerator routine)
        {
            if (target != null)
                src.StopCoroutine(target);
            target = routine == null ? null : src.StartCoroutine(routine);
        }
        public static Coroutine StopAllAndStartCoroutine(this MonoBehaviour src, IEnumerator routine)
        {
            src.StopAllCoroutines();
            return src.StartCoroutine(routine);
        }

        public static void CancelAndInvoke(this MonoBehaviour src, string methodName, float time)
        {
            src.CancelInvoke(methodName);
            src.Invoke(methodName, time);
        }
        public static void CancelAllAndInvoke(this MonoBehaviour src, string methodName, float time)
        {
            src.CancelInvoke();
            src.Invoke(methodName, time);
        }
        #endregion// Coroutine, Invoke


        #region Canvas
        public static void Show(this CanvasGroup cg, bool blocksRaycasts = true, bool interactable = true)
        {
            cg.alpha = 1.0f;
            cg.blocksRaycasts = blocksRaycasts;
            cg.interactable = interactable;
        }
        public static void Hide(this CanvasGroup cg, bool blocksRaycasts = false, bool interactable = false)
        {
            cg.alpha = 0.0f;
            cg.blocksRaycasts = blocksRaycasts;
            cg.interactable = interactable;
        }
        #endregion// Canvas
    }
}