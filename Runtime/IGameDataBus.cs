using System;

namespace UnityMVC
{
    /// <summary>
    /// Contract for registering data providers keyed by return type and retrieving values.
    /// </summary>
    public interface IGameDataBus
    {
        
        /// <summary>Registers a parameterless provider using reflection.</summary>
        void AddListenerReflection<T>(Func<T> listener);
        /// <summary>Registers a parameterless provider.</summary>
        void AddListener<T>(Func<T> listener);
        /// <summary>Registers a provider with one argument.</summary>
        void AddListener<TResult, T>(Func<T, TResult> listener);
        /// <summary>Registers a provider with two arguments.</summary>
        void AddListener<TResult, T1, T2>(Func<T1, T2, TResult> listener);
        /// <summary>Registers a provider with three arguments.</summary>
        void AddListener<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> listener);

        
        /// <summary>Unregisters a parameterless provider added via reflection.</summary>
        void RemoveListenerReflection<T>(Func<T> listener);
        /// <summary>Unregisters a parameterless provider.</summary>
        void RemoveListener<T>(Func<T> listener);
        /// <summary>Unregisters a provider with one argument.</summary>
        void RemoveListener<TResult, T>(Func<T, TResult> listener);
        /// <summary>Unregisters a provider with two arguments.</summary>
        void RemoveListener<TResult, T1, T2>(Func<T1, T2, TResult> listener);
        /// <summary>Unregisters a provider with three arguments.</summary>
        void RemoveListener<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> listener);

        
        /// <summary>Returns the first registered value for type <typeparamref name="T"/>.</summary>
        T GetData<T>() where T : class;
        /// <summary>Invokes providers for <typeparamref name="T"/> passing arguments and returns the first result.</summary>
        T GetData<T>(params object[] args) where T : class;
    }
}
