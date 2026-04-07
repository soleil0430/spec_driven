using System;
using System.Collections;
using UnityEngine;

namespace Codenoob.Util
{
    public static class CoroutineUtil
    {
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();


        public static IEnumerator WaitForSeconds(double sec)
        {
            var secDone = Time.timeAsDouble + sec;
            while (Time.timeAsDouble < secDone)
                yield return null;
        }
        public static IEnumerator WaitForSecondsUnscaled(double sec)
        {
            var secDone = Time.unscaledTimeAsDouble + sec;
            while (Time.unscaledTimeAsDouble < secDone)
                yield return null;
        }
        public static IEnumerator WaitForSecondsRealtime(double sec)
        {
            var secDone = Time.realtimeSinceStartupAsDouble + sec;
            while (Time.realtimeSinceStartupAsDouble < secDone)
                yield return null;
        }

        public static IEnumerator WaitUntil(Func<bool> predicate)
        {
            if (predicate != null)
            {
                while (!predicate())
                    yield return null;
            }
        }
        public static IEnumerator WaitForSecondsOrUntil(double sec, Func<bool> predicate)           => WaitForSeconds_PredicateJob(sec, predicate, true, true);
        public static IEnumerator WaitForSecondsAndUntil(double sec, Func<bool> predicate)          => WaitForSeconds_PredicateJob(sec, predicate, false, true);
        public static IEnumerator WaitForSecondsUnscaledOrUntil(double sec, Func<bool> predicate)   => WaitForUnscaled_PredicateJob(sec, predicate, true, true);
        public static IEnumerator WaitForSecondsUnscaledAndUntil(double sec, Func<bool> predicate)  => WaitForUnscaled_PredicateJob(sec, predicate, false, true);
        public static IEnumerator WaitForSecondsRealtimeOrUntil(double sec, Func<bool> predicate)   => WaitForRealtime_PredicateJob(sec, predicate, true, true);
        public static IEnumerator WaitForSecondsRealtimeAndUntil(double sec, Func<bool> predicate)  => WaitForRealtime_PredicateJob(sec, predicate, false, true);

        public static IEnumerator WaitWhile(Func<bool> predicate)
        {
            if (predicate != null)
            {
                while (predicate())
                    yield return null;
            }
        }
        public static IEnumerator WaitForSecondsOrWhile(double sec, Func<bool> predicate)           => WaitForSeconds_PredicateJob(sec, predicate, true, false);
        public static IEnumerator WaitForSecondsAndWhile(double sec, Func<bool> predicate)          => WaitForSeconds_PredicateJob(sec, predicate, false, false);
        public static IEnumerator WaitForSecondsUnscaledOrWhile(double sec, Func<bool> predicate)   => WaitForUnscaled_PredicateJob(sec, predicate, true, false);
        public static IEnumerator WaitForSecondsUnscaledAndWhile(double sec, Func<bool> predicate)  => WaitForUnscaled_PredicateJob(sec, predicate, false, false);
        public static IEnumerator WaitForSecondsRealtimeOrWhile(double sec, Func<bool> predicate)   => WaitForRealtime_PredicateJob(sec, predicate, true, false);
        public static IEnumerator WaitForSecondsRealtimeAndWhile(double sec, Func<bool> predicate)  => WaitForRealtime_PredicateJob(sec, predicate, false, false);


        static IEnumerator WaitForSeconds_PredicateJob(double sec, Func<bool> predicate, bool isOr, bool isUntil)
        {
            var secDone = Time.timeAsDouble + sec;
            if (predicate == null)
                while (Time.timeAsDouble < secDone)
                    yield return null;
            else
            {
                if (isOr)
                {
                    if (isUntil)
                        while (Time.timeAsDouble < secDone && !predicate())
                            yield return null;
                    else
                        while (Time.timeAsDouble < secDone && predicate())
                            yield return null;
                }
                else
                {
                    if (isUntil)
                        while (Time.timeAsDouble < secDone || !predicate())
                            yield return null;
                    else
                        while (Time.timeAsDouble < secDone || predicate())
                            yield return null;
                }
            }
        }
        static IEnumerator WaitForUnscaled_PredicateJob(double sec, Func<bool> predicate, bool isOr, bool isUntil)
        {
            var secDone = Time.unscaledTimeAsDouble + sec;
            if (predicate == null)
                while (Time.unscaledTimeAsDouble < secDone)
                    yield return null;
            else
            {
                if (isOr)
                {
                    if (isUntil)
                        while (Time.unscaledTimeAsDouble < secDone && !predicate())
                            yield return null;
                    else
                        while (Time.unscaledTimeAsDouble < secDone && predicate())
                            yield return null;
                }
                else
                {
                    if (isUntil)
                        while (Time.unscaledTimeAsDouble < secDone || !predicate())
                            yield return null;
                    else
                        while (Time.unscaledTimeAsDouble < secDone || predicate())
                            yield return null;
                }
            }
        }
        static IEnumerator WaitForRealtime_PredicateJob(double sec, Func<bool> predicate, bool isOr, bool isUntil)
        {
            var secDone = Time.realtimeSinceStartupAsDouble + sec;
            if (predicate == null)
                while (Time.realtimeSinceStartupAsDouble < secDone)
                    yield return null;
            else
            {
                if (isOr)
                {
                    if (isUntil)
                        while (Time.realtimeSinceStartupAsDouble < secDone && !predicate())
                            yield return null;
                    else
                        while (Time.realtimeSinceStartupAsDouble < secDone && predicate())
                            yield return null;
                }
                else
                {
                    if (isUntil)
                        while (Time.realtimeSinceStartupAsDouble < secDone || !predicate())
                            yield return null;
                    else
                        while (Time.realtimeSinceStartupAsDouble < secDone || predicate())
                            yield return null;
                }
            }
        }
    }
}