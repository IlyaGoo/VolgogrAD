Shader "Custom/MyShader3" 
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

	  BlendOp Min
	  Tags { "RenderType" = "Transparent" }
      CGPROGRAM
      #pragma surface surf Lambert
      
      struct Input {
          float2 uv_MainTex;
		  fixed4 color;
      };
      
      sampler2D _MainTex;
	  //#include "UnitySprites.cginc"
      
      void surf (Input IN, inout SurfaceOutput o) {
		  fixed4 test = tex2D (_MainTex, IN.uv_MainTex);

		  if(test.a<.001)
                 o.Albedo= 1;
		  else if(test.a<1)
				{
                 o.Albedo=1 - test.a;
				 //o.Alpha = 0;
				 }
		  else
				 o.Albedo = test.rgba * test.a;
		  //o.Alpha = tex2D (_MainTex, IN.uv_MainTex).a;
      }
      ENDCG
	
    }
}