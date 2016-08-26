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

float3 HueToRgb(float h)
{
    float r = abs(h * 6 - 3) - 1;
    float g = 2 - abs(h * 6 - 2);
    float b = 2 - abs(h * 6 - 4);
    return saturate(float3(r, g, b));
}

    void vert(inout appdata_full v, out Input data)
    {
        UNITY_INITIALIZE_OUTPUT(Input, data);

        float4 uv = float4(v.texcoord.xy, 0, 0);

        float3 pos = tex2Dlod(_PositionTex, uv).xyz;
        float3 pos2 = tex2Dlod(_PositionTex, uv + float4(0, _PositionTex_TexelSize.y, 0, 0)).xyz;

        float3 ax = normalize(float3(1, frac(uv.x * 31.492) * 2 - 1, 0));
        float3 az = normalize(pos2 - pos);
        float3 ay = normalize(cross(az, ax));

        float3 vv = v.vertex.x * ax + v.vertex.y * ay + v.vertex.z * az;
        float3 vn = v.normal.x * ax + v.normal.y * ay + v.normal.z * az;

        v.vertex.xyz = pos.xyz + vv * 0.02 * (1 - uv.y);
        v.normal = normalize(vn);

        data.color = uv.x;
    }

    void surf(Input IN, inout SurfaceOutputStandard o)
    {
        o.Albedo = HueToRgb(frac(IN.color * 3142.213)) * 0.1;
        o.Smoothness = 0.8;
        o.Metallic = 0;
        o.Emission = o.Albedo * 20 * (frac(IN.color * 314.322 + _Time.y) > 0.9);
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
