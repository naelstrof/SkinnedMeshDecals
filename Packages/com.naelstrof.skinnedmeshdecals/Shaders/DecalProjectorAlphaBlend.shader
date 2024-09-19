// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/DecalProjectorAlphaBlend"
{
	Properties
	{
		[Toggle(_BACKFACECULLING_ON)] _BACKFACECULLING("BACKFACECULLING", Float) = 1
		_MainTex("MainTex", 2D) = "white" {}
		[HDR]_Color("Color", Color) = (1,1,1,1)

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
		AlphaToMask Off
		Cull Off
		ColorMask RGBA
		ZWrite Off
		ZTest Always
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			#define ASE_ABSOLUTE_VERTEX_POS 1


			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _BACKFACECULLING_ON


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				float3 ase_normal : NORMAL;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
#endif
				float4 ase_texcoord1 : TEXCOORD1;
			};

			uniform float4 _Color;
			uniform sampler2D _MainTex;

			
			v2f vert ( appdata v )
			{
				v2f o;
				float2 texCoord10_g1 = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 break14_g1 = texCoord10_g1;
				float2 appendResult21_g1 = (float2(break14_g1.x , ( 1.0 - break14_g1.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch23_g1 = texCoord10_g1;
				#else
				float2 staticSwitch23_g1 = appendResult21_g1;
				#endif
				float3 objectToClip33_g1 = UnityObjectToClipPos(v.vertex.xyz).xyz;
				float3 appendResult26_g1 = (float3(staticSwitch23_g1 , objectToClip33_g1.z));
				
				float2 vertexToFrag5_g1 = ( ( (objectToClip33_g1).xy * float2( 0.5,0.5 ) ) + float2( 0.5,0.5 ) );
				o.ase_texcoord1.xy = vertexToFrag5_g1;
				float3 objectToClipDir39_g1 = normalize( mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(v.ase_normal, 0.0))) );
				float dotResult9_g1 = dot( objectToClipDir39_g1 , float3(0,0,1) );
				#ifdef UNITY_UV_STARTS_AT_TOP
				float staticSwitch42_g1 = dotResult9_g1;
				#else
				float staticSwitch42_g1 = -dotResult9_g1;
				#endif
				float vertexToFrag18_g1 = saturate( sign( staticSwitch42_g1 ) );
				o.ase_texcoord1.z = vertexToFrag18_g1;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( ( appendResult26_g1 * float3( 2,-2,1 ) ) + float3( -1,1,0 ) );
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
				float2 vertexToFrag5_g1 = i.ase_texcoord1.xy;
				float2 break6_g1 = vertexToFrag5_g1;
				float2 appendResult13_g1 = (float2(break6_g1.x , ( 1.0 - break6_g1.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch16_g1 = vertexToFrag5_g1;
				#else
				float2 staticSwitch16_g1 = appendResult13_g1;
				#endif
				float4 tex2DNode19_g1 = tex2Dlod( _MainTex, float4( staticSwitch16_g1, 0, 0.0) );
				float vertexToFrag18_g1 = i.ase_texcoord1.z;
				float4 appendResult22_g1 = (float4(1.0 , 1.0 , 1.0 , vertexToFrag18_g1));
				#ifdef _BACKFACECULLING_ON
				float4 staticSwitch28_g1 = ( tex2DNode19_g1 * appendResult22_g1 );
				#else
				float4 staticSwitch28_g1 = tex2DNode19_g1;
				#endif
				
				
				finalColor = ( _Color * saturate( staticSwitch28_g1 ) );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19303
Node;AmplifyShaderEditor.TexturePropertyNode;108;368,-128;Inherit;True;Property;_MainTex;MainTex;2;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.ColorNode;109;654.8534,-317.6597;Inherit;False;Property;_Color;Color;3;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;112;640,-128;Inherit;False;ProjectDecal;0;;1;66b18916e8faf2e499cabbb30e9dd724;0;1;34;SAMPLER2D;0;False;2;COLOR;0;FLOAT3;32
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;976,-224;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;1200,-144;Float;False;True;-1;2;ASEMaterialInspector;100;17;Naelstrof/DecalProjectorAlphaBlend;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;False;True;2;5;False;;10;False;;3;1;False;;10;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;True;True;2;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;2;False;;True;7;False;;True;False;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;0;0;1;True;False;;False;0
WireConnection;112;34;108;0
WireConnection;110;0;109;0
WireConnection;110;1;112;0
WireConnection;42;0;110;0
WireConnection;42;1;112;32
ASEEND*/
//CHKSM=10672539E79ED3A6ED4F3F6E6CDDCE5FA93195BF