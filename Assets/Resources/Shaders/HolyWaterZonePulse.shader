Shader "VSL/HolyWaterZonePulse"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _InnerColor ("Inner Color", Color) = (0.78, 0.45, 0.98, 0.55)
        _OuterColor ("Outer Color", Color) = (0.34, 0.16, 0.56, 0.72)
        _RingColor ("Ring Color", Color) = (0.92, 0.84, 1.0, 0.96)
        _Radius ("Radius", Range(0.1, 0.95)) = 0.45
        _RingThickness ("Ring Thickness", Range(0.01, 0.35)) = 0.13
        _Feather ("Feather", Range(0.01, 0.35)) = 0.13
        _NoiseScale ("Noise Scale", Range(0.5, 8.0)) = 3.6
        _SwirlSpeed ("Swirl Speed", Range(0.1, 4.0)) = 0.72
        _Intensity ("Intensity", Range(0.1, 3.0)) = 1.35
        _GuideAlpha ("Guide Alpha", Range(0.1, 1.0)) = 0.88
        _Life01 ("Life01", Range(0,1)) = 0
        _TickFlash ("TickFlash", Range(0,1)) = 0
        _Seed ("Seed", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _InnerColor;
            float4 _OuterColor;
            float4 _RingColor;
            float _Radius;
            float _RingThickness;
            float _Feather;
            float _NoiseScale;
            float _SwirlSpeed;
            float _Intensity;
            float _GuideAlpha;
            float _Life01;
            float _TickFlash;
            float _Seed;
        CBUFFER_END

        struct Attributes
        {
            float3 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            float4 color : COLOR;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float4 color : COLOR;
        };

        float hash21(float2 p)
        {
            p = frac(p * float2(123.34, 345.45));
            p += dot(p, p + 34.345);
            return frac(p.x * p.y);
        }

        float noise2(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            float a = hash21(i);
            float b = hash21(i + float2(1, 0));
            float c = hash21(i + float2(0, 1));
            float d = hash21(i + float2(1, 1));
            float2 u = f * f * (3.0 - 2.0 * f);
            return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
        }

        float fbm(float2 p)
        {
            float v = 0.0;
            float a = 0.55;
            float2 shift = float2(31.34, 17.17);
            [unroll]
            for (int i = 0; i < 4; i++)
            {
                v += noise2(p) * a;
                p = p * 2.03 + shift;
                a *= 0.5;
            }
            return v;
        }

        Varyings vert(Attributes v)
        {
            Varyings o;
            o.positionCS = TransformObjectToHClip(v.positionOS);
            o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
            o.color = v.color;
            return o;
        }

        half4 frag(Varyings i) : SV_Target
        {
            float2 uv = i.uv;
            float2 p = uv * 2.0 - 1.0;
            float dist = length(p);

            float lifeFade = 1.0 - smoothstep(0.72, 1.0, _Life01);
            float swirlTime = _Time.y * _SwirlSpeed;

            float angle = atan2(p.y, p.x) + swirlTime * 0.85;
            float2 swirlDir = float2(cos(angle), sin(angle));
            float2 seedOffset = float2(_Seed * 0.013, _Seed * 0.007);
            float2 noiseUv = p * _NoiseScale
                           + swirlDir * (0.42 + _TickFlash * 0.25)
                           + seedOffset
                           + float2(swirlTime * 0.28, -swirlTime * 0.19);

            float n1 = fbm(noiseUv);
            float n2 = fbm(noiseUv * 1.87 - swirlDir * 0.73);
            float cloud = saturate(0.35 + n1 * 0.55 + n2 * 0.25);

            float radialMask = 1.0 - smoothstep(_Radius - _Feather, _Radius + _Feather, dist);
            float cloudMask = saturate(radialMask * (0.62 + cloud * 0.72));

            float ringCenter = _Radius - _RingThickness * 0.45;
            float ringDist = abs(dist - ringCenter);
            float ringMask = 1.0 - smoothstep(_RingThickness, _RingThickness + _Feather * 0.8, ringDist);
            ringMask = saturate(ringMask + _TickFlash * 0.26);

            float core = saturate(1.0 - smoothstep(0.0, _Radius * 0.78, dist));
            float4 cloudCol = lerp(_OuterColor, _InnerColor, saturate(core * 0.85 + cloud * 0.4));
            float4 ringCol = _RingColor * ringMask * (_Intensity + _TickFlash * 0.55);
            float4 col = cloudCol * cloudMask + ringCol;

            float texAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a * i.color.a;
            col.a *= texAlpha * _GuideAlpha * lifeFade;
            col.rgb *= i.color.rgb;
            return col;
        }
        ENDHLSL

        Pass
        {
            Name "HolyWaterZone2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass
        {
            Name "HolyWaterZoneUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
