using System;
using UnityEngine;

namespace CryoDI.Providers
{
    public class UnitySingletonProvider<T> : IObjectProvider where T : Component
    {
        private readonly ILifeTimeManager _lifeTimeManager;
        private bool _exist;
        private T _instance;

        public UnitySingletonProvider(ILifeTimeManager lifeTimeManager, LifeTime lifeTime)
            : this(Activator.CreateInstance<T>, lifeTimeManager, lifeTime)
        {
        }

        public UnitySingletonProvider(Func<T> factoryMethod, ILifeTimeManager lifeTimeManager, LifeTime lifeTime)
        {
            _lifeTimeManager = lifeTimeManager;
            LifeTime = lifeTime;
        }

        public LifeTime LifeTime { get; }               

        public object GetObject(object owner, CryoContainer container, params object[] parameters)
        {
            if (!_exist)
            {
                UnityEngine.Object prefab = Resources.Load("Singletons/" + typeof(T).Name);

                var instance = UnityEngine.Object.Instantiate(prefab) as GameObject;
                instance.name = typeof(T).ToString();

                _instance = instance.GetComponent(typeof(T)) as T;
                _exist = true;

                container.BuildUp(_instance, parameters);
                _lifeTimeManager.Add(this, LifeTime);

                UnityEngine.Object.DontDestroyOnLoad(_instance);
            }

            return _instance;
        }

        public object WeakGetObject(CryoContainer container, params object[] parameters)
        {
            if (_exist)
                return _instance;

            return null;
        }

        public void Dispose()
        {
            if (!_exist)
                return;

            if (LifeTime != LifeTime.External)
            {
                var disposable = _instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            _instance = default;
            _exist = false;
        }
    }
}