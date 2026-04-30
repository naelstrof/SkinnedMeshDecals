// Made with Amplify Shader Editor v1.9.9.9
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/DecalProjectorSubtractiveBlend"
{
	Properties
	{
		[Toggle( _BACKFACECULLING_ON )] _BACKFACECULLING( "BACKFACECULLING", Float ) = 1
		_MainTex( "MainTex", 2D ) = "white" {}
		[HDR] _Color1( "Color", Color ) = ( 1, 1, 1, 1 )

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend One One, Zero One
		BlendOp RevSub
		AlphaToMask Off
		Cull Off
		ColorMask RGBA
		ZWrite Off
		ZClip True
		ZTest Always
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define ASE_VERSION 19909


			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#define ASE_NEEDS_TEXTURE_COORDINATES1
			#define ASE_NEEDS_VERT_POSITION
			#pragma multi_compile_local __ _BACKFACECULLING_ON


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
#endif
				float4 ase_texcoord1 : TEXCOORD1;
			};

			uniform float4 _Color1;
			uniform sampler2D _MainTex;

			
			v2f vert ( appdata v )
			{
				v2f o;
				float2 texCoord10_g2 = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 break14_g2 = texCoord10_g2;
				float2 appendResult21_g2 = (float2(break14_g2.x , ( 1.0 - break14_g2.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch23_g2 = texCoord10_g2;
				#else
				float2 staticSwitch23_g2 = appendResult21_g2;
				#endif
				float4 objectToClip33_g2 = UnityObjectToClipPos( v.vertex.xyz );
				float3 appendResult26_g2 = (float3(staticSwitch23_g2 , objectToClip33_g2.z));
				
				float2 vertexToFrag5_g2 = ( ( (objectToClip33_g2).xy * float2( 0.5,0.5 ) ) + float2( 0.5,0.5 ) );
				o.ase_texcoord1.xy = vertexToFrag5_g2;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( ( appendResult26_g2 * float3( 2,-2,1 ) ) + float3( -1,1,0 ) );
				o.vertex = float4(vertexValue.xyz,1);

#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed4 finalColor;
#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
#endif
				float2 vertexToFrag5_g2 = i.ase_texcoord1.xy;
				float2 break6_g2 = vertexToFrag5_g2;
				float2 appendResult13_g2 = (float2(break6_g2.x , ( 1.0 - break6_g2.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch16_g2 = vertexToFrag5_g2;
				#else
				float2 staticSwitch16_g2 = appendResult13_g2;
				#endif
				
				
				finalColor = ( _Color1 * saturate( tex2Dlod( _MainTex, float4( staticSwitch16_g2, 0, 0.0) ) ) * _Color1.a );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19909
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;104;-48,-64;Inherit;True;Property;_MainTex;MainTex;2;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;101;240,-64;Inherit;False;ProjectDecal;0;;2;66b18916e8faf2e499cabbb30e9dd724;0;1;34;SAMPLER2D;0;False;3;FLOAT;43;COLOR;0;FLOAT3;32
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;107;288,-304;Inherit;False;Property;_Color1;Color;3;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;108;800,-256;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;42;1104,-64;Float;False;True;-1;2;AmplifyShaderEditor.MaterialInspector;100;12;Naelstrof/DecalProjectorSubtractiveBlend;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;True;True;4;1;False;;1;False;;1;0;False;;1;False;;True;3;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;2;False;;True;7;False;;True;False;0;False;;0;False;;True;1;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position;0;0;0;1;True;False;;False;0
WireConnection;101;34;104;0
WireConnection;108;0;107;0
WireConnection;108;1;101;0
WireConnection;108;2;107;4
WireConnection;42;0;108;0
WireConnection;42;1;101;32
ASEEND*/
//CHKSM=965DF65CE31AF54ED7F9363F4CF98183C24280B8