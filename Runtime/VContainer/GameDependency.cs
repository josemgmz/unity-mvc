using System.Collections;
using UnityEngine;
#if UNITYMVC_VCONTAINER
using VContainer;
using VContainer.Unity;
#endif

namespace UnityMVC
{
    /// <summary>
    /// Manages the dependency injection for the game using VContainer.
    /// </summary>
#if UNITYMVC_VCONTAINER
    public class GameDependency : LifetimeScope
#else
    public class GameDependency
#endif
    {
        #region Variables

        public static GameDependency GameDependencyInstance;
#if UNITYMVC_VCONTAINER
        private static IObjectResolver GlobalContainer { get; set; }
        private static LifetimeScope Instance { get; set; }

#endif
        #endregion
        
        #region Methods

#if UNITYMVC_VCONTAINER
        protected override void Awake()
#else
        protected virtual void Awake()
#endif
        {
            GameDependencyInstance = this;
#if UNITYMVC_VCONTAINER
            base.Awake();
            GlobalContainer = Container;
            Instance = this;
#endif
        }

        /// <summary>
        /// Injects dependencies into the specified instance.
        /// </summary>
        /// <param name="instance">The instance to inject dependencies into.</param>
        public void Inject(object instance)
        {
#if UNITYMVC_VCONTAINER
            Container.Inject(instance);
#endif
        }
        
#if UNITYMVC_VCONTAINER
        public static T Resolve<T>() => GlobalContainer.Resolve<T>();
        public new static Coroutine StartCoroutine(IEnumerator routine) => Instance.StartCoroutine(routine);
        public new static void StopCoroutine(IEnumerator routine) => Instance.StopCoroutine(routine);
#endif

        #endregion
    }
}