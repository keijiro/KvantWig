using UnityEngine;
using UnityEngine.Rendering;

namespace Kvant
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Kvant/Wig Controller")]
    public class WigController : MonoBehaviour
    {
        #region Editable properties

        [SerializeField]
        Transform _target;

        [SerializeField]
        WigTemplate _template;

        [SerializeField, Range(0.01f, 5)]
        float _length = 1;

        [SerializeField, Range(0, 1)]
        float _lengthRandomness = 0.5f;

        [SerializeField]
        float _spring = 600;

        [SerializeField]
        float _damping = 30;

        [SerializeField]
        Vector3 _gravity = new Vector3(0, -8, 2);

        [SerializeField]
        float _maxTimeStep = 0.006f;

        [SerializeField]
        float _randomSeed;

        #endregion

        #region Private members

        // Just used to have references to the shader asset.
        [SerializeField, HideInInspector] Shader _kernels;

        // Temporary objects for simulation
        Material _material;
        RenderTexture _positionBuffer1;
        RenderTexture _positionBuffer2;
        RenderTexture _velocityBuffer1;
        RenderTexture _velocityBuffer2;
        RenderTexture _basisBuffer1;
        RenderTexture _basisBuffer2;

        // Temporary objects for controlling other components
        MeshFilter _meshFilter;
        MaterialPropertyBlock _propertyBlock;

        // Previous position/rotation of the target transform.
        Vector3 _targetPosition;
        Quaternion _targetRotation;

        // Reset flag
        bool _needsReset = true;

        // Create a buffer for simulation.
        RenderTexture CreateSimulationBuffer()
        {
            var format = RenderTextureFormat.ARGBFloat;
            var width = _template.filamentCount;
            var buffer = new RenderTexture(width, _template.segmentCount, 0, format);
            buffer.hideFlags = HideFlags.DontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Clamp;
            return buffer;
        }

        // Initialize private resources.
        // Can be called multiple times.
        void SetUpResources()
        {
            if (_material == null)
            {
                var shader = Shader.Find("Hidden/Kvant/Wig/Kernels");
                _material = new Material(shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            _material.SetTexture("_FoundationData", _template.foundation);
            _material.SetFloat("_RandomSeed", _randomSeed);

            if (_positionBuffer1 != null) DestroyImmediate(_positionBuffer1);
            if (_positionBuffer2 != null) DestroyImmediate(_positionBuffer2);
            if (_velocityBuffer1 != null) DestroyImmediate(_velocityBuffer1);
            if (_velocityBuffer2 != null) DestroyImmediate(_velocityBuffer2);
            if (_basisBuffer1 != null) DestroyImmediate(_basisBuffer1);
            if (_basisBuffer2 != null) DestroyImmediate(_basisBuffer2);

            _positionBuffer1 = CreateSimulationBuffer();
            _positionBuffer2 = CreateSimulationBuffer();
            _velocityBuffer1 = CreateSimulationBuffer();
            _velocityBuffer2 = CreateSimulationBuffer();
            _basisBuffer1 = CreateSimulationBuffer();
            _basisBuffer2 = CreateSimulationBuffer();

            _meshFilter = GetComponent<MeshFilter>();

            if (_meshFilter == null)
            {
                _meshFilter = gameObject.AddComponent<MeshFilter>();
                _meshFilter.hideFlags = HideFlags.NotEditable;
            }

            _meshFilter.sharedMesh = _template.mesh;
        }

        // Reset the simulation state.
        void ResetSimulationState()
        {
            _targetPosition = _target.position;
            _targetRotation = _target.rotation;

            UpdateSimulationParameters(_targetPosition, _targetRotation, 0);

            // This is needed to give the texel size for the shader.
            _material.SetTexture("_PositionBuffer", _positionBuffer1);

            Graphics.Blit(null, _positionBuffer2, _material, 0);
            Graphics.Blit(null, _velocityBuffer2, _material, 1);
            Graphics.Blit(null, _basisBuffer2, _material, 2);
        }

        // Update the parameters in the simulation kernels.
        void UpdateSimulationParameters(Vector3 pos, Quaternion rot, float dt)
        {
            _material.SetMatrix("_FoundationTransform", Matrix4x4.TRS(pos, rot, Vector3.one));
            _material.SetVector("_SegmentLength", new Vector2(
                _length / _template.segmentCount, _lengthRandomness
            ));
            _material.SetFloat("_Spring", _spring);
            _material.SetFloat("_Damping", _damping);
            _material.SetVector("_Gravity", _gravity);
            _material.SetFloat("_DeltaTime", dt);
        }

        // Invoke the simulation kernels.
        void InvokeSimulationKernels(Vector3 pos, Quaternion rot, float dt)
        {
            // Swap the buffers.
            var pb = _positionBuffer1;
            var vb = _velocityBuffer1;
            var nb = _basisBuffer1;
            _positionBuffer1 = _positionBuffer2;
            _velocityBuffer1 = _velocityBuffer2;
            _basisBuffer1 = _basisBuffer2;
            _positionBuffer2 = pb;
            _velocityBuffer2 = vb;
            _basisBuffer2 = nb;

            // Update the velocity buffer.
            UpdateSimulationParameters(pos, rot, dt);
            _material.SetTexture("_PositionBuffer", _positionBuffer1);
            _material.SetTexture("_VelocityBuffer", _velocityBuffer1);
            Graphics.Blit(null, _velocityBuffer2, _material, 4);

            // Update the position buffer.
            _material.SetTexture("_VelocityBuffer", _velocityBuffer2);
            Graphics.Blit(null, _positionBuffer2, _material, 3);

            // Update the basis buffer.
            _material.SetTexture("_PositionBuffer", _positionBuffer2);
            _material.SetTexture("_BasisBuffer", _basisBuffer1);
            Graphics.Blit(null, _basisBuffer2, _material, 5);
        }

        // Do simulation.
        void Simulate(float deltaTime)
        {
            var newTargetPosition = _target.position;
            var newTargetRotation = _target.rotation;

            var steps = Mathf.CeilToInt(deltaTime / _maxTimeStep);
            steps = Mathf.Clamp(steps, 1, 100);

            var dt = deltaTime / steps;

            for (var i = 0; i < steps; i++)
            {
                var p = (float)i / steps;
                var pos = Vector3.Lerp(_targetPosition, newTargetPosition, p);
                var rot = Quaternion.Lerp(_targetRotation, newTargetRotation, p);
                InvokeSimulationKernels(pos, rot, dt);
            }

            _targetPosition = newTargetPosition;
            _targetRotation = newTargetRotation;
        }

        #endregion

        #if UNITY_EDITOR
        #region Editor functions

        public void RequestResetFromEditor()
        {
            _needsReset = true;
        }

        #endregion
        #endif

        #region MonoBehaviour functions

        void Reset()
        {
            _needsReset = true;
        }

        void OnDestroy()
        {
            if (_material != null) DestroyImmediate(_material);
            if (_positionBuffer1 != null) DestroyImmediate(_positionBuffer1);
            if (_positionBuffer2 != null) DestroyImmediate(_positionBuffer2);
            if (_velocityBuffer1 != null) DestroyImmediate(_velocityBuffer1);
            if (_velocityBuffer2 != null) DestroyImmediate(_velocityBuffer2);
            if (_basisBuffer1 != null) DestroyImmediate(_basisBuffer1);
            if (_basisBuffer2 != null) DestroyImmediate(_basisBuffer2);
        }

        void LateUpdate()
        {
            // Do nothing if something is missing.
            if (_template == null || _target == null) return;

            // Reset/Initialization
            if (_needsReset)
            {
                SetUpResources();
                ResetSimulationState();

                // Do warmup in edit mode.
                if (!Application.isPlaying) Simulate(0.4f);

                _needsReset = false;
            }

            // Advance simulation time.
            if (Application.isPlaying) Simulate(Time.deltaTime);

            // Update the material property block of the mesh renderer.
            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            _propertyBlock.SetTexture("_PositionBuffer", _positionBuffer2);
            _propertyBlock.SetTexture("_BasisBuffer", _basisBuffer2);
            _propertyBlock.SetFloat("_RandomSeed", _randomSeed);

            GetComponent<MeshRenderer>().SetPropertyBlock(_propertyBlock);
        }

        #endregion
    }
}
