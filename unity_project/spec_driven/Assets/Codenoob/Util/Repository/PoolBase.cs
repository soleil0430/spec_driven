using System;
using System.Collections.Generic;
using UnityEngine;

namespace Codenoob.Util
{
    public abstract class PoolBase<T> : IPoolBase where T : class
    {
        public const int Limit = int.MaxValue - 56;
        readonly HashSet<T> actives = new HashSet<T>();
        readonly List<T> inactives = new List<T>();

        public readonly T Origin = default;
        readonly Func<T, T> generator = null;
        readonly Action<T> terminator = null;

        public int Capacity { private set; get; } = 0;
        public int Count => actives.Count + inactives.Count;
        public int ActiveCount => actives.Count;
        public int InactiveCount => inactives.Count;
        public int AvailableCount => Capacity - actives.Count;
        public int EmptyCount => Capacity - Count;



        public PoolBase(T origin, Func<T, T> generator, Action<T> terminator, int capacity)
        {
            if (generator == null) throw new Exception($"{nameof(PoolBase<T>)} : {nameof(generator)} is null");
            if (terminator == null) throw new Exception($"{nameof(PoolBase<T>)} : {nameof(terminator)} is null");

            Origin = origin;
            this.generator = generator;
            this.terminator = terminator;
            Resize(capacity);
            Prepare(capacity, true);
        }

        protected virtual void OnGenerate(T item) { }
        protected virtual void OnGet(T item) { }
        protected virtual void OnReturn(T item) { }
        protected virtual void OnTerminate(T item) { }



        protected static bool IsTrueNull(T obj) => obj is null;

        protected static int GetOptimalCapacity(int count)
        {
            if (count < 4) return 4;
            if (Limit <= count) return Limit;

            int round = 0;
            uint number = (uint)count;
            while (1 < number)
            {
                ++round;
                number >>= 1;
            }

            number <<= round;
            if (number < count)
                number <<= 1;
            return number < Limit ? (int)number : Limit;
        }


        bool TryGenerate(out T item)
        {
            item = null;

            try
            {
                item = generator(Origin);
                if (item == null)
                {
                    Debug.LogAssertion($"{nameof(TryGenerate)} : {nameof(item)} is null");

                    if (!IsTrueNull(item))
                        TerminateEvent(item);
                    return false;
                }

                GenerateEvent(item);
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                if (!IsTrueNull(item))
                    TerminateEvent(item);
                return false;
            }

            return true;
        }

        void GenerateEvent(T item)
        {
            if (item is IPoolObject iObj)
            {
                iObj.OwnPool = this;
                OnGenerate(item);
                iObj.OnGenerate();
            }
            else
                OnGenerate(item);
        }
        void GetEvent(T item)
        {
            OnGet(item);
            if (item is IPoolObject iObj)
                iObj.OnGet();
        }
        void ReturnEvent(T item)
        {
            OnReturn(item);
            if (item is IPoolObject iObj)
                iObj.OnReturn();
        }
        void TerminateEvent(T item)
        {
            try
            {
                OnTerminate(item);
                if (item is IPoolObject iObj)
                {
                    iObj.OnTerminate();
                    iObj.OwnPool = null;
                }
            }
            catch (Exception e) { Debug.LogException(e); }

            try { terminator?.Invoke(item); }
            catch (Exception e) { Debug.LogException(e); }
        }


        protected void TrimExcess()
        {
            actives.TrimExcess();
            inactives.Capacity = GetOptimalCapacity(inactives.Count);
        }

        public virtual void Resize(int capacity, bool trimExcess = false)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (Capacity == capacity) return;
            if (Limit < capacity)
            {
                Debug.Log($"{nameof(Resize)} : {nameof(capacity)} greater than {nameof(Limit)}({capacity}/{Limit})");
                return;
            }
            if (capacity < actives.Count)
            {
                Debug.LogAssertion($"{nameof(Resize)} : When you reduce the size, please return the object first, {nameof(capacity)}({capacity}), {nameof(ActiveCount)}({ActiveCount})");
                return;
            }

            var decreaseCount = Capacity - capacity;
            Capacity = capacity;
            if (decreaseCount < 1)
                return;

