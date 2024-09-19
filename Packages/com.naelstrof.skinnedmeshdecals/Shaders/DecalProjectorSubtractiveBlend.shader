// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/DecalProjectorSubtractiveBlend"
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
		Blend SrcAlpha OneMinusSrcAlpha, One One
		BlendOp Add, RevSub
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
				
				float2 vertexToFrag5_g2 = ( (objectToClip33_g2).xy + float2( 0.5,0.5 ) );
				o.ase_texcoord1.xy = vertexToFrag5_g2;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult9_g2 = dot( normalizedWorldNormal , ase_worldViewDir );
				float vertexToFrag18_g2 = saturate( sign( dotResult9_g2 ) );
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
				float4 tex2DNode19_g2 = tex2Dlod( _MainTex, float4( staticSwitch16_g2, 0, 0.0) );
				float vertexToFrag18_g2 = i.ase_texcoord1.z;
				float4 appendResult22_g2 = (float4(1.0 , 1.0 , 1.0 , vertexToFrag18_g2));
				#ifdef _BACKFACECULLING_ON
				float4 staticSwitch28_g2 = ( tex2DNode19_g2 * appendResult22_g2 );
				#else
				float4 staticSwitch28_g2 = tex2DNode19_g2;
				#endif
				
				
				finalColor = ( _Color * saturate( staticSwitch28_g2 ) );
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
Node;AmplifyShaderEditor.TexturePropertyNode;104;256,-64;Inherit;True;Property;_MainTex;MainTex;2;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FunctionNode;101;544,-64;Inherit;False;ProjectDecal;0;;2;66b18916e8faf2e499cabbb30e9dd724;0;1;34;SAMPLER2D;0;False;2;COLOR;0;FLOAT3;32
Node;AmplifyShaderEditor.ColorNode;103;448,-304;Inherit;False;Property;_Color;Color;3;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;102;832,-224;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;1027.263,-90.47208;Float;False;True;-1;2;ASEMaterialInspector;100;16;Naelstrof/DecalProjectorSubtractiveBlend;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;False;True;2;5;False;;10;False;;4;1;False;;1;False;;True;0;False;;3;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;2;False;;True;7;False;;True;False;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;0;0;1;True;False;;False;0
WireConnection;101;34;104;0
WireConnection;102;0;103;0
WireConnection;102;1;101;0
WireConnection;42;0;102;0
WireConnection;42;1;101;32
ASEEND*/
//CHKSM=EE2D3B3F912A3C728C80A7CCCF930330871DB099