using System;
using UnityEngine;

namespace UnityMVC
{
    /// <summary>
    /// Helpers to pull controllers from GameObjects that host a GameView.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Tries to resolve a controller of type <typeparamref name="T"/> from a GameObject containing a GameView.
        /// </summary>
        public static bool TryGetController<T>(this GameObject go, out T controller)
        {
            if (go == null)
            {
                controller = default!;
                return false;
            }

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
            if (go == null || controllerType == null)
            {
                controller = null!;
                return false;
            }

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
    /// Collider2D helpers to fetch controllers from the attached GameObject.
    /// </summary>
    public static class Collider2DExtensions
    {
        /// <inheritdoc cref="GameObjectExtensions.TryGetController{T}(GameObject,out T)"/>
        public static bool TryGetController<T>(this Collider2D col, out T controller)
        {
            if (col == null)
            {
                controller = default!;
                return false;
            }

            return col.gameObject.TryGetController<T>(out controller);
        }

        /// <inheritdoc cref="GameObjectExtensions.TryGetController(UnityEngine.GameObject,System.Type,out object)"/>
        public static bool TryGetController(this Collider2D col, Type controllerType, out object controller)
        {
            if (col == null)
            {
                controller = null!;
                return false;
            }

            return col.gameObject.TryGetController(controllerType, out controller);
        }
    }
    
    /// <summary>
    /// Collision2D helpers to fetch controllers from the impacted GameObject.
    /// </summary>
    public static class Collision2DExtensions
    {
        /// <inheritdoc cref="GameObjectExtensions.TryGetController{T}(GameObject,out T)"/>
        public static bool TryGetController<T>(this Collision2D collision, out T controller)
        {
            if (collision == null)
            {
                controller = default!;
                return false;
            }

            return collision.gameObject.TryGetController<T>(out controller);
        }
        /// <inheritdoc cref="GameObjectExtensions.TryGetController(UnityEngine.GameObject,System.Type,out object)"/>
        public static bool TryGetController(this Collision2D collision, Type controllerType, out object controller)
        {
            if (collision == null)
            {
                controller = null!;
                return false;
            }

            return collision.gameObject.TryGetController(controllerType, out controller);
        }
    }
}
