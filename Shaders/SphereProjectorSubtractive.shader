// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/SphereProjectorSubtractive"
{
	Properties
	{
		[HDR]_Color("Color", Color) = (1,1,1,1)
		[Toggle(_BACKFACECULLING_ON)] _BACKFACECULLING("BACKFACECULLING", Float) = 1

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
				float4 ase_texcoord2 : TEXCOORD2;
			};

			uniform float4 _Color;

			
			v2f vert ( appdata v )
			{
				v2f o;
				float2 texCoord7 = v.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 break101 = texCoord7;
				float2 appendResult103 = (float2(break101.x , ( 1.0 - break101.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch100 = texCoord7;
				#else
				float2 staticSwitch100 = appendResult103;
				#endif
				float4 unityObjectToClipPos38 = UnityObjectToClipPos( v.vertex.xyz );
				float4 appendResult8 = (float4(staticSwitch100 , (unityObjectToClipPos38).zw));
				
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult43 = dot( normalizedWorldNormal , ase_worldViewDir );
				float vertexToFrag49 = saturate( sign( dotResult43 ) );
				o.ase_texcoord2.x = vertexToFrag49;
				
				o.ase_texcoord1 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.yzw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( ( appendResult8 * float4( 2,-2,1,0 ) ) + float4( -1,1,0,0 ) ).xyz;
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
				float3 appendResult115 = (float3(0.0 , 0.0 , 0.5));
				float4 unityObjectToClipPos38 = UnityObjectToClipPos( i.ase_texcoord1.xyz );
				float temp_output_3_0_g1 = ( ( 1.0 - ( distance( appendResult115 , ( (unityObjectToClipPos38).xyz * float3(0.5,0.5,1) ) ) * 2.0 ) ) - 0.0 );
				float4 lerpResult111 = lerp( ( _Color * float4( 1,1,1,0 ) ) , _Color , saturate( ( temp_output_3_0_g1 / fwidth( temp_output_3_0_g1 ) ) ));
				float vertexToFrag49 = i.ase_texcoord2.x;
				float4 appendResult87 = (float4(1.0 , 1.0 , 1.0 , vertexToFrag49));
				#ifdef _BACKFACECULLING_ON
				float4 staticSwitch86 = ( lerpResult111 * appendResult87 );
				#else
				float4 staticSwitch86 = lerpResult111;
				#endif
				
				
				finalColor = saturate( staticSwitch86 );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18900
182;353;1800;736;249.267;638.3806;1.138827;True;False
Node;AmplifyShaderEditor.PosVertexDataNode;37;-1503.841,-313.4268;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.UnityObjToClipPosHlpNode;38;-1233.169,-320.4389;Inherit;False;1;0;FLOAT3;0,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;114;-1063.359,-476.4157;Inherit;False;Constant;_Vector1;Vector 0;1;0;Create;True;0;0;0;False;0;False;0.5,0.5,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SwizzleNode;113;-1117.853,-567.7182;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;116;-867.3853,-510.578;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;115;-952.735,-713.2866;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;64;-682.5044,-56.14828;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;66;-718.7157,-203.077;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-605.219,227.2467;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DistanceOpNode;117;-729.7483,-538.3726;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;43;-514.2676,-136.6484;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SignOpNode;91;-322.1364,-130.7534;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;101;-804.4938,480.4535;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-552.6143,-507.0725;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;39;-403.8542,-721.1726;Inherit;False;Property;_Color;Color;0;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;102;-662.494,537.4532;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;109;-389.6143,-485.0726;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;89;-182.2188,-125.5905;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;49;4.79973,-84.86951;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;112;-162.8562,-466.4956;Inherit;False;Step Antialiasing;-1;;1;2a825e80dfb3290468194f83380797bd;0;2;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-59.52679,-688.1842;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;103;-437.494,506.4532;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;100;-260.9267,476.4027;Inherit;False;Property;UNITY_UV_STARTS_AT_TOP1;UNITY_UV_STARTS_AT_TOP;3;0;Create;False;0;0;0;False;0;False;0;0;0;False;UNITY_UV_STARTS_AT_TOP;Toggle;2;Key0;Key1;Fetch;False;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;87;259.4811,-161.6906;Inherit;False;FLOAT4;4;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SwizzleNode;85;-945.6664,-153.8007;Inherit;False;FLOAT2;2;3;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;111;166.8212,-436.2725;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;407.681,-323.4907;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;8;-55.76405,138.1129;Inherit;False;FLOAT4;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;86;696.9604,-399.0311;Inherit;False;Property;_BACKFACECULLING;BACKFACECULLING;1;0;Create;True;0;0;0;True;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;136.6073,136.7019;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;2,-2,1,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;104;892.4716,-141.6931;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;10;304.0794,137.444;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;-1,1,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;1097.151,-111.8234;Float;False;True;-1;2;ASEMaterialInspector;100;18;Naelstrof/SphereProjectorSubtractive;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;True;True;2;5;False;-1;10;False;-1;4;1;False;-1;1;False;-1;True;0;False;-1;3;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;2;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;7;False;-1;True;False;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;0;1;True;False;;False;0
WireConnection;38;0;37;0
WireConnection;113;0;38;0
WireConnection;116;0;113;0
WireConnection;116;1;114;0
WireConnection;117;0;115;0
WireConnection;117;1;116;0
WireConnection;43;0;66;0
WireConnection;43;1;64;0
WireConnection;91;0;43;0
WireConnection;101;0;7;0
WireConnection;110;0;117;0
WireConnection;102;0;101;1
WireConnection;109;0;110;0
WireConnection;89;0;91;0
WireConnection;49;0;89;0
WireConnection;112;2;109;0
WireConnection;40;0;39;0
WireConnection;103;0;101;0
WireConnection;103;1;102;0
WireConnection;100;1;103;0
WireConnection;100;0;7;0
WireConnection;87;3;49;0
WireConnection;85;0;38;0
WireConnection;111;0;40;0
WireConnection;111;1;39;0
WireConnection;111;2;112;0
WireConnection;90;0;111;0
WireConnection;90;1;87;0
WireConnection;8;0;100;0
WireConnection;8;2;85;0
WireConnection;86;1;111;0
WireConnection;86;0;90;0
WireConnection;9;0;8;0
WireConnection;104;0;86;0
WireConnection;10;0;9;0
WireConnection;42;0;104;0
WireConnection;42;1;10;0
ASEEND*/
//CHKSM=85675549B66A1DB8E75F4A4CB2A2E4A2626A7F20