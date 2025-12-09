using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityMVC
{
    /// <summary>
    /// Cached delegate for a controller method with fast invoke paths by arity.
    /// </summary>
    public sealed class GameMethod
    {
        private static readonly object[] EmptyArgs = Array.Empty<object>();

        private readonly MethodInfo _methodInfo;
        private readonly int _parameterCount;

        private readonly Action _zeroInvoker;
        private readonly Action<object> _singleInvoker;
        private readonly Action<object, object> _doubleInvoker;
        private readonly Action<object, object, object> _tripleInvoker;
        private readonly Action<object[]> _fallbackInvoker;

        private GameMethod(MethodInfo methodInfo, int parameterCount, Action zeroInvoker, Action<object> singleInvoker,
            Action<object, object> doubleInvoker, Action<object, object, object> tripleInvoker, Action<object[]> fallbackInvoker)
        {
            _methodInfo = methodInfo;
            _parameterCount = parameterCount;
            _zeroInvoker = zeroInvoker;
            _singleInvoker = singleInvoker;
            _doubleInvoker = doubleInvoker;
            _tripleInvoker = tripleInvoker;
            _fallbackInvoker = fallbackInvoker;
        }

        public static GameMethod Create(object target, MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            var parameters = method.GetParameters();
            var parameterLength = parameters.Length;

            if (method.ReturnType != typeof(void) || parameterLength > 3)
            {
                var fallback = BuildReflectionInvoker(target, method);
                return new GameMethod(method, parameterLength, null, null, null, null, fallback);
            }

            return parameterLength switch
            {
                0 => new GameMethod(method, 0, BuildZeroParameterInvoker(target, method), null, null, null, null),
                1 => new GameMethod(method, 1, null,
                    BuildSingleParameterInvoker(target, method, parameters[0].ParameterType), null, null, null),
                2 => new GameMethod(method, 2, null, null,
                    BuildDoubleParameterInvoker(target, method, parameters[0].ParameterType, parameters[1].ParameterType),
                    null, null),
                3 => new GameMethod(
                    method,
                    3,
                    null,
                    null,
                    null,
                    BuildTripleParameterInvoker(
                        target,
                        method,
                        parameters[0].ParameterType,
                        parameters[1].ParameterType,
                        parameters[2].ParameterType),
                    null),
                _ => throw new ArgumentOutOfRangeException(nameof(parameterLength), $"Unsupported parameter count {parameterLength} for method '{method.Name}'.")
            };
        }

        public void Invoke()
        {
            if (_parameterCount == 0 && _zeroInvoker != null)
            {
                Execute(_zeroInvoker);
                return;
            }

            InvokeWithArgs(EmptyArgs);
        }

        public void Invoke<T>(T arg)
        {
            if (_parameterCount == 1 && _singleInvoker != null)
            {
                ExecuteSingle(arg);
                return;
            }

            InvokeWithArgs(new object[] { arg });
        }

        public void Invoke<TFirst, TSecond>(TFirst arg1, TSecond arg2)
        {
            if (_parameterCount == 2 && _doubleInvoker != null)
            {
                ExecuteDouble(arg1, arg2);
                return;
            }

            InvokeWithArgs(new object[] { arg1, arg2 });
        }

        public void Invoke<TFirst, TSecond, TThird>(TFirst arg1, TSecond arg2, TThird arg3)
        {
            if (_parameterCount == 3 && _tripleInvoker != null)
            {
                ExecuteTriple(arg1, arg2, arg3);
                return;
            }

            InvokeWithArgs(new object[] { arg1, arg2, arg3 });
        }

        public void Invoke(params object[] args)
        {
            switch (_parameterCount)
            {
                case 0:
                    Invoke();
                    return;
                case 1:
                    Invoke(args != null && args.Length > 0 ? args[0] : null);
                    return;
                case 2:
                    Invoke(
                        args != null && args.Length > 0 ? args[0] : null,
                        args != null && args.Length > 1 ? args[1] : null);
                    return;
                case 3:
                    Invoke(
                        args != null && args.Length > 0 ? args[0] : null,
                        args != null && args.Length > 1 ? args[1] : null,
                        args != null && args.Length > 2 ? args[2] : null);
                    return;
                default:
                    InvokeWithArgs(args);
                    return;
            }
        }

        private void Execute(Action call)
        {
            try
            {
                call();
            }
            catch (Exception e)
            {
                LogExecutionError(e);
            }
        }

        private void ExecuteSingle(object arg)
        {
            try
            {
                _singleInvoker(arg);
            }
            catch (Exception e)
            {
                LogExecutionError(e);
            }
        }

        private void ExecuteDouble(object arg1, object arg2)
        {
            try
            {
                _doubleInvoker(arg1, arg2);
            }
            catch (Exception e)
            {
                LogExecutionError(e);
            }
        }

        private void ExecuteTriple(object arg1, object arg2, object arg3)
        {
            try
            {
                _tripleInvoker(arg1, arg2, arg3);
            }
            catch (Exception e)
            {
                LogExecutionError(e);
            }
        }

        private void InvokeWithArgs(object[] args)
        {
            if (_fallbackInvoker == null)
            {
                Debug.LogWarning($"[MVC] Argument mismatch for method '{_methodInfo.Name}' on '{_methodInfo.DeclaringType?.Name}'.");
                return;
            }

            var payload = args ?? EmptyArgs;

            try
            {
                _fallbackInvoker(payload);
            }
            catch (Exception e)
            {
                LogExecutionError(e);
            }
        }

        private void LogExecutionError(Exception e)
        {
            Debug.LogError($"[MVC] Error executing cached delegate for '{_methodInfo.Name}' on '{_methodInfo.DeclaringType?.Name}': {e.Message}");
        }

        private static Action BuildZeroParameterInvoker(object target, MethodInfo method)
        {
            return (Action)method.CreateDelegate(typeof(Action), target);
        }

        private static Action<object> BuildSingleParameterInvoker(object target, MethodInfo method, Type parameterType)
        {
            var delegateType = typeof(Action<>).MakeGenericType(parameterType);
            var typedDelegate = method.CreateDelegate(delegateType, target);
            var wrapper = typeof(GameMethod).GetMethod(nameof(WrapSingle), BindingFlags.Static | BindingFlags.NonPublic);
            var concreteWrapper = wrapper!.MakeGenericMethod(parameterType);
            return (Action<object>)concreteWrapper.Invoke(null, new object[] { typedDelegate })!;
        }

        private static Action<object, object> BuildDoubleParameterInvoker(object target, MethodInfo method, Type firstParameter, Type secondParameter)
        {
            var delegateType = typeof(Action<,>).MakeGenericType(firstParameter, secondParameter);
            var typedDelegate = method.CreateDelegate(delegateType, target);
            var wrapper = typeof(GameMethod).GetMethod(nameof(WrapDouble), BindingFlags.Static | BindingFlags.NonPublic);
            var concreteWrapper = wrapper!.MakeGenericMethod(firstParameter, secondParameter);
            return (Action<object, object>)concreteWrapper.Invoke(null, new object[] { typedDelegate })!;
        }

        private static Action<object, object, object> BuildTripleParameterInvoker(object target, MethodInfo method, Type firstParameter, Type secondParameter, Type thirdParameter)
        {
            var delegateType = typeof(Action<,,>).MakeGenericType(firstParameter, secondParameter, thirdParameter);
            var typedDelegate = method.CreateDelegate(delegateType, target);
            var wrapper = typeof(GameMethod).GetMethod(nameof(WrapTriple), BindingFlags.Static | BindingFlags.NonPublic);
            var concreteWrapper = wrapper!.MakeGenericMethod(firstParameter, secondParameter, thirdParameter);
            return (Action<object, object, object>)concreteWrapper.Invoke(null, new object[] { typedDelegate })!;
        }

        private static Action<object[]> BuildReflectionInvoker(object target, MethodInfo method)
        {
            return args => method.Invoke(target, args);
        }

        private static Action<object> WrapSingle<T>(Action<T> action)
        {
            return arg =>
            {
                var value = arg == null ? default! : (T)arg;
                action(value);
            };
        }

        private static Action<object, object> WrapDouble<TFirst, TSecond>(Action<TFirst, TSecond> action)
        {
            return (first, second) =>
            {
                var firstValue = first == null ? default! : (TFirst)first;
                var secondValue = second == null ? default! : (TSecond)second;
                action(firstValue, secondValue);
            };
        }

        private static Action<object, object, object> WrapTriple<TFirst, TSecond, TThird>(Action<TFirst, TSecond, TThird> action)
        {
            return (first, second, third) =>
            {
                var firstValue = first == null ? default! : (TFirst)first;
                var secondValue = second == null ? default! : (TSecond)second;
                var thirdValue = third == null ? default! : (TThird)third;
                action(firstValue, secondValue, thirdValue);
            };
        }
    }
    
    /// <summary>
    /// Registry of controller methods keyed by <see cref="GameMethodEvents"/>.
    /// </summary>
    public class GameMethods
    {
        #region Properties
        
        private readonly List<GameMethod>[] _methods;

        #endregion

        #region Initialization

        public GameMethods()
        {
            var eventValues = (GameMethodEvents[])Enum.GetValues(typeof(GameMethodEvents));
            var maxValue = 0;
            for (var index = 0; index < eventValues.Length; index++)
            {
                var numericValue = (int)eventValues[index];
                if (numericValue > maxValue)
                {
                    maxValue = numericValue;
                }
            }

            _methods = new List<GameMethod>[maxValue + 1];

            for (var index = 0; index < eventValues.Length; index++)
            {
                var numericValue = (int)eventValues[index];
                if (numericValue < 0)
                {
                    continue;
                }

                _methods[numericValue] = new List<GameMethod>();
            }
        }

        #endregion

        #region Methods

        public List<GameMethod> GetList(GameMethodEvents value)
        {
            var numericValue = (int)value;
            if (numericValue < 0 || numericValue >= _methods.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"GameMethodEvents value '{value}' is outside the allocated range.");
            }

            return _methods[numericValue] ??= new List<GameMethod>();
        }

        public void AddMethod(GameMethodEvents key, GameMethod value)
        {
            var numericValue = (int)key;
            if (numericValue < 0 || numericValue >= _methods.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(key), $"GameMethodEvents value '{key}' is outside the allocated range.");
            }

            (_methods[numericValue] ??= new List<GameMethod>()).Add(value);
        }

        #endregion
    }
}
