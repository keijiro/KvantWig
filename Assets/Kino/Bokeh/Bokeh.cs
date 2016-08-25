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

namespace Kino
{
    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public class Bokeh : MonoBehaviour
    {
        #region Public Properties

        [SerializeField]
        Transform _subject;

        Transform subject {
            get { return _subject; }
            set { _subject = value; }
        }

        [SerializeField]
        float _distance = 10.0f;

        float distance {
            get { return _distance; }
            set { _distance = value; }
        }

        [SerializeField]
        float _fNumber = 1.4f;

        float fNumber {
            get { return _fNumber; }
            set { _fNumber = value; }
        }

        [SerializeField]
        bool _useCameraFov = true;

        bool useCameraFov {
            get { return _useCameraFov; }
            set { _useCameraFov = value; }
        }

        [SerializeField]
        float _focalLength = 0.05f;

        float focalLength {
            get { return _focalLength; }
            set { _focalLength = value; }
        }

        [SerializeField]
        float _maxBlur = 0.03f;

        float maxBlur {
            get { return _maxBlur; }
            set { _maxBlur = value; }
        }

        [SerializeField]
        float _irisAngle = 0;

        float irisAngle {
            get { return _irisAngle; }
            set { _irisAngle = value; }
        }

        public enum SampleCount { Low, Medium, High, UltraHigh }

        [SerializeField]
        public SampleCount _sampleCount = SampleCount.Medium;

        SampleCount sampleCount {
            get { return _sampleCount; }
            set { _sampleCount = value; }
        }

        [SerializeField]
        bool _foregroundBlur = true;

        bool foregroundBlur {
            get { return _foregroundBlur; }
            set { _foregroundBlur = value; }
        }

        [SerializeField]
        bool _visualize;

        #endregion

        #region Private Properties and Functions

        // Standard film width = 24mm
        const float filmWidth = 0.024f;

        [SerializeField] Shader _shader;
        Material _material;

        int SeparableBlurSteps {
            get {
                if (_sampleCount == SampleCount.Low) return 5;
                if (_sampleCount == SampleCount.Medium) return 10;
                if (_sampleCount == SampleCount.High) return 15;
                return 20;
            }
        }

        float CalculateSubjectDistance()
        {
            if (_subject == null) return _distance;
            var cam = GetComponent<Camera>().transform;
            return Vector3.Dot(_subject.position - cam.position, cam.forward);
        }

        float CalculateFocalLength()
        {
            if (!_useCameraFov) return _focalLength;
            var fov = GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad;
            return 0.5f * filmWidth / Mathf.Tan(0.5f * fov);
        }

        void SetUpShaderKeywords()
        {
            if (_sampleCount == SampleCount.Low)
            {
                _material.DisableKeyword("BLUR_STEP10");
                _material.DisableKeyword("BLUR_STEP15");
                _material.DisableKeyword("BLUR_STEP20");
            }
            else if (_sampleCount == SampleCount.Medium)
            {
                _material.EnableKeyword("BLUR_STEP10");
                _material.DisableKeyword("BLUR_STEP15");
                _material.DisableKeyword("BLUR_STEP20");
            }
            else if (_sampleCount == SampleCount.High)
            {
                _material.DisableKeyword("BLUR_STEP10");
                _material.EnableKeyword("BLUR_STEP15");
                _material.DisableKeyword("BLUR_STEP20");
            }
            else // SampleCount.UltraHigh
            {
                _material.DisableKeyword("BLUR_STEP10");
                _material.DisableKeyword("BLUR_STEP15");
                _material.EnableKeyword("BLUR_STEP20");
            }

            if (_foregroundBlur)
                _material.EnableKeyword("FOREGROUND_BLUR");
            else
                _material.DisableKeyword("FOREGROUND_BLUR");
        }

        void SetUpShaderParameters(RenderTexture source)
        {
            var s1 = CalculateSubjectDistance();
            _material.SetFloat("_SubjectDistance", s1);

            var f = CalculateFocalLength();
            var coeff = f * f / (_fNumber * (s1 - f) * filmWidth);
            _material.SetFloat("_LensCoeff", coeff);

            var aspect = new Vector2((float)source.height / source.width, 1);
            _material.SetVector("_Aspect", aspect);
        }

        void SetSeparableBlurParameter(float dx, float dy)
        {
            float sin = Mathf.Sin(_irisAngle * Mathf.Deg2Rad);
            float cos = Mathf.Cos(_irisAngle * Mathf.Deg2Rad);
            var v = new Vector2(dx * cos - dy * sin, dx * sin + dy * cos);
            v *= _maxBlur * 0.5f / SeparableBlurSteps;
            _material.SetVector("_BlurDisp", v);
        }

        #endregion

        #region MonoBehaviour Functions

        void OnEnable()
        {
            var cam = GetComponent<Camera>();
            cam.depthTextureMode |= DepthTextureMode.Depth;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            // Set up the shader parameters.
            SetUpShaderKeywords();
            SetUpShaderParameters(source);

            // Create temporary buffers.
            var rt1 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
            var rt2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
            var rt3 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

            // Make CoC map in alpha channel.
            Graphics.Blit(source, rt1, _material, 0);

            if (_visualize)
            {
                // CoC visualization.
                Graphics.Blit(rt1, destination, _material, 1);
            }
            else
            {
                // 1st separable filter: horizontal blur.
                SetSeparableBlurParameter(1, 0);
                Graphics.Blit(rt1, rt2, _material, 2);

                // 2nd separable filter: skewed vertical blur (left).
                SetSeparableBlurParameter(-0.5f, -1);
                Graphics.Blit(rt2, rt3, _material, 2);

                // 3rd separable filter: skewed vertical blur (right).
                SetSeparableBlurParameter(0.5f, -1);
                Graphics.Blit(rt2, rt1, _material, 2);

                // Combine the result.
                _material.SetTexture("_BlurTex1", rt1);
                _material.SetTexture("_BlurTex2", rt3);
                Graphics.Blit(source, destination, _material, 3);
            }

            // Release the temporary buffers.
            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
            RenderTexture.ReleaseTemporary(rt3);
        }

        #endregion
    }
}
