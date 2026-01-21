using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace UnityMVC
{
    /// <summary>
    /// Represents the view component in the MVC pattern, managing the lifecycle and interactions of models and controllers.
    /// </summary>
    [ExecuteAlways]
    public class GameView : MonoBehaviour
    {
        #region Variables
        
        internal GameMethods _gameMethods;
        private Dictionary<Type, object> _rawModels;
        private Dictionary<Type, object> _rawControllers;
#if UNITYMVC_VCONTAINER
        private static GameDependency _dependency = null;
#endif
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private bool _controllersInitialized = false;
        private bool _modelsInitialized = false;
        
        private const string INITIALIZE_METHOD = "Initialize";
        private const string AWAKE_METHOD = "Awake";
        private const string PRE_AWAKE_METHOD = "PreAwake";
        private const string SET_MODEL_METHOD = "SetModel";
        private const string SET_INTERNAL_MODEL_METHOD = "SetInternalModel";
        private const string SET_VIEW_METHOD = "SetView";
        private const string DESTROY_METHOD = "Destroy";
        private bool _awakeWasCalled = false;
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        #endregion

        #region Lifecycle

        public GameView()
        {
            InitializeModelAttributes();
        }

        internal virtual void Awake()
        {
            if (_awakeWasCalled) return;
            _awakeWasCalled = true;
            try
            {
#if UNITYMVC_VCONTAINER
                _dependency = _dependency == null ? GameDependency.GameDependencyInstance : _dependency;
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MVC] Error trying to find the GameDependency component. Please make sure you have a GameDependency component in the scene. Error: " + e.Message);
            }
            InitializeControllerAttributes(allowEditor: true);
        }

        #endregion

        #region Internal Methods

        public void Initialize<T>(params object[] value)
        {
            var model = _rawModels[typeof(T)];
            var method = model.GetType().GetMethod(INITIALIZE_METHOD, BINDING_FLAGS);
            method?.Invoke(model, value);
        }

        internal void ReceiveEventFromChild<T>(string functionName, params object[] value)
        {
            var controller = _rawControllers[typeof(T)];
            var method = controller.GetType().GetMethod(functionName, BINDING_FLAGS);
            method?.Invoke(controller, value);
        }

        private void EnsureControllersInitialized(bool allowEditor = false)
        {
            if (!_awakeWasCalled)
            {
                Awake();
            }

            InitializeControllerAttributes(allowEditor);
        }

        private bool TryInvokeControllerMethod(object controller, string functionName, object[] parameters)
        {
            if (controller == null || string.IsNullOrWhiteSpace(functionName))
            {
                return false;
            }

            var method = GetInvokableMethod(controller.GetType(), functionName, parameters);
            if (method == null)
            {
                return false;
            }

            try
            {
                method.Invoke(controller, parameters);
                return true;
            }
            catch (TargetInvocationException e)
            {
                Debug.LogError($"[MVC] Error invoking method '{functionName}' on controller '{controller.GetType().Name}': {e.InnerException?.Message ?? e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MVC] Error invoking method '{functionName}' on controller '{controller.GetType().Name}': {e.Message}");
            }

            return false;
        }

        private MethodInfo GetInvokableMethod(Type controllerType, string functionName, object[] parameters)
        {
            var methods = controllerType.GetMethods(BINDING_FLAGS).Where(it => it.Name == functionName).ToList();
            if (methods.Count == 0)
            {
                return null;
            }

            var args = parameters ?? Array.Empty<object>();
            if (args.Length == 0)
            {
                return methods.FirstOrDefault(it => it.GetParameters().Length == 0) ?? methods.FirstOrDefault();
            }

            foreach (var candidate in methods)
            {
                var candidateParameters = candidate.GetParameters();
                if (candidateParameters.Length != args.Length)
                {
                    continue;
                }

                var isMatch = true;
                for (var index = 0; index < candidateParameters.Length; index++)
                {
                    var targetParameterType = candidateParameters[index].ParameterType;
                    var providedValue = args[index];

                    if (providedValue == null)
                    {
                        if (targetParameterType.IsValueType && Nullable.GetUnderlyingType(targetParameterType) == null)
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (!targetParameterType.IsInstanceOfType(providedValue))
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    return candidate;
                }
            }

            return methods.FirstOrDefault();
        }

        private void InitializeModelAttributes(bool force = false)
        {
            if (_modelsInitialized && _rawModels != null && _rawModels.Count > 0 && !force) return;
            Type modelType = null;
            try
            {
                _rawModels = new Dictionary<Type, object>();
                InitializeModelByType(GetType());
                _rawModels.ToList().ForEach(it =>
                {
                    modelType = it.Key;
                    var currentValue = ((GameModel)it.Value);
                    var callAwake = it.Key.GetMethod(AWAKE_METHOD, BINDING_FLAGS)!.MakeGenericMethod(it.Key);
                    callAwake?.Invoke(currentValue, null);
                });
                _modelsInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MVC] " + e.Message + " | Current Class View: " + GetType().Name + " | Current Type Model: " + modelType?.Name + "\n");
                throw;
            }
        }

        private void InitializeModelByType(Type value)
        {
            var fields = new List<FieldInfo>();
            var currentType = value;
            while (currentType != null)
            {
                fields.AddRange(currentType.GetFields(BINDING_FLAGS));
                currentType = currentType.BaseType;
            }
            
            fields.ForEach(field =>
            {
                var attributes = field.GetCustomAttributes(typeof(GameFieldAttributes.ModelFieldAttribute), false);
                if (attributes.Length <= 0) return;
                var modelType = field.FieldType;
                var modelValue = field.GetValue(this);
                if (modelValue == null)
                {
                    modelValue = Activator.CreateInstance(modelType);
                    field.SetValue(this, modelValue);
                }
                _rawModels.Add(modelType, modelValue);
            });
        }

        private bool ShouldInstantiateController(GameFieldAttributes.ControllerExecutionMode executionMode, bool allowEditor)
        {
            var inPlayMode = Application.isPlaying;
            if (!inPlayMode && !allowEditor)
            {
                return false;
            }

            return executionMode switch
            {
                GameFieldAttributes.ControllerExecutionMode.PlayOnly => inPlayMode,
                GameFieldAttributes.ControllerExecutionMode.EditorOnly => !inPlayMode,
                GameFieldAttributes.ControllerExecutionMode.Always => inPlayMode || allowEditor,
                _ => inPlayMode
            };
        }

        private void InitializeControllerAttributes(bool allowEditor = false)
        {
            if (_controllersInitialized && _rawControllers != null && _rawControllers.Count > 0 && _gameMethods != null)
            {
                return;
            }

            var allowInitialization = Application.isPlaying || allowEditor;
            if (!allowInitialization)
            {
                return;
            }

            _gameMethods = new GameMethods();
            _rawControllers ??= new Dictionary<Type, object>();
            _rawControllers.Clear();

            // Discover all controller fields on the view
            var type = GetType();
            var fields = new List<FieldInfo>();
            var currentType = type;
            while (currentType != null)
            {
                fields.AddRange(currentType.GetFields(BINDING_FLAGS));
                currentType = currentType.BaseType;
            }
            
            // Check if any field has the reverse order attribute
            var shouldReverse = fields.Any(field =>
                field.GetCustomAttributes(typeof(GameFieldAttributes.ControllerReverseOrderAttribute), false).Length > 0
            );
            
            if (!shouldReverse)
            {
                fields.Reverse();
            }
                
            fields.ForEach(field =>
            {
                var attributes = field.GetCustomAttributes(typeof(GameFieldAttributes.ControllerFieldAttribute), false);
                if (attributes.Length > 0)
                {
                    var executionAttribute = field.GetCustomAttributes(typeof(GameFieldAttributes.ControllerExecutionAttribute), false)
                        .FirstOrDefault() as GameFieldAttributes.ControllerExecutionAttribute;
                    var executionMode = executionAttribute?.Mode ?? GameFieldAttributes.ControllerExecutionMode.PlayOnly;

                    if (!ShouldInstantiateController(executionMode, allowEditor))
                    {
                        return;
                    }

                    var controllerType = field.FieldType;
                    var controllerInstance = Activator.CreateInstance(controllerType, this);
                    field.SetValue(this, controllerInstance);
                    try
                    {
#if UNITYMVC_VCONTAINER
                        _dependency?.Inject(controllerInstance); // Inject dependencies into the controller
#endif
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("[MVC] Dependency injection failed for controller " + controllerType.Name + ": " + e.Message);
                    }
                    _rawControllers.Add(controllerType, controllerInstance);
                }
            });
            
            // Wire up each controller
            foreach (var it in _rawControllers.Values)
            {
                // Bind SetView
                var setViewMethod = it.GetType().GetMethod(SET_VIEW_METHOD, BINDING_FLAGS);
                var setViewParameters = new object[] { this };
                setViewMethod?.Invoke(it, setViewParameters);
                
                // Bind SetModel for every model
                foreach (var model in _rawModels)
                {
                    var setModelMethod = it.GetType().GetMethod(SET_MODEL_METHOD, BINDING_FLAGS)!.MakeGenericMethod(model.Key);
                    var setModelParameters = new object[] { model.Value };
                    setModelMethod?.Invoke(it, setModelParameters);
                }
                
                Type baseType = it.GetType();
                while (baseType != null && baseType != typeof(object))
                {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(GameController<,>))
                    {
                        // Found a GameController<TView, TModel> base class
                        break;
                    }
                    baseType = baseType.BaseType!;
                }
                // If a GameController<TView, TModel> base was found
                if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(GameController<,>))
                {
                    Type[] genericArguments = baseType.GenericTypeArguments;
                    var modelType = genericArguments[1]; // This is TModel
                    var model = _rawModels[modelType];
                    
                    var setModelMethod = it.GetType().GetMethod(SET_INTERNAL_MODEL_METHOD, BINDING_FLAGS);
                    var setModelParameters = new object[] { model };
                    setModelMethod?.Invoke(it, setModelParameters);
                }
            }

            // Gather all event methods to bind
            var methodsToBindToController =
                Enum.GetValues(typeof(GameMethodEvents)).Cast<GameMethodEvents>().ToList();
            
            // Bind controller methods to GameMethodEvents
            foreach (var it in _rawControllers.Values)
            {
                methodsToBindToController.ForEach(methodToBind =>
                {
                    var newMethod = it.GetType().GetMethod(methodToBind.ToString(), BINDING_FLAGS);
                    if (newMethod != null)
                    {
                        var cachedMethod = GameMethod.Create(it, newMethod);
                        _gameMethods.AddMethod(methodToBind, cachedMethod);
                    }
                });
            }
            
            // PreAwake on controllers
            foreach (var it in _rawControllers.Values)
            {
                // Find Awake and call
                var invokeMethod = it.GetType().GetMethod(PRE_AWAKE_METHOD, BINDING_FLAGS);
                invokeMethod?.Invoke(it, null);
            }
            
            // Awake on controllers
            foreach (var it in _rawControllers.Values)
            {
                var invokeMethod = it.GetType().GetMethod(AWAKE_METHOD, BINDING_FLAGS);
                invokeMethod?.Invoke(it, null);
            }

            _controllersInitialized = true;
        }

        #endregion

        #region Getters

        /// <summary>
        /// Returns a clone of the model matching <typeparamref name="T"/> (exact type first, then assignable types).
        /// </summary>
        public T GetModel<T>() where T : GameModel
        {
            var type = typeof(T);
            // Try exact type first
            if (_rawModels.TryGetValue(type, out var rawModel))
            {
                GameModel m = rawModel as GameModel;
                return (T)m.Clone();
            }
            
            // Otherwise search for an assignable model type
            foreach (var kvp in _rawModels)
            {
                if (type.IsAssignableFrom(kvp.Key)) // Check if kvp.Key inherits from or implements T
                {
                    GameModel herModel = kvp.Value as GameModel;
                    return (T)herModel.Clone();
                }
            }
            throw new Exception($"Model not found for type {type}");
        }
        
        /// <summary>
        /// Returns a controller instance of type <typeparamref name="T"/> (exact match or assignable).
        /// </summary>
        public T GetController<T>()
        {
            if (!_awakeWasCalled)
            {
                Awake();
            }
            
            var type = typeof(T);
            // Try exact type first
            if (_rawControllers.TryGetValue(type, out var rawController))
            {
                return (T) rawController;
            }

            // Then any controller assignable to T
            foreach (var controller in _rawControllers.Values)
            {
                if (controller is T matchedController)
                {
                    return matchedController;
                }
            }

            throw new Exception($"Controller of type {typeof(T).Name} not found");
        }
        
        public bool TryGetController<T>(out T controller)
        {
            var targetType = typeof(T);
            if (_rawControllers.TryGetValue(targetType, out var raw) && raw is T exactMatch)
            {
                controller = exactMatch;
                return true;
            }
            foreach (var kvp in _rawControllers)
            {
                if (targetType.IsAssignableFrom(kvp.Key) && kvp.Value is T assignableMatch)
                {
                    controller = assignableMatch;
                    return true;
                }
            }

            controller = default!;
            return false;
        }

        /// <summary>
        /// Non-generic lookup by runtime <paramref name="targetType"/>; matches exact or assignable controllers.
        /// </summary>
        public bool TryGetController(Type targetType, out object controller)
        {
            if (_rawControllers.TryGetValue(targetType, out var raw))
            {
                controller = raw!;
                return true;
            }
            foreach (var kvp in _rawControllers)
            {
                if (targetType.IsAssignableFrom(kvp.Key))
                {
                    controller = kvp.Value!;
                    return true;
                }
            }

            controller = null!;
            return false;
        }
        
        public CancellationToken GetCancellationToken() => _cancellationToken.Token;

        #endregion

        #region Methods
        
        public void InvokeControllerMethod(string functionName)
        {
            InvokeControllerMethod(functionName, null);
        }

        public bool InvokeControllerMethod(string functionName, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                Debug.LogWarning("[MVC] Function name cannot be null or whitespace.");
                return false;
            }

            EnsureControllersInitialized();

            if (_rawControllers == null || _rawControllers.Count == 0)
            {
                Debug.LogWarning($"[MVC] No controllers available in view {GetType().Name} to invoke method '{functionName}'.");
                return false;
            }

            var wasInvoked = false;
            foreach (var controller in _rawControllers.Values)
            {
                wasInvoked |= TryInvokeControllerMethod(controller, functionName, parameters);
            }

            if (!wasInvoked)
            {
                Debug.LogWarning($"[MVC] Method '{functionName}' not found in any controller associated with view {GetType().Name}.");
            }

            return wasInvoked;
        }

        public bool InvokeControllerMethod(Type controllerType, string functionName, params object[] parameters)
        {
            if (controllerType == null)
            {
                Debug.LogWarning("[MVC] Controller type cannot be null.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(functionName))
            {
                Debug.LogWarning("[MVC] Function name cannot be null or whitespace.");
                return false;
            }

            EnsureControllersInitialized();

            if (!TryGetController(controllerType, out var controller))
            {
                Debug.LogWarning($"[MVC] Controller of type {controllerType.Name} not found for view {GetType().Name}.");
                return false;
            }

            var wasInvoked = TryInvokeControllerMethod(controller, functionName, parameters);
            if (!wasInvoked)
            {
                Debug.LogWarning($"[MVC] Method '{functionName}' not found in controller {controllerType.Name} for view {GetType().Name}.");
            }

            return wasInvoked;
        }

        public bool InvokeControllerMethod<TController>(string functionName, params object[] parameters)
        {
            return InvokeControllerMethod(typeof(TController), functionName, parameters);
        }

        protected void InvokeGameMethods(GameMethodEvents eventName)
        {
            if (_gameMethods == null)
            {
                return;
            }

            var handlers = _gameMethods.GetList(eventName);
            if (handlers == null || handlers.Count == 0)
            {
                return;
            }

            for (var index = 0; index < handlers.Count; index++)
            {
                handlers[index].Invoke();
            }
        }

        protected void InvokeGameMethods<T>(GameMethodEvents eventName, T arg)
        {
            if (_gameMethods == null)
            {
                return;
            }

            var handlers = _gameMethods.GetList(eventName);
            if (handlers == null || handlers.Count == 0)
            {
                return;
            }

            for (var index = 0; index < handlers.Count; index++)
            {
                handlers[index].Invoke(arg);
            }
        }

        protected void InvokeGameMethods<TFirst, TSecond>(GameMethodEvents eventName, TFirst arg1, TSecond arg2)
        {
            if (_gameMethods == null)
            {
                return;
            }

            var handlers = _gameMethods.GetList(eventName);
            if (handlers == null || handlers.Count == 0)
            {
                return;
            }

            for (var index = 0; index < handlers.Count; index++)
            {
                handlers[index].Invoke(arg1, arg2);
            }
        }

        #endregion

        #region Unity Methods

        private void Start()
        {
            InvokeGameMethods(GameMethodEvents.Start);
        }
        
        private void Update()
        {
            InvokeGameMethods(GameMethodEvents.Update);
        }
        
        private void FixedUpdate()
        {
            InvokeGameMethods(GameMethodEvents.FixedUpdate);
        }
        
        private void LateUpdate()
        {
            InvokeGameMethods(GameMethodEvents.LateUpdate);
        }
        
        private void OnEnable()
        {
            InvokeGameMethods(GameMethodEvents.OnEnable);
        }
        
        private void OnDisable()
        {
            InvokeGameMethods(GameMethodEvents.OnDisable);
        }
        
        private void OnDestroy()
        {
            InvokeGameMethods(GameMethodEvents.OnDestroy);
            _rawModels?.ToList().ForEach(it =>
            {
                var currentValue = ((GameModel)it.Value);
                var callAwake = it.Key.GetMethod(DESTROY_METHOD, BINDING_FLAGS)?.MakeGenericMethod(it.Key);
                callAwake?.Invoke(currentValue, null);
            });
            _cancellationToken.Cancel();
        }

#if UNITY_EDITOR
        public void EditorReinitialize()
        {
            _controllersInitialized = false;
            _modelsInitialized = false;
            InitializeModelAttributes(force: true);
            InitializeControllerAttributes(allowEditor: true);
        }
#endif
        
        private void OnCollisionEnter(Collision other)
        {
            InvokeGameMethods(GameMethodEvents.OnCollisionEnter, other);
        }
        
        private void OnCollisionExit(Collision other)
        {
            InvokeGameMethods(GameMethodEvents.OnCollisionExit, other);
        }
        
        private void OnCollisionStay(Collision other)
        {
            InvokeGameMethods(GameMethodEvents.OnCollisionStay, other);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            InvokeGameMethods(GameMethodEvents.OnTriggerEnter, other);
        }
        
        private void OnTriggerExit(Collider other)
        {
            InvokeGameMethods(GameMethodEvents.OnTriggerExit, other);
        }
        
        private void OnTriggerStay(Collider other)
        {
            InvokeGameMethods(GameMethodEvents.OnTriggerStay, other);
        }
        
        private void OnMouseDown()
        {
            InvokeGameMethods(GameMethodEvents.OnMouseDown);
        }
        
        private void OnMouseEnter()
        {
            InvokeGameMethods(GameMethodEvents.OnMouseEnter); 
        }
        
        private void OnMouseExit()
        {
            InvokeGameMethods(GameMethodEvents.OnMouseExit);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            InvokeGameMethods(GameMethodEvents.OnCollisionEnter2D, other);
        }
        
        private void OnCollisionExit2D(Collision2D other)
        {
            InvokeGameMethods(GameMethodEvents.OnCollisionExit2D, other);
        }
        
        private void OnCollisionStay2D(Collision2D other)
        {
            InvokeGameMethods(GameMethodEvents.OnCollisionStay2D, other);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            InvokeGameMethods(GameMethodEvents.OnTriggerEnter2D, other); 
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            InvokeGameMethods(GameMethodEvents.OnTriggerExit2D, other);
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            InvokeGameMethods(GameMethodEvents.OnTriggerStay2D, other);
        }
        
        private void OnAnimatorEvent(string value)
        {
            InvokeGameMethods(GameMethodEvents.OnAnimatorEvent, value);
        }

        private void OnBecameInvisible()
        {
            InvokeGameMethods(GameMethodEvents.OnBecameInvisible);
        }

        private void OnParticleSystemStopped()
        {
            InvokeGameMethods(GameMethodEvents.OnParticleSystemStopped);
        }

        private void OnDrawGizmos()
        {
            EnsureControllersInitialized(allowEditor: true);
            InvokeGameMethods(GameMethodEvents.OnDrawGizmos);        
        }

        private void OnDrawGizmosSelected()
        {
            EnsureControllersInitialized(allowEditor: true);
            InvokeGameMethods(GameMethodEvents.OnDrawGizmosSelected);
        }

        #endregion
    }
}
