using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace UnityMVC.Tests.Editor
{
    public class InfrastructureServicesEditorTests
    {
        [SetUp]
        public void SetUp()
        {
            ResetDataBus();
        }

        [TearDown]
        public void TearDown()
        {
            ResetDataBus();
        }

        [Test]
        public void DataBusReturnsFirstRegisteredValueAndClonesParameterizedModels()
        {
            var bus = GameDataBusImpl.Instance;
            Func<string> first = () => "first";
            Func<string> second = () => "second";
            Func<string, DataBusModel> modelFactory = value => new DataBusModel { Value = value };

            bus.AddListener(first);
            bus.AddListener(second);
            bus.AddListener<DataBusModel, string>(modelFactory);

            Assert.That(bus.GetData<string>(), Is.EqualTo("first"));

            var model = bus.GetData<DataBusModel>("payload");
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Value, Is.EqualTo("payload"));

            var modelFromSecondCall = bus.GetData<DataBusModel>("payload-2");
            Assert.That(modelFromSecondCall, Is.Not.Null);
            Assert.That(modelFromSecondCall.Value, Is.EqualTo("payload-2"));

            bus.RemoveListener(first);
            bus.RemoveListener(second);
            bus.RemoveListener<DataBusModel, string>(modelFactory);
        }

        [Test]
        public void DataBusThrowsWhenArgumentCountDoesNotMatchHandler()
        {
            var bus = GameDataBusImpl.Instance;
            Func<string, DataBusModel> modelFactory = value => new DataBusModel { Value = value };
            bus.AddListener<DataBusModel, string>(modelFactory);

            var exception = Assert.Throws<Exception>(() => bus.GetData<DataBusModel>("a", "b"));
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Does.Contain("parameters"));

            bus.RemoveListener<DataBusModel, string>(modelFactory);
        }

        [Test]
        public void GameMethodInvokesZeroToFourParameterMethods()
        {
            var receiver = new GameMethodReceiver();
            var type = receiver.GetType();

            GameMethod.Create(receiver, type.GetMethod("Zero", BindingFlags.Instance | BindingFlags.Public)).Invoke();
            GameMethod.Create(receiver, type.GetMethod("One", BindingFlags.Instance | BindingFlags.Public)).Invoke("alpha");
            GameMethod.Create(receiver, type.GetMethod("Two", BindingFlags.Instance | BindingFlags.Public)).Invoke("beta", 2);
            GameMethod.Create(receiver, type.GetMethod("Three", BindingFlags.Instance | BindingFlags.Public)).Invoke("gamma", 3, true);
            GameMethod.Create(receiver, type.GetMethod("Four", BindingFlags.Instance | BindingFlags.Public)).Invoke("delta", 4, false, 1.5f);

            Assert.That(receiver.Calls, Is.EqualTo(new[]
            {
                "Zero",
                "One:alpha",
                "Two:beta:2",
                "Three:gamma:3:True",
                "Four:delta:4:False:1.5"
            }));
        }

        [Test]
        public void GameMethodDisablesCachedDelegateAfterDestroyedObjectException()
        {
            var receiver = new MissingReferenceGameMethodReceiver();
            var method = GameMethod.Create(receiver, receiver.GetType().GetMethod("TouchDestroyedObject", BindingFlags.Instance | BindingFlags.Public));

            LogAssert.Expect(LogType.Warning, "[MVC] Disabling cached delegate for 'TouchDestroyedObject' on 'MissingReferenceGameMethodReceiver' because target is destroyed.");
            method.Invoke();

            LogAssert.NoUnexpectedReceived();
            method.Invoke();
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void GameMethodSkipsInvocationWhenControllerViewIsDestroyed()
        {
            var view = new GameObject("destroyed-view");
            var receiver = new ViewBackedGameMethodReceiver(view);
            var method = GameMethod.Create(receiver, receiver.GetType().GetMethod("Tick", BindingFlags.Instance | BindingFlags.Public));

            Object.DestroyImmediate(view);
            method.Invoke();

            Assert.That(receiver.Calls, Is.EqualTo(0));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void EventBusRemovalIsImmediate()
        {
            var bus = CreateEventBus();
            var hits = new List<string>();

            void OnEvent(TestEvent payload) => hits.Add(payload.Value);

            bus.AddListener<TestEvent>(OnEvent);
            bus.RaiseEvent(new TestEvent("first"));
            Assert.That(hits, Is.EqualTo(new[] { "first" }));

            bus.RemoveListener<TestEvent>(OnEvent);
            bus.RaiseEvent(new TestEvent("second"));
            Assert.That(hits, Is.EqualTo(new[] { "first" }));
        }

        [Test]
        public void EventBusSupportsRemovalDuringDispatch()
        {
            var bus = CreateEventBus();
            var hits = new List<string>();

            Action<TestEvent> first = null;
            first = _ =>
            {
                hits.Add("first");
                bus.RemoveListener<TestEvent>(first);
            };

            void Second(TestEvent _) => hits.Add("second");

            bus.AddListener<TestEvent>(first);
            bus.AddListener<TestEvent>(Second);

            Assert.DoesNotThrow(() => bus.RaiseEvent(new TestEvent("event-a")));
            Assert.That(hits, Is.EqualTo(new[] { "first", "second" }));

            bus.RaiseEvent(new TestEvent("event-b"));
            Assert.That(hits, Is.EqualTo(new[] { "first", "second", "second" }));
        }

        private static GameEventBusImpl CreateEventBus()
        {
            return (GameEventBusImpl)Activator.CreateInstance(typeof(GameEventBusImpl), true);
        }

        private static void ResetDataBus()
        {
            var instanceField = typeof(GameDataBusImpl).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            var instance = instanceField?.GetValue(null);
            if (instance == null)
            {
                return;
            }

            var handlersField = typeof(GameDataBusImpl).GetField("_eventHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
            var handlers = handlersField?.GetValue(instance) as IDictionary;
            handlers?.Clear();
            instanceField?.SetValue(null, null);
        }

        [Serializable]
        private class DataBusModel : GameModel
        {
            public string Value;
        }

        private sealed class GameMethodReceiver
        {
            public readonly List<string> Calls = new List<string>();

            public void Zero() => Calls.Add("Zero");
            public void One(string value) => Calls.Add($"One:{value}");
            public void Two(string value, int count) => Calls.Add($"Two:{value}:{count}");
            public void Three(string value, int count, bool flag) => Calls.Add($"Three:{value}:{count}:{flag}");
            public void Four(string value, int count, bool flag, float amount) => Calls.Add($"Four:{value}:{count}:{flag}:{amount}");
        }

        private sealed class TestEvent
        {
            public readonly string Value;

            public TestEvent(string value)
            {
                Value = value;
            }
        }

        private sealed class MissingReferenceGameMethodReceiver
        {
            public void TouchDestroyedObject()
            {
                throw new MissingReferenceException("The object has been destroyed.");
            }
        }

        private sealed class ViewBackedGameMethodReceiver
        {
            private readonly GameObject _view;
            public int Calls { get; private set; }

            public ViewBackedGameMethodReceiver(GameObject view)
            {
                _view = view;
            }

            public GameObject GetView() => _view;

            public void Tick()
            {
                Calls++;
            }
        }
    }
}
