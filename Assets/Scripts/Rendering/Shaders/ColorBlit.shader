Shader "BlitWithMaterial"
{
    Properties {
        _Palette("Palette", 2D) = "white" {}
        _DitherPattern("Dither Pattern", 2D) = "white" {}
        _NumColors("Num Colors", Integer) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off Cull Off
        Pass
        {
            Name "BlitWithMaterialPass"
            HLSLPROGRAM
            #define UNITY_DECLARE_TEX2D(tex) TEXTURE2D(tex); SAMPLER(sampler##tex)
            #define UNITY_SAMPLE_TEX2D(tex,coord) SAMPLE_TEXTURE2D(tex, sampler##tex, coord)

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            TEXTURE2D(_Palette);
            int _NumColors;
            TEXTURE2D(_DitherPattern);
            int _DitherBoxSize;
            float4 _DitherPattern_TexelSize;
            SamplerState s1_point_clamp_sampler; 
            SamplerState s2_point_clamp_sampler; 
            SamplerState s3_point_repeat_sampler; 

            static float _DitherMask[8][8] = {
                {0.0, 0.5, 0.125, 0.625, 0.03125, 0.53125, 0.15625, 0.65625},
                {0.75, 0.25, 0.875, 0.375, 0.78125, 0.28125, 0.90625, 0.40625},
                {0.1875, 0.6875, 0.0625, 0.5625, 0.21875, 0.71875, 0.09375, 0.59375},
                {0.9375, 0.4375, 0.8125, 0.3125, 0.96875, 0.46875, 0.84375, 0.34375}, 
                {0.046875, 0.546875, 0.171875, 0.671875, 0.015625, 0.515625, 0.140625, 0.640625},
                {0.796875, 0.296875, 0.921875, 0.421875, 0.765625, 0.265625, 0.890625, 0.390625}, 
                {0.234375, 0.734375, 0.109375, 0.609375, 0.203125, 0.703125, 0.078125, 0.578125},
                {0.984375, 0.484375, 0.859375, 0.359375, 0.953125, 0.453125, 0.828125, 0.328125}
            };

            // Out frag function takes as input a struct that contains the screen space coordinate we are going to use to sample our texture. It also writes to SV_Target0, this has to match the index set in the UseTextureFragment(sourceTexture, 0, …) we defined in our render pass script.   
            float4 Frag(Varyings input) : SV_Target0
            {
                // this is needed so we account XR platform differences in how they handle texture arrays
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // sample the texture using the SAMPLE_TEXTURE2D
                float2 uv = input.texcoord.xy;
                float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, uv);
                float grey = pow(1.0 - saturate(0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b), 3.2);
                
                // colors go light -> dark so x = 0 is brightest color
                half4 colorAbove = SAMPLE_TEXTURE2D(_Palette, s1_point_clamp_sampler, float2(floor(grey * _NumColors) / _NumColors + 0.5 / _NumColors, 0));
                half4 colorBelow = SAMPLE_TEXTURE2D(_Palette, s2_point_clamp_sampler, float2(ceil(grey * _NumColors) / _NumColors + 0.5 / _NumColors, 0));

                float bayerX = uv.x * _BlitTexture_TexelSize.z * _DitherPattern_TexelSize.x;
                float bayerY = uv.y * _BlitTexture_TexelSize.w * _DitherPattern_TexelSize.y;

                int bayerX2 = (int)(uv.x * _BlitTexture_TexelSize.z) % 8;
                int bayerY2 = (int)(uv.y * _BlitTexture_TexelSize.w) % 8;

                half4 bayerSample = SAMPLE_TEXTURE2D(_DitherPattern, s3_point_repeat_sampler, float2(bayerX, bayerY));

                float colorFrac = 1.0 - frac(grey * _NumColors);
                //colorAbove = float4(1, 1, 1, 1);
                //colorBelow = float4(0,0,0,0);
                //if (bayerSample.r < 0.001) {
                //    return float4(1, 0, 0, 0);
                //}
                //return abs(bayerSample.r) * float4(1, 1, 1, 1);
                if (colorFrac > bayerSample.r) {
                //if (uv.x > _DitherMask[bayerX2][bayerY2]) {
                    return colorAbove;
                }
                return colorBelow;
            }

            ENDHLSL
        }
    }
}