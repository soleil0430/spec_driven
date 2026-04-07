using System.Collections.Generic;
using UnityEngine;

namespace Codenoob.Common
{
    public static class Identifier<TIdentity> where TIdentity : class
    {
        //------------------------------------------------------------------------------
        // Variables
        //------------------------------------------------------------------------------
        static Dictionary<int, TIdentity> _dict = new Dictionary<int, TIdentity>();

        //------------------------------------------------------------------------------
        // Methods
        //------------------------------------------------------------------------------
        public static void Regist(Collider collider, TIdentity identity) 
        { 
            if (collider == null)
                return;
            
            var instanceID = collider.GetInstanceID();
            if (_dict.ContainsKey(instanceID))
                return;

            _dict.Add(instanceID, identity);
        }
        public static void Regist(IList<Collider> colliders, TIdentity identity) 
        { 
            if (colliders == null)
                return;

            foreach (var collider in colliders)
                Regist(collider, identity);
        }

        public static void Unregist(Collider collider) 
        { 
            if (collider == null)
                return;

            var instanceID = collider.GetInstanceID();
            if (_dict.ContainsKey(instanceID) == false)
                return;

            _dict.Remove(instanceID);
        }
        public static void Unregist(IList<Collider> colliders) 
        { 
            if (colliders == null)
                return;

            foreach (var collider in colliders)
                Unregist(collider);
        }

        public static bool TryIdentify(Collider collider, out TIdentity identity) 
        { 
            identity = default;

            if (collider == null)
                return false;

            var instanceID = collider.GetInstanceID();
            if (_dict.TryGetValue(instanceID, out identity) == false)
                return false;

            return true;
        }
        
        public static TIdentity Identify(Collider collider) 
        { 
            if (collider == null)
                return default;

            var instanceID = collider.GetInstanceID();
            if (_dict.TryGetValue(instanceID, out var identity) == false)
                return default;

            return identity;
        }

        public static void ClearAll() { _dict.Clear(); } 
    }
}