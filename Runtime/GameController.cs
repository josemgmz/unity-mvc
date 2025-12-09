using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityMVC
{
    /// <summary>
    /// Abstract base class for game controllers, providing common functionality for managing views and models.
    /// </summary>
    /// <typeparam name="TView">The type of the view associated with the controller.</typeparam>
    public abstract class GameController<TView> where TView : GameView
    {
        #region Properties

        /// <summary>
        /// The view associated with the controller.
        /// </summary>
        private TView _view = null;

        /// <summary>
        /// Dictionary to store models associated with the controller.
        /// </summary>
        private Dictionary<Type, object> _models;

        /// <summary>
        /// Gets the view transform; setter repositions/rotates the view to match the given transform.
        /// </summary>
        protected Transform transform
        {
            get => _view.transform;
            set => _view.transform.SetPositionAndRotation(value.position, value.rotation);
        }

        /// <summary>
        /// Gets the game object of the view.
        /// </summary>
        protected GameObject gameObject
        {
            get => _view.gameObject;
        }
        
        protected bool activeSelf
        {
            get => _view.gameObject.activeSelf;
        }

        /// <summary>
        /// Gets the RectTransform component of the view.
        /// </summary>
        protected RectTransform rectTransform
        {
            get => _view.GetComponent<RectTransform>();
        }

        /// <summary>
        /// True when Unity is running in Play Mode.
        /// </summary>
        protected bool IsPlayMode => Application.isPlaying;

        /// <summary>
        /// True when the controller is running in the Editor outside of Play Mode.
        /// </summary>
        protected bool IsEditorMode => !Application.isPlaying;

        #endregion

        #region Getters and Setters
        
        protected IEnumerable<KeyValuePair<Type, GameModel>> EnumerateRawModels()
        {
            if (_models == null)
            {
                yield break;
            }

            foreach (var kvp in _models)
            {
                if (kvp.Value is GameModel model)
                {
                    yield return new KeyValuePair<Type, GameModel>(kvp.Key, model);
                }
            }
        }

        /// <summary>
        /// Starts a coroutine on the view.
        /// </summary>
        /// <param name="coroutine">The coroutine to start.</param>
        /// <returns>The started coroutine.</returns>
        protected Coroutine StartCoroutine(IEnumerator coroutine)
        {
            return _view.StartCoroutine(coroutine);
        }
        /// <summary>
        /// Stops a coroutine on the view.
        /// </summary>
        /// <param name="coroutine">The coroutine to stop.</param>
        protected void StopCoroutine(Coroutine coroutine)
        {
            _view.StopCoroutine(coroutine);
        }
        
        protected void SetActive(bool value)
        {
            _view.gameObject.SetActive(value);
        }
        
        protected void SetLayer(int layer)
        {
            _view.gameObject.layer = layer;
        }

        /// <summary>
        /// Instantiates a game object.
        /// </summary>
        /// <param name="value">The game object to instantiate.</param>
        /// <returns>The instantiated game object.</returns>
        protected GameObject Instantiate(GameObject value)
        {
            return GameObject.Instantiate(value);
        }

        /// <summary>
        /// Instantiates a game object at a specified position and rotation.
        /// </summary>
        /// <param name="value">The game object to instantiate.</param>
        /// <param name="position">The position to instantiate the game object at.</param>
        /// <param name="rotation">The rotation to instantiate the game object with.</param>
        /// <returns>The instantiated game object.</returns>
        protected GameObject Instantiate(GameObject value, Vector3 position, Quaternion rotation)
        {
            return GameObject.Instantiate(value, position, rotation);
        }

        /// <summary>
        /// Instantiates a game object at a specified position, rotation, and parent transform.
        /// </summary>
        /// <param name="value">The game object to instantiate.</param>
        /// <param name="position">The position to instantiate the game object at.</param>
        /// <param name="rotation">The rotation to instantiate the game object with.</param>
        /// <param name="parent">The parent transform to instantiate the game object under.</param>
        /// <returns>The instantiated game object.</returns>
        protected GameObject Instantiate(GameObject value, Vector3 position, Quaternion rotation, Transform parent)
        {
            return GameObject.Instantiate(value, position, rotation, parent);
        }

        /// <summary>
        /// Instantiates a game object under a specified parent transform.
        /// </summary>
        /// <param name="value">The game object to instantiate.</param>
        /// <param name="parent">The parent transform to instantiate the game object under.</param>
        /// <returns>The instantiated game object.</returns>
        protected GameObject Instantiate(GameObject value, Transform parent)
        {
            return GameObject.Instantiate(value, parent);
        }

        /// <summary>
        /// Gets a component of a specified type from the view.
        /// </summary>
        /// <typeparam name="T">The type of the component to get.</typeparam>
        /// <returns>The component of the specified type.</returns>
        protected T GetComponent<T>()
        {
            return _view.GetComponent<T>();
        }
        /// <summary>
        /// Gets components of a specified type from the view.
        /// </summary>
        /// <typeparam name="T">The type of the component to get.</typeparam>
        /// <returns>Array of components of the specified type.</returns>
        protected T[] GetComponents<T>()
        {
            return _view.GetComponents<T>();
        }
        /// <summary>
        /// Gets the first component of type <typeparamref name="T"/> in the view's children.
        /// </summary>
        /// <typeparam name="T">Component type to search for.</typeparam>
        /// <returns>The component instance if found; otherwise null.</returns>
        protected T GetComponentInChildren<T>()
        {
            return _view.GetComponentInChildren<T>();
        }

        /// <summary>
        /// Sets the view for the controller.
        /// </summary>
        /// <param name="view">The view to set.</param>
        /// <exception cref="Exception">Thrown if the view is already set.</exception>
        public void SetView(TView view)
        {
            if (_view != null) throw new Exception("View already exist, can't set new view");
            _view = view;
        }

        /// <summary>
        /// Sets a model for the controller.
        /// </summary>
        /// <typeparam name="T">The type of the model to set.</typeparam>
        /// <param name="model">The model to set.</param>
        /// <exception cref="Exception">Thrown if a model of the specified type already exists.</exception>
        public void SetModel<T>(object model)
        {
            var type = typeof(T);
            _models ??= new Dictionary<Type, object>();
            if (!_models.ContainsKey(type))
            {
                _models.Add(type, model);
                return;
            }
            throw new Exception("Model already exist, can't set new model with this type");
        }

        /// <summary>
        /// Gets a model of a specified type from the controller.
        /// </summary>
        /// <typeparam name="T">The type of the model to get.</typeparam>
        /// <returns>The model of the specified type.</returns>
        /// <exception cref="Exception">Thrown if a model of the specified type is not found.</exception>
        protected T GetModel<T>()
        {
            var type = typeof(T);
            return _models.ContainsKey(type) ? (T) _models[type] : throw new Exception("Model not found");
        }

        /// <summary>
        /// Gets the view associated with the controller.
        /// </summary>
        /// <returns>The view associated with the controller.</returns>
        public TView GetView() => _view;

        /// <summary>
        /// Gets a model of a specified type from another game object.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the model to get.</typeparam>
        /// <param name="value">The game object to get the model from.</param>
        /// <returns>The model of the specified type.</returns>
        /// <exception cref="Exception">Thrown if the game object does not have a GameView component.</exception>
        protected TControllerType GetModelFromOtherObject<TControllerType>(GameObject value) where TControllerType : GameModel
        {
            var componentExist = value.TryGetComponent(out GameView viewComponent);
            if(!componentExist) throw new Exception("GameView component not found");
            return viewComponent.GetModel<TControllerType>();
        }
        
        protected void Invoke(string methodName, float time)
        {
            _view.Invoke(methodName, time);
        }
        
        protected void InvokeRepeating(string methodName, float time, float repeatRate)
        {
            _view.InvokeRepeating(methodName, time, repeatRate);
        }
        
        protected void CancelInvoke(string methodName)
        {
            _view.CancelInvoke(methodName);
        }

        /// <summary>
        /// Gets a controller of a specified type.
        /// </summary>
        /// <typeparam name="ControllerType">The type of the controller to get.</typeparam>
        /// <returns>The controller of the specified type.</returns>
        protected ControllerType GetController<ControllerType>()
        {
            return GetControllerFromOtherObject<ControllerType>(gameObject);
        }

        /// <summary>
        /// Gets a controller of a specified type from another game object.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller to get.</typeparam>
        /// <param name="value">The game object to get the controller from.</param>
        /// <returns>The controller of the specified type.</returns>
        /// <exception cref="Exception">Thrown if the game object does not have a GameView component.</exception>
        protected TControllerType GetControllerFromOtherObject<TControllerType>(GameObject value)
        {
            var componentExist = value.TryGetComponent(out GameView viewComponent);
            if(!componentExist) throw new Exception("GameView component not found");
            return viewComponent.GetController<TControllerType>();
        }

        /// <summary>
        /// Gets a list of child controllers of a specified type.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the child controllers to get.</typeparam>
        /// <returns>A list of child controllers of the specified type.</returns>
        protected List<TControllerType> GetChildControllers<TControllerType>()
        {
            var controllers = new List<TControllerType>();
            var transform = GetView().transform;
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                var childGameObject = child.gameObject;
                var childViewExists = childGameObject.TryGetComponent(out GameView viewComponent);
                if(!childViewExists) continue;
                try
                {
                    var childController = viewComponent.GetController<TControllerType>();
                    controllers.Add(childController);
                }
                catch { /*ignored*/ }
            }
            return controllers;
        }

        /// <summary>
        /// Gets a list of child models of a specified type.
        /// </summary>
        /// <typeparam name="TModelType">The type of the child models to get.</typeparam>
        /// <returns>A list of child models of the specified type.</returns>
        protected List<TModelType> GetChildModels<TModelType>() where TModelType : GameModel
        {
            var models = new List<TModelType>();
            var transform = GetView().transform;
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                models.Add(GetModelFromOtherObject<TModelType>(child.gameObject));
            }
            return models;
        }

        /// <summary>
        /// Gets a controller of a specified type from the parent game object.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller to get.</typeparam>
        /// <returns>The controller of the specified type.</returns>
        /// <exception cref="Exception">Thrown if the parent game object does not have a GameView component.</exception>
        protected TControllerType GetControllerFromParent<TControllerType>()
        {
            var componentExist = GetView().transform.parent.TryGetComponent(out GameView viewComponent);
            if(!componentExist) throw new Exception("GameView component not found");
            return viewComponent.GetController<TControllerType>();
        }

        /// <summary>
        /// Gets a model of a specified type from the parent game object.
        /// </summary>
        /// <typeparam name="TModelType">The type of the model to get.</typeparam>
        /// <returns>The model of the specified type.</returns>
        /// <exception cref="Exception">Thrown if the parent game object does not have a GameView component.</exception>
        protected TModelType GetModelFromParent<TModelType>() where TModelType : GameModel
        {
            var componentExist = GetView().transform.parent.TryGetComponent(out GameView viewComponent);
            if(!componentExist) throw new Exception("GameView component not found");
            return viewComponent.GetModel<TModelType>();
        }

        /// <summary>
        /// Destroys the specified object or the GameView component if no object is specified.
        /// </summary>
        /// <param name="value">The object to destroy. If null, the view is destroyed.</param>
        protected void Destroy(Object value = null)
        {
            if (value == null)
            {
                Object.Destroy(_view);
                return;
            }
            Object.Destroy(value);
        }

        #endregion
    }
}
