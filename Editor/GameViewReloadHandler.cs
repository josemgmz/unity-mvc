using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.SceneManagement;

namespace UnityMVC.Editor
{
    /// <summary>
    /// Rebuilds GameView controllers/models after a domain reload so editor-only controllers (e.g., gizmos) keep working.
    /// </summary>
    [InitializeOnLoad]
    public static class GameViewReloadHandler
    {
        static GameViewReloadHandler()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        [DidReloadScripts]
        private static void OnAfterAssemblyReload()
        {
            if (UnityEngine.Application.isPlaying)
            {
                return;
            }

            EditorApplication.delayCall += ReinitializeAllGameViews;
        }

        private static void ReinitializeAllGameViews()
        {
            if (UnityEngine.Application.isPlaying)
            {
                return;
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    var views = root.GetComponentsInChildren<GameView>(true);
                    if (views == null || views.Length == 0) continue;

                    for (var index = 0; index < views.Length; index++)
                    {
                        var view = views[index];
                        if (view == null) continue;
                        view.EditorReinitialize();
                    }
                }
            }
        }
    }
}
