using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CustomEditor(typeof(WigController))]
    public class WigControllerEditor : Editor
    {
        #region Editor functions

        SerializedProperty _template;
        SerializedProperty _stepDivide;
        SerializedProperty _randomSeed;

        SerializedProperty _target;
        SerializedProperty _length;

        void OnEnable()
        {
            _template = serializedObject.FindProperty("_template");
            _stepDivide = serializedObject.FindProperty("_stepDivide");
            _randomSeed = serializedObject.FindProperty("_randomSeed");

            _target = serializedObject.FindProperty("_target");
            _length = serializedObject.FindProperty("_length");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool needsReset = false;
            EditorGUI.BeginChangeCheck();

            // Build time options
            EditorGUILayout.PropertyField(_template);
            EditorGUILayout.PropertyField(_stepDivide);
            EditorGUILayout.PropertyField(_randomSeed);

            // Play mode: check changes at this point.
            if (Application.isPlaying)
                needsReset = EditorGUI.EndChangeCheck();

            EditorGUILayout.Space();

            // Runtime options
            EditorGUILayout.PropertyField(_target);
            EditorGUILayout.PropertyField(_length);

            // Edit mode: check changes at this point.
            if (!Application.isPlaying)
                needsReset = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            // Request reset if there are any changes.
            if (needsReset)
                foreach (var t in targets)
                    ((WigController)t).RequestResetFromEditor();
        }

        #endregion
    }
}
