Shader "Hidden/Kvant/Wig/Kernel"
{
    Properties
    {
        _PositionTex("", 2D) = ""{}
        _VelocityTex("", 2D) = ""{}
        _FoundationTex("", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _PositionTex;
    float4 _PositionTex_TexelSize;

    sampler2D _VelocityTex;
    float4 _VelocityTex_TexelSize;

    sampler2D _FoundationTex;
    float4 _FoundationTex_TexelSize;

    float4x4 _Transform;
    float _DeltaTime;
    float _RandomSeed;

    float4 SamplePosition(float2 uv, float delta)
    {
        uv += float2(0, _PositionTex_TexelSize.y * delta);
        return float4(tex2D(_PositionTex, uv).xyz, 1);
    }

    float3 SampleVelocity(float2 uv, float delta)
    {
        uv += float2(0, _VelocityTex_TexelSize.y * delta);
        return tex2D(_VelocityTex, uv).xyz;
    }

    float4 SampleFoundationPosition(float2 uv)
    {
        float3 p = tex2D(_FoundationTex, float2(uv.x, 0)).xyz;
        return mul(_Transform, float4(p, 1));
    }

    float3 SampleFoundationNormal(float2 uv)
    {
        float3 n = tex2D(_FoundationTex, float2(uv.x, 1)).xyz;
        return mul((float3x3)_Transform, n);
    }

    float4 frag_InitPosition(v2f_img i) : SV_Target
    {
        float4 p = SampleFoundationPosition(i.uv);
        p.xyz += SampleFoundationNormal(i.uv) * i.uv.y;
        return p;
    }

    float4 frag_InitVelocity(v2f_img i) : SV_Target
    {
        return float4(SampleFoundationNormal(i.uv), 0);
    }

    float4 frag_UpdatePosition(v2f_img i) : SV_Target
    {
        if (i.uv.y < _PositionTex_TexelSize.y)
        {
            return SampleFoundationPosition(i.uv);
        }
        else
        {
            float4 p = SamplePosition(i.uv, 0);
            p.xyz += SampleVelocity(i.uv, 0) * _DeltaTime;
            return p;
        }
    }

    static const float kSegmentLength = 0.05;

    float4 frag_UpdateVelocity(v2f_img i) : SV_Target
    {
        float3 v = SampleVelocity(i.uv, 0);

        float3 p0 = SamplePosition(i.uv, -2).xyz;
        float3 p1 = SamplePosition(i.uv, -1).xyz;
        float3 p2 = SamplePosition(i.uv, 0).xyz;

        float3 pt = p1 + normalize(p1 - p0) * kSegmentLength;

        if (i.uv.y < _PositionTex_TexelSize.y * 2)
            pt = p1 + SampleFoundationNormal(i.uv) * kSegmentLength;
        
        float3 diff = pt - p2;
        v += diff * _DeltaTime * 400;

        v *= exp(-40 * _DeltaTime);

        v += float3(0, -8, 2) * _DeltaTime;

        v = normalize(v) * min(length(v), 100);

        return float4(v, 0);
    }

    ENDCG

    SubShader
    {
        // Pass 0 - Position buffer initialization
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_InitPosition
            #pragma target 3.0
            ENDCG
        }
        // Pass 1 - Velocity buffer initialization
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_InitVelocity
            #pragma target 3.0
            ENDCG
        }
        // Pass 2 - Position buffer update
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_UpdatePosition
            #pragma target 3.0
            ENDCG
        }
        // Pass 3 - Velocity buffer update
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_UpdateVelocity
            #pragma target 3.0
            ENDCG
        }
    }
}
