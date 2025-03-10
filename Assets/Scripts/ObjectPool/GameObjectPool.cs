//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ObjectPool
{
    public class GameObjectPool<T> : IObjectPool, IDisposable where T : MonoBehaviour
    {
        private readonly ObjectPool<T> _pool;
        private readonly T _prefab;
        private readonly Transform _poolRoot;
        private readonly Queue<T> _activeObjects = new();
        private readonly List<T> _allCreatedObjects = new();
        private readonly int _maxActiveObjects;

        public Type ObjectType => typeof(T);

        public GameObjectPool(T prefab, Transform poolRoot, int defaultCapacity = 10, int maxSize = 20)
        {
            if (!prefab) throw new ArgumentNullException(nameof(prefab));
            if (!poolRoot) throw new ArgumentNullException(nameof(poolRoot));

            _prefab = prefab;
            _poolRoot = poolRoot;
            _maxActiveObjects = maxSize;

            _pool = new ObjectPool<T>(
                CreatePooledItem,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                true,
                defaultCapacity,
                maxSize
            );
        }

        private T CreatePooledItem()
        {
            var instance = UnityEngine.Object.Instantiate(_prefab, _poolRoot);
            instance.gameObject.SetActive(false);
            _allCreatedObjects.Add(instance);
            return instance;
        }

        private void OnTakeFromPool(T obj)
        {
            obj.gameObject.SetActive(true);
            MaintainActiveObjectsLimit();
            _activeObjects.Enqueue(obj);
        }

        private void OnReturnedToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_poolRoot);
        }

        private void OnDestroyPoolObject(T obj)
        {
            _allCreatedObjects.Remove(obj);
            if (obj)
            {
                UnityEngine.Object.Destroy(obj.gameObject);
            }
        }

        private void MaintainActiveObjectsLimit()
        {
            while (_activeObjects.Count >= _maxActiveObjects)
            {
                var oldest = _activeObjects.Dequeue();
                _pool.Release(oldest);
            }
        }

        public T Get(Transform parent)
        {
            var item = _pool.Get();
            item.transform.SetParent(parent);
            item.transform.SetPositionAndRotation(parent.position, parent.rotation);
            return item;
        }

        public void Release(T obj) => _pool.Release(obj);

        public void Dispose()
        {
            // 强制回收所有活跃对象
            while (_activeObjects.Count > 0)
            {
                var obj = _activeObjects.Dequeue();
                _pool.Release(obj);
            }

            // 销毁所有已创建对象
            foreach (var obj in _allCreatedObjects.ToArray())
            {
                OnDestroyPoolObject(obj);
            }

            _allCreatedObjects.Clear();
            _pool?.Dispose();
        }

        MonoBehaviour IObjectPool.Get(Transform parent) => Get(parent);
        void IObjectPool.Release(MonoBehaviour obj) => Release(obj as T);
    }
}