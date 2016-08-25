//
// KinoBokeh - Fast DOF filter with hexagonal aperture
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;

namespace Kino
{
    [CanEditMultipleObjects, CustomEditor(typeof(Bokeh))]
    public class BokehEditor : Editor
    {
        SerializedProperty _subject;
        SerializedProperty _distance;
        SerializedProperty _fNumber;
        SerializedProperty _useCameraFov;
        SerializedProperty _focalLength;
        SerializedProperty _maxBlur;
        SerializedProperty _irisAngle;
        SerializedProperty _sampleCount;
        SerializedProperty _foregroundBlur;
        SerializedProperty _visualize;

        static GUIContent _textFNumber = new GUIContent("f/");
        static GUIContent _textFocalLengthMM = new GUIContent("Focal Length (mm)");
        static GUIContent _textMaxBlurPercent = new GUIContent("Max Blur (%)");

        void OnEnable()
        {
            _subject        = serializedObject.FindProperty("_subject");
            _distance       = serializedObject.FindProperty("_distance");
            _fNumber        = serializedObject.FindProperty("_fNumber");
            _useCameraFov   = serializedObject.FindProperty("_useCameraFov");
            _focalLength    = serializedObject.FindProperty("_focalLength");
            _maxBlur        = serializedObject.FindProperty("_maxBlur");
            _irisAngle      = serializedObject.FindProperty("_irisAngle");
            _sampleCount    = serializedObject.FindProperty("_sampleCount");
            _foregroundBlur = serializedObject.FindProperty("_foregroundBlur");
            _visualize      = serializedObject.FindProperty("_visualize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Subject/Distance
            EditorGUILayout.PropertyField(_subject);
            if (_subject.hasMultipleDifferentValues || _subject.objectReferenceValue == null)
                EditorGUILayout.PropertyField(_distance);

            // f/
            EditorGUILayout.PropertyField(_fNumber, _textFNumber);

            // Use Camera FOV
            EditorGUILayout.PropertyField(_useCameraFov);

            // Focal Length
            if (_useCameraFov.hasMultipleDifferentValues || !_useCameraFov.boolValue)
            {
                if (_focalLength.hasMultipleDifferentValues)
                    EditorGUILayout.PropertyField(_focalLength);
                else
                {
                    EditorGUI.BeginChangeCheck();
                    var f = _focalLength.floatValue * 1000;
                    f = EditorGUILayout.Slider(_textFocalLengthMM, f, 10.0f, 300.0f);
                    if (EditorGUI.EndChangeCheck())
                        _focalLength.floatValue = f / 1000;
                }
            }

            // Max Blur
            if (_maxBlur.hasMultipleDifferentValues)
                EditorGUILayout.PropertyField(_maxBlur);
            else
            {
                EditorGUI.BeginChangeCheck();
                var blur = _maxBlur.floatValue * 100;
                blur = EditorGUILayout.Slider(_textMaxBlurPercent, blur, 1, 10);
                if (EditorGUI.EndChangeCheck())
                    _maxBlur.floatValue = blur / 100;
            }

            // Iris Angle
            EditorGUILayout.Slider(_irisAngle, 0, 90);

            // Sample Count
            EditorGUILayout.PropertyField(_sampleCount);

            // Foreground Blur
            EditorGUILayout.PropertyField(_foregroundBlur);

            // Visualize
            EditorGUILayout.PropertyField(_visualize);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
