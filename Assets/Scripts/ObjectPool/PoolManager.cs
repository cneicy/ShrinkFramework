//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ObjectPool
{
    public static class PoolManager
    {
        private static readonly Dictionary<string, IObjectPool> Pools = new();
        private static Transform _poolRoot;

        private static Transform PoolRoot
        {
            get
            {
                if (_poolRoot) return _poolRoot;
                _poolRoot = new GameObject("PoolManager").transform;
                UnityEngine.Object.DontDestroyOnLoad(_poolRoot);
                return _poolRoot;
            }
        }

        public static void CreatePool<T>(string poolId, T prefab, int defaultCapacity = 10, int maxSize = 20) 
            where T : MonoBehaviour
        {
            if (string.IsNullOrEmpty(poolId))
            {
                Debug.LogError("池ID不能为空");
                return;
            }

            if (Pools.ContainsKey(poolId))
            {
                Debug.LogWarning($"对象池 '{poolId}' 已存在");
                return;
            }

            var container = new GameObject($"{poolId}_Container").transform;
            container.SetParent(PoolRoot);

            try
            {
                var pool = new GameObjectPool<T>(prefab, container, defaultCapacity, maxSize);
                Pools.Add(poolId, pool);
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建对象池失败: {ex.Message}");
                UnityEngine.Object.Destroy(container.gameObject);
            }
        }

        public static T Get<T>(string poolId, Transform parent = null) where T : MonoBehaviour
        {
            if (!Pools.TryGetValue(poolId, out var pool))
            {
                Debug.LogError($"对象池 '{poolId}' 不存在");
                return null;
            }

            if (pool.ObjectType == typeof(T)) return pool.Get(parent ?? PoolRoot) as T;
            Debug.LogError($"类型不匹配: 请求的 {typeof(T)} 但池包含 {pool.ObjectType}");
            return null;

        }

        public static void Release(string poolId, MonoBehaviour obj)
        {
            if (!Pools.TryGetValue(poolId, out var pool))
            {
                Debug.LogError($"对象池 '{poolId}' 不存在");
                return;
            }
            if (obj.GetType() != pool.ObjectType)
            {
                Debug.LogError($"类型不匹配: 尝试释放 {obj.GetType()} 到包含 {pool.ObjectType} 的池");
                return;
            }

            pool.Release(obj);
        }

        public static void DisposePool(string poolId)
        {
            if (!Pools.TryGetValue(poolId, out var pool)) return;

            try
            {
                if (pool is IDisposable disposablePool)
                {
                    disposablePool.Dispose();
                }

                var container = PoolRoot.Find($"{poolId}_Container");
                if (container)
                {
                    UnityEngine.Object.Destroy(container.gameObject);
                }
            }
            finally
            {
                Pools.Remove(poolId);
            }
        }

        public static void DisposeAllPools()
        {
            foreach (var poolId in Pools.Keys.ToArray())
            {
                DisposePool(poolId);
            }
        }
    }
}