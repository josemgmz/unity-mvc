using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityMVC.Tests.Editor
{
    public class GameViewLifecycleEditorTests
    {
        private const BindingFlags AllBindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly List<GameObject> _createdObjects = new List<GameObject>();
        private List<string> _callLog;

        [SetUp]
        public void SetUp()
        {
            _callLog = new List<string>();
            PrimaryModel.Log = _callLog;
            SecondaryModel.Log = _callLog;
            FirstOrderController.Log = _callLog;
            SecondOrderController.Log = _callLog;
            DeclaredFirstController.Log = _callLog;
            DeclaredSecondController.Log = _callLog;
            AllEventsController.Log = _callLog;
            AllUiEventsController.Log = _callLog;
            InvokableController.Log = _callLog;
            ReinitializeController.Log = _callLog;
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = _createdObjects.Count - 1; index >= 0; index--)
            {
                if (_createdObjects[index] != null)
                {
                    UnityEngine.Object.DestroyImmediate(_createdObjects[index]);
                }
            }

            _createdObjects.Clear();
        }

        [Test]
        public void ModelsAreCreatedAwakenedClonedAndDestroyedInCurrentFlow()
        {
            var view = CreateComponent<ModelLifecycleView>("model-lifecycle");

            Assert.That(_callLog, Is.EqualTo(new[]
            {
                "PrimaryModel.ctor",
                "SecondaryModel.ctor",
                "PrimaryModel.Awake",
                "SecondaryModel.Awake"
            }));

            var primaryClone = view.GetModel<PrimaryModel>();
            var secondaryClone = view.GetModel<SecondaryModel>();

            Assert.That(primaryClone, Is.Not.Null);
            Assert.That(secondaryClone, Is.Not.Null);
            Assert.That(primaryClone, Is.Not.SameAs(GetField<PrimaryModel>(view, "primaryModel")));
            Assert.That(secondaryClone, Is.Not.SameAs(GetField<SecondaryModel>(view, "secondaryModel")));

            _callLog.Clear();
            InvokeHidden(view, "OnDestroy");

            Assert.That(_callLog, Is.EqualTo(new[]
            {
                "PrimaryModel.Destroy",
                "SecondaryModel.Destroy"
            }));
            Assert.That(view.GetCancellationToken().IsCancellationRequested, Is.True);
        }

        [Test]
        public void ControllersInitializeInCurrentDefaultReverseFieldOrder()
        {
            var view = CreateComponent<DefaultOrderView>("default-order");
            view.GetController<FirstOrderController>();

            Assert.That(_callLog, Is.EqualTo(new[]
            {
                "PrimaryModel.ctor",
                "SecondaryModel.ctor",
                "PrimaryModel.Awake",
                "SecondaryModel.Awake",
                "SecondOrderController.ctor",
                "FirstOrderController.ctor",
                "SecondOrderController.SetView",
                "SecondOrderController.SetModel:PrimaryModel",
                "SecondOrderController.SetModel:SecondaryModel",
                "SecondOrderController.SetInternalModel",
                "FirstOrderController.SetView",
                "FirstOrderController.SetModel:PrimaryModel",
                "FirstOrderController.SetModel:SecondaryModel",
                "SecondOrderController.PreAwake",
                "FirstOrderController.PreAwake",
                "SecondOrderController.Awake",
                "FirstOrderController.Awake"
            }));
        }

        [Test]
        public void ControllerReverseOrderAttributePreservesDeclaredOrder()
        {
            var view = CreateComponent<DeclaredOrderView>("declared-order");
            view.GetController<DeclaredFirstController>();

            Assert.That(_callLog, Is.EqualTo(new[]
            {
                "PrimaryModel.ctor",
                "SecondaryModel.ctor",
                "PrimaryModel.Awake",
                "SecondaryModel.Awake",
                "DeclaredFirstController.ctor",
                "DeclaredSecondController.ctor",
                "DeclaredFirstController.SetView",
                "DeclaredFirstController.SetModel:PrimaryModel",
                "DeclaredFirstController.SetModel:SecondaryModel",
                "DeclaredSecondController.SetView",
                "DeclaredSecondController.SetModel:PrimaryModel",
                "DeclaredSecondController.SetModel:SecondaryModel",
                "DeclaredFirstController.PreAwake",
                "DeclaredSecondController.PreAwake",
                "DeclaredFirstController.Awake",
                "DeclaredSecondController.Awake"
            }));
        }

        [Test]
        public void EditorInitializationPathCreatesOnlyEditorAndAlwaysControllers()
        {
            var view = CreateComponent<EditorExecutionModesView>("editor-execution");

            _callLog.Clear();
            InvokeHidden(view, "OnDrawGizmos");

            Assert.That(view.TryGetController<EditorOnlyController>(out _), Is.True);
            Assert.That(view.TryGetController<AlwaysController>(out _), Is.True);
            Assert.That(view.TryGetController<PlayOnlyController>(out _), Is.False);
        }

        [Test]
        public void GameViewForwardsAllNonUiEventsToControllers()
        {
            var view = CreateComponent<AllEventsView>("all-events");
            view.GetController<AllEventsController>();
            _callLog.Clear();

            InvokeHidden(view, "Start");
            InvokeHidden(view, "Update");
            InvokeHidden(view, "LateUpdate");
            InvokeHidden(view, "FixedUpdate");
            InvokeHidden(view, "OnEnable");
            InvokeHidden(view, "OnDisable");
            InvokeHidden(view, "OnMouseDown");
            InvokeHidden(view, "OnMouseEnter");
            InvokeHidden(view, "OnMouseExit");
            InvokeHidden(view, "OnBecameInvisible");
            InvokeHidden(view, "OnParticleSystemStopped");
            InvokeHidden(view, "OnAnimatorEvent", "anim");
            InvokeHidden(view, "OnCollisionEnter", (Collision)null);
            InvokeHidden(view, "OnCollisionExit", (Collision)null);
            InvokeHidden(view, "OnCollisionStay", (Collision)null);
            InvokeHidden(view, "OnTriggerEnter", (Collider)null);
            InvokeHidden(view, "OnTriggerExit", (Collider)null);
            InvokeHidden(view, "OnTriggerStay", (Collider)null);
            InvokeHidden(view, "OnCollisionEnter2D", (Collision2D)null);
            InvokeHidden(view, "OnCollisionExit2D", (Collision2D)null);
            InvokeHidden(view, "OnCollisionStay2D", (Collision2D)null);
            InvokeHidden(view, "OnTriggerEnter2D", (Collider2D)null);
            InvokeHidden(view, "OnTriggerExit2D", (Collider2D)null);
            InvokeHidden(view, "OnTriggerStay2D", (Collider2D)null);
            InvokeHidden(view, "OnDestroy");

            Assert.That(_callLog, Is.EqualTo(new[]
            {
                "Start",
                "Update",
                "LateUpdate",
                "FixedUpdate",
                "OnEnable",
                "OnDisable",
                "OnMouseDown",
                "OnMouseEnter",
                "OnMouseExit",
                "OnBecameInvisible",
                "OnParticleSystemStopped",
                "OnAnimatorEvent:anim",
                "OnCollisionEnter:True",
                "OnCollisionExit:True",
                "OnCollisionStay:True",
                "OnTriggerEnter:True",
                "OnTriggerExit:True",
                "OnTriggerStay:True",
                "OnCollisionEnter2D:True",
                "OnCollisionExit2D:True",
                "OnCollisionStay2D:True",
                "OnTriggerEnter2D:True",
                "OnTriggerExit2D:True",
                "OnTriggerStay2D:True",
                "OnDestroy"
            }));
        }

        [Test]
        public void GameViewUiForwardsAllUiEventsToControllers()
        {
            var eventSystem = CreateGameObject("event-system");
            eventSystem.AddComponent<EventSystem>();

            var view = CreateComponent<AllUiEventsView>("all-ui-events");
            view.GetController<AllUiEventsController>();
            var eventSystemComponent = eventSystem.GetComponent<EventSystem>();
            var pointerData = new PointerEventData(eventSystemComponent);
            var baseEventData = new BaseEventData(eventSystemComponent);

            _callLog.Clear();
            view.OnBeginDrag(pointerData);
            view.OnDrag(pointerData);
            view.OnEndDrag(pointerData);
            view.OnPointerDown(pointerData);
            view.OnPointerExit(pointerData);
            view.OnPointerMove(pointerData);
            view.OnPointerUp(pointerData);
            view.OnPointerEnter(pointerData);
            view.OnSelect(baseEventData);
            view.OnDeselect(baseEventData);

            Assert.That(_callLog, Is.EqualTo(new[]
            {
                "OnBeginDrag:True",
                "OnDrag:True",
                "OnEndDrag:True",
                "OnPointerDown:True",
                "OnPointerExit:True",
                "OnPointerMove:True",
                "OnPointerUp:True",
                "OnPointerEnter:True",
                "OnSelect:True",
                "OnDeselect:True"
            }));
        }

        [Test]
        public void ControllerResolutionAndInvokeControllerMethodMatchCurrentBehavior()
        {
            var view = CreateComponent<LookupView>("lookup");
            var exact = view.GetController<LookupController>();

            Assert.That(exact, Is.Not.Null);
            Assert.That(view.GetController<ILookupMarker>(), Is.SameAs(exact));
            Assert.That(view.gameObject.GetController<LookupController>(), Is.SameAs(exact));
            Assert.That(view.gameObject.GetController(typeof(ILookupMarker)), Is.SameAs(exact));
            Assert.That(view.gameObject.TryGetController<LookupController>(out var fromGameObject), Is.True);
            Assert.That(fromGameObject, Is.SameAs(exact));

            var collider = view.gameObject.AddComponent<BoxCollider>();
            Assert.That(collider.GetController<LookupController>(), Is.SameAs(exact));
            Assert.That(collider.GetController(typeof(ILookupMarker)), Is.SameAs(exact));
            Assert.That(collider.TryGetController<LookupController>(out var fromCollider), Is.True);
            Assert.That(fromCollider, Is.SameAs(exact));

            var model = view.GetModel<BaseLookupModel>();
            Assert.That(model, Is.TypeOf<DerivedLookupModel>());
            Assert.That(model, Is.Not.SameAs(GetField<DerivedLookupModel>(view, "lookupModel")));

            var invokableView = CreateComponent<InvokableView>("invokable");
            invokableView.GetController<InvokableController>();
            _callLog.Clear();

            Assert.That(invokableView.InvokeControllerMethod(typeof(InvokableController), "Ping"), Is.True);
            Assert.That(invokableView.InvokeControllerMethod("SetLabel", "alpha"), Is.True);
            Assert.That(invokableView.InvokeControllerMethod(typeof(InvokableController), "SetPair", "beta", 3), Is.True);
            Assert.That(invokableView.InvokeControllerMethod<InvokableController>("SetPair", "gamma", 7), Is.True);
            Assert.That(invokableView.InvokeControllerMethod(typeof(InvokableController), "MissingMethod"), Is.False);

            Assert.That(_callLog, Is.EqualTo(new[]
            {
                "Ping",
                "SetLabel:alpha",
                "SetPair:beta:3",
                "SetPair:gamma:7"
            }));
        }

        [Test]
        public void EditorReinitializeRebuildsControllerInstances()
        {
            var view = CreateComponent<ReinitializeView>("reinitialize");
            var firstController = view.GetController<ReinitializeController>();

            _callLog.Clear();
            view.EditorReinitialize();
            var secondController = view.GetController<ReinitializeController>();

            Assert.That(secondController, Is.Not.SameAs(firstController));
            Assert.That(_callLog, Does.Contain("ReinitializeController.ctor"));
            Assert.That(_callLog, Does.Contain("ReinitializeController.Awake"));
        }

        private T CreateComponent<T>(string name) where T : Component
        {
            var gameObject = CreateGameObject(name);
            return gameObject.AddComponent<T>();
        }

        private GameObject CreateGameObject(string name)
        {
            var gameObject = new GameObject(name);
            _createdObjects.Add(gameObject);
            return gameObject;
        }

        private static TField GetField<TField>(object target, string fieldName)
        {
            var type = target.GetType();
            while (type != null)
            {
                var field = type.GetField(fieldName, AllBindings | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return (TField)field.GetValue(target);
                }

                type = type.BaseType;
            }

            throw new MissingFieldException(target.GetType().FullName, fieldName);
        }

        private static void InvokeHidden(object target, string methodName, params object[] args)
        {
            var type = target.GetType();
            while (type != null)
            {
                var methods = type.GetMethods(AllBindings | BindingFlags.DeclaredOnly);
                for (var index = 0; index < methods.Length; index++)
                {
                    var method = methods[index];
                    if (method.Name != methodName)
                    {
                        continue;
                    }

                    if (method.GetParameters().Length != (args?.Length ?? 0))
                    {
                        continue;
                    }

                    method.Invoke(target, args);
                    return;
                }

                type = type.BaseType;
            }

            throw new MissingMethodException(target.GetType().FullName, methodName);
        }

        [Serializable]
        public class PrimaryModel : GameModel
        {
            public static List<string> Log;
            public PrimaryModel() => Log?.Add("PrimaryModel.ctor");
            public new void Awake<T>() where T : GameModel => Log?.Add("PrimaryModel.Awake");
            public new void Destroy<T>() where T : GameModel => Log?.Add("PrimaryModel.Destroy");
        }

        [Serializable]
        public class SecondaryModel : GameModel
        {
            public static List<string> Log;
            public SecondaryModel() => Log?.Add("SecondaryModel.ctor");
            public new void Awake<T>() where T : GameModel => Log?.Add("SecondaryModel.Awake");
            public new void Destroy<T>() where T : GameModel => Log?.Add("SecondaryModel.Destroy");
        }

        public class ModelLifecycleView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PrimaryModel primaryModel;
            [SerializeField, GameFieldAttributes.ModelField] private SecondaryModel secondaryModel;
        }

        public class DefaultOrderView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PrimaryModel primaryModel;
            [SerializeField, GameFieldAttributes.ModelField] private SecondaryModel secondaryModel;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways] private FirstOrderController firstController;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways] private SecondOrderController secondController;
        }

        public class DeclaredOrderView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PrimaryModel primaryModel;
            [SerializeField, GameFieldAttributes.ModelField] private SecondaryModel secondaryModel;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways, GameFieldAttributes.ControllerReverseOrder] private DeclaredFirstController firstController;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways] private DeclaredSecondController secondController;
        }

        public class FirstOrderController : GameController<DefaultOrderView>
        {
            public static List<string> Log;
            public FirstOrderController() => Log?.Add("FirstOrderController.ctor");
            public new void SetView(DefaultOrderView view) { Log?.Add("FirstOrderController.SetView"); base.SetView(view); }
            public new void SetModel<T>(object model) { Log?.Add($"FirstOrderController.SetModel:{typeof(T).Name}"); base.SetModel<T>(model); }
            private void PreAwake() => Log?.Add("FirstOrderController.PreAwake");
            private void Awake() => Log?.Add("FirstOrderController.Awake");
        }

        public class SecondOrderController : GameController<DefaultOrderView, PrimaryModel>
        {
            public static List<string> Log;
            public SecondOrderController() => Log?.Add("SecondOrderController.ctor");
            public new void SetView(DefaultOrderView view) { Log?.Add("SecondOrderController.SetView"); base.SetView(view); }
            public new void SetModel<T>(object model) { Log?.Add($"SecondOrderController.SetModel:{typeof(T).Name}"); base.SetModel<T>(model); }
            public new void SetInternalModel(PrimaryModel model) { Log?.Add("SecondOrderController.SetInternalModel"); base.SetInternalModel(model); }
            private void PreAwake() => Log?.Add("SecondOrderController.PreAwake");
            private void Awake() => Log?.Add("SecondOrderController.Awake");
        }

        public class DeclaredFirstController : GameController<DeclaredOrderView>
        {
            public static List<string> Log;
            public DeclaredFirstController() => Log?.Add("DeclaredFirstController.ctor");
            public new void SetView(DeclaredOrderView view) { Log?.Add("DeclaredFirstController.SetView"); base.SetView(view); }
            public new void SetModel<T>(object model) { Log?.Add($"DeclaredFirstController.SetModel:{typeof(T).Name}"); base.SetModel<T>(model); }
            private void PreAwake() => Log?.Add("DeclaredFirstController.PreAwake");
            private void Awake() => Log?.Add("DeclaredFirstController.Awake");
        }

        public class DeclaredSecondController : GameController<DeclaredOrderView>
        {
            public static List<string> Log;
            public DeclaredSecondController() => Log?.Add("DeclaredSecondController.ctor");
            public new void SetView(DeclaredOrderView view) { Log?.Add("DeclaredSecondController.SetView"); base.SetView(view); }
            public new void SetModel<T>(object model) { Log?.Add($"DeclaredSecondController.SetModel:{typeof(T).Name}"); base.SetModel<T>(model); }
            private void PreAwake() => Log?.Add("DeclaredSecondController.PreAwake");
            private void Awake() => Log?.Add("DeclaredSecondController.Awake");
        }

        public class EditorExecutionModesView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PrimaryModel primaryModel;
            [GameFieldAttributes.ControllerField] private PlayOnlyController playOnlyController;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerEditorOnly] private EditorOnlyController editorOnlyController;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways] private AlwaysController alwaysController;
        }

        public class PlayOnlyController : GameController<EditorExecutionModesView> { public PlayOnlyController() {} }
        public class EditorOnlyController : GameController<EditorExecutionModesView> { public EditorOnlyController() {} }
        public class AlwaysController : GameController<EditorExecutionModesView> { public AlwaysController() {} }

        [Serializable]
        public class EventModel : GameModel
        {
        }

        public class AllEventsView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private EventModel model;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways] private AllEventsController controller;
        }

        public class AllEventsController : GameController<AllEventsView>
        {
            public static List<string> Log;
            public AllEventsController() {}
            private void Start() => Log?.Add("Start");
            private void Update() => Log?.Add("Update");
            private void LateUpdate() => Log?.Add("LateUpdate");
            private void FixedUpdate() => Log?.Add("FixedUpdate");
            private void OnDestroy() => Log?.Add("OnDestroy");
            private void OnEnable() => Log?.Add("OnEnable");
            private void OnDisable() => Log?.Add("OnDisable");
            private void OnMouseDown() => Log?.Add("OnMouseDown");
            private void OnMouseEnter() => Log?.Add("OnMouseEnter");
            private void OnMouseExit() => Log?.Add("OnMouseExit");
            private void OnBecameInvisible() => Log?.Add("OnBecameInvisible");
            private void OnParticleSystemStopped() => Log?.Add("OnParticleSystemStopped");
            private void OnAnimatorEvent(string value) => Log?.Add($"OnAnimatorEvent:{value}");
            private void OnCollisionEnter(Collision value) => Log?.Add($"OnCollisionEnter:{value == null}");
            private void OnCollisionExit(Collision value) => Log?.Add($"OnCollisionExit:{value == null}");
            private void OnCollisionStay(Collision value) => Log?.Add($"OnCollisionStay:{value == null}");
            private void OnTriggerEnter(Collider value) => Log?.Add($"OnTriggerEnter:{value == null}");
            private void OnTriggerExit(Collider value) => Log?.Add($"OnTriggerExit:{value == null}");
            private void OnTriggerStay(Collider value) => Log?.Add($"OnTriggerStay:{value == null}");
            private void OnCollisionEnter2D(Collision2D value) => Log?.Add($"OnCollisionEnter2D:{value == null}");
            private void OnCollisionExit2D(Collision2D value) => Log?.Add($"OnCollisionExit2D:{value == null}");
            private void OnCollisionStay2D(Collision2D value) => Log?.Add($"OnCollisionStay2D:{value == null}");
            private void OnTriggerEnter2D(Collider2D value) => Log?.Add($"OnTriggerEnter2D:{value == null}");
            private void OnTriggerExit2D(Collider2D value) => Log?.Add($"OnTriggerExit2D:{value == null}");
            private void OnTriggerStay2D(Collider2D value) => Log?.Add($"OnTriggerStay2D:{value == null}");
        }

        public class AllUiEventsView : GameViewUI
        {
            [SerializeField, GameFieldAttributes.ModelField] private EventModel model;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways] private AllUiEventsController controller;
        }

        public class AllUiEventsController : GameController<AllUiEventsView>
        {
            public static List<string> Log;
            public AllUiEventsController() {}
            private void OnBeginDrag(PointerEventData value) => Log?.Add($"OnBeginDrag:{value != null}");
            private void OnDrag(PointerEventData value) => Log?.Add($"OnDrag:{value != null}");
            private void OnEndDrag(PointerEventData value) => Log?.Add($"OnEndDrag:{value != null}");
            private void OnPointerDown(PointerEventData value) => Log?.Add($"OnPointerDown:{value != null}");
            private void OnPointerExit(PointerEventData value) => Log?.Add($"OnPointerExit:{value != null}");
            private void OnPointerMove(PointerEventData value) => Log?.Add($"OnPointerMove:{value != null}");
            private void OnPointerUp(PointerEventData value) => Log?.Add($"OnPointerUp:{value != null}");
            private void OnPointerEnter(PointerEventData value) => Log?.Add($"OnPointerEnter:{value != null}");
            private void OnSelect(BaseEventData value) => Log?.Add($"OnSelect:{value != null}");
            private void OnDeselect(BaseEventData value) => Log?.Add($"OnDeselect:{value != null}");
        }

        public interface ILookupMarker {}

        [Serializable]
        public class BaseLookupModel : GameModel
        {
            public string Name;
        }

        [Serializable]
        public class DerivedLookupModel : BaseLookupModel
        {
        }

        public class LookupView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private DerivedLookupModel lookupModel;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways] private LookupController controller;
        }

        public class LookupController : GameController<LookupView>, ILookupMarker
        {
            public LookupController() {}
        }

        public class InvokableView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private EventModel model;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways] private InvokableController controller;
        }

        public class InvokableController : GameController<InvokableView>
        {
            public static List<string> Log;
            public InvokableController() {}
            private void Ping() => Log?.Add("Ping");
            private void SetLabel(string value) => Log?.Add($"SetLabel:{value}");
            private void SetPair(string label, int number) => Log?.Add($"SetPair:{label}:{number}");
        }

        public class ReinitializeView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private EventModel model;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways] private ReinitializeController controller;
        }

        public class ReinitializeController : GameController<ReinitializeView>
        {
            public static List<string> Log;
            public ReinitializeController() => Log?.Add("ReinitializeController.ctor");
            private void Awake() => Log?.Add("ReinitializeController.Awake");
        }
    }
}
