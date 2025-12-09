using System;

namespace UnityMVC
{
    /// <summary>
    /// Represents the base class for game models, providing lifecycle management and cloning capabilities.
    /// </summary>
    public abstract class GameModel : ICloneable
    {
        #region Lifecycle

        /// <summary>
        /// Hook for model initialization; override in derived classes as needed.
        /// </summary>
        public void Awake<T>() where T: GameModel
        {
        }
        
        /// <summary>
        /// Hook for cleanup; override in derived classes as needed.
        /// </summary>
        public virtual void Destroy<T>()  where T: GameModel
        {
        }

        /// <summary>
        /// Returns the current instance cast to the requested model type.
        /// </summary>
        internal T GetType<T>() where T: GameModel
        {
            return (T) this;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shallow clone of the model instance.
        /// </summary>
        public object Clone() => MemberwiseClone();

        #endregion

    }
}
