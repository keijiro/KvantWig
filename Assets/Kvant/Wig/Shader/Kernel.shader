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
    float _SegmentLength;

    float3 SamplePosition(float2 uv, float delta)
    {
        uv += float2(0, _PositionTex_TexelSize.y * delta);
        return tex2D(_PositionTex, uv).xyz;
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
        float3 p = SampleFoundationPosition(i.uv);
        p += SampleFoundationNormal(i.uv) * i.uv.y;
        return float4(p, 1);
    }

    float4 frag_InitVelocity(v2f_img i) : SV_Target
    {
        float3 n = SampleFoundationNormal(i.uv);
        return float4(n, 0);
    }

    float4 frag_UpdatePosition(v2f_img i) : SV_Target
    {
        if (i.uv.y < _PositionTex_TexelSize.y)
        {
            float3 p = SampleFoundationPosition(i.uv);
            return float4(p, 1);
        }
        else
        {
            float3 p = SamplePosition(i.uv, 0);
            p += SampleVelocity(i.uv, 0) * _DeltaTime;

            float3 p0 = SamplePosition(i.uv, -1);
            float3 diff = p - p0;
            float len = length(diff);

            if (len > _SegmentLength * lerp(0.5, 1, frac(i.uv.x * 123.4)));
                p = p0 + diff / len * _SegmentLength * lerp(0.5, 1, frac(i.uv.x * 123.4));

            return float4(p, 1);
        }
    }

    float4 frag_UpdateVelocity(v2f_img i) : SV_Target
    {
        float3 v = SampleVelocity(i.uv, 0);

        float3 p0 = SamplePosition(i.uv, -3);
        float3 p1 = SamplePosition(i.uv, -1);
        float3 p2 = SamplePosition(i.uv, 0);

        float3 pt = p1 + normalize(p1 - p0) * _SegmentLength * lerp(0.5, 1, frac(i.uv.x * 123.4));

        if (i.uv.y < _PositionTex_TexelSize.y * 2)
            pt = p1 + SampleFoundationNormal(i.uv) * _SegmentLength * lerp(0.5, 1, frac(i.uv.x * 123.4));

        v *= exp(-30 * _DeltaTime);
        //v = normalize(v) * min(length(v), 400);
        
        float3 diff = pt - p2;
        v += diff * _DeltaTime * 600;

        v += float3(0, -8, 2) * _DeltaTime;

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
