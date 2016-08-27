Shader "Kvant/Wig/Filament"
{
    Properties
    {
        [HideInInspector]
        _PositionBuffer("", 2D) = ""{}

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

    sampler2D _PositionBuffer;
    float4 _PositionBuffer_TexelSize;

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

    half3 HueToRGB(half h)
    {
        half r = abs(h * 6 - 3) - 1;
        half g = 2 - abs(h * 6 - 2);
        half b = 2 - abs(h * 6 - 4);
        half3 rgb = saturate(half3(r, g, b));
#if UNITY_COLORSPACE_GAMMA
        return rgb;
#else
        return GammaToLinearSpace(rgb);
#endif
    }

    void vert(inout appdata_full v, out Input data)
    {
        UNITY_INITIALIZE_OUTPUT(Input, data);

        float filament = v.texcoord.x;
        float segment = v.texcoord.y;
        float dseg = _PositionBuffer_TexelSize.y;

        float3 p0 = tex2Dlod(_PositionBuffer, float4(filament, segment - dseg, 0, 0)).xyz;
        float3 p1 = tex2Dlod(_PositionBuffer, float4(filament, segment       , 0, 0)).xyz;
        float3 p2 = tex2Dlod(_PositionBuffer, float4(filament, segment + dseg, 0, 0)).xyz;

        float3 ax = normalize(float3(1, frac(filament * 31.492 + segment * 0.2) * 2 - 1, 0));
        float3 az = normalize(p2 - p0);
        float3 ay = normalize(cross(az, ax));
        ax = normalize(cross(ay, az));

        float3x3 axes = float3x3(ax, ay, az);

        float radius = _Thickness * (1 - segment);

        v.vertex.xyz = p1.xyz + mul(v.vertex, axes) * radius;
        v.normal = mul(v.normal, axes);

        data.filamentID = filament + _RandomSeed * 58.92128;
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
