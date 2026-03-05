using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
        private int _dispatchDepth;
        private bool _hasDeferredCleanup;
        private List<Type> _dirtyEventTypes;

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

        private void MarkTypeDirty(Type type)
        {
            if (!_dirtyEventTypes.Contains(type))
            {
                _dirtyEventTypes.Add(type);
            }

            _hasDeferredCleanup = true;
        }

        private static void MarkHandlerRemoved(List<DelegateTask> handlers, int index)
        {
            var entry = handlers[index];
            if (entry.delegateCallback == null)
            {
                return;
            }

            entry.delegateCallback = null;
            entry.cancellationToken = default;
            handlers[index] = entry;
        }

        private void CleanupDeferredRemovals()
        {
            for (var typeIndex = 0; typeIndex < _dirtyEventTypes.Count; typeIndex++)
            {
                var eventType = _dirtyEventTypes[typeIndex];
                if (!_eventHandlers.TryGetValue(eventType, out var handlers))
                {
                    continue;
                }

                for (var handlerIndex = handlers.Count - 1; handlerIndex >= 0; handlerIndex--)
                {
                    if (handlers[handlerIndex].delegateCallback == null)
                    {
                        handlers.RemoveAt(handlerIndex);
                    }
                }

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(eventType);
                }
            }

            _dirtyEventTypes.Clear();
            _hasDeferredCleanup = false;
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
        
        internal void _forceRemoveListener(Type type, Delegate listener)
        {
            if (!_eventHandlers.TryGetValue(type, out var handlers))
            {
                return;
            }

            var indexToRemove = handlers.FindIndex(it => it.delegateCallback == listener);
            if (indexToRemove < 0)
            {
                return;
            }

            if (_dispatchDepth > 0)
            {
                MarkHandlerRemoved(handlers, indexToRemove);
                MarkTypeDirty(type);
            }
            else
            {
                handlers.RemoveAt(indexToRemove);
                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(type);
                }
            }
        }
        
        internal void _forceRemoveListeners(Type type)
        {
            if (!_eventHandlers.TryGetValue(type, out var handlers)) return;

            if (_dispatchDepth > 0)
            {
                for (var i = 0; i < handlers.Count; i++)
                {
                    MarkHandlerRemoved(handlers, i);
                }

                MarkTypeDirty(type);
            }
            else
            {
                handlers.Clear();
                _eventHandlers.Remove(type);
            }
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

            _dispatchDepth++;
            try
            {
                var initialHandlersCount = handlers.Count;
                for (var i = 0; i < initialHandlersCount; i++)
                {
                    if (i >= handlers.Count)
                    {
                        break;
                    }

                    var handler = handlers[i];
                    if (handler.delegateCallback == null)
                    {
                        continue;
                    }

                    if (handler.cancellationToken.IsCancellationRequested)
                    {
                        MarkHandlerRemoved(handlers, i);
                        MarkTypeDirty(type);
                        continue;
                    }

                    switch (handler.delegateCallback)
                    {
                        case Action action:
                        {
                            action.Invoke();
                            break;
                        }
                        case Action<T> typedAction:
                        {
                            typedAction.Invoke(args);
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
            finally
            {
                _dispatchDepth--;
                if (_dispatchDepth == 0 && _hasDeferredCleanup)
                {
                    CleanupDeferredRemovals();
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
            _dirtyEventTypes ??= new List<Type>();
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
