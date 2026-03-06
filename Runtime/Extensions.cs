using System;
using System.Reflection;
using UnityEngine;

namespace UnityMVC
{
    /// <summary>
    /// Helpers to pull controllers from GameObjects that host a GameView.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Resolves a controller of type <typeparamref name="T"/> from a GameObject containing a GameView.
        /// Throws if the view or controller cannot be found.
        /// </summary>
        public static T GetController<T>(this GameObject go)
        {
            var view = go.GetComponent<GameView>();
            if (view != null)
            {
                return view.GetController<T>();
            }

            throw new Exception($"GameObject '{go.name}' does not contain a GameView component.");
        }

        /// <summary>
        /// Resolves a controller by runtime <paramref name="controllerType"/> from a GameObject containing a GameView.
        /// Throws if the view or controller cannot be found.
        /// </summary>
        public static object GetController(this GameObject go, Type controllerType)
        {
            var view = go.GetComponent<GameView>();
            if (view == null)
            {
                throw new Exception($"GameObject '{go.name}' does not contain a GameView component.");
            }

            var getControllerMethod = typeof(GameView).GetMethod(nameof(GameView.GetController), Type.EmptyTypes);
            var genericGetControllerMethod = getControllerMethod!.MakeGenericMethod(controllerType);

            try
            {
                return genericGetControllerMethod.Invoke(view, null)!;
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException ?? e;
            }
        }

        /// <summary>
        /// Tries to resolve a controller of type <typeparamref name="T"/> from a GameObject containing a GameView.
        /// </summary>
        public static bool TryGetController<T>(this GameObject go, out T controller)
        {
            var view = go.GetComponent<GameView>();
            if (view != null && view.TryGetController<T>(out controller))
            {
                return true;
            }

            controller = default!;
            return false;
        }

        /// <summary>
        /// Tries to resolve a controller by runtime <paramref name="controllerType"/> from a GameObject containing a GameView.
        /// </summary>
        public static bool TryGetController(this GameObject go, Type controllerType, out object controller)
        {
            var view = go.GetComponent<GameView>();
            if (view != null && view.TryGetController(controllerType, out controller))
            {
                return true;
            }

            controller = null!;
            return false;
        }
    }

    /// <summary>
    /// Collider helpers to fetch controllers from the attached GameObject.
    /// </summary>
    public static class ColliderExtensions
    {
        /// <inheritdoc cref="GameObjectExtensions.GetController{T}(GameObject)"/>
        public static T GetController<T>(this Collider col)
        {
            return col.gameObject.GetController<T>();
        }

        /// <inheritdoc cref="GameObjectExtensions.GetController(UnityEngine.GameObject,System.Type)"/>
        public static object GetController(this Collider col, Type controllerType)
        {
            return col.gameObject.GetController(controllerType);
        }

        /// <inheritdoc cref="GameObjectExtensions.TryGetController{T}(GameObject,out T)"/>
        public static bool TryGetController<T>(this Collider col, out T controller)
        {
            return col.gameObject.TryGetController<T>(out controller);
        }

        /// <inheritdoc cref="GameObjectExtensions.TryGetController(UnityEngine.GameObject,System.Type,out object)"/>
        public static bool TryGetController(this Collider col, Type controllerType, out object controller)
        {
            return col.gameObject.TryGetController(controllerType, out controller);
        }
    }

    /// <summary>
    /// Collision helpers to fetch controllers from the impacted GameObject.
    /// </summary>
    public static class CollisionExtensions
    {
        /// <inheritdoc cref="GameObjectExtensions.GetController{T}(GameObject)"/>
        public static T GetController<T>(this Collision collision)
        {
            return collision.gameObject.GetController<T>();
        }

        /// <inheritdoc cref="GameObjectExtensions.GetController(UnityEngine.GameObject,System.Type)"/>
        public static object GetController(this Collision collision, Type controllerType)
        {
            return collision.gameObject.GetController(controllerType);
        }

        /// <inheritdoc cref="GameObjectExtensions.TryGetController{T}(GameObject,out T)"/>
        public static bool TryGetController<T>(this Collision collision, out T controller)
        {
            return collision.gameObject.TryGetController<T>(out controller);
        }

        /// <inheritdoc cref="GameObjectExtensions.TryGetController(UnityEngine.GameObject,System.Type,out object)"/>
        public static bool TryGetController(this Collision collision, Type controllerType, out object controller)
        {
            return collision.gameObject.TryGetController(controllerType, out controller);
        }
    }

    /// <summary>
    /// Collider2D helpers to fetch controllers from the attached GameObject.
    /// </summary>
    public static class Collider2DExtensions
    {
        /// <inheritdoc cref="GameObjectExtensions.GetController{T}(GameObject)"/>
        public static T GetController<T>(this Collider2D col)
        {
            return col.gameObject.GetController<T>();
        }

        /// <inheritdoc cref="GameObjectExtensions.GetController(UnityEngine.GameObject,System.Type)"/>
        public static object GetController(this Collider2D col, Type controllerType)
        {
            return col.gameObject.GetController(controllerType);
        }

        /// <inheritdoc cref="GameObjectExtensions.TryGetController{T}(GameObject,out T)"/>
        public static bool TryGetController<T>(this Collider2D col, out T controller)
        {
            return col.gameObject.TryGetController<T>(out controller);
        }

        /// <inheritdoc cref="GameObjectExtensions.TryGetController(UnityEngine.GameObject,System.Type,out object)"/>
        public static bool TryGetController(this Collider2D col, Type controllerType, out object controller)
        {
            return col.gameObject.TryGetController(controllerType, out controller);
        }
    }

    /// <summary>
    /// Collision2D helpers to fetch controllers from the impacted GameObject.
    /// </summary>
    public static class Collision2DExtensions
    {
        /// <inheritdoc cref="GameObjectExtensions.GetController{T}(GameObject)"/>
        public static T GetController<T>(this Collision2D collision)
        {
            return collision.gameObject.GetController<T>();
        }

        /// <inheritdoc cref="GameObjectExtensions.GetController(UnityEngine.GameObject,System.Type)"/>
        public static object GetController(this Collision2D collision, Type controllerType)
        {
            return collision.gameObject.GetController(controllerType);
        }

        /// <inheritdoc cref="GameObjectExtensions.TryGetController{T}(GameObject,out T)"/>
        public static bool TryGetController<T>(this Collision2D collision, out T controller)
        {
            return collision.gameObject.TryGetController<T>(out controller);
        }

        /// <inheritdoc cref="GameObjectExtensions.TryGetController(UnityEngine.GameObject,System.Type,out object)"/>
        public static bool TryGetController(this Collision2D collision, Type controllerType, out object controller)
        {
            return collision.gameObject.TryGetController(controllerType, out controller);
        }
    }
}
