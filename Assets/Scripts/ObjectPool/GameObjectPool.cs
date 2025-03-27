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
using Object = UnityEngine.Object;

namespace ObjectPool
{
    public class GameObjectPool<T> : IObjectPool where T : MonoBehaviour
    {
        private readonly List<T> _activeObjects = new();
        private readonly int _maxActiveObjects;
        private readonly ObjectPool<T> _pool;
        private readonly Transform _poolRoot;
        private readonly T _prefab;

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
        

        public Type ObjectType => typeof(T);

        MonoBehaviour IObjectPool.Get(Transform parent)
        {
            return Get(parent);
        }

        void IObjectPool.Release(MonoBehaviour obj)
        {
            Release(obj as T);
        }

        private T CreatePooledItem()
        {
            var instance = Object.Instantiate(_prefab, _poolRoot);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private void OnTakeFromPool(T obj)
        {
            obj.gameObject.SetActive(true);
            _activeObjects.Add(obj);
            MaintainActiveObjectsLimit();
        }

        private void OnReturnedToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_poolRoot);
            _activeObjects.Remove(obj);
        }

        private void OnDestroyPoolObject(T obj)
        {
            if (obj) Object.Destroy(obj.gameObject);
        }

        private void MaintainActiveObjectsLimit()
        {
            while (_activeObjects.Count > _maxActiveObjects)
            {
                var oldest = _activeObjects[0];
                _activeObjects.RemoveAt(0);
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

        public void Release(T obj)
        {
            _pool.Release(obj);
        }
    }
}