using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine.Diagnostics;

namespace UnityMVC
{
    /// <summary>
    /// In-memory data bus that lets controllers register data providers and query by return type.
    /// </summary>
    public class GameDataBusImpl : IGameDataBus
    {
        #region Properties

        private Dictionary<Type, List<Delegate>> _eventHandlers;

        private static GameDataBusImpl _instance;

        /// <summary>
        /// Lazy singleton instance of the data bus.
        /// </summary>
        public static GameDataBusImpl Instance =>  _instance ?? (_instance = new GameDataBusImpl());

        #endregion
        
        #region Events Methods

        /// <summary>
        /// Registers a data provider for the given return <paramref name="type"/>.
        /// </summary>
        private void _addListener(Type type, Delegate listener)
        {
            Utils.Events.ValidateType(type);
            List<Delegate> handlers;
            if (!Instance._eventHandlers.TryGetValue(type, out handlers))
            {
                handlers = new List<Delegate>();
                Instance._eventHandlers.Add(type, handlers);
            }
            handlers.Add(listener);
        }
        
        /// <summary>
        /// Removes a previously registered provider.
        /// </summary>
        private void _removeListener(Type type, Delegate listener)
        {
            Utils.Events.ValidateType(type);
            List<Delegate> handlers;
            if (!Instance._eventHandlers.TryGetValue(type, out handlers)) return;
            handlers.Remove(listener);
            if (handlers.Count == 0)
            {
                Instance._eventHandlers.Remove(type);
            }
        }
        
        public void AddListenerReflection<T>(Func<T> listener) => _addListener(typeof(T), listener);
        public void AddListener<T>(Func<T> listener) => _addListener(typeof(T), listener);
        public void AddListener<TResult, T>(Func<T, TResult> listener) => _addListener(typeof(TResult), listener);
        public void AddListener<TResult, T1, T2>(Func<T1, T2, TResult> listener)  => _addListener(typeof(TResult), listener);
        public void AddListener<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> listener)  => _addListener(typeof(TResult), listener);
        
        public void RemoveListenerReflection<T>(Func<T> listener) => _removeListener(typeof(T), listener);
        public void RemoveListener<T>(Func<T> listener) => _removeListener(typeof(T), listener);
        public void RemoveListener<TResult, T>(Func<T, TResult> listener) => _removeListener(typeof(TResult), listener);
        public void RemoveListener<TResult, T1, T2>(Func<T1, T2, TResult> listener)  => _removeListener(typeof(TResult), listener);
        public void RemoveListener<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> listener)  => _removeListener(typeof(TResult), listener);
        
        /// <summary>
        /// Retrieves the first registered value of type <typeparamref name="T"/> (clones if it is a <see cref="GameModel"/>).
        /// </summary>
        public T GetData<T>() where T : class
        {
            var type = typeof(T);
            List<Delegate> handlers;
            return Instance._eventHandlers.TryGetValue(type, out handlers) ? (from Func<T> handler in handlers select handler()).FirstOrDefault() : null;
        }
        /// <summary>
        /// Invokes providers for <typeparamref name="T"/> with arguments and returns the first result (clones if it is a <see cref="GameModel"/>).
        /// </summary>
        public T GetData<T>(params object[] args) where T : class
        {
            var type = typeof(T);
            List<Delegate> handlers;
            if (!Instance._eventHandlers.TryGetValue(type, out handlers)) return null;
            foreach (Delegate handler in handlers)
            {
                if (handler.Method.ReturnType != type) throw new Exception($"Return type of handler {handler.Method.Name} is not {type}");
                if (handler.Method.GetParameters().Length != args.Length) throw new Exception($"Handler {handler.Method.Name} has {handler.Method.GetParameters().Length} parameters, expected {args.Length}");
                try
                {
                    var paramTypes = handler.Method.GetParameters().Select(p => p.ParameterType).ToArray();
                    var genericType = Expression.GetFuncType(paramTypes.Concat(new[] { type }).ToArray());
                    var target = handler.Target;
                    var customDelegate = Delegate.CreateDelegate(genericType, target, handler.Method, true);
                    var result = (T) customDelegate.DynamicInvoke(args);
                    if (result is GameModel gameModelInstance) return (T) gameModelInstance.Clone();
                    return result;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error executing handler for type {type}: {ex.Message}", ex);
                }
            }
            return null;
        }
        #endregion
        
        #region Base Service Methods

        private GameDataBusImpl()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_eventHandlers != null) return;
            _eventHandlers = new Dictionary<Type, List<Delegate>>();
        }

        ~GameDataBusImpl()
        {
            foreach (var keyValuePair in _eventHandlers)
            {
                keyValuePair.Value.Clear();
            }
            _eventHandlers.Clear();
            _eventHandlers = null;
            _instance = null;
        }
        #endregion
    }
}
