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
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

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
				float2 texCoord14_g2 = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 break17_g2 = texCoord14_g2;
				float2 appendResult24_g2 = (float2(break17_g2.x , ( 1.0 - break17_g2.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch30_g2 = texCoord14_g2;
				#else
				float2 staticSwitch30_g2 = appendResult24_g2;
				#endif
				float4 objectToClip2_g2 = UnityObjectToClipPos(v.vertex.xyz);
				float4 objectToClip2_g2NDC = objectToClip2_g2.xyz/objectToClip2_g2.w;
				float3 appendResult32_g2 = (float3(staticSwitch30_g2 , objectToClip2_g2NDC.z));
				
				float3 objectToClipDir41_g2 = normalize( mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(v.ase_normal, 0.0))) );
				float dotResult44_g2 = dot( objectToClipDir41_g2 , float3(0,0,1) );
				#ifdef UNITY_UV_STARTS_AT_TOP
				float staticSwitch43_g2 = dotResult44_g2;
				#else
				float staticSwitch43_g2 = -dotResult44_g2;
				#endif
				float vertexToFrag26_g2 = saturate( sign( staticSwitch43_g2 ) );
				o.ase_texcoord2.x = vertexToFrag26_g2;
				
				o.ase_texcoord1 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.yzw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( ( appendResult32_g2 * float3( 2,-2,1 ) ) + float3( -1,1,0 ) );
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
				float4 objectToClip2_g2 = UnityObjectToClipPos(i.ase_texcoord1.xyz);
				float4 objectToClip2_g2NDC = objectToClip2_g2.xyz/objectToClip2_g2.w;
				#ifdef UNITY_UV_STARTS_AT_TOP
				float4 staticSwitch9_g2 = ( ( objectToClip2_g2NDC - float4( 0,0,0.5,0 ) ) * float4( float3(1,1,2) , 0.0 ) );
				#else
				float4 staticSwitch9_g2 = objectToClip2_g2NDC;
				#endif
				float temp_output_27_0_g2 = saturate( pow( saturate( ( 1.0 - distance( float4( float3(0,0,0) , 0.0 ) , staticSwitch9_g2 ) ) ) , _Power ) );
				float vertexToFrag26_g2 = i.ase_texcoord2.x;
				#ifdef _BACKFACECULLING_ON
				float staticSwitch33_g2 = ( temp_output_27_0_g2 * vertexToFrag26_g2 );
				#else
				float staticSwitch33_g2 = temp_output_27_0_g2;
				#endif
				float4 lerpResult148 = lerp( appendResult149 , _Color , saturate( staticSwitch33_g2 ));
				
				
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
Node;AmplifyShaderEditor.FunctionNode;155;656,-96;Inherit;False;ProjectDecalSphere;0;;2;0210e53a33ec5d2438280b488af95eff;0;0;2;FLOAT;0;FLOAT3;38
Node;AmplifyShaderEditor.LerpOp;148;912,-272;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;156;1104,-240;Float;False;True;-1;2;ASEMaterialInspector;100;16;Naelstrof/SphereProjectorAlphaBlend;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
WireConnection;149;0;145;1
WireConnection;149;1;145;2
WireConnection;149;2;145;3
WireConnection;148;0;149;0
WireConnection;148;1;145;0
WireConnection;148;2;155;0
WireConnection;156;0;148;0
WireConnection;156;1;155;38
ASEEND*/
//CHKSM=A222440684FD27C424DBB6554436A9E5D2606BC6