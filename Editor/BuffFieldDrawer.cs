using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Demegraunt.Framework {
    /// <summary>
    /// Draws <see cref="BuffField{T}"/> as a single field of its original value, without a foldout.<br/><br/>
    /// Unity versions before 2023.2 don't apply drawers of generic type definitions to derived classes.
    /// For your own non-generic buff-field classes, inherit from this drawer:
    /// <code>[CustomPropertyDrawer(typeof(MyBuffField))] public sealed class MyBuffFieldDrawer : BuffFieldDrawer { }</code>
    /// </summary>
    [CustomPropertyDrawer(typeof(BuffField<>), true)]
    [CustomPropertyDrawer(typeof(FloatBuffField))]
    [CustomPropertyDrawer(typeof(IntBuffField))]
    public class BuffFieldDrawer : PropertyDrawer {
        private const string OriginalValuePath = "<OriginalValue>k__BackingField";

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var originalValue = property.FindPropertyRelative(OriginalValuePath);
            return originalValue != null ? new PropertyField(originalValue, property.displayName) : null;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var originalValue = property.FindPropertyRelative(OriginalValuePath);
            return originalValue != null
                ? EditorGUI.GetPropertyHeight(originalValue, label, true)
                : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var originalValue = property.FindPropertyRelative(OriginalValuePath);
            if (originalValue == null) {
                EditorGUI.LabelField(position, label, new GUIContent("Original value is not serializable."));
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, originalValue, label, true);
            EditorGUI.EndProperty();
        }
    }
}
