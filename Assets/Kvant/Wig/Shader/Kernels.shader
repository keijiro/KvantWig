Shader "Hidden/Kvant/Wig/Kernels"
{
    Properties
    {
        [HideInInspector] _PositionBuffer("", 2D) = ""{}
        [HideInInspector] _VelocityBuffer("", 2D) = ""{}
        [HideInInspector] _BasisBuffer("", 2D) = ""{}
        [HideInInspector] _FoundationData("", 2D) = ""{}
    }

    CGINCLUDE

    #include "Common.cginc"

    float _RandomSeed;
    float _SegmentLength;
    float _Spring;
    float _Damping;
    float3 _Gravity;
    float _DeltaTime;

    float FilamentScale(float2 uv)
    {
        return lerp(0.5, 1, frac(uv.x * 123.4));
    }

    float4 frag_InitPosition(v2f_img i) : SV_Target
    {
        float3 origin = SampleFoundationPosition(i.uv);
        float3 normal = SampleFoundationNormal(i.uv);

        float dv = _PositionBuffer_TexelSize.y;
        float v = i.uv.y - dv * 0.5;

        float l = _SegmentLength / dv * FilamentScale(i.uv);

        return float4(origin + normal * (l * v), 1);
    }

    float4 frag_InitVelocity(v2f_img i) : SV_Target
    {
        return 0;
    }

    float4 frag_InitBasis(v2f_img i) : SV_Target
    {
        // Make a random basis around the foundation normal vector.
        float3 ax = float3(1, frac(i.uv.x * 131.492) * 2 - 1, 0);
        float3 az = SampleFoundationNormal(i.uv);
        float3 ay = normalize(cross(az, ax));
        ax = normalize(cross(ay, az));
        return EncodeBasis(ax, az);
    }

    float4 frag_UpdatePosition(v2f_img i) : SV_Target
    {
        if (i.uv.y < _PositionBuffer_TexelSize.y * 2)
        {
            // P0 and P1: Simply move with the foundation without physics.
            return frag_InitPosition(i);
        }
        else
        {
            // Newtonian motion
            float3 p = SamplePosition(i.uv, 0);
            p += SampleVelocity(i.uv) * _DeltaTime;

            // Segment length constraint
            float3 pp = SamplePosition(i.uv, -1);
            float l = _SegmentLength * FilamentScale(i.uv);
            p = pp + normalize(p - pp) * l;

            return float4(p, 1);
        }
    }

    float4 frag_UpdateVelocity(v2f_img i) : SV_Target
    {
        float3 p = SamplePosition(i.uv, 0);
        float3 v = SampleVelocity(i.uv);

        // Damping
        v *= exp(-_Damping * _DeltaTime);

        // Target position
        float l = _SegmentLength * FilamentScale(i.uv);
        float3 pp2 = SamplePosition(i.uv, -4);
        float3 pp1 = SamplePosition(i.uv, -1);
        float3 pt = pp1 + normalize(pp1 - pp2) * l;

        // Acceleration (spring model)
        v += (pt - p) * _DeltaTime * _Spring;

        // Gravity
        v += _Gravity * _DeltaTime;

        return float4(v, 0);
    }

    float4 frag_UpdateBasis(v2f_img i) : SV_Target
    {
        // Use the parent normal vector from the previous frame.
        float3 ax = StereoInverseProjection(SampleBasis(i.uv, -1).xy);

        // Tangent vector
        float3 p0 = SamplePosition(i.uv, -1);
        float3 p1 = SamplePosition(i.uv, 1);
        float3 az = normalize(p1 - p0);

        // Reconstruct the orthonormal basis.
        float3 ay = normalize(cross(az, ax));
        ax = normalize(cross(ay, az));

        return EncodeBasis(ax, az);
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
        // Pass 2 - Basis buffer initialization
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_InitBasis
            #pragma target 3.0
            ENDCG
        }
        // Pass 3 - Position buffer update
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_UpdatePosition
            #pragma target 3.0
            ENDCG
        }
        // Pass 4 - Velocity buffer update
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_UpdateVelocity
            #pragma target 3.0
            ENDCG
        }
        // Pass 5 - Basis buffer update
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_UpdateBasis
            #pragma target 3.0
            ENDCG
        }
    }
}