            decreaseCount -= actives.Count;
            if (decreaseCount < inactives.Count)
            {
                int startIndex = inactives.Count - decreaseCount;
                for (int index = startIndex; index < inactives.Count; ++index)
                    TerminateEvent(inactives[index]);
                inactives.RemoveRange(startIndex, decreaseCount);
            }
            else
            {
                for (int index = 0; index < inactives.Count; ++index)
                    TerminateEvent(inactives[index]);
                inactives.Clear();
            }

            if (trimExcess)
                TrimExcess();
        }

        public void Prepare(int count, bool increaseCapacity = true)
        {
            if (count == 0) return;
            if (count < 1)
            {
                Debug.LogAssertion($"{nameof(Prepare)} : {nameof(count)} less then 0");
                return;
            }

            if (increaseCapacity && EmptyCount < count)
                Resize(Capacity + count);

            if (EmptyCount < count)
            {
                Debug.LogAssertion($"{nameof(Prepare)} : {nameof(count)} greater than {nameof(EmptyCount)}({count}/{EmptyCount})");
                return;
            }

            var increaseCount = inactives.Count + count - inactives.Capacity;
            if (0 < increaseCount)
                inactives.Capacity = GetOptimalCapacity(inactives.Capacity + increaseCount);

            for (int index = 0; index < count; ++index)
            {
                if (TryGenerate(out var item))
                {
                    ReturnEvent(item);
                    inactives.Add(item);
                }
            }
        }

        public void SortInactives(Comparison<T> comparison) => inactives.Sort(comparison);



        bool IPoolBase.Contains(object item) => Contains(item as T);
        public bool Contains(T item) => actives.Contains(item) || inactives.Contains(item);
        public T Get(bool increaseCapacity = true)
        {
            if (0 < inactives.Count)
            {
                var item = inactives[inactives.Count - 1];
                inactives.RemoveAt(inactives.Count - 1);
                actives.Add(item);
                GetEvent(item);
                return item;
            }

            if (increaseCapacity && EmptyCount < 1)
                Resize(Capacity + 1);

            if (0 < EmptyCount)
            {
                if (TryGenerate(out var item))
                {
                    actives.Add(item);
                    GetEvent(item);
                    return item;
                }
            }

            return null;
        }

        public IEnumerator<T> GetActives()
        {
            foreach (var item in actives)
                yield return item;
        }
        public IEnumerator<T> GetInactives()
        {
            foreach (var item in inactives)
                yield return item;
        }

        public T[] CloneActives()
        {
            var result = new T[actives.Count];
            actives.CopyTo(result);
            return result;
        }
        public T[] CloneInactives() => inactives.ToArray();


        void IPoolBase.Return(object item) => Return(item as T);
        public void Return(T item)
        {
            if (IsTrueNull(item))
            {
                Debug.LogAssertion($"{nameof(Return)} : {nameof(item)} is null");
                return;
            }

            if (actives.Remove(item))
            {
                ReturnEvent(item);

                if (item == null)
                    TerminateEvent(item);
                else
                    inactives.Add(item);
            }
        }

        public void ReturnAll()
        {
            foreach (var item in actives)
                ReturnEvent(item);
            inactives.AddRange(actives);
            actives.Clear();
        }

        void IPoolBase.Terminate(object item) => Terminate(item as T);
        public void Terminate(T item)
        {
            if (IsTrueNull(item))
            {
                Debug.LogAssertion($"{nameof(Terminate)} : {nameof(item)} is null");
                return;
            }

            if (actives.Remove(item))
            {
                ReturnEvent(item);
                TerminateEvent(item);
            }
            else if (inactives.Remove(item))
            {
                TerminateEvent(item);
            }
        }

        public virtual void Clear(bool trimExcess = false)
        {
            foreach (var item in actives)
            {
                ReturnEvent(item);
                TerminateEvent(item);
            }
            actives.Clear();

            for (int index = 0; index < inactives.Count; ++index)
                TerminateEvent(inactives[index]);
            inactives.Clear();

            if (trimExcess)
                TrimExcess();
        }
    }
}
