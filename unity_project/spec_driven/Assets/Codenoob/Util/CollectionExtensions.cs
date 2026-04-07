using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Codenoob.Util
{
    public static class CollectionExtensions
    {
        public static bool IsWithinRange<T>(this IList<T> src, int index)
        {
            if (src == null)
            {
                Debug.LogAssertion($"{nameof(IsWithinRange)} : {nameof(src)}가 null 입니다.");
                return default;
            }

            return 0 <= index && index < src.Count;
        }
        public static bool IsOutOfRange<T>(this IList<T> src, int index)
        {
            if (src == null)
            {
                Debug.LogAssertion($"{nameof(IsOutOfRange)} : {nameof(src)}가 null 입니다.");
                return default;
            }

            return index < 0 || src.Count <= index;
        }

        public static bool TryGetValue<T>(this IList<T> list, int index, out T value)
        {
            value = default;
            {
                if (null == list) return false;
                if (index < 0) return false;
                if (list.Count <= index) return false;

                value = list[index];
            }
            return true;
        }

        public static T GetOrNew<K, T>(this IDictionary<K, T> src, K key) where T : class, new()
        {
            if (src == null)
            {
                Debug.LogAssertion($"{nameof(GetOrNew)} : {nameof(src)}가 null 입니다.");
                return default;
            }

            if (!src.TryGetValue(key, out var value))
                src.Add(key, value = new T());
            return value;
        }
        public static T[] GetOrNewArray<K, T>(this IDictionary<K, T[]> src, K key, int length) where T : class, new()
        {
            if (src == null)
            {
                Debug.LogAssertion($"{nameof(GetOrNewArray)} : {nameof(src)}가 null 입니다.");
                return default;
            }

            if (!src.TryGetValue(key, out var value))
                src.Add(key, value = new T[length]);
            return value;
        }

        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str) || string.CompareOrdinal(str, "null") == 0;
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection) => null == collection || collection.Count <= 0;



        #region Parse
        public static bool BoolValue(this string o, bool fallback = default)        => bool.TryParse(o, out var result) ? result : fallback;

        public static float FloatValue(this string o, float fallback = default)     => float.TryParse(o, NumberStyles.Float, new CultureInfo("en-US"), out var result) ? result : fallback;
        public static double DoubleValue(this string o, double fallback = default)  => double.TryParse(o, NumberStyles.Float, new CultureInfo("en-US"), out var result) ? result : fallback;

        public static sbyte SbyteValue(this string o, sbyte fallback = default)     => sbyte.TryParse(o, out var result) ? result : fallback;
        public static short ShortValue(this string o, short fallback = default)     => short.TryParse(o, out var result) ? result : fallback;
        public static int IntValue(this string o, int fallback = default)           => int.TryParse(o, out var result) ? result : fallback;
        public static long LongValue(this string o, long fallback = default)        => long.TryParse(o, out var result) ? result : fallback;

        public static byte ByteValue(this string o, byte fallback = default)        => byte.TryParse(o, out var result) ? result : fallback;
        public static ushort UshortValue(this string o, ushort fallback = default)  => ushort.TryParse(o, out var result) ? result : fallback;
        public static uint UintValue(this string o, uint fallback = default)        => uint.TryParse(o, out var result) ? result : fallback;
        public static ulong UlongValue(this string o, ulong fallback = default)     => ulong.TryParse(o, out var result) ? result : fallback;
        #endregion// Parse


        #region ToList
        public static List<string> ToStringList(this string s, char separator = ',')
        {
            var result = new List<string>();
            {
                if (s.IsNullOrEmpty() || '\0' == separator)
                    return result;

                var ss = s.Split(separator);
                if (null == ss || 0 == ss.Length)
                    return result;

                result = ss.ToList();
            }
            return result;
        }


        public static List<T> ToList<T>(this string s, Func<string, T> onConvert, char separator = ',')
        {
            var result = new List<T>();
            {
                if (s.IsNullOrEmpty() || '\0' == separator)
                    return result;

                var ss = s.Split(separator);
                if (null == ss || 0 == ss.Length)
                    return result;

                for (int i = 0, size = ss.Length; i < size; ++i)
                {
                    if (ss[i].IsNullOrEmpty())
                        continue;
                    result.Add(onConvert(ss[i]));
                }
            }
            return result;
        }

        public static List<bool> ToBoolList(this string s, char separator = ',')        => s.ToList(value => value.BoolValue(), separator);

        public static List<float> ToFloatList(this string s, char separator = ',')      => s.ToList(value => value.FloatValue(), separator);
        public static List<double> ToDoubleList(this string s, char separator = ',')    => s.ToList(value => value.DoubleValue(), separator);

        public static List<sbyte> ToSbyteList(this string s, char separator = ',')      => s.ToList(value => value.SbyteValue(), separator);
        public static List<short> ToShortList(this string s, char separator = ',')      => s.ToList(value => value.ShortValue(), separator);
        public static List<int> ToIntList(this string s, char separator = ',')          => s.ToList(value => value.IntValue(), separator);
        public static List<long> ToLongList(this string s, char separator = ',')        => s.ToList(value => value.LongValue(), separator);

        public static List<byte> ToByteList(this string s, char separator = ',')        => s.ToList(value => value.ByteValue(), separator);
        public static List<ushort> ToUshortList(this string s, char separator = ',')    => s.ToList(value => value.UshortValue(), separator);
        public static List<uint> ToUintList(this string s, char separator = ',')        => s.ToList(value => value.UintValue(), separator);
        public static List<ulong> ToUlongList(this string s, char separator = ',')      => s.ToList(value => value.UlongValue(), separator);
        #endregion// ToList


        #region Shuffle
        public static void Shuffle<T>(this IList<T> list, int? seed = null)
        {
            // HACK : Unity에서 Array 유형이 IsReadOnly로 반환되는 버그가 있어 조건 추가함
            if (list.IsReadOnly && !(list is T[]))
            {
                Debug.LogAssertion($"{nameof(Shuffle)}<{typeof(T).Name}> : {nameof(list)}가 읽기 전용 입니다");
                return;
            }

            if (list.Count < 2)
                return;

            var oldState = UnityRandom.state;
            UnityRandom.InitState(seed.HasValue ? seed.Value : (int)DateTime.UtcNow.Ticks);
            {
                int halfIndex = list.Count / 2;
                // 홀수일때 셔플범위 1증가
                var randRange = halfIndex + ((list.Count & 1) == 0 ? 0 : 1);
                T tmp;
                for (int no = 0; no < 5; ++no)
                {
                    for (int index = 0; index < halfIndex; ++index)
                    {
                        var randIndex = UnityRandom.Range(0, randRange) + halfIndex;
                        tmp = list[index];
                        list[index] = list[randIndex];
                        list[randIndex] = tmp;
                    }
                }
            }
            UnityRandom.state = oldState;
        }

        public static void FisherYatesShuffle<T>(IList<T> list, int? seed = null)
        {
            // HACK : Unity에서 Array 유형이 IsReadOnly로 반환되는 버그가 있어 조건 추가함
            if (list.IsReadOnly && !(list is T[]))
            {
                Debug.LogAssertion($"{nameof(FisherYatesShuffle)}<{typeof(T).Name}> : {nameof(list)}가 읽기 전용 입니다");
                return;
            }

            if (list.Count < 2)
                return;

            var oldState = UnityRandom.state;
            UnityRandom.InitState(seed.HasValue ? seed.Value : (int)DateTime.UtcNow.Ticks);
            {
                for (int index = list.Count; 1 < index; --index)
                {
                    int randIndex = UnityRandom.Range(0, index);
                    var setIndex = index - 1;
                    if (randIndex == setIndex)
                    {
                        if ((setIndex & 1) == 1)
                            continue;
                        randIndex = setIndex - 1;
                    }

                    T tmp = list[randIndex];
                    list[randIndex] = list[setIndex];
                    list[setIndex] = tmp;
                }
            }
            UnityRandom.state = oldState;
        }


        [Obsolete("Shuffle을 이용하세요")] public static void ShuffleArray<T>(this T[] array) => array.Shuffle();
        [Obsolete("Shuffle을 이용하세요")] public static void ShuffleList<T>(this IList<T> list) => list.Shuffle();

        [Obsolete("Shuffle을 이용하세요")] public static void ShffleTArray<T>(this T[] array) => array.Shuffle();
        [Obsolete("Shuffle을 이용하세요")] public static void ShuffleIntArray(this int[] array) => array.Shuffle();
        [Obsolete("Shuffle을 이용하세요")] public static void ShuffleStringList(this string[] array) => array.Shuffle();

        [Obsolete("Shuffle을 이용하세요")] public static void ShuffleTList<T>(this List<T> list) => list.Shuffle();
        [Obsolete("Shuffle을 이용하세요")] public static void ShuffleIntList(this List<int> list) => list.Shuffle();
        [Obsolete("Shuffle을 이용하세요")] public static void ShuffleStringList(this List<string> list) => list.Shuffle();
        #endregion// Shuffle
    }
}