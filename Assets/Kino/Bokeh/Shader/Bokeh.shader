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

// The idea of the separable hex bokeh filter came from the paper by
// L. McIntosh (2012). See the following paper for further details.
// http://ivizlab.sfu.ca/media/DiPaolaMcIntoshRiecke2012.pdf

Shader "Hidden/Kino/Bokeh"
{
    Properties
    {
        _MainTex("-", 2D) = "black"{}
        _BlurTex1("-", 2D) = "black"{}
        _BlurTex2("-", 2D) = "black"{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    #pragma multi_compile BLUR_STEP5 BLUR_STEP10 BLUR_STEP15 BLUR_STEP20
    #pragma multi_compile _ FOREGROUND_BLUR

#if BLUR_STEP5
    static const int BLUR_STEP = 5;
#elif BLUR_STEP10
    static const int BLUR_STEP = 10;
#elif BLUR_STEP15
    static const int BLUR_STEP = 15;
#else
    static const int BLUR_STEP = 20;
#endif

    // Source textures
    sampler2D _MainTex;
    sampler2D_float _CameraDepthTexture;

    // Only used in the combiner pass.
    sampler2D _BlurTex1;
    sampler2D _BlurTex2;

    // Camera parameters
    float _SubjectDistance;
    float _LensCoeff;  // f^2 / (N * (S1 - f) * film_width)

    // Blur parameters
    float2 _Aspect;
    float2 _BlurDisp;

    // 1st pass - make CoC map in alpha plane
    half4 frag_make_coc(v2f_img i) : SV_Target
    {
        // Calculate the radius of CoC.
        // https://en.wikipedia.org/wiki/Circle_of_confusion
        half3 c = tex2D(_MainTex, i.uv).rgb;
        float d = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
        float r = 0.5 * (d - _SubjectDistance) * _LensCoeff / d;
        return half4(c, r);
    }

    // 2nd pass - CoC visualization
    half4 frag_alpha_to_grayscale(v2f_img i) : SV_Target
    {
        half a = tex2D(_MainTex, i.uv).a * 2;
        return saturate(half4(abs(a), a, a, 1));
    }

    // 3rd pass - separable blur filter
    half4 frag_blur(v2f_img i) : SV_Target
    {
        half4 c0 = tex2D(_MainTex, i.uv);
        half r0 = abs(c0.a); // CoC radius

        half3 acc = c0.rgb;  // accumulation
        half total = 1;      // total weight

        for (int di = 1; di < BLUR_STEP; di++)
        {
            float2 disp = _BlurDisp * di;
            float disp_len = length(disp);

            float2 duv = disp * _Aspect;
            float2 uv1 = i.uv - duv;
            float2 uv2 = i.uv + duv;

            half4 c1 = tex2D(_MainTex, uv1);
            half4 c2 = tex2D(_MainTex, uv2);

#if FOREGROUND_BLUR
            // Complex version of sample weight calculation,
            // which supports both background and foreground blurring.

            // if depth > depth0
            //   weight = min(CoC, CoC0) > |disp|
            // else
            //   weight = CoC > |disp|

            half r1 = abs(c1.a);
            half r2 = abs(c2.a);

            float w1 = min(r1, (c1.a <= r0) * r1 + r0) > disp_len;
            float w2 = min(r2, (c2.a <= r0) * r2 + r0) > disp_len;
#else
            // Simpler version only supports background blurring.
            float w1 = min(c1.a, c0.a) > disp_len;
            float w2 = min(c2.a, c0.a) > disp_len;
#endif
            acc += c1.rgb * w1 + c2.rgb * w2;
            total += w1 + w2;
        }

        return half4(acc / total, c0.a);
    }

    // 4th pass - combiner
    half4 frag_combiner(v2f_img i) : SV_Target
    {
        half4 c1 = tex2D(_BlurTex1, i.uv);
        half4 c2 = tex2D(_BlurTex2, i.uv);
        return min(c1, c2);
    }

    ENDCG

    Subshader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_make_coc
            ENDCG
        }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_alpha_to_grayscale
            ENDCG
        }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_blur
            ENDCG
        }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_combiner
            ENDCG
        }
    }
}
