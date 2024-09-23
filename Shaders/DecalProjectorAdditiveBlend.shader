// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/DecalProjectorAdditiveBlend"
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
		Blend One One
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
			#pragma multi_compile_local __ _BACKFACECULLING_ON


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
				float2 texCoord10_g2 = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 break14_g2 = texCoord10_g2;
				float2 appendResult21_g2 = (float2(break14_g2.x , ( 1.0 - break14_g2.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch23_g2 = texCoord10_g2;
				#else
				float2 staticSwitch23_g2 = appendResult21_g2;
				#endif
				float3 objectToClip33_g2 = UnityObjectToClipPos(v.vertex.xyz).xyz;
				float3 appendResult26_g2 = (float3(staticSwitch23_g2 , objectToClip33_g2.z));
				
				float2 vertexToFrag5_g2 = ( ( (objectToClip33_g2).xy * float2( 0.5,0.5 ) ) + float2( 0.5,0.5 ) );
				o.ase_texcoord1.xy = vertexToFrag5_g2;
				float3 objectToClipDir39_g2 = normalize( mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(v.ase_normal, 0.0))) );
				float dotResult9_g2 = dot( objectToClipDir39_g2 , float3(0,0,1) );
				#ifdef UNITY_UV_STARTS_AT_TOP
				float staticSwitch42_g2 = dotResult9_g2;
				#else
				float staticSwitch42_g2 = -dotResult9_g2;
				#endif
				float vertexToFrag18_g2 = saturate( sign( staticSwitch42_g2 ) );
				o.ase_texcoord1.z = vertexToFrag18_g2;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
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
				float vertexToFrag18_g2 = i.ase_texcoord1.z;
				#ifdef _BACKFACECULLING_ON
				float staticSwitch28_g2 = vertexToFrag18_g2;
				#else
				float staticSwitch28_g2 = 1.0;
				#endif
				
				
				finalColor = ( _Color * saturate( tex2Dlod( _MainTex, float4( staticSwitch16_g2, 0, 0.0) ) ) * staticSwitch28_g2 );
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
Node;AmplifyShaderEditor.FunctionNode;114;640,-128;Inherit;False;ProjectDecal;0;;2;66b18916e8faf2e499cabbb30e9dd724;0;1;34;SAMPLER2D;0;False;3;FLOAT;43;COLOR;0;FLOAT3;32
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;976,-288;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;1200,-144;Float;False;True;-1;2;ASEMaterialInspector;100;16;Naelstrof/DecalProjectorAdditiveBlend;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;True;True;4;1;False;;1;False;;0;1;False;;10;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;True;True;2;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;2;False;;True;7;False;;True;False;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;0;0;1;True;False;;False;0
WireConnection;114;34;108;0
WireConnection;110;0;109;0
WireConnection;110;1;114;0
WireConnection;110;2;114;43
WireConnection;42;0;110;0
WireConnection;42;1;114;32
ASEEND*/
//CHKSM=98DFEC786F5F92B5C595B9508E09A830CBE29EF0