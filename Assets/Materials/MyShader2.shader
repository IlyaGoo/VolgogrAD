// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MyShader2" 
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
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
            "RenderType" = "Transparent"
			"IgnoreProjector"="True"
            "Queue" = "Transparent"
			"PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
		ZWrite Off
		Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            fixed4 _Color;
            sampler2D _MainTex;            

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.color = v.color;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {                           
                fixed4 texColor = tex2D(_MainTex, i.uv)*i.color;                                
                return texColor;
            }
            ENDCG
        }

		Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 
		BlendOp Min

		CGPROGRAM
        #pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha	
        #pragma multi_compile_local _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #include "UnitySprites.cginc"

        struct Input
        {
            float2 uv_MainTex;
            fixed4 color;
        };

		
        void vert (inout appdata_full v, out Input o)
        {
            v.vertex = UnityFlipSprite(v.vertex, _Flip);

            #if defined(PIXELSNAP_ON)
            v.vertex = UnityPixelSnap (v.vertex);
            #endif

            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = v.color * _Color * _RendererColor;
        }
		

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = SampleSpriteTexture (IN.uv_MainTex) * IN.color;

			if(c.a<.00001)
				{
                 o.Albedo = 2;
				}
			else if(c.a<1)
				{
                 o.Albedo = 1 - c.a + tex2D (_MainTex, IN.uv_MainTex).rgb * c.a;
				 //o.Alpha = 0;
				 //o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgba;
				}
			else
				 o.Albedo = c.rgba * c.a;

			

            //o.Alpha = c.a;
        }
        ENDCG
    }
}