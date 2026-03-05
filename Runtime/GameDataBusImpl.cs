using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UnityMVC
{
    /// <summary>
    /// In-memory data bus that lets controllers register data providers and query by return type.
    /// </summary>
    public class GameDataBusImpl : IGameDataBus
    {
        #region Properties
        
        private sealed class DelegateInvoker
        {
            public int ParameterCount { get; }
            public Type ReturnType { get; }
            public Func<Delegate, object[], object> Invoke { get; }

            public DelegateInvoker(int parameterCount, Type returnType, Func<Delegate, object[], object> invoke)
            {
                ParameterCount = parameterCount;
                ReturnType = returnType;
                Invoke = invoke;
            }
        }

        private Dictionary<Type, List<Delegate>> _eventHandlers;
        private Dictionary<Type, DelegateInvoker> _delegateInvokersByType;

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

        private DelegateInvoker GetOrCreateDelegateInvoker(Delegate handler)
        {
            var delegateType = handler.GetType();
            if (Instance._delegateInvokersByType.TryGetValue(delegateType, out var cachedInvoker))
            {
                return cachedInvoker;
            }

            var invokeMethod = delegateType.GetMethod("Invoke");
            if (invokeMethod == null)
            {
                throw new InvalidOperationException($"Cannot resolve Invoke method for delegate type {delegateType.FullName}");
            }

            var parameters = invokeMethod.GetParameters();
            var parameterCount = parameters.Length;
            var handlerParameter = Expression.Parameter(typeof(Delegate), "handler");
            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");
            var castedHandler = Expression.Convert(handlerParameter, delegateType);
            var callArguments = new Expression[parameterCount];
            for (var index = 0; index < parameterCount; index++)
            {
                var parameterType = parameters[index].ParameterType;
                var argumentAtIndex = Expression.ArrayIndex(argumentsParameter, Expression.Constant(index));
                callArguments[index] = Expression.Convert(argumentAtIndex, parameterType);
            }

            var invokeExpression = Expression.Invoke(castedHandler, callArguments);
            var boxedResult = Expression.Convert(invokeExpression, typeof(object));
            var lambda = Expression.Lambda<Func<Delegate, object[], object>>(boxedResult, handlerParameter, argumentsParameter);
            var compiledInvoker = lambda.Compile();
            var delegateInvoker = new DelegateInvoker(parameterCount, invokeMethod.ReturnType, compiledInvoker);
            Instance._delegateInvokersByType.Add(delegateType, delegateInvoker);
            return delegateInvoker;
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
            if (!Instance._eventHandlers.TryGetValue(type, out var handlers))
            {
                return null;
            }

            foreach (Delegate handler in handlers)
            {
                var result = ((Func<T>)handler).Invoke();
                return result;
            }

            return null;
        }
        /// <summary>
        /// Invokes providers for <typeparamref name="T"/> with arguments and returns the first result (clones if it is a <see cref="GameModel"/>).
        /// </summary>
        public T GetData<T>(params object[] args) where T : class
        {
            var type = typeof(T);
            if (!Instance._eventHandlers.TryGetValue(type, out var handlers))
            {
                return null;
            }

            var invokeArguments = args ?? Array.Empty<object>();
            foreach (Delegate handler in handlers)
            {
                var invoker = GetOrCreateDelegateInvoker(handler);
                if (invoker.ReturnType != type)
                {
                    throw new Exception($"Return type of handler {handler.Method.Name} is not {type}");
                }
                
                if (invoker.ParameterCount != invokeArguments.Length)
                {
                    throw new Exception($"Handler {handler.Method.Name} has {invoker.ParameterCount} parameters, expected {invokeArguments.Length}");
                }
                
                try
                {
                    var result = (T)invoker.Invoke(handler, invokeArguments);
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
            _delegateInvokersByType = new Dictionary<Type, DelegateInvoker>();
        }

        ~GameDataBusImpl()
        {
            foreach (var keyValuePair in _eventHandlers)
            {
                keyValuePair.Value.Clear();
            }
            _eventHandlers.Clear();
            _eventHandlers = null;
            _delegateInvokersByType?.Clear();
            _delegateInvokersByType = null;
            _instance = null;
        }
        #endregion
    }
}
