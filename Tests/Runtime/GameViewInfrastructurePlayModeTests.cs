using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityMVC.Tests.Runtime
{
    public class GameViewInfrastructurePlayModeTests
    {
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
    }
}
