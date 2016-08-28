using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CustomEditor(typeof(WigController))]
    public class WigControllerEditor : Editor
    {
        #region Editor functions

        SerializedProperty _target;
        SerializedProperty _template;

        SerializedProperty _maxTimeStep;
        SerializedProperty _randomSeed;

        SerializedProperty _length;
        SerializedProperty _lengthRandomness;

        SerializedProperty _spring;
        SerializedProperty _damping;
        SerializedProperty _gravity;

        SerializedProperty _noiseAmplitude;
        SerializedProperty _noiseFrequency;
        SerializedProperty _noiseSpeed;

        static GUIContent _textAmplitude = new GUIContent("Amplitude");
        static GUIContent _textFrequency = new GUIContent("Frequency");
        static GUIContent _textRandomness = new GUIContent("Randomness");
        static GUIContent _textSpeed = new GUIContent("Speed");

        void OnEnable()
        {
            _target = serializedObject.FindProperty("_target");
            _template = serializedObject.FindProperty("_template");

            _maxTimeStep = serializedObject.FindProperty("_maxTimeStep");
            _randomSeed = serializedObject.FindProperty("_randomSeed");

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

            // Edit mode: check changes from here.
            if (!Application.isPlaying) EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_target);

            // Play mode: check changes from here.
            if (Application.isPlaying) EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_template);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_maxTimeStep);
            EditorGUILayout.PropertyField(_randomSeed);

            // Play mode: check changes to here.
            if (Application.isPlaying) needsReset = EditorGUI.EndChangeCheck();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Filament", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_length);
            EditorGUILayout.PropertyField(_lengthRandomness, _textRandomness);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Dynamics", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_spring);
            EditorGUILayout.PropertyField(_damping);
            EditorGUILayout.PropertyField(_gravity);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Noise Field Force", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_noiseAmplitude, _textAmplitude);
            EditorGUILayout.PropertyField(_noiseFrequency, _textFrequency);
            EditorGUILayout.PropertyField(_noiseSpeed, _textSpeed);

            // Edit mode: check changes to here.
            if (!Application.isPlaying) needsReset = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            // Request reset if there are any changes.
            if (needsReset)
                foreach (var t in targets)
                    ((WigController)t).RequestResetFromEditor();
        }

        #endregion
    }
}
