using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
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

            PlayModeTrigger3DController.EnterCalls = 0;
            PlayModeTrigger3DController.StayCalls = 0;
            PlayModeTrigger3DController.ExitCalls = 0;

            PlayModeCollision2DController.EnterCalls = 0;
            PlayModeCollision2DController.StayCalls = 0;
            PlayModeCollision2DController.ExitCalls = 0;

            PlayModeTrigger2DController.EnterCalls = 0;
            PlayModeTrigger2DController.StayCalls = 0;
            PlayModeTrigger2DController.ExitCalls = 0;

            PlayModeRenderEventController.AnimatorEventCalls = 0;
            PlayModeRenderEventController.BecameInvisibleCalls = 0;
            PlayModeRenderEventController.ParticleSystemStoppedCalls = 0;
            PlayModeRenderEventController.DrawGizmosCalls = 0;
            PlayModeRenderEventController.DrawGizmosSelectedCalls = 0;
            PlayModeRenderEventController.LastAnimatorEventValue = null;

            PlayModeMouseEventController.MouseDownCalls = 0;
            PlayModeMouseEventController.MouseEnterCalls = 0;
            PlayModeMouseEventController.MouseExitCalls = 0;

            PlayModeUIEventController.BeginDragCalls = 0;
            PlayModeUIEventController.DragCalls = 0;
            PlayModeUIEventController.EndDragCalls = 0;
            PlayModeUIEventController.PointerDownCalls = 0;
            PlayModeUIEventController.PointerExitCalls = 0;
            PlayModeUIEventController.PointerUpCalls = 0;
            PlayModeUIEventController.PointerMoveCalls = 0;
            PlayModeUIEventController.PointerEnterCalls = 0;
            PlayModeUIEventController.SelectCalls = 0;
            PlayModeUIEventController.DeselectCalls = 0;
            PlayModeUIEventController.CallOrder = new List<string>();
            PlayModeUIEventController.LastBeginDragData = null;
            PlayModeUIEventController.LastDragData = null;
            PlayModeUIEventController.LastEndDragData = null;
            PlayModeUIEventController.LastPointerDownData = null;
            PlayModeUIEventController.LastPointerExitData = null;
            PlayModeUIEventController.LastPointerUpData = null;
            PlayModeUIEventController.LastPointerMoveData = null;
            PlayModeUIEventController.LastPointerEnterData = null;
            PlayModeUIEventController.LastSelectData = null;
            PlayModeUIEventController.LastDeselectData = null;
        }

        [UnityTest]
        public IEnumerator PlayModeInitializesAllControllersEligibleForCurrentExecutionMode()
        {
            var gameObject = new GameObject("playmode-execution");
            try
            {
                var view = gameObject.AddComponent<PlayModeExecutionModesView>();

                yield return null;

                Assert.That(view.TryGetController<PlayModePlayOnlyController>(out _), Is.True);
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
        public IEnumerator PlayModeControllerExecutionAttributeInitializesEligibleControllers()
        {
            var gameObject = new GameObject("playmode-execution-attribute");
            try
            {
                var view = gameObject.AddComponent<PlayModeExecutionAttributeModesView>();

                yield return null;

                Assert.That(view.TryGetController<PlayModeAttributePlayOnlyController>(out _), Is.True);
                Assert.That(view.TryGetController<PlayModeAttributeAlwaysController>(out _), Is.True);
                Assert.That(view.TryGetController<PlayModeAttributeEditorOnlyController>(out _), Is.False);
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
        public IEnumerator PlayModeTryGetControllerReturnsFalseWhenViewHasNoControllers()
        {
            var gameObject = new GameObject("playmode-no-controllers");
            var view = gameObject.AddComponent<PlayModeModelOnlyView>();

            var found = view.TryGetController<PlayModeLifecycleController>(out _);
            Assert.That(found, Is.False);

            var foundByType = view.TryGetController(typeof(PlayModeLifecycleController), out _);
            Assert.That(foundByType, Is.False);

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeTryGetControllerReturnsFalseWhenControllerTypeIsNull()
        {
            var gameObject = new GameObject("playmode-null-controller-type");
            var view = gameObject.AddComponent<PlayModeLifecycleView>();

            var found = view.TryGetController(null, out _);
            Assert.That(found, Is.False);

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeTryGetControllerExtensionsHandleNullInputs()
        {
            GameObject nullGameObject = null;
            Collider2D nullCollider2D = null;
            Collision2D nullCollision2D = null;

            Assert.That(GameObjectExtensions.TryGetController<PlayModeLifecycleController>(nullGameObject, out _), Is.False);
            Assert.That(GameObjectExtensions.TryGetController(nullGameObject, typeof(PlayModeLifecycleController), out _), Is.False);
            Assert.That(Collider2DExtensions.TryGetController<PlayModeLifecycleController>(nullCollider2D, out _), Is.False);
            Assert.That(Collider2DExtensions.TryGetController(nullCollider2D, typeof(PlayModeLifecycleController), out _), Is.False);
            Assert.That(Collision2DExtensions.TryGetController<PlayModeLifecycleController>(nullCollision2D, out _), Is.False);
            Assert.That(Collision2DExtensions.TryGetController(nullCollision2D, typeof(PlayModeLifecycleController), out _), Is.False);

            yield return null;
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

        [UnityTest]
        public IEnumerator PlayModeTriggerEnter3DIsInvoked()
        {
            CreateTrigger3DScene(
                "trigger3d-enter",
                out var triggerObject,
                out var movingObject,
                out _,
                out _);

            yield return WaitForFixedCondition(
                () => PlayModeTrigger3DController.EnterCalls > 0,
                120,
                "OnTriggerEnter (3D) was not triggered in time.");

            Assert.That(PlayModeTrigger3DController.EnterCalls, Is.GreaterThanOrEqualTo(1));

            if (movingObject != null)
            {
                Object.Destroy(movingObject);
            }

            if (triggerObject != null)
            {
                Object.Destroy(triggerObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeTriggerStay3DIsInvokedWhileOverlapping()
        {
            CreateTrigger3DScene(
                "trigger3d-stay",
                out var triggerObject,
                out var movingObject,
                out _,
                out _);

            yield return WaitForFixedCondition(
                () => PlayModeTrigger3DController.EnterCalls > 0,
                120,
                "OnTriggerEnter (3D) was not triggered in time.");

            yield return WaitForFixedCondition(
                () => PlayModeTrigger3DController.StayCalls > 0,
                120,
                "OnTriggerStay (3D) was not triggered in time.");

            Assert.That(PlayModeTrigger3DController.StayCalls, Is.GreaterThanOrEqualTo(1));

            if (movingObject != null)
            {
                Object.Destroy(movingObject);
            }

            if (triggerObject != null)
            {
                Object.Destroy(triggerObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeTriggerExit3DIsInvokedWhenPairIsRemoved()
        {
            CreateTrigger3DScene(
                "trigger3d-exit",
                out var triggerObject,
                out var movingObject,
                out var triggerCollider,
                out var movingCollider);

            yield return WaitForFixedCondition(
                () => PlayModeTrigger3DController.EnterCalls > 0,
                120,
                "OnTriggerEnter (3D) was not triggered in time.");

            yield return WaitForFixedCondition(
                () => PlayModeTrigger3DController.StayCalls > 0,
                120,
                "OnTriggerStay (3D) was not triggered in time.");

            Assert.That(triggerCollider, Is.Not.Null);
            Assert.That(movingCollider, Is.Not.Null);
            Physics.IgnoreCollision(movingCollider, triggerCollider, true);

            yield return WaitForFixedCondition(
                () => PlayModeTrigger3DController.ExitCalls > 0,
                120,
                "OnTriggerExit (3D) was not triggered in time.");

            Assert.That(PlayModeTrigger3DController.ExitCalls, Is.GreaterThanOrEqualTo(1));

            if (movingObject != null)
            {
                Object.Destroy(movingObject);
            }

            if (triggerObject != null)
            {
                Object.Destroy(triggerObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeCollisionEnter2DIsInvoked()
        {
            CreateCollision2DScene(
                "collision2d-enter",
                out var floorObject,
                out var movingObject,
                out _,
                out _);

            yield return WaitForFixedCondition(
                () => PlayModeCollision2DController.EnterCalls > 0,
                120,
                "OnCollisionEnter2D was not triggered in time.");

            Assert.That(PlayModeCollision2DController.EnterCalls, Is.GreaterThanOrEqualTo(1));

            if (movingObject != null)
            {
                Object.Destroy(movingObject);
            }

            if (floorObject != null)
            {
                Object.Destroy(floorObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeCollisionStay2DIsInvoked()
        {
            CreateCollision2DScene(
                "collision2d-stay",
                out var floorObject,
                out var movingObject,
                out _,
                out _);

            yield return WaitForFixedCondition(
                () => PlayModeCollision2DController.EnterCalls > 0,
                120,
                "OnCollisionEnter2D was not triggered in time.");

            yield return WaitForFixedCondition(
                () => PlayModeCollision2DController.StayCalls > 0,
                120,
                "OnCollisionStay2D was not triggered in time.");

            Assert.That(PlayModeCollision2DController.StayCalls, Is.GreaterThanOrEqualTo(1));

            if (movingObject != null)
            {
                Object.Destroy(movingObject);
            }

            if (floorObject != null)
            {
                Object.Destroy(floorObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeCollisionExit2DIsInvokedWhenPairIsRemoved()
        {
            CreateCollision2DScene(
                "collision2d-exit",
                out var floorObject,
                out var movingObject,
                out var floorCollider,
                out var movingCollider);

            yield return WaitForFixedCondition(
                () => PlayModeCollision2DController.EnterCalls > 0,
                120,
                "OnCollisionEnter2D was not triggered in time.");

            yield return WaitForFixedCondition(
                () => PlayModeCollision2DController.StayCalls > 0,
                120,
                "OnCollisionStay2D was not triggered in time.");

            Assert.That(floorCollider, Is.Not.Null);
            Assert.That(movingCollider, Is.Not.Null);
            Physics2D.IgnoreCollision(movingCollider, floorCollider, true);

            yield return WaitForFixedCondition(
                () => PlayModeCollision2DController.ExitCalls > 0,
                120,
                "OnCollisionExit2D was not triggered in time.");

            Assert.That(PlayModeCollision2DController.ExitCalls, Is.GreaterThanOrEqualTo(1));

            if (movingObject != null)
            {
                Object.Destroy(movingObject);
            }

            if (floorObject != null)
            {
                Object.Destroy(floorObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeTriggerEnter2DIsInvoked()
        {
            CreateTrigger2DScene(
                "trigger2d-enter",
                out var triggerObject,
                out var movingObject,
                out _,
                out _);

            yield return WaitForFixedCondition(
                () => PlayModeTrigger2DController.EnterCalls > 0,
                120,
                "OnTriggerEnter2D was not triggered in time.");

            Assert.That(PlayModeTrigger2DController.EnterCalls, Is.GreaterThanOrEqualTo(1));

            if (movingObject != null)
            {
                Object.Destroy(movingObject);
            }

            if (triggerObject != null)
            {
                Object.Destroy(triggerObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeTriggerStay2DIsInvoked()
        {
            CreateTrigger2DScene(
                "trigger2d-stay",
                out var triggerObject,
                out var movingObject,
                out _,
                out _);

            yield return WaitForFixedCondition(
                () => PlayModeTrigger2DController.EnterCalls > 0,
                120,
                "OnTriggerEnter2D was not triggered in time.");

            yield return WaitForFixedCondition(
                () => PlayModeTrigger2DController.StayCalls > 0,
                120,
                "OnTriggerStay2D was not triggered in time.");

            Assert.That(PlayModeTrigger2DController.StayCalls, Is.GreaterThanOrEqualTo(1));

            if (movingObject != null)
            {
                Object.Destroy(movingObject);
            }

            if (triggerObject != null)
            {
                Object.Destroy(triggerObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeTriggerExit2DIsInvokedWhenPairIsRemoved()
        {
            CreateTrigger2DScene(
                "trigger2d-exit",
                out var triggerObject,
                out var movingObject,
                out var triggerCollider,
                out var movingCollider);

            yield return WaitForFixedCondition(
                () => PlayModeTrigger2DController.EnterCalls > 0,
                120,
                "OnTriggerEnter2D was not triggered in time.");

            yield return WaitForFixedCondition(
                () => PlayModeTrigger2DController.StayCalls > 0,
                120,
                "OnTriggerStay2D was not triggered in time.");

            Assert.That(triggerCollider, Is.Not.Null);
            Assert.That(movingCollider, Is.Not.Null);
            Physics2D.IgnoreCollision(movingCollider, triggerCollider, true);

            yield return WaitForFixedCondition(
                () => PlayModeTrigger2DController.ExitCalls > 0,
                120,
                "OnTriggerExit2D was not triggered in time.");

            Assert.That(PlayModeTrigger2DController.ExitCalls, Is.GreaterThanOrEqualTo(1));

            if (movingObject != null)
            {
                Object.Destroy(movingObject);
            }

            if (triggerObject != null)
            {
                Object.Destroy(triggerObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnAnimatorEventIsForwardedToController()
        {
            var gameObject = new GameObject("playmode-render-onanimatorevent");
            var view = gameObject.AddComponent<PlayModeRenderEventView>();
            yield return null;

            view.SendMessage("OnAnimatorEvent", "anim-event-value", SendMessageOptions.RequireReceiver);

            Assert.That(PlayModeRenderEventController.AnimatorEventCalls, Is.EqualTo(1));
            Assert.That(PlayModeRenderEventController.LastAnimatorEventValue, Is.EqualTo("anim-event-value"));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnBecameInvisibleIsForwardedToController()
        {
            var gameObject = new GameObject("playmode-render-onbecameinvisible");
            var view = gameObject.AddComponent<PlayModeRenderEventView>();
            yield return null;

            view.SendMessage("OnBecameInvisible", SendMessageOptions.RequireReceiver);

            Assert.That(PlayModeRenderEventController.BecameInvisibleCalls, Is.EqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnParticleSystemStoppedIsForwardedToController()
        {
            var gameObject = new GameObject("playmode-render-onparticlestopped");
            var view = gameObject.AddComponent<PlayModeRenderEventView>();
            yield return null;

            view.SendMessage("OnParticleSystemStopped", SendMessageOptions.RequireReceiver);

            Assert.That(PlayModeRenderEventController.ParticleSystemStoppedCalls, Is.EqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnDrawGizmosIsForwardedToController()
        {
            var gameObject = new GameObject("playmode-render-ondrawgizmos");
            var view = gameObject.AddComponent<PlayModeRenderEventView>();
            yield return null;

            view.SendMessage("OnDrawGizmos", SendMessageOptions.RequireReceiver);

            Assert.That(PlayModeRenderEventController.DrawGizmosCalls, Is.EqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnDrawGizmosSelectedIsForwardedToController()
        {
            var gameObject = new GameObject("playmode-render-ondrawgizmosselected");
            var view = gameObject.AddComponent<PlayModeRenderEventView>();
            yield return null;

            view.SendMessage("OnDrawGizmosSelected", SendMessageOptions.RequireReceiver);

            Assert.That(PlayModeRenderEventController.DrawGizmosSelectedCalls, Is.EqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnMouseDownIsForwardedToController()
        {
            var gameObject = new GameObject("playmode-mouse-onmousedown");
            var view = gameObject.AddComponent<PlayModeMouseEventView>();
            yield return null;

            view.SendMessage("OnMouseDown", SendMessageOptions.RequireReceiver);

            Assert.That(PlayModeMouseEventController.MouseDownCalls, Is.EqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnMouseEnterIsForwardedToController()
        {
            var gameObject = new GameObject("playmode-mouse-onmouseenter");
            var view = gameObject.AddComponent<PlayModeMouseEventView>();
            yield return null;

            view.SendMessage("OnMouseEnter", SendMessageOptions.RequireReceiver);

            Assert.That(PlayModeMouseEventController.MouseEnterCalls, Is.EqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnMouseExitIsForwardedToController()
        {
            var gameObject = new GameObject("playmode-mouse-onmouseexit");
            var view = gameObject.AddComponent<PlayModeMouseEventView>();
            yield return null;

            view.SendMessage("OnMouseExit", SendMessageOptions.RequireReceiver);

            Assert.That(PlayModeMouseEventController.MouseExitCalls, Is.EqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnBeginDragIsForwardedToController()
        {
            CreateUiEventScene(
                "playmode-ui-onbegindrag",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var pointerEventData = new PointerEventData(eventSystem);
            view.OnBeginDrag(pointerEventData);

            Assert.That(PlayModeUIEventController.BeginDragCalls, Is.EqualTo(1));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnDragIsForwardedToController()
        {
            CreateUiEventScene(
                "playmode-ui-ondrag",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var pointerEventData = new PointerEventData(eventSystem);
            view.OnDrag(pointerEventData);

            Assert.That(PlayModeUIEventController.DragCalls, Is.EqualTo(1));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnEndDragIsForwardedToController()
        {
            CreateUiEventScene(
                "playmode-ui-onenddrag",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var pointerEventData = new PointerEventData(eventSystem);
            view.OnEndDrag(pointerEventData);

            Assert.That(PlayModeUIEventController.EndDragCalls, Is.EqualTo(1));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnPointerDownIsForwardedToController()
        {
            CreateUiEventScene(
                "playmode-ui-onpointerdown",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var pointerEventData = new PointerEventData(eventSystem);
            view.OnPointerDown(pointerEventData);

            Assert.That(PlayModeUIEventController.PointerDownCalls, Is.EqualTo(1));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnPointerExitIsForwardedToController()
        {
            CreateUiEventScene(
                "playmode-ui-onpointerexit",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var pointerEventData = new PointerEventData(eventSystem);
            view.OnPointerExit(pointerEventData);

            Assert.That(PlayModeUIEventController.PointerExitCalls, Is.EqualTo(1));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnPointerUpIsForwardedToController()
        {
            CreateUiEventScene(
                "playmode-ui-onpointerup",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var pointerEventData = new PointerEventData(eventSystem);
            view.OnPointerUp(pointerEventData);

            Assert.That(PlayModeUIEventController.PointerUpCalls, Is.EqualTo(1));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnPointerMoveIsForwardedToController()
        {
            CreateUiEventScene(
                "playmode-ui-onpointermove",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var pointerEventData = new PointerEventData(eventSystem);
            view.OnPointerMove(pointerEventData);

            Assert.That(PlayModeUIEventController.PointerMoveCalls, Is.EqualTo(1));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnPointerEnterIsForwardedToController()
        {
            CreateUiEventScene(
                "playmode-ui-onpointerenter",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var pointerEventData = new PointerEventData(eventSystem);
            view.OnPointerEnter(pointerEventData);

            Assert.That(PlayModeUIEventController.PointerEnterCalls, Is.EqualTo(1));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnSelectIsForwardedToController()
        {
            CreateUiEventScene(
                "playmode-ui-onselect",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var eventData = new BaseEventData(eventSystem);
            view.OnSelect(eventData);

            Assert.That(PlayModeUIEventController.SelectCalls, Is.EqualTo(1));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeOnDeselectIsForwardedToController()
        {
            CreateUiEventScene(
                "playmode-ui-ondeselect",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var eventData = new BaseEventData(eventSystem);
            view.OnDeselect(eventData);

            Assert.That(PlayModeUIEventController.DeselectCalls, Is.EqualTo(1));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeUiDragFlowFollowsExpectedOrder()
        {
            CreateUiEventScene(
                "playmode-ui-order",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var pointerEventData = new PointerEventData(eventSystem)
            {
                pointerId = 42,
                position = new Vector2(111f, 222f)
            };

            view.OnPointerDown(pointerEventData);
            view.OnBeginDrag(pointerEventData);
            view.OnDrag(pointerEventData);
            view.OnEndDrag(pointerEventData);
            view.OnPointerUp(pointerEventData);

            CollectionAssert.AreEqual(
                new[]
                {
                    "OnPointerDown",
                    "OnBeginDrag",
                    "OnDrag",
                    "OnEndDrag",
                    "OnPointerUp"
                },
                PlayModeUIEventController.CallOrder);

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayModeUiPayloadForwardsOriginalEventDataInstances()
        {
            CreateUiEventScene(
                "playmode-ui-payload",
                out var eventSystemObject,
                out var eventSystem,
                out var viewObject,
                out var view);

            var pointerEventData = new PointerEventData(eventSystem)
            {
                pointerId = 7,
                position = new Vector2(300f, 120f)
            };
            var baseEventData = new BaseEventData(eventSystem);

            view.OnPointerEnter(pointerEventData);
            view.OnPointerMove(pointerEventData);
            view.OnPointerExit(pointerEventData);
            view.OnSelect(baseEventData);
            view.OnDeselect(baseEventData);

            Assert.That(PlayModeUIEventController.LastPointerEnterData, Is.SameAs(pointerEventData));
            Assert.That(PlayModeUIEventController.LastPointerMoveData, Is.SameAs(pointerEventData));
            Assert.That(PlayModeUIEventController.LastPointerExitData, Is.SameAs(pointerEventData));
            Assert.That(PlayModeUIEventController.LastSelectData, Is.SameAs(baseEventData));
            Assert.That(PlayModeUIEventController.LastDeselectData, Is.SameAs(baseEventData));
            Assert.That(PlayModeUIEventController.LastPointerMoveData.pointerId, Is.EqualTo(7));
            Assert.That(PlayModeUIEventController.LastPointerMoveData.position, Is.EqualTo(new Vector2(300f, 120f)));

            Object.Destroy(viewObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

#if ENABLE_PARTICLE_SYSTEM
        [UnityTest]
        public IEnumerator PlayModeOnParticleSystemStoppedIsRaisedByUnityParticleCallback()
        {
            var gameObject = new GameObject("playmode-render-particle-real");
            gameObject.AddComponent<PlayModeRenderEventView>();
            var particleSystem = gameObject.AddComponent<ParticleSystem>();

            yield return null;

            var main = particleSystem.main;
            main.loop = false;
            main.duration = 0.05f;
            main.startLifetime = 0.05f;
            main.startSpeed = 0f;
            main.stopAction = ParticleSystemStopAction.Callback;

            var emission = particleSystem.emission;
            emission.rateOverTime = 20f;

            particleSystem.Play();

            yield return WaitForCondition(
                () => PlayModeRenderEventController.ParticleSystemStoppedCalls > 0,
                240,
                "OnParticleSystemStopped was not raised by Unity callback in time.");

            Assert.That(PlayModeRenderEventController.ParticleSystemStoppedCalls, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(gameObject);
            yield return null;
        }
#endif

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

        public class PlayModeExecutionAttributeModesView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeModel model;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecution(GameFieldAttributes.ControllerExecutionMode.PlayOnly)] private PlayModeAttributePlayOnlyController playOnlyController;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecution(GameFieldAttributes.ControllerExecutionMode.EditorOnly)] private PlayModeAttributeEditorOnlyController editorOnlyController;
            [GameFieldAttributes.ControllerField, GameFieldAttributes.ControllerExecution(GameFieldAttributes.ControllerExecutionMode.Always)] private PlayModeAttributeAlwaysController alwaysController;
        }

        public class PlayModeModelOnlyView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeModel model;
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

        public class PlayModeAttributePlayOnlyController : GameController<PlayModeExecutionAttributeModesView>
        {
            public PlayModeAttributePlayOnlyController()
            {
            }
        }

        public class PlayModeAttributeEditorOnlyController : GameController<PlayModeExecutionAttributeModesView>
        {
            public PlayModeAttributeEditorOnlyController()
            {
            }
        }

        public class PlayModeAttributeAlwaysController : GameController<PlayModeExecutionAttributeModesView>
        {
            public PlayModeAttributeAlwaysController()
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

        public class PlayModeTrigger3DView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeTrigger3DModel model;
            [GameFieldAttributes.ControllerField] private PlayModeTrigger3DController triggerController;
        }

        public class PlayModeCollision2DView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeCollision2DModel model;
            [GameFieldAttributes.ControllerField] private PlayModeCollision2DController collisionController;
        }

        public class PlayModeTrigger2DView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeTrigger2DModel model;
            [GameFieldAttributes.ControllerField] private PlayModeTrigger2DController triggerController;
        }

        public class PlayModeRenderEventView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeRenderEventModel model;
            [GameFieldAttributes.ControllerField] private PlayModeRenderEventController renderEventController;
        }

        public class PlayModeMouseEventView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeMouseEventModel model;
            [GameFieldAttributes.ControllerField] private PlayModeMouseEventController mouseEventController;
        }

        public class PlayModeUIEventView : GameViewUI
        {
            [SerializeField, GameFieldAttributes.ModelField] private PlayModeUIEventModel model;
            [GameFieldAttributes.ControllerField] private PlayModeUIEventController uiEventController;
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

        [System.Serializable]
        public class PlayModeTrigger3DModel : GameModel
        {
        }

        [System.Serializable]
        public class PlayModeCollision2DModel : GameModel
        {
        }

        [System.Serializable]
        public class PlayModeTrigger2DModel : GameModel
        {
        }

        [System.Serializable]
        public class PlayModeRenderEventModel : GameModel
        {
        }

        [System.Serializable]
        public class PlayModeMouseEventModel : GameModel
        {
        }

        [System.Serializable]
        public class PlayModeUIEventModel : GameModel
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

        public class PlayModeTrigger3DController : GameController<PlayModeTrigger3DView, PlayModeTrigger3DModel>
        {
            public static int EnterCalls;
            public static int StayCalls;
            public static int ExitCalls;

            private void OnTriggerEnter(Collider other)
            {
                EnterCalls++;
            }

            private void OnTriggerStay(Collider other)
            {
                StayCalls++;
            }

            private void OnTriggerExit(Collider other)
            {
                ExitCalls++;
            }
        }

        public class PlayModeCollision2DController : GameController<PlayModeCollision2DView, PlayModeCollision2DModel>
        {
            public static int EnterCalls;
            public static int StayCalls;
            public static int ExitCalls;

            private void OnCollisionEnter2D(Collision2D other)
            {
                EnterCalls++;
            }

            private void OnCollisionStay2D(Collision2D other)
            {
                StayCalls++;
            }

            private void OnCollisionExit2D(Collision2D other)
            {
                ExitCalls++;
            }
        }

        public class PlayModeTrigger2DController : GameController<PlayModeTrigger2DView, PlayModeTrigger2DModel>
        {
            public static int EnterCalls;
            public static int StayCalls;
            public static int ExitCalls;

            private void OnTriggerEnter2D(Collider2D other)
            {
                EnterCalls++;
            }

            private void OnTriggerStay2D(Collider2D other)
            {
                StayCalls++;
            }

            private void OnTriggerExit2D(Collider2D other)
            {
                ExitCalls++;
            }
        }

        public class PlayModeRenderEventController : GameController<PlayModeRenderEventView, PlayModeRenderEventModel>
        {
            public static int AnimatorEventCalls;
            public static int BecameInvisibleCalls;
            public static int ParticleSystemStoppedCalls;
            public static int DrawGizmosCalls;
            public static int DrawGizmosSelectedCalls;
            public static string LastAnimatorEventValue;

            private void OnAnimatorEvent(string value)
            {
                AnimatorEventCalls++;
                LastAnimatorEventValue = value;
            }

            private void OnBecameInvisible()
            {
                BecameInvisibleCalls++;
            }

            private void OnParticleSystemStopped()
            {
                ParticleSystemStoppedCalls++;
            }

            private void OnDrawGizmos()
            {
                DrawGizmosCalls++;
            }

            private void OnDrawGizmosSelected()
            {
                DrawGizmosSelectedCalls++;
            }
        }

        public class PlayModeMouseEventController : GameController<PlayModeMouseEventView, PlayModeMouseEventModel>
        {
            public static int MouseDownCalls;
            public static int MouseEnterCalls;
            public static int MouseExitCalls;

            private void OnMouseDown()
            {
                MouseDownCalls++;
            }

            private void OnMouseEnter()
            {
                MouseEnterCalls++;
            }

            private void OnMouseExit()
            {
                MouseExitCalls++;
            }
        }

        public class PlayModeUIEventController : GameController<PlayModeUIEventView, PlayModeUIEventModel>
        {
            public static int BeginDragCalls;
            public static int DragCalls;
            public static int EndDragCalls;
            public static int PointerDownCalls;
            public static int PointerExitCalls;
            public static int PointerUpCalls;
            public static int PointerMoveCalls;
            public static int PointerEnterCalls;
            public static int SelectCalls;
            public static int DeselectCalls;
            public static List<string> CallOrder;
            public static PointerEventData LastBeginDragData;
            public static PointerEventData LastDragData;
            public static PointerEventData LastEndDragData;
            public static PointerEventData LastPointerDownData;
            public static PointerEventData LastPointerExitData;
            public static PointerEventData LastPointerUpData;
            public static PointerEventData LastPointerMoveData;
            public static PointerEventData LastPointerEnterData;
            public static BaseEventData LastSelectData;
            public static BaseEventData LastDeselectData;

            private void OnBeginDrag(PointerEventData other)
            {
                BeginDragCalls++;
                LastBeginDragData = other;
                CallOrder?.Add("OnBeginDrag");
            }

            private void OnDrag(PointerEventData other)
            {
                DragCalls++;
                LastDragData = other;
                CallOrder?.Add("OnDrag");
            }

            private void OnEndDrag(PointerEventData other)
            {
                EndDragCalls++;
                LastEndDragData = other;
                CallOrder?.Add("OnEndDrag");
            }

            private void OnPointerDown(PointerEventData other)
            {
                PointerDownCalls++;
                LastPointerDownData = other;
                CallOrder?.Add("OnPointerDown");
            }

            private void OnPointerExit(PointerEventData other)
            {
                PointerExitCalls++;
                LastPointerExitData = other;
                CallOrder?.Add("OnPointerExit");
            }

            private void OnPointerUp(PointerEventData other)
            {
                PointerUpCalls++;
                LastPointerUpData = other;
                CallOrder?.Add("OnPointerUp");
            }

            private void OnPointerMove(PointerEventData other)
            {
                PointerMoveCalls++;
                LastPointerMoveData = other;
                CallOrder?.Add("OnPointerMove");
            }

            private void OnPointerEnter(PointerEventData other)
            {
                PointerEnterCalls++;
                LastPointerEnterData = other;
                CallOrder?.Add("OnPointerEnter");
            }

            private void OnSelect(BaseEventData eventData)
            {
                SelectCalls++;
                LastSelectData = eventData;
                CallOrder?.Add("OnSelect");
            }

            private void OnDeselect(BaseEventData eventData)
            {
                DeselectCalls++;
                LastDeselectData = eventData;
                CallOrder?.Add("OnDeselect");
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

        private static void CreateTrigger3DScene(
            string testName,
            out GameObject triggerObject,
            out GameObject movingObject,
            out Collider triggerCollider,
            out Collider movingCollider)
        {
            triggerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            triggerObject.name = $"{testName}-trigger";
            triggerObject.transform.position = Vector3.zero;
            triggerObject.transform.localScale = new Vector3(6f, 1f, 6f);
            triggerCollider = triggerObject.GetComponent<Collider>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }

            movingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            movingObject.name = $"{testName}-view";
            movingObject.transform.position = new Vector3(0f, 2f, 0f);
            movingCollider = movingObject.GetComponent<Collider>();
            movingObject.AddComponent<Rigidbody>();
            movingObject.AddComponent<PlayModeTrigger3DView>();
        }

        private static void CreateCollision2DScene(
            string testName,
            out GameObject floorObject,
            out GameObject movingObject,
            out Collider2D floorCollider,
            out Collider2D movingCollider)
        {
            floorObject = new GameObject($"{testName}-floor");
            floorObject.transform.position = Vector3.zero;
            floorCollider = floorObject.AddComponent<BoxCollider2D>();
            floorCollider.offset = new Vector2(0f, -0.5f);
            floorCollider.transform.localScale = new Vector3(6f, 1f, 1f);

            movingObject = new GameObject($"{testName}-view");
            movingObject.transform.position = new Vector3(0f, 1.5f, 0f);
            movingCollider = movingObject.AddComponent<BoxCollider2D>();
            var rigidbody = movingObject.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 4f;
            movingObject.AddComponent<PlayModeCollision2DView>();
        }

        private static void CreateTrigger2DScene(
            string testName,
            out GameObject triggerObject,
            out GameObject movingObject,
            out Collider2D triggerCollider,
            out Collider2D movingCollider)
        {
            triggerObject = new GameObject($"{testName}-trigger");
            triggerObject.transform.position = Vector3.zero;
            triggerCollider = triggerObject.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.offset = new Vector2(0f, -0.5f);
            triggerCollider.transform.localScale = new Vector3(6f, 1f, 1f);

            movingObject = new GameObject($"{testName}-view");
            movingObject.transform.position = new Vector3(0f, 1.5f, 0f);
            movingCollider = movingObject.AddComponent<BoxCollider2D>();
            var rigidbody = movingObject.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 4f;
            movingObject.AddComponent<PlayModeTrigger2DView>();
        }

        private static void CreateUiEventScene(
            string testName,
            out GameObject eventSystemObject,
            out EventSystem eventSystem,
            out GameObject viewObject,
            out PlayModeUIEventView view)
        {
            eventSystemObject = new GameObject($"{testName}-eventsystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();

            viewObject = new GameObject($"{testName}-view");
            view = viewObject.AddComponent<PlayModeUIEventView>();
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
