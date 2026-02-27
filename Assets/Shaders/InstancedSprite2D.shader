Shader "VampireSurvivorLike/InstancedSprite2D"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "InstancedSprite2D"
            Tags { "LightMode"="Universal2D" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            UNITY_INSTANCING_BUFFER_START(PerInstance)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float4, _UvRect)
            UNITY_INSTANCING_BUFFER_END(PerInstance)

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);

                o.positionCS = TransformObjectToHClip(input.positionOS);

                float4 uvRect = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _UvRect);
                o.uv = input.uv * uvRect.zw + uvRect.xy;
                o.color = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                c *= input.color;
                return c;
            }
            ENDHLSL
        }
    }
}

