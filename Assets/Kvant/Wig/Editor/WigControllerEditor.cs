using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CustomEditor(typeof(WigController))]
    public class WigControllerEditor : Editor
    {
        #region Editor functions

        SerializedProperty _template;
        SerializedProperty _maxTimeStep;
        SerializedProperty _randomSeed;

        SerializedProperty _target;
        SerializedProperty _length;
        SerializedProperty _lengthRandomness;

        SerializedProperty _spring;
        SerializedProperty _damping;
        SerializedProperty _gravity;

        SerializedProperty _noiseAmplitude;
        SerializedProperty _noiseFrequency;
        SerializedProperty _noiseSpeed;

        static GUIContent _textRandomness = new GUIContent("Randomness");

        void OnEnable()
        {
            _template = serializedObject.FindProperty("_template");
            _maxTimeStep = serializedObject.FindProperty("_maxTimeStep");
            _randomSeed = serializedObject.FindProperty("_randomSeed");

            _target = serializedObject.FindProperty("_target");
            _length = serializedObject.FindProperty("_length");
            _lengthRandomness = serializedObject.FindProperty("_lengthRandomness");

            _spring = serializedObject.FindProperty("_spring");
            _damping = serializedObject.FindProperty("_damping");
            _gravity = serializedObject.FindProperty("_gravity");

            _noiseAmplitude = serializedObject.FindProperty("_noiseAmplitude");
            _noiseFrequency = serializedObject.FindProperty("_noiseFrequency");
            _noiseSpeed = serializedObject.FindProperty("_noiseSpeed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool needsReset = false;
            EditorGUI.BeginChangeCheck();

            // Build time options
            EditorGUILayout.PropertyField(_template);
            EditorGUILayout.PropertyField(_maxTimeStep);
            EditorGUILayout.PropertyField(_randomSeed);

            // Play mode: check changes at this point.
            if (Application.isPlaying)
                needsReset = EditorGUI.EndChangeCheck();

            EditorGUILayout.Space();

            // Runtime options
            EditorGUILayout.PropertyField(_target);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_length);
            EditorGUILayout.PropertyField(_lengthRandomness, _textRandomness);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_spring);
            EditorGUILayout.PropertyField(_damping);
            EditorGUILayout.PropertyField(_gravity);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_noiseAmplitude);
            EditorGUILayout.PropertyField(_noiseFrequency);
            EditorGUILayout.PropertyField(_noiseSpeed);

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
