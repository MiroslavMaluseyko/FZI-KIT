using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace FZI.Tools
{
    public class FZIPool<T> where T : class
    {
        private ObjectPool<T> _pool;
        private List<T> _activeObjects;

        public List<T> ActiveObjects => _activeObjects;

        public FZIPool(Func<T> onCreate, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null)
        {
            _activeObjects = new List<T>();
            _pool = new ObjectPool<T>(
                () => onCreate?.Invoke(),
                obj =>
                {
                    onGet?.Invoke(obj);
                    _activeObjects.Add(obj);
                },
                obj =>
                {
                    onRelease?.Invoke(obj);
                    _activeObjects.Remove(obj);
                },
                obj => onDestroy?.Invoke(obj));
        }

        public T Get() => _pool.Get();
        public void Release(T obj) => _pool.Release(obj);
        
        public void Clear()
        {
            foreach (T obj in new List<T>(_activeObjects))
            {
                _pool.Release(obj);
            }
        }
    }
}