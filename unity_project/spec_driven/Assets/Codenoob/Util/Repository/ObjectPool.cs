using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Codenoob.Util
{
    public sealed class ObjectPool<T> : PoolBase<T> where T : Component, IPoolObject
    {
        public bool InactiveObject = true;
        private Transform parent;

        public ObjectPool(T origin, Func<T, T> generator, int capacity) : base(origin, generator, Terminator, capacity) { }
        public ObjectPool(T origin, Transform initParent, int capacity) : base
        (
            origin,
            o =>
            {
                var item = UnityObject.Instantiate(o, initParent);
                if (item != null)
                    item.name = o.name;
                return item;
            },
            Terminator,
            capacity
        ) { }
        public ObjectPool(T origin, Transform initParent, Action<T> initializer, int capacity) : base
        (
            origin,
            o =>
            {
                var item = UnityObject.Instantiate(o, initParent);
                if (item != null)
                {
                    item.name = o.name;
                    initializer?.Invoke(item);
                }
                return item;
            },
            Terminator,
            capacity
        ) { }


        static void Terminator(T item)
        {
            if (item != null)
                UnityObject.Destroy(item.gameObject);
        }

        protected override void OnReturn(T item)
        {
            base.OnReturn(item);
            if (item == null)
                return;

            if (InactiveObject)
                item.gameObject.SetActive(false);
        }
    }
}