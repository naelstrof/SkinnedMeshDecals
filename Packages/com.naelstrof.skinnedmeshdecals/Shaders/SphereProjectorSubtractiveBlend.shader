// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/SphereProjectorSubtractiveBlend"
{
	Properties
	{
		[Toggle(_BACKFACECULLING_ON)] _BACKFACECULLING("BACKFACECULLING", Float) = 1
		_Power("Power", Float) = 1
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
				float4 ase_texcoord2 : TEXCOORD2;
			};

			uniform float4 _Color;
			uniform float _Power;

			
			v2f vert ( appdata v )
			{
				v2f o;
				float2 texCoord14_g1 = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 break17_g1 = texCoord14_g1;
				float2 appendResult24_g1 = (float2(break17_g1.x , ( 1.0 - break17_g1.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch30_g1 = texCoord14_g1;
				#else
				float2 staticSwitch30_g1 = appendResult24_g1;
				#endif
				float4 objectToClip2_g1 = UnityObjectToClipPos(v.vertex.xyz);
				float3 objectToClip2_g1NDC = objectToClip2_g1.xyz/objectToClip2_g1.w;
				float3 appendResult32_g1 = (float3(staticSwitch30_g1 , objectToClip2_g1NDC.z));
				
				float3 objectToClipDir41_g1 = normalize( mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(v.ase_normal, 0.0))) );
				float dotResult44_g1 = dot( objectToClipDir41_g1 , float3(0,0,1) );
				#ifdef UNITY_UV_STARTS_AT_TOP
				float staticSwitch43_g1 = dotResult44_g1;
				#else
				float staticSwitch43_g1 = -dotResult44_g1;
				#endif
				float vertexToFrag26_g1 = saturate( sign( staticSwitch43_g1 ) );
				o.ase_texcoord2.x = vertexToFrag26_g1;
				
				o.ase_texcoord1 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.yzw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( ( appendResult32_g1 * float3( 2,-2,1 ) ) + float3( -1,1,0 ) );
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
				float4 appendResult146 = (float4(_Color.r , _Color.g , _Color.b , 0.0));
				float4 objectToClip2_g1 = UnityObjectToClipPos(i.ase_texcoord1.xyz);
				float3 objectToClip2_g1NDC = objectToClip2_g1.xyz/objectToClip2_g1.w;
				#ifdef UNITY_UV_STARTS_AT_TOP
				float3 staticSwitch9_g1 = ( ( objectToClip2_g1NDC - float3( 0,0,0.5 ) ) * float3(1,1,2) );
				#else
				float3 staticSwitch9_g1 = objectToClip2_g1NDC;
				#endif
				float temp_output_27_0_g1 = saturate( pow( saturate( ( 1.0 - distance( float3(0,0,0) , staticSwitch9_g1 ) ) ) , _Power ) );
				float vertexToFrag26_g1 = i.ase_texcoord2.x;
				#ifdef _BACKFACECULLING_ON
				float staticSwitch33_g1 = ( temp_output_27_0_g1 * vertexToFrag26_g1 );
				#else
				float staticSwitch33_g1 = temp_output_27_0_g1;
				#endif
				float4 lerpResult147 = lerp( appendResult146 , _Color , saturate( staticSwitch33_g1 ));
				
				
				finalColor = lerpResult147;
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
Node;AmplifyShaderEditor.ColorNode;144;464,-288;Inherit;False;Property;_Color;Color;3;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;146;704,-256;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;150;672,-32;Inherit;False;ProjectSphere;0;;1;0210e53a33ec5d2438280b488af95eff;0;0;2;FLOAT;0;FLOAT3;38
Node;AmplifyShaderEditor.LerpOp;147;912,-224;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;1097.151,-111.8234;Float;False;True;-1;2;ASEMaterialInspector;100;16;Naelstrof/SphereProjectorSubtractiveBlend;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;True;True;2;5;False;;10;False;;4;1;False;;1;False;;True;0;False;;3;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;2;False;;True;7;False;;True;False;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;0;0;1;True;False;;False;0
WireConnection;146;0;144;1
WireConnection;146;1;144;2
WireConnection;146;2;144;3
WireConnection;147;0;146;0
WireConnection;147;1;144;0
WireConnection;147;2;150;0
WireConnection;42;0;147;0
WireConnection;42;1;150;38
ASEEND*/
//CHKSM=8A1CFD9EA8140E85A6207B6BE2950F3490429660