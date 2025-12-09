using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityMVC
{
    /// <summary>
    /// Event bus implementation that dispatches actions or coroutines keyed by event type and respects owner cancellation tokens when provided.
    /// </summary>
    public class GameEventBusImpl : IGameEventBus
    {
        #region Custom type

        public struct DelegateTask
        {
            public Delegate delegateCallback;
            public CancellationToken cancellationToken;
        }

        #endregion
        
        #region Properties

        private Dictionary<Type, List<DelegateTask>> _eventHandlers;
        private CancellationTokenSource _cancellationToken;

        #endregion

        #region Events Methods

        private void _addListener(Type type, Delegate listener, GameController<GameView> owner = null)
        {
            _forceAddListener(type, listener, owner);
        }
        
        private void _removeListener(Type type, Delegate listener)
        {
            _forceRemoveListener(type, listener);
        }

        internal void _forceAddListener(Type type, Delegate listener, GameController<GameView> owner = null)
        {
            List<DelegateTask> handlers;
            if (!_eventHandlers.TryGetValue(type, out handlers))
            {
                handlers = new List<DelegateTask>();
                _eventHandlers.Add(type, handlers);
            }
            handlers.Add(new DelegateTask
            {
                delegateCallback = listener, 
                cancellationToken = owner != null && owner.GetView() != null ? owner.GetView().GetCancellationToken() : _cancellationToken.Token
            });
        }
        
        internal async void _forceRemoveListener(Type type, Delegate listener)
        {
            await Task.Yield();
            List<DelegateTask> handlers;
            if (_eventHandlers.TryGetValue(type, out handlers))
            {
                var itemToRemove = handlers.Find(it => it.delegateCallback == listener);
                handlers.Remove(itemToRemove);
                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(type);
                }
            }
        }
        
        internal async void _forceRemoveListeners(Type type)
        {
            await Task.Yield();
            if (!_eventHandlers.TryGetValue(type, out var handlers)) return;
            handlers.Clear();
            _eventHandlers.Remove(type);
        }
        
        public void AddListener<T>(Action<T> listener, GameController<GameView> owner = null) => _addListener(typeof(T), listener, owner);
        
        public void AddListener<T>(Action listener, GameController<GameView> owner = null) => _addListener(typeof(T), listener, owner);
        public void AddListener(Action<Type> listener, Type type) => _addListener(type, listener);
        public void AddListener(Action listener, Type type) => _addListener(type, listener);
        
        public void AddListener<T>(Func<IEnumerator> listener, GameController<GameView> owner = null) => _addListener(typeof(T), listener, owner);
        public void AddListener<T>(Func<T,IEnumerator> listener, GameController<GameView> owner = null) => _addListener(typeof(T), listener, owner);
        
        public void RemoveListener<T>(Action<T> listener) => _removeListener(typeof(T), listener);
        
        public void RemoveListener<T>(Action listener) => _removeListener(typeof(T), listener);
        public void RemoveListeners<T>() => _forceRemoveListeners(typeof(T));

        public void RemoveListener(Action listener, Type type) => _removeListener(type, listener);

        
        public void RemoveListener<T>(Func<IEnumerator> listener) => _removeListener(typeof(T), listener);
        public void RemoveListener<T>(Func<T,IEnumerator> listener) => _removeListener(typeof(T), listener);
        
        /// <summary>
        /// Raises an event of type <typeparamref name="T"/> and dispatches all registered listeners.
        /// </summary>
        public void RaiseEvent<T>(T args = null) where T : class
        {
            var type = typeof(T);
            if (!_eventHandlers.TryGetValue(type, out var handlers)) return;
            foreach (DelegateTask handler in handlers)
            {
                switch (handler.delegateCallback)
                {
                    case Action action:
                    {
                        if (!handler.cancellationToken.IsCancellationRequested)
                        {
                            action.Invoke();
                        }
                        break;
                    }
                    case Action<T> typedAction:
                    {
                        if (!handler.cancellationToken.IsCancellationRequested)
                        {
                            typedAction.Invoke(args);
                        }
                        break;
                    }
                    case Func<IEnumerator> delegateCallback: 
                    {
#if UNITYMVC_VCONTAINER
                        GameDependency.StartCoroutine(delegateCallback());
#else
                        Debug.LogError("Coroutine not implemented, enabled #if UNITYMVC_VCONTAINER to support Coroutine launch");
#endif  
                        break;
                    }
                    default:
                    {
                        var delegateCallback = (Func<T,IEnumerator>)handler.delegateCallback;
                        
#if UNITYMVC_VCONTAINER
                        GameDependency.StartCoroutine(delegateCallback(args));
#else
                        Debug.LogError("Coroutine not implemented, enabled #if UNITYMVC_VCONTAINER to support Coroutine launch");
#endif  
                        break;
                    }
                }
            }
        }
        

        #endregion
        
        #region Base Service Methods

        private GameEventBusImpl()
        {
            Initialize();
        }

        private void Initialize()
        {
            _eventHandlers ??= new Dictionary<Type, List<DelegateTask>>();
            _cancellationToken ??= new CancellationTokenSource();
        }

        ~GameEventBusImpl()
        {
            _cancellationToken.Cancel();
            _cancellationToken = null;
            foreach (var keyValuePair in _eventHandlers)
            {
                keyValuePair.Value.Clear();
            }
            _eventHandlers.Clear();
            _eventHandlers = null;
        }

        #endregion
       
    }
}
