// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/SphereProjectorAlphaBlend"
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
				float4 ase_texcoord2 : TEXCOORD2;
			};

			uniform float4 _Color;
			uniform float _Power;

			
			v2f vert ( appdata v )
			{
				v2f o;
				float2 texCoord14_g5 = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 break17_g5 = texCoord14_g5;
				float2 appendResult24_g5 = (float2(break17_g5.x , ( 1.0 - break17_g5.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch30_g5 = texCoord14_g5;
				#else
				float2 staticSwitch30_g5 = appendResult24_g5;
				#endif
				float4 objectToClip2_g5 = UnityObjectToClipPos(v.vertex.xyz);
				float3 objectToClip2_g5NDC = objectToClip2_g5.xyz/objectToClip2_g5.w;
				float3 appendResult32_g5 = (float3(staticSwitch30_g5 , objectToClip2_g5NDC.z));
				
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult13_g5 = dot( normalizedWorldNormal , ase_worldViewDir );
				float vertexToFrag26_g5 = saturate( sign( dotResult13_g5 ) );
				o.ase_texcoord2.x = vertexToFrag26_g5;
				
				o.ase_texcoord1 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.yzw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( ( appendResult32_g5 * float3( 2,-2,1 ) ) + float3( -1,1,0 ) );
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
				float4 appendResult149 = (float4(_Color.r , _Color.g , _Color.b , 0.0));
				float4 objectToClip2_g5 = UnityObjectToClipPos(i.ase_texcoord1.xyz);
				float3 objectToClip2_g5NDC = objectToClip2_g5.xyz/objectToClip2_g5.w;
				#ifdef UNITY_UV_STARTS_AT_TOP
				float3 staticSwitch9_g5 = ( ( objectToClip2_g5NDC - float3( 0,0,0.5 ) ) * float3(0.5,0.5,2) );
				#else
				float3 staticSwitch9_g5 = ( objectToClip2_g5NDC * float3(0.5,0.5,1) );
				#endif
				float temp_output_27_0_g5 = saturate( pow( saturate( ( 1.0 - distance( float3(0,0,0) , staticSwitch9_g5 ) ) ) , _Power ) );
				float vertexToFrag26_g5 = i.ase_texcoord2.x;
				#ifdef _BACKFACECULLING_ON
				float staticSwitch33_g5 = ( temp_output_27_0_g5 * vertexToFrag26_g5 );
				#else
				float staticSwitch33_g5 = temp_output_27_0_g5;
				#endif
				float4 lerpResult148 = lerp( appendResult149 , _Color , saturate( staticSwitch33_g5 ));
				
				
				finalColor = lerpResult148;
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
Node;AmplifyShaderEditor.ColorNode;145;464,-320;Inherit;False;Property;_Color;Color;3;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;149;720,-336;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;150;688,-80;Inherit;False;ProjectSphere;0;;5;0210e53a33ec5d2438280b488af95eff;0;0;2;FLOAT;0;FLOAT3;38
Node;AmplifyShaderEditor.LerpOp;148;912,-272;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;1097.151,-111.8234;Float;False;True;-1;2;ASEMaterialInspector;100;16;Naelstrof/SphereProjectorAlphaBlend;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;False;True;2;5;False;;10;False;;3;1;False;;10;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;2;False;;True;7;False;;True;False;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;0;0;1;True;False;;False;0
WireConnection;149;0;145;1
WireConnection;149;1;145;2
WireConnection;149;2;145;3
WireConnection;148;0;149;0
WireConnection;148;1;145;0
WireConnection;148;2;150;0
WireConnection;42;0;148;0
WireConnection;42;1;150;38
ASEEND*/
//CHKSM=051D9B70E1339DD6CB40828F03C7702B263A73F5