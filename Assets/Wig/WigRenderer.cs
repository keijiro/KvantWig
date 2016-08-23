using UnityEngine;
using UnityEngine.Rendering;

namespace Kvant
{
    [ExecuteInEditMode, AddComponentMenu("Kvant/Wig")]
    public class WigRenderer : MonoBehaviour
    {
        #region Editable properties

        [SerializeField]
        ShadowCastingMode _castShadows;

        [SerializeField]
        bool _receiveShadows = false;

        [SerializeField]
        Mesh _templateMesh;

        [SerializeField]
        Texture2D _skinTexture;

        [SerializeField]
        float _randomSeed;

        // Just used to have references to the shader assets.
        [SerializeField] Shader _kernelShader;
        [SerializeField] Shader _hairShader;

        #endregion

        #region Private members

        Material _kernelMaterial;
        Material _hairMaterial;

        RenderTexture _positionBuffer1;
        RenderTexture _positionBuffer2;
        RenderTexture _velocityBuffer1;
        RenderTexture _velocityBuffer2;

        bool _needsReset = true;

        Material CreateMaterial(string shaderName)
        {
            var material = new Material(Shader.Find(shaderName));
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        RenderTexture CreateBuffer()
        {
            var format = RenderTextureFormat.ARGBFloat;
            var width = _skinTexture.width;
            var buffer = new RenderTexture(width, 32, 0, format);
            buffer.hideFlags = HideFlags.DontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Clamp;
            return buffer;
        }

        void InitializeBuffers()
        {
            if (_positionBuffer1) DestroyImmediate(_positionBuffer1);
            if (_positionBuffer2) DestroyImmediate(_positionBuffer2);
            if (_velocityBuffer1) DestroyImmediate(_velocityBuffer1);
            if (_velocityBuffer2) DestroyImmediate(_velocityBuffer2);

            _positionBuffer1 = CreateBuffer();
            _positionBuffer2 = CreateBuffer();
            _velocityBuffer1 = CreateBuffer();
            _velocityBuffer2 = CreateBuffer();
        }

        void UpdateKernelParameters(float deltaTime)
        {
            var m = _kernelMaterial;
            m.SetTexture("_SkinTex", _skinTexture);
            m.SetMatrix("_Transform", transform.localToWorldMatrix);
            m.SetFloat("_DeltaTime", deltaTime);
            m.SetFloat("_RandomSeed", _randomSeed);
        }

        void ResetState()
        {
            UpdateKernelParameters(0);
            Graphics.Blit(null, _positionBuffer2, _kernelMaterial, 0);
            Graphics.Blit(null, _velocityBuffer2, _kernelMaterial, 1);
        }

        void InvokeKernels(float deltaTime)
        {
            // Swap buffers.
            var pb = _positionBuffer1;
            var vb = _velocityBuffer1;
            _positionBuffer1 = _positionBuffer2;
            _velocityBuffer1 = _velocityBuffer2;
            _positionBuffer2 = pb;
            _velocityBuffer2 = vb;

            // Update the velocity buffer.
            UpdateKernelParameters(deltaTime);
            _kernelMaterial.SetTexture("_PositionTex", _positionBuffer1);
            _kernelMaterial.SetTexture("_VelocityTex", _velocityBuffer1);
            Graphics.Blit(null, _velocityBuffer2, _kernelMaterial, 3);

            // Update the position buffer.
            _kernelMaterial.SetTexture("_VelocityTex", _velocityBuffer2);
            Graphics.Blit(null, _positionBuffer2, _kernelMaterial, 2);
        }

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            if (_kernelMaterial == null)
                _kernelMaterial = CreateMaterial("Hidden/Kvant/Wig/Kernel");

            if (_hairMaterial == null)
                _hairMaterial = CreateMaterial("Hidden/Kvant/Wig/Hair");
        }

        void Reset()
        {
            _needsReset = true;
        }

        void OnDestroy()
        {
            if (_kernelMaterial != null)
                DestroyImmediate(_kernelMaterial);

            if (_hairMaterial != null)
                DestroyImmediate(_hairMaterial);
        }

        void Update()
        {
            if (_needsReset)
            {
                InitializeBuffers();
                ResetState();
                _needsReset = false;
            }

            InvokeKernels(Time.deltaTime);

            _hairMaterial.SetTexture("_PositionTex", _positionBuffer2);
            _hairMaterial.SetTexture("_SkinTex", _skinTexture);

            Graphics.DrawMesh(
                _templateMesh, Matrix4x4.identity, _hairMaterial, gameObject.layer,
                null, 0, null, _castShadows, _receiveShadows
            );
        }

        #endregion
    }
}
