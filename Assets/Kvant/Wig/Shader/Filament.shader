Shader "Kvant/Wig/Filament"
{
    Properties
    {
        [HideInInspector] _PositionBuffer("", 2D) = ""{}
        [HideInInspector] _BasisBuffer("", 2D) = ""{}

        [Gamma] _Metallic("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0

        [Space]
        _Thickness("Thickness", Float) = 0.02

        [Header(Base)]

        _BaseColor("Color", Color) = (1, 1, 1)
        _BaseRandom("Randomize", Range(0, 1)) = 1

        [Header(Glow)]

        _GlowIntensity("Intensity", Range(0, 20)) = 1
        _GlowProb("Probability", Range(0, 1)) = 0.1
        _GlowColor("Color", Color) = (1, 1, 1)
        _GlowRandom("Randomize", Range(0, 1)) = 1
    }

    CGINCLUDE

    #include "Common.cginc"

    half _Metallic;
    half _Smoothness;

    half _Thickness;

    half3 _BaseColor;
    half _BaseRandom;

    half _GlowIntensity;
    half _GlowProb;
    half3 _GlowColor;
    half _GlowRandom;

    float _RandomSeed;

    struct Input {
        half filamentID;
    };

    void vert(inout appdata_full v, out Input data)
    {
        UNITY_INITIALIZE_OUTPUT(Input, data);

        float2 uv = v.texcoord.xy;

        // Point position
        float3 p = SamplePosition(uv, 0);

        // Orthonormal basis vectors
        float3x3 basis = DecodeBasis(SampleBasis(uv, 0));

        // Filament radius
        float radius = _Thickness * (1 - uv.y * uv.y);

        // Modify the vertex
        v.vertex.xyz = p.xyz + mul(v.vertex, basis) * radius;
        v.normal = mul(v.normal, basis);

        // Parameters for the pixel shader
        data.filamentID = uv.x + _RandomSeed * 58.92128;
    }

    void surf(Input IN, inout SurfaceOutputStandard o)
    {
        // Random color
        half3 color = HueToRGB(frac(IN.filamentID * 314.2213));

        // Glow effect
        half glow = frac(IN.filamentID * 138.9044 + _Time.y / 2) < _GlowProb;

        o.Albedo = lerp(_BaseColor, color, _BaseRandom);
        o.Smoothness = _Smoothness;
        o.Metallic = _Metallic;
        o.Emission = lerp(_GlowColor, color, _GlowRandom) * _GlowIntensity * glow;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
        #pragma target 3.0
        ENDCG
    }
}
