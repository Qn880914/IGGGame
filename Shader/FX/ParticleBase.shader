Shader "Zero/FX/ParticleBase" {
Properties {
    _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
    _MainTex ("Main Texture", 2D) = "white" {}	
	[Header(AlphaBlendMode)] //Zero = 0,One = 1,DstColor = 2,SrcColor = 3,OneMinusDstColor = 4,SrcAlpha = 5,OneMinusSrcColor = 6,DstAlpha = 7,OneMinusDstAlpha = 8,SrcAlphaSaturate = 9,OneMinusSrcAlpha = 10
	[Enum(One,1,SrcAlpha,5)]  _SrcBlend("SrcFactor",Float) = 5
	[Enum(One,1,OneMinusSrcAlpha,10)]  _DstBlend("DstFactor",Float) = 1
	//[Header(Additive(SrcAlpha.One))][Header(AlphaBlend(SrcAlpha.OneMinusSrcAlpha))][Header(Transparent(One.OneMinusSrcAlpha))][Header(Opaque(One.Zero))][Header(AdditiveSoft(One.OneMinusSrcColor))]
	[Header(RenderState)]
	[Enum(RGB,14,RGBA,15)] _ColorMask("Color Mask", Float) = 14 //Alpha = 1,Blue = 2,Green = 4,Red = 8,All = 15
	[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode",Float) = 0
	[Enum(Off,0,On,1)] _Zwrite("Zwrite", Float) = 0
	[Enum(Off,0,On,2)] _Ztest("Ztest", Float) = 2
	[KeywordEnum(No,Fade,Color)] _FOG("Fog", Float) = 0
	[Toggle] _RGBXA("RGB x A", Float) = 0
	[Header(Stencil)][IntRange] _Stencil("Stencil ID", Range(0,8)) = 0 
	[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison",Float) = 0 //Disabled = 0,Never = 1,Less = 2,Equal = 3,LessEqual = 4,Greater = 5,NotEqual = 6,GreaterEqual = 7,Always = 8		
	[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp("Stencil Option",Float) = 0 //Keep = 0,Zero = 1,Replace = 2,IncrementSaturate = 3,DecrementSaturate = 4,Invert = 5,IncrementWrap = 6,DecrementWrap = 7
	_StencilWriteMask("Stencil Write Mask", Float) = 255
	_StencilReadMask("Stencil Read Mask", Float) = 255
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Blend [_SrcBlend] [_DstBlend]
    ColorMask [_ColorMask]
    Cull [_Cull] Lighting Off ZWrite [_Zwrite] ZTest[_Ztest]

	Stencil
	{
		Ref[_Stencil]
		Comp[_StencilComp]
		Pass[_StencilOp]
		ReadMask[_StencilReadMask]
		WriteMask[_StencilWriteMask]
	}

    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog
			#pragma multi_compile _FOG_NO _FOG_FADE _FOG_COLOR
			#pragma multi_compile __ _RGBXA_ON
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _TintColor;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                fixed2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
				float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                fixed2 texcoord : TEXCOORD0;
#ifdef _FOG_FADE
                UNITY_FOG_COORDS(1)
#elif _FOG_COLOR
				UNITY_FOG_COORDS(1)
#endif
            };

            fixed4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
#ifdef _FOG_FADE
                UNITY_TRANSFER_FOG(o,o.vertex);
#elif _FOG_COLOR
				UNITY_TRANSFER_FOG(o,o.vertex);
#endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
				col *= 2.0f * i.color * _TintColor;
#ifdef _RGBXA_ON
				col.rgb *= col.a;
#endif
				col = saturate(col);
#ifdef _FOG_FADE
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0));
#elif _FOG_COLOR
				UNITY_APPLY_FOG(i.fogCoord, col);
#endif
                return col;
            }
            ENDCG
        }
    }
}
}
