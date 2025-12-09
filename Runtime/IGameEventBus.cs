using System;
using System.Collections;

namespace UnityMVC
{
    /// <summary>
    /// Interface for the game event bus, providing methods for adding, removing, and raising events.
    /// </summary>
    public interface IGameEventBus
    {
        /// <summary>
        /// Adds a listener for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="listener">The listener to add.</param>
        /// <param name="owner">The owner of the listener.</param>
        void AddListener<T>(Action<T> listener, GameController<GameView> owner = null);

        /// <summary>
        /// Adds a listener for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="listener">The listener to add.</param>
        /// <param name="owner">The owner of the listener.</param>
        void AddListener<T>(Action listener, GameController<GameView> owner = null);

        /// <summary>
        /// Adds a listener for the specified event type.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        /// <param name="type">The type of the event.</param>
        void AddListener(Action<Type> listener, Type type);

        /// <summary>
        /// Adds a listener for the specified event type.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        /// <param name="type">The type of the event.</param>
        void AddListener(Action listener, Type type);

        /// <summary>
        /// Adds a listener for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="listener">The listener to add.</param>
        /// <param name="owner">The owner of the listener.</param>
        void AddListener<T>(Func<IEnumerator> listener, GameController<GameView> owner = null);

        /// <summary>
        /// Adds a listener for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="listener">The listener to add.</param>
        /// <param name="owner">The owner of the listener.</param>
        void AddListener<T>(Func<T, IEnumerator> listener, GameController<GameView> owner = null);

        /// <summary>
        /// Removes a listener for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="listener">The listener to remove.</param>
        void RemoveListener<T>(Action<T> listener);

        /// <summary>
        /// Removes a listener for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="listener">The listener to remove.</param>
        void RemoveListener<T>(Action listener);

        /// <summary>
        /// Removes all listeners for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        void RemoveListeners<T>();

        /// <summary>
        /// Removes a listener for the specified event type.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        /// <param name="type">The type of the event.</param>
        void RemoveListener(Action listener, Type type);

        /// <summary>
        /// Removes a listener for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="listener">The listener to remove.</param>
        void RemoveListener<T>(Func<IEnumerator> listener);

        /// <summary>
        /// Removes a listener for the specified event type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="listener">The listener to remove.</param>
        void RemoveListener<T>(Func<T, IEnumerator> listener);

        /// <summary>
        /// Raises an event of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="args">The event arguments.</param>
        void RaiseEvent<T>(T args = null) where T : class;
    }
}