Shader "Hidden/Kvant/Wig/Hair"
{
    Properties
    {
        _PositionTex("", 2D) = ""{}
    }

    CGINCLUDE

    sampler2D _PositionTex;
    float4 _PositionTex_TexelSize;

    struct Input {
        half color;
    };

    void vert(inout appdata_full v, out Input data)
    {
        UNITY_INITIALIZE_OUTPUT(Input, data);

        float4 uv = float4(v.vertex.xy, 0, 0);

        float3 pos = tex2Dlod(_PositionTex, uv).xyz;

        v.vertex.xyz = pos.xyz;

        data.color = 1.5 * (1 - uv.y);
    }

    void surf(Input IN, inout SurfaceOutputStandard o)
    {
        o.Emission = IN.color;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma target 3.0
        ENDCG
    }
}
