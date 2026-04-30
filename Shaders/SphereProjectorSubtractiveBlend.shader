// Made with Amplify Shader Editor v1.9.9.9
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/SphereProjectorSubtractiveBlend"
{
	Properties
	{
		[Toggle( _BACKFACECULLING_ON )] _BACKFACECULLING( "BACKFACECULLING", Float ) = 1
		[HDR] _Color( "Color", Color ) = ( 1, 1, 1, 1 )

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
				
			};

			uniform float4 _Color;

			
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
				float4 objectToClip2_g1 = UnityObjectToClipPos( v.vertex.xyz );
				float4 objectToClip2_g1NDC = objectToClip2_g1/objectToClip2_g1.w;
				float3 appendResult32_g1 = (float3(staticSwitch30_g1 , objectToClip2_g1NDC.z));
				
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
				float4 temp_output_151_0 = ( _Color * _Color.a );
				
				
				finalColor = temp_output_151_0;
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
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;144;352,-304;Inherit;False;Property;_Color;Color;3;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;146;608,-256;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;150;672,-32;Inherit;False;ProjectDecalSphere;0;;1;0210e53a33ec5d2438280b488af95eff;0;0;2;FLOAT;0;FLOAT3;38
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;151;784,-320;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;147;944,-272;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;42;1097.151,-111.8234;Float;False;True;-1;2;AmplifyShaderEditor.MaterialInspector;100;12;Naelstrof/SphereProjectorSubtractiveBlend;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;True;True;4;1;False;;1;False;;1;0;False;;1;False;;True;3;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;2;False;;True;7;False;;True;False;0;False;;0;False;;True;1;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position;0;0;0;1;True;False;;False;0
WireConnection;146;0;144;1
WireConnection;146;1;144;2
WireConnection;146;2;144;3
WireConnection;151;0;144;0
WireConnection;151;1;144;4
WireConnection;147;0;151;0
WireConnection;147;1;144;0
WireConnection;147;2;150;0
WireConnection;42;0;151;0
WireConnection;42;1;150;38
ASEEND*/
//CHKSM=2D2E2424A2CDA4C53E86EBB32B4B7AB7F7E7E508