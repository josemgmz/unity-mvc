using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityMVC.Tests.Runtime
{
    public class GameControllerChildModelPlayModeTests
    {
        [UnityTest]
        public IEnumerator GetChildModels_IgnoresChildrenWithoutMatchingModel()
        {
            var rootObject = new GameObject("child-model-root");
            var rootView = rootObject.AddComponent<ChildModelRootView>();

            var childWithModel = new GameObject("child-with-model");
            childWithModel.transform.SetParent(rootObject.transform);
            childWithModel.AddComponent<ChildModelView>();

            var childWithoutView = new GameObject("child-without-view");
            childWithoutView.transform.SetParent(rootObject.transform);

            var childViewWithoutTargetModel = new GameObject("child-view-without-target-model");
            childViewWithoutTargetModel.transform.SetParent(rootObject.transform);
            childViewWithoutTargetModel.AddComponent<ChildModelEmptyView>();

            yield return null;

            var controller = rootView.GetController<ChildModelRootController>();
            List<ChildModel> childModels = null;
            Assert.DoesNotThrow(() => childModels = controller.GetChildModelClones());

            Assert.That(childModels, Is.Not.Null);
            Assert.That(childModels.Count, Is.EqualTo(1));
            Assert.That(childModels[0].Value, Is.EqualTo(7));

            if (rootObject != null)
            {
                UnityEngine.Object.Destroy(rootObject);
            }

            yield return null;
        }

        [Serializable]
        private class ChildModel : GameModel
        {
            public int Value = 7;
        }

        private class ChildModelView : GameView
        {
            [SerializeField, GameFieldAttributes.ModelField]
            private ChildModel model;
        }

        private class ChildModelEmptyView : GameView
        {
        }

        private class ChildModelRootView : GameView
        {
            [GameFieldAttributes.ControllerField]
            private ChildModelRootController controller;
        }

        private class ChildModelRootController : GameController<ChildModelRootView>
        {
            public List<ChildModel> GetChildModelClones()
            {
                return GetChildModels<ChildModel>();
            }
        }
    }
}
