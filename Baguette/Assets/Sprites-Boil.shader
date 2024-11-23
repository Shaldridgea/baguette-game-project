// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/Boil"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[NoScaleOffset] _NoiseTex("Noise Texture", 2D) = "white" {}
		_BoilSpeed("Speed", Range(0, 20)) = 1
		[PerRendererData] _TimeSnap("Snap", Range(0, 0.1)) = 0.001
		_BoilIntensityX("X Axis Intensity", Range(0, 0.05)) = 0.02
		_BoilIntensityY("Y Axis Intensity", Range(0, 0.05)) = 0.02
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
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
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
#include "UnityCG.cginc"
#ifdef UNITY_INSTANCING_ENABLED

			UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
			// SpriteRenderer.Color while Non-Batched/Instanced.
			UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
			// this could be smaller but that's how bit each entry is regardless of type
			UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
			UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

		#define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
		#define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)

	#endif // instancing

	CBUFFER_START(UnityPerDrawSprite)
	#ifndef UNITY_INSTANCING_ENABLED
		fixed4 _RendererColor;
		fixed2 _Flip;
	#endif
		float _EnableExternalAlpha;
	CBUFFER_END

		// Material Color.
		fixed4 _Color;

		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
		{
			return float4(pos.xy * flip, pos.z, 1.0);
		}

		v2f SpriteVert(appdata_t IN)
		{
			v2f OUT;

			UNITY_SETUP_INSTANCE_ID(IN);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

			OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
			OUT.vertex = UnityObjectToClipPos(OUT.vertex);
			OUT.texcoord = IN.texcoord;
			OUT.color = IN.color * _Color * _RendererColor;

			#ifdef PIXELSNAP_ON
			OUT.vertex = UnityPixelSnap(OUT.vertex);
			#endif

			return OUT;
		}

		sampler2D _MainTex;
		sampler2D _AlphaTex;
		sampler2D _NoiseTex;
		float _BoilSpeed;
		float _BoilIntensityX;
		float _BoilIntensityY;
		float _TimeSnap;

		fixed4 SampleSpriteTexture(float2 uv)
		{
			fixed4 color = tex2D(_MainTex, uv);

		#if ETC1_EXTERNAL_ALPHA
			fixed4 alpha = tex2D(_AlphaTex, uv);
			color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
		#endif

			return color;
		}

		float snap(float x, float snap)
		{
			return snap * round(x / snap);
		}

		fixed4 SpriteFrag(v2f IN) : SV_Target
		{
			float t = snap(_Time.x, _TimeSnap);
			t *= _BoilSpeed;
			float f = tex2D(_NoiseTex, IN.texcoord + float2(t, t)).a;
			float bx = lerp(-_BoilIntensityX, _BoilIntensityX, f);
			float by = lerp(-_BoilIntensityY, _BoilIntensityY, f);
			fixed2 xyOffset = fixed2(bx, by);
			fixed4 c = SampleSpriteTexture(IN.texcoord + xyOffset) * IN.color;
			c.rgb *= c.a;
			return c;
		}
        ENDCG
        }
    }
}