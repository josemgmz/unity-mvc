using System;

namespace UnityMVC
{
    /// <summary>
    /// Contains custom attributes for game fields.
    /// </summary>
    public class GameFieldAttributes
    {
        /// <summary>
        /// Attribute to mark a field as a controller field.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class ControllerFieldAttribute : System.Attribute {}

        /// <summary>
        /// Determines when a controller should be initialized.
        /// </summary>
        public enum ControllerExecutionMode
        {
            /// <summary>Initialize only while the game is playing.</summary>
            PlayOnly,
            /// <summary>Initialize only while in the editor outside Play Mode.</summary>
            EditorOnly,
            /// <summary>Initialize both in Play Mode and while editing.</summary>
            Always
        }

        /// <summary>
        /// Attribute to configure when a controller should run.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class ControllerExecutionAttribute : System.Attribute
        {
            public ControllerExecutionMode Mode { get; }

            public ControllerExecutionAttribute(ControllerExecutionMode mode)
            {
                Mode = mode;
            }
        }

        /// <summary>
        /// Marks a controller to run in both Play Mode and Edit Mode.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class ControllerExecuteAlwaysAttribute : ControllerExecutionAttribute
        {
            public ControllerExecuteAlwaysAttribute() : base(ControllerExecutionMode.Always) {}
        }

        /// <summary>
        /// Marks a controller to run only in Edit Mode (not during Play).
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class ControllerEditorOnlyAttribute : ControllerExecutionAttribute
        {
            public ControllerEditorOnlyAttribute() : base(ControllerExecutionMode.EditorOnly) {}
        }

        /// <summary>
        /// Attribute to mark a field as a model field.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class ModelFieldAttribute : System.Attribute {}

        /// <summary>
        /// Attribute to indicate that controller fields should be processed in reverse order.
        /// If at least one controller field has this attribute, all fields will be reversed before processing.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class ControllerReverseOrderAttribute : System.Attribute {}
    }
}
