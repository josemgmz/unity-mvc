using System;
using System.Collections.Generic;
using System.Reflection;
using UnityMVC;
using UnityEditor;
using UnityEngine;

namespace UnityMVC.Editor
{
    /// <summary>
    /// Custom inspector to expose useful tooling for GameView/Controllers/Models while in Editor.
    /// </summary>
    [CustomEditor(typeof(GameView), true)]
    public class GameViewEditor : UnityEditor.Editor
    {
        private GameView _view;
        private Dictionary<Type, object> _controllersCache;
        private Dictionary<Type, object> _modelsCache;
        private bool _controllersFoldout = true;
        private bool _modelsFoldout = false;
        private bool _actionsFoldout = true;
        private List<FieldInfo> _controllerFieldsFallback = new List<FieldInfo>();

        private const BindingFlags BindingFlagsAll = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private void OnEnable()
        {
            _view = (GameView)target;
            EnsureInitialized();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            EnsureInitialized();

            DrawLifecycleUtilities();
            DrawControllersList();
            DrawControllerActions();
            DrawModelsList();
        }

        private void EnsureInitialized()
        {
            if (_view == null) return;
            try
            {
                // Force initialization in editor so that controllers/models are discoverable
                InvokeIfExists("Awake");

                // If controllers are still missing, invoke the private init methods directly
                _controllersCache = GetBackingDictionary<object>("_rawControllers");
                _modelsCache = GetBackingDictionary<object>("_rawModels");

                if (_controllersCache == null || _controllersCache.Count == 0)
                {
                    InvokeIfExists("InitializeModelAttributes", true);
                    InvokeIfExists("InitializeControllerAttributes", true);
                    _controllersCache = GetBackingDictionary<object>("_rawControllers");
                    _modelsCache = GetBackingDictionary<object>("_rawModels");
                }

                BuildControllerFieldFallback();
            }
            catch
            {
                // Swallow exceptions to avoid breaking inspector; user will see missing data instead
            }
        }

        private void DrawLifecycleUtilities()
        {
            EditorGUILayout.LabelField("MVC Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Rebuild Controllers/Models"))
            {
                RebuildMVC();
            }
            EditorGUILayout.Space();
        }

        private void DrawControllerActions()
        {
            if (!HasControllerActions()) return;

            _actionsFoldout = EditorGUILayout.Foldout(_actionsFoldout, "Controller Actions");
            if (!_actionsFoldout) return;

            foreach (var kvp in _controllersCache)
            {
                var controllerType = kvp.Key;
                var controller = kvp.Value;
                if (controller == null) continue;

                var contextMenuMethods = GetContextMenuMethods(controllerType);
                if (contextMenuMethods.Count == 0) continue;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(controllerType.Name, EditorStyles.boldLabel);

                foreach (var method in contextMenuMethods)
                {
                    var label = $"[Context] {method.Name}";
                    if (GUILayout.Button(label))
                    {
                        method.Invoke(controller, null);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
        }

        private void DrawControllersList()
        {
            _controllersFoldout = EditorGUILayout.Foldout(_controllersFoldout, "Controllers");
            if (!_controllersFoldout) return;

            var controllerTypes = GetAllControllerTypes();
            if (controllerTypes.Count == 0)
            {
                EditorGUILayout.HelpBox("No controllers declared.", MessageType.Info);
                return;
            }

            foreach (var type in controllerTypes)
            {
                var isActive = IsControllerActive(type);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(type.Name, EditorStyles.boldLabel);
                DrawStatusDot(
                    isActive,
                    isActive
                        ? "Active"
                        : EditorApplication.isPlaying
                            ? "Waiting (Editor mode)"
                            : "Waiting (Play mode)",
                    EditorApplication.isPlaying);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
        }

        private void DrawModelsList()
        {
            _modelsFoldout = EditorGUILayout.Foldout(_modelsFoldout, "Models (readonly clone)");
            if (!_modelsFoldout) return;

            if (_modelsCache == null || _modelsCache.Count == 0)
            {
                EditorGUILayout.HelpBox("No models found.", MessageType.Info);
                return;
            }

            foreach (var kvp in _modelsCache)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(kvp.Key.Name, EditorStyles.boldLabel);

                var model = kvp.Value as GameModel;
                if (model == null)
                {
                    EditorGUILayout.LabelField("Null model instance");
                }
                else
                {
                    DrawModelFields(model);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawModelFields(GameModel model)
        {
            var fields = model.GetType().GetFields(BindingFlagsAll);
            EditorGUI.BeginDisabledGroup(true);
            foreach (var field in fields)
            {
                var value = field.GetValue(model);
                EditorGUILayout.LabelField(field.Name, value != null ? value.ToString() : "null");
            }
            EditorGUI.EndDisabledGroup();
        }

        private Dictionary<Type, T> GetBackingDictionary<T>(string fieldName)
        {
            var field = typeof(GameView).GetField(fieldName, BindingFlagsAll);
            if (field == null)
            {
                return null;
            }

            return field.GetValue(_view) as Dictionary<Type, T>;
        }

        private void InvokeIfExists(string methodName, params object[] args)
        {
            var method = typeof(GameView).GetMethod(methodName, BindingFlagsAll);
            method?.Invoke(_view, args);
        }

        private void BuildControllerFieldFallback()
        {
            _controllerFieldsFallback.Clear();
            var current = _view.GetType();
            while (current != null)
            {
                var fields = current.GetFields(BindingFlagsAll);
                foreach (var field in fields)
                {
                    var hasAttribute = field.GetCustomAttribute<GameFieldAttributes.ControllerFieldAttribute>() != null;
                    if (hasAttribute)
                    {
                        _controllerFieldsFallback.Add(field);
                    }
                }
                current = current.BaseType;
            }
        }

        private bool HasControllerActions()
        {
            if (_controllersCache == null || _controllersCache.Count == 0)
            {
                return false;
            }

            foreach (var kvp in _controllersCache)
            {
                if (GetContextMenuMethods(kvp.Key).Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private List<MethodInfo> GetContextMenuMethods(Type controllerType)
        {
            var result = new List<MethodInfo>();
            var methods = controllerType.GetMethods(BindingFlagsAll);
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<ContextMenu>() != null && method.GetParameters().Length == 0)
                {
                    result.Add(method);
                }
            }

            return result;
        }

        private bool IsControllerActive(Type controllerType)
        {
            if (_view == null) return false;
            var hasInstance = _controllersCache != null && _controllersCache.ContainsKey(controllerType);
            return hasInstance && _view.isActiveAndEnabled && _view.gameObject.activeInHierarchy;
        }

        private void DrawStatusDot(bool isActive, string label, bool isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            var rect = GUILayoutUtility.GetRect(16, 16, GUILayout.Width(18), GUILayout.Height(18));
            var center = new Vector3(rect.x + rect.width / 2f, rect.y + rect.height / 2f, 0f);
            var radius = Mathf.Min(rect.width, rect.height) / 2.5f;
            var color = isActive ? Color.green : Color.red;

            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawSolidDisc(center, Vector3.forward, radius);
            Handles.color = Color.white;
            Handles.EndGUI();

            var labelContent = new GUIContent(label);
            EditorGUILayout.LabelField(labelContent, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
        }

        private HashSet<Type> GetAllControllerTypes()
        {
            var types = new HashSet<Type>();

            if (_controllerFieldsFallback.Count > 0)
            {
                foreach (var field in _controllerFieldsFallback)
                {
                    types.Add(field.FieldType);
                }
            }

            if (_controllersCache != null)
            {
                foreach (var kvp in _controllersCache)
                {
                    types.Add(kvp.Key);
                }
            }

            return types;
        }

        private void RebuildMVC()
        {
            _controllersCache?.Clear();
            _modelsCache?.Clear();

            // Recreate models and controllers
            var initModels = typeof(GameView).GetMethod("InitializeModelAttributes", BindingFlagsAll);
            var initControllers = typeof(GameView).GetMethod("InitializeControllerAttributes", BindingFlagsAll);

            initModels?.Invoke(_view, new object[] { true });
            initControllers?.Invoke(_view, new object[] { true });

            EnsureInitialized();
        }
    }
}
