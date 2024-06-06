Shader "UI/Hologram"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
		[Header(Hologram properties)]
    	_GrainStrength ("Grain Strength", float) = 10.
    	_LinesSpeed ("Lines Speed", float) = 20.
    	_LinesStrength ("Lines Strength", float) = 400.
    	_ShadowSpeed ("Shadow Speed", float) = 0.2
    	_ShadowRange ("Shadow Range", float) = 0.05
    	_ShadowCount ("Shadow Count", float) = 3.
    	_HighlightSpeed ("Highlight Speed", float) = 0.2
    	_HighlightRange ("Highlight Range", float) = 0.1
    	_HighlightCount ("Highlight Count", float) = 5.
    	
    	
		[Header(Standart properties)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
        Name "Default"
	    CGPROGRAM
	        #pragma vertex vert
	        #pragma fragment frag
	        #pragma target 2.0

	        #include "UnityCG.cginc"
	        #include "UnityUI.cginc"

	        #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
	        #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
	            fixed4 color    : COLOR;
	            float2 texcoord  : TEXCOORD0;
	            float4 worldPosition : TEXCOORD1;
	            UNITY_VERTEX_OUTPUT_STEREO
	        };

	        sampler2D _MainTex;
	        sampler2D _NoiseTex;
	        fixed4 _Color;
	        fixed4 _TextureSampleAdd;
	        float4 _ClipRect;
	        float4 _MainTex_ST;

			float _GrainStrength;
			float _LinesSpeed;
			float _LinesStrength;
			float _ShadowSpeed;
			float _ShadowRange;
			float _ShadowCount;
			float _HighlightSpeed;
			float _HighlightRange;
			float _HighlightCount;
	    

	        v2f vert(appdata_t v)
	        {
	            v2f OUT;
	            UNITY_SETUP_INSTANCE_ID(v);
	            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
	            OUT.worldPosition = v.vertex;
	            OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

	            OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

	            OUT.color = v.color * _Color;
	            return OUT;
	        }

			const float intensity = 0.35;

			//tex2Ds:
			float3 tx1(float2 uv)
			{
				return tex2D(_NoiseTex,uv).rgb;
			}

			float3 tx1(float u)
			{
				return tex2D(_NoiseTex,float2(u,0.0)).rgb;
			} 

			//geometry:
			float sph(float2 pos, float2 xy, float radius)
			{
				return 1.0 / (length(xy - pos) / radius);
			}

			//math:
			float sum(float3 c)
			{
				return (c.x + c.y + c.z);
			}

			float rand(float v)// 0 .. 1
			{
				return sum(tx1(v)) * 0.6666666666;
			}

			float noise(float2 p)
			{
				float noise = tex2D(_NoiseTex,float2(1.,2.*cos(_Time.y))*_Time.y*8. + p*1.).x;
				noise *= noise;
				return noise;
			}
			float onOff(float a, float b, float c)
			{
				return step(c, sin(_Time.y + a*cos(_Time.y*b)));
			}

			fixed4 getVideo(float2 uv)
			{
				float2 look = uv;
				float window = 1./(1.+20.*(look.y-fmod(_Time.y/4.,1.))*(look.y-fmod(_Time.y/4.,1.)));
				look.x = look.x + sin(look.y*10. + _Time.y)/100.*onOff(4.,4.,.3)*(1.+cos(_Time.y*80.))*window;
				float vShift = 0;//0.1*onOff(2.,3.,.9)*(sin(_Time.y)*sin(_Time.y*20.) + (0.5 + 0.1*sin(_Time.y*200.)*cos(_Time.y)));
				look.y = fmod(look.y + vShift, 1.);
				fixed4 video = (tex2D(_MainTex, look) + _TextureSampleAdd);
			        
				return video;
			}

	        fixed4 frag(v2f IN) : SV_Target
	        {
	            half4 color = getVideo(IN.texcoord) * IN.color;//;

	            #ifdef UNITY_UI_CLIP_RECT
	            color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
	            #endif

	            #ifdef UNITY_UI_ALPHACLIP
	            clip (color.a - 0.001);
	            #endif
	            
	            float t = _Time.y;
	            float x = (IN.texcoord.x + 4.0) * (IN.texcoord.y + 4.0) * t * 10.0;
	            float grain = 1.0 - (fmod((fmod(x, 13.0) + 1.0) * (fmod(x, 123.0) + 1.0), 0.01) - 0.005) * _GrainStrength;
	            float random = rand(_Time.y);
	            float flicker = max(1., random * 1.5);
	            float scanlines = clamp(sin(IN.texcoord.y *_LinesStrength + t*_LinesSpeed), 0.85, 1.0);
	            
	            float shadow = 1.0 - _ShadowRange + (_ShadowRange * sin((IN.texcoord.y + (_Time.y	 * _ShadowSpeed)) * _ShadowCount));
	            color *= shadow;
	            float highlight = 1.0 - _HighlightRange + (_HighlightRange * sin((IN.texcoord.y + (_Time.y * -_HighlightSpeed)) * _HighlightCount));
	            color.rgb += highlight * .5;
	            
	            color *= flicker * grain * scanlines;

	            return color;
	        }
        ENDCG
        }
    }
}