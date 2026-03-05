using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityMVC.Tests.Runtime
{
    public class GameViewInfrastructurePlayModeTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayModeLifecycleModel.AwakeCalls = 0;
            PlayModeLifecycleModel.DestroyCalls = 0;
            PlayModeLifecycleController.AwakeCalls = 0;
            PlayModeLifecycleController.StartCalls = 0;
            PlayModeLifecycleController.UpdateCalls = 0;
            PlayModeLifecycleController.LateUpdateCalls = 0;
            PlayModeLifecycleController.FixedUpdateCalls = 0;
            PlayModeLifecycleController.OnDestroyCalls = 0;
            PlayModeLifecycleController.OnEnableCalls = 0;
            PlayModeLifecycleController.OnDisableCalls = 0;
            PlayModeLifecycleController.ModelWasInjected = false;
            PlayModeLifecycleController.ModelValueSeenInAwake = -1;
            PlayModeLifecycleController.ModelPropertyNotNull = false;
            PlayModeLifecycleController.GetModelNotNull = false;
            PlayModeLifecycleController.ModelAndGetModelShareReference = false;
            PlayModeLifecycleController.ViewCloneHasExpectedValue = false;
            PlayModeLifecycleController.ViewCloneDiffersFromInjectedModel = false;
            PlayModeLifecycleController.LastUpdateFrame = -1;
            PlayModeLifecycleController.UpdateBeforeLateUpdateInSameFrame = false;
            PlayModeLifecycleController.Log = new List<string>();

            PlayModeControllerSurfaceController.GameObjectMatchesView = false;
            PlayModeControllerSurfaceController.TransformMatchesView = false;
            PlayModeControllerSurfaceController.TransformSetterApplied = false;
            PlayModeControllerSurfaceController.LayerSetApplied = false;
            PlayModeControllerSurfaceController.GetComponentFound = false;
            PlayModeControllerSurfaceController.GetControllerFound = false;
            PlayModeControllerSurfaceController.GetModelFound = false;
            PlayModeControllerSurfaceController.ActiveSelfAfterDisable = false;
            PlayModeControllerSurfaceController.ActiveSelfAfterEnable = false;
            PlayModeControllerSurfaceController.TempObjects = new List<GameObject>();

            PlayModeCollisionController.EnterCalls = 0;
            PlayModeCollisionController.StayCalls = 0;
            PlayModeCollisionController.ExitCalls = 0;
            PlayModeCollisionController.DestroyRequestedOnEnter = false;
            PlayModeCollisionController.DestroyedObjectName = null;
            PlayModeCollisionController.DestroyOnEnterEnabled = false;
        }

        [UnityTest]
        public IEnumerator PlayModeSkipsControllersAfterFirstNonInstantiableEntryInCurrentImplementation()
        {
            var gameObject = new GameObject("playmode-execution");
            try
            {
                var view = gameObject.AddComponent<PlayModeExecutionModesView>();

                yield return null;

                Assert.That(view.TryGetController<PlayModePlayOnlyController>(out _), Is.False);
                Assert.That(view.TryGetController<PlayModeAlwaysController>(out _), Is.True);
                Assert.That(view.TryGetController<PlayModeEditorOnlyController>(out _), Is.False);
            }
            finally
            {
                if (gameObject != null)
                {
                    Object.Destroy(gameObject);
                }
            }
        }

        [UnityTest]
        public IEnumerator PlayModeLifecycleAwakeIsInvokedOnce()
        {
            var gameObject = CreateLifecycleGameObject("playmode-awake");

            yield return null;

            Assert.That(PlayModeLifecycleModel.AwakeCalls, Is.EqualTo(1));
            Assert.That(PlayModeLifecycleController.AwakeCalls, Is.EqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeLifecycleStartIsInvokedOnce()
        {
            var gameObject = CreateLifecycleGameObject("playmode-start");

            yield return WaitForCondition(
                () => PlayModeLifecycleController.StartCalls == 1,
                20,
                "Start was not triggered exactly once in time.");

            Assert.That(PlayModeLifecycleController.StartCalls, Is.EqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeLifecycleUpdateIsInvoked()
        {
            var gameObject = CreateLifecycleGameObject("playmode-update");

            yield return WaitForCondition(
                () => PlayModeLifecycleController.UpdateCalls > 0,
                20,
                "Update was not triggered in time.");

            Assert.That(PlayModeLifecycleController.UpdateCalls, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeLifecycleLateUpdateIsInvoked()
        {
            var gameObject = CreateLifecycleGameObject("playmode-lateupdate");

            yield return WaitForCondition(
                () => PlayModeLifecycleController.LateUpdateCalls > 0,
                20,
                "LateUpdate was not triggered in time.");

            Assert.That(PlayModeLifecycleController.LateUpdateCalls, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeLifecycleFixedUpdateIsInvoked()
        {
            var gameObject = CreateLifecycleGameObject("playmode-fixedupdate");

            yield return WaitForFixedCondition(
                () => PlayModeLifecycleController.FixedUpdateCalls > 0,
                30,
                "FixedUpdate was not triggered in time.");

            Assert.That(PlayModeLifecycleController.FixedUpdateCalls, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeLifecycleOnDestroyIsInvokedAndCancelsToken()
        {
            var gameObject = new GameObject("playmode-ondestroy");
            var view = gameObject.AddComponent<PlayModeLifecycleView>();

            yield return null;

            var token = view.GetCancellationToken();
            Assert.That(token.IsCancellationRequested, Is.False);

            Object.Destroy(gameObject);
            yield return null;

            Assert.That(PlayModeLifecycleController.OnDestroyCalls, Is.EqualTo(1));
            Assert.That(PlayModeLifecycleModel.DestroyCalls, Is.EqualTo(1));
            Assert.That(token.IsCancellationRequested, Is.True);
        }

        [UnityTest]
        public IEnumerator PlayModeLifecycleOrderAwakeThenOnEnableThenStart()
        {
            var gameObject = CreateLifecycleGameObject("playmode-lifecycle-order-boot");

            yield return WaitForCondition(
                () => PlayModeLifecycleController.StartCalls == 1,
                20,
                "Start was not triggered in time for order validation.");

            var awakeIndex = PlayModeLifecycleController.Log.IndexOf("Awake");
            var onEnableIndex = PlayModeLifecycleController.Log.IndexOf("OnEnable");
            var startIndex = PlayModeLifecycleController.Log.IndexOf("Start");

            Assert.That(awakeIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(onEnableIndex, Is.GreaterThan(awakeIndex));
            Assert.That(startIndex, Is.GreaterThan(onEnableIndex));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeLifecycleOrderUpdateBeforeLateUpdate()
        {
            var gameObject = CreateLifecycleGameObject("playmode-lifecycle-order-loop");

            yield return WaitForCondition(
                () => PlayModeLifecycleController.UpdateBeforeLateUpdateInSameFrame,
                20,
                "Update/LateUpdate were not triggered in time for order validation.");

            Assert.That(PlayModeLifecycleController.UpdateBeforeLateUpdateInSameFrame, Is.True);

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeControllerReceivesModelAndViewModelClone()
        {
            var gameObject = new GameObject("playmode-model-access");
            gameObject.AddComponent<PlayModeLifecycleView>();

            yield return null;

            Assert.That(PlayModeLifecycleController.ModelWasInjected, Is.True);
            Assert.That(PlayModeLifecycleController.ModelValueSeenInAwake, Is.EqualTo(99));
            Assert.That(PlayModeLifecycleController.ViewCloneHasExpectedValue, Is.True);
            Assert.That(PlayModeLifecycleController.ViewCloneDiffersFromInjectedModel, Is.True);

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeControllerModelAndGetModelAreNotNull()
        {
            var gameObject = new GameObject("playmode-controller-getmodel");
            gameObject.AddComponent<PlayModeLifecycleView>();

            yield return null;

            Assert.That(PlayModeLifecycleController.ModelPropertyNotNull, Is.True);
            Assert.That(PlayModeLifecycleController.GetModelNotNull, Is.True);
            Assert.That(PlayModeLifecycleController.ModelAndGetModelShareReference, Is.True);

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeControllerExposesCoreUnitySurfaceHelpers()
        {
            var gameObject = new GameObject("playmode-controller-surface");
            gameObject.AddComponent<PlayModeSurfaceMarker>();
            var view = gameObject.AddComponent<PlayModeControllerSurfaceView>();

            yield return null;

            Assert.That(PlayModeControllerSurfaceController.GameObjectMatchesView, Is.True);
            Assert.That(PlayModeControllerSurfaceController.TransformMatchesView, Is.True);
            Assert.That(PlayModeControllerSurfaceController.TransformSetterApplied, Is.True);
            Assert.That(PlayModeControllerSurfaceController.LayerSetApplied, Is.True);
            Assert.That(PlayModeControllerSurfaceController.GetComponentFound, Is.True);
            Assert.That(PlayModeControllerSurfaceController.GetControllerFound, Is.True);
            Assert.That(PlayModeControllerSurfaceController.GetModelFound, Is.True);

            var controller = view.GetController<PlayModeControllerSurfaceController>();
            controller.DisableView();
            yield return null;

            Assert.That(gameObject.activeSelf, Is.False);
            Assert.That(PlayModeControllerSurfaceController.ActiveSelfAfterDisable, Is.True);

            controller.EnableView();
            yield return null;

            Assert.That(gameObject.activeSelf, Is.True);
            Assert.That(PlayModeControllerSurfaceController.ActiveSelfAfterEnable, Is.True);

            if (PlayModeControllerSurfaceController.TempObjects != null)
            {
                for (var index = 0; index < PlayModeControllerSurfaceController.TempObjects.Count; index++)
                {
                    var temporaryObject = PlayModeControllerSurfaceController.TempObjects[index];
                    if (temporaryObject != null)
                    {
                        Object.Destroy(temporaryObject);
                    }
                }
            }

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnDisableIsInvokedWhenViewIsSetInactive()
        {
            var gameObject = CreateLifecycleGameObject("playmode-ondisable");

            yield return null;

            var initialDisableCalls = PlayModeLifecycleController.OnDisableCalls;
            gameObject.SetActive(false);
            yield return null;

            Assert.That(PlayModeLifecycleController.OnDisableCalls, Is.EqualTo(initialDisableCalls + 1));

            if (gameObject != null)
            {
                Object.Destroy(gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnEnableIsInvokedWhenViewIsReactivated()
        {
            var gameObject = CreateLifecycleGameObject("playmode-onenable");

            yield return null;

            gameObject.SetActive(false);
            yield return null;

            var initialEnableCalls = PlayModeLifecycleController.OnEnableCalls;
            gameObject.SetActive(true);
            yield return null;

            Assert.That(PlayModeLifecycleController.OnEnableCalls, Is.EqualTo(initialEnableCalls + 1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeCollisionEnterIsInvokedAndCanDestroyFloorOnEnter()
        {
            PlayModeCollisionController.DestroyOnEnterEnabled = true;
            CreateCollisionScene(
                "collision-enter",
                out var floor,
                out var fallingObject,
                out _,
                out _);

            yield return WaitForFixedCondition(
                () => PlayModeCollisionController.EnterCalls > 0,
                120,
                "OnCollisionEnter was not triggered in time.");

            yield return WaitForFixedCondition(
                () => floor == null,
                120,
                "Floor was not destroyed from OnCollisionEnter in time.");

            Assert.That(PlayModeCollisionController.DestroyRequestedOnEnter, Is.True);
            Assert.That(PlayModeCollisionController.DestroyedObjectName, Is.EqualTo("collision-enter-floor"));
            Assert.That(PlayModeCollisionController.EnterCalls, Is.GreaterThanOrEqualTo(1));
            Assert.That(floor == null, Is.True);

            if (fallingObject != null)
            {
                Object.Destroy(fallingObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeCollisionStayIsInvokedWhileBodiesRemainInContact()
        {
            CreateCollisionScene(
                "collision-stay",
                out var floor,
                out var fallingObject,
                out _,
                out _);

            yield return WaitForFixedCondition(
                () => PlayModeCollisionController.EnterCalls > 0,
                120,
                "OnCollisionEnter was not triggered in time.");

            yield return WaitForFixedCondition(
                () => PlayModeCollisionController.StayCalls > 0,
                120,
                "OnCollisionStay was not triggered in time.");

            Assert.That(PlayModeCollisionController.StayCalls, Is.GreaterThanOrEqualTo(1));

            if (fallingObject != null)
            {
                Object.Destroy(fallingObject);
            }

            if (floor != null)
            {
                Object.Destroy(floor);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeCollisionExitIsInvokedWhenCollisionPairIsRemoved()
        {
            CreateCollisionScene(
                "collision-exit",
                out var floor,
                out var fallingObject,
                out var floorCollider,
                out var fallingCollider);

            yield return WaitForFixedCondition(
                () => PlayModeCollisionController.EnterCalls > 0,
                120,
                "OnCollisionEnter was not triggered in time.");

            yield return WaitForFixedCondition(
                () => PlayModeCollisionController.StayCalls > 0,
                120,
                "OnCollisionStay was not triggered in time.");

            Assert.That(floorCollider, Is.Not.Null);
            Assert.That(fallingCollider, Is.Not.Null);
            Physics.IgnoreCollision(fallingCollider, floorCollider, true);

            yield return WaitForFixedCondition(
                () => PlayModeCollisionController.ExitCalls > 0,
                120,
                "OnCollisionExit was not triggered in time.");

            Assert.That(PlayModeCollisionController.ExitCalls, Is.GreaterThanOrEqualTo(1));

            if (fallingObject != null)
            {
                Object.Destroy(fallingObject);
            }

            if (floor != null)
            {
                Object.Destroy(floor);
            }

            yield return null;
        }

        [System.Serializable]
        public class PlayModeModel : GameModel
        {
        }

        public class PlayModeExecutionModesView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeModel model;
            [GameFieldAttributes.ControllerField] private PlayModePlayOnlyController playOnlyController;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerEditorOnly] private PlayModeEditorOnlyController editorOnlyController;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecuteAlways] private PlayModeAlwaysController alwaysController;
        }

        public class PlayModePlayOnlyController : GameController<PlayModeExecutionModesView>
        {
            public PlayModePlayOnlyController()
            {
            }
        }

        public class PlayModeEditorOnlyController : GameController<PlayModeExecutionModesView>
        {
            public PlayModeEditorOnlyController()
            {
            }
        }

        public class PlayModeAlwaysController : GameController<PlayModeExecutionModesView>
        {
            public PlayModeAlwaysController()
            {
            }
        }

        [System.Serializable]
        public class PlayModeLifecycleModel : GameModel
        {
            public static int AwakeCalls;
            public static int DestroyCalls;
            public int Value = 99;

            public new void Awake<T>() where T : GameModel
            {
                AwakeCalls++;
            }

            public override void Destroy<T>()
            {
                DestroyCalls++;
            }
        }

        public class PlayModeLifecycleView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeLifecycleModel model;
            [GameFieldAttributes.ControllerField] private PlayModeLifecycleController controller;
        }

        public class PlayModeControllerSurfaceView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeLifecycleModel model;
            [GameFieldAttributes.ControllerField] private PlayModeControllerSurfaceController surfaceController;
            [GameFieldAttributes.ControllerField] private PlayModeSurfaceSecondaryController secondaryController;
        }

        public class PlayModeCollisionView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeCollisionModel model;
            [GameFieldAttributes.ControllerField] private PlayModeCollisionController collisionController;
        }

        public class PlayModeSurfaceMarker : MonoBehaviour
        {
        }

        public class PlayModeSurfaceSecondaryController : GameController<PlayModeControllerSurfaceView>
        {
            public PlayModeSurfaceSecondaryController()
            {
            }
        }

        [System.Serializable]
        public class PlayModeCollisionModel : GameModel
        {
        }

        public class PlayModeCollisionController : GameController<PlayModeCollisionView, PlayModeCollisionModel>
        {
            public static int EnterCalls;
            public static int StayCalls;
            public static int ExitCalls;
            public static bool DestroyRequestedOnEnter;
            public static string DestroyedObjectName;
            public static bool DestroyOnEnterEnabled;

            private void OnCollisionEnter(Collision other)
            {
                EnterCalls++;
                if (DestroyOnEnterEnabled && other != null && other.gameObject != null)
                {
                    DestroyRequestedOnEnter = true;
                    DestroyedObjectName = other.gameObject.name;
                    StartCoroutine(DestroyAfterFixedSteps(other.gameObject, 2));
                }
            }

            private void OnCollisionStay(Collision other)
            {
                StayCalls++;
            }

            private void OnCollisionExit(Collision other)
            {
                ExitCalls++;
            }

            private IEnumerator DestroyAfterFixedSteps(GameObject value, int stepsToWait)
            {
                var safeStepsToWait = Mathf.Max(1, stepsToWait);
                for (var index = 0; index < safeStepsToWait; index++)
                {
                    yield return new WaitForFixedUpdate();
                }

                // Force a deterministic separation pair removal so OnCollisionExit is raised.
                var selfCollider = GetComponent<Collider>();
                var otherCollider = value != null ? value.GetComponent<Collider>() : null;
                if (selfCollider != null && otherCollider != null)
                {
                    Physics.IgnoreCollision(selfCollider, otherCollider, true);
                    yield return new WaitForFixedUpdate();
                }

                if (value != null)
                {
                    Destroy(value);
                }
            }
        }

        public class PlayModeControllerSurfaceController : GameController<PlayModeControllerSurfaceView, PlayModeLifecycleModel>
        {
            public static bool GameObjectMatchesView;
            public static bool TransformMatchesView;
            public static bool TransformSetterApplied;
            public static bool LayerSetApplied;
            public static bool GetComponentFound;
            public static bool GetControllerFound;
            public static bool GetModelFound;
            public static bool ActiveSelfAfterDisable;
            public static bool ActiveSelfAfterEnable;
            public static List<GameObject> TempObjects;

            private void Awake()
            {
                var view = GetView();
                GameObjectMatchesView = ReferenceEquals(gameObject, view.gameObject);
                TransformMatchesView = ReferenceEquals(transform, view.transform);

                var marker = GetComponent<PlayModeSurfaceMarker>();
                GetComponentFound = marker != null;

                var localModel = GetModel<PlayModeLifecycleModel>();
                GetModelFound = localModel != null;

                var secondary = GetController<PlayModeSurfaceSecondaryController>();
                GetControllerFound = secondary != null;

                var probeTransformObject = new GameObject("playmode-surface-probe");
                TempObjects ??= new List<GameObject>();
                TempObjects.Add(probeTransformObject);
                probeTransformObject.transform.SetPositionAndRotation(new Vector3(3f, 2f, 1f), Quaternion.Euler(0f, 90f, 0f));
                transform = probeTransformObject.transform;

                TransformSetterApplied =
                    Vector3.Distance(view.transform.position, probeTransformObject.transform.position) <= 0.0001f &&
                    Quaternion.Angle(view.transform.rotation, probeTransformObject.transform.rotation) <= 0.01f;

                SetLayer(13);
                LayerSetApplied = view.gameObject.layer == 13;
            }

            public void DisableView()
            {
                SetActive(false);
                ActiveSelfAfterDisable = !activeSelf;
            }

            public void EnableView()
            {
                SetActive(true);
                ActiveSelfAfterEnable = activeSelf;
            }
        }

        public class PlayModeLifecycleController : GameController<PlayModeLifecycleView, PlayModeLifecycleModel>
        {
            public static int AwakeCalls;
            public static int StartCalls;
            public static int UpdateCalls;
            public static int LateUpdateCalls;
            public static int FixedUpdateCalls;
            public static int OnDestroyCalls;
            public static int OnEnableCalls;
            public static int OnDisableCalls;
            public static bool ModelWasInjected;
            public static int ModelValueSeenInAwake;
            public static bool ModelPropertyNotNull;
            public static bool GetModelNotNull;
            public static bool ModelAndGetModelShareReference;
            public static bool ViewCloneHasExpectedValue;
            public static bool ViewCloneDiffersFromInjectedModel;
            public static int LastUpdateFrame;
            public static bool UpdateBeforeLateUpdateInSameFrame;
            public static List<string> Log;

            private void Awake()
            {
                AwakeCalls++;
                Log?.Add("Awake");
                ModelWasInjected = Model != null;
                ModelPropertyNotNull = Model != null;
                ModelValueSeenInAwake = Model.Value;

                var modelFromGetModel = GetModel<PlayModeLifecycleModel>();
                GetModelNotNull = modelFromGetModel != null;
                ModelAndGetModelShareReference = ReferenceEquals(modelFromGetModel, Model);

                var cloneFromView = GetView().GetModel<PlayModeLifecycleModel>();
                ViewCloneHasExpectedValue = cloneFromView.Value == 99;
                ViewCloneDiffersFromInjectedModel = !ReferenceEquals(cloneFromView, Model);
            }

            private void Start()
            {
                StartCalls++;
                Log?.Add("Start");
            }

            private void Update()
            {
                UpdateCalls++;
                LastUpdateFrame = Time.frameCount;
                if (UpdateCalls == 1)
                {
                    Log?.Add("Update");
                }
            }

            private void LateUpdate()
            {
                LateUpdateCalls++;
                if (LastUpdateFrame == Time.frameCount)
                {
                    UpdateBeforeLateUpdateInSameFrame = true;
                }

                if (LateUpdateCalls == 1)
                {
                    Log?.Add("LateUpdate");
                }
            }

            private void FixedUpdate()
            {
                FixedUpdateCalls++;
                if (FixedUpdateCalls == 1)
                {
                    Log?.Add("FixedUpdate");
                }
            }

            private void OnDestroy()
            {
                OnDestroyCalls++;
                Log?.Add("OnDestroy");
            }

            private void OnEnable()
            {
                OnEnableCalls++;
                Log?.Add("OnEnable");
            }

            private void OnDisable()
            {
                OnDisableCalls++;
                Log?.Add("OnDisable");
            }
        }

        private static GameObject CreateLifecycleGameObject(string name)
        {
            var gameObject = new GameObject(name);
            gameObject.AddComponent<PlayModeLifecycleView>();
            return gameObject;
        }

        private static void CreateCollisionScene(
            string testName,
            out GameObject floor,
            out GameObject fallingObject,
            out Collider floorCollider,
            out Collider fallingCollider)
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = $"{testName}-floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2f, 1f, 2f);

            fallingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallingObject.name = $"{testName}-view";
            fallingObject.transform.position = new Vector3(0f, 1.5f, 0f);
            fallingObject.AddComponent<Rigidbody>();
            fallingObject.AddComponent<PlayModeCollisionView>();

            floorCollider = floor.GetComponent<Collider>();
            fallingCollider = fallingObject.GetComponent<Collider>();
        }

        private static IEnumerator WaitForCondition(System.Func<bool> condition, int maxFrames, string failureMessage)
        {
            for (var index = 0; index < maxFrames; index++)
            {
                if (condition())
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail(failureMessage);
        }

        private static IEnumerator WaitForFixedCondition(System.Func<bool> condition, int maxFixedSteps, string failureMessage)
        {
            for (var index = 0; index < maxFixedSteps; index++)
            {
                if (condition())
                {
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }

            Assert.Fail(failureMessage);
        }
    }
}
