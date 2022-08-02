// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Naelstrof/DecalProjector"
{
	Properties
	{
		_Decal("Decal", 2D) = "white" {}
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
			uniform sampler2D _Decal;

			
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
				
				float2 vertexToFrag32 = ( (unityObjectToClipPos38).xy + float2( 0.5,0.5 ) );
				o.ase_texcoord1.xy = vertexToFrag32;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult43 = dot( normalizedWorldNormal , ase_worldViewDir );
				float vertexToFrag49 = saturate( sign( dotResult43 ) );
				o.ase_texcoord1.z = vertexToFrag49;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
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
				float2 vertexToFrag32 = i.ase_texcoord1.xy;
				float2 break96 = vertexToFrag32;
				float2 appendResult98 = (float2(break96.x , ( 1.0 - break96.y )));
				#ifdef UNITY_UV_STARTS_AT_TOP
				float2 staticSwitch99 = vertexToFrag32;
				#else
				float2 staticSwitch99 = appendResult98;
				#endif
				float4 temp_output_40_0 = ( _Color * tex2Dlod( _Decal, float4( staticSwitch99, 0, 0.0) ) );
				float vertexToFrag49 = i.ase_texcoord1.z;
				float4 appendResult87 = (float4(1.0 , 1.0 , 1.0 , vertexToFrag49));
				#ifdef _BACKFACECULLING_ON
				float4 staticSwitch86 = ( temp_output_40_0 * appendResult87 );
				#else
				float4 staticSwitch86 = temp_output_40_0;
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
182;353;1800;736;1785.618;626.2747;1.969968;True;False
Node;AmplifyShaderEditor.PosVertexDataNode;37;-1503.841,-313.4268;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.UnityObjToClipPosHlpNode;38;-1233.169,-320.4389;Inherit;False;1;0;FLOAT3;0,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;83;-1033.156,-417.5779;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-866.5571,-409.0598;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexToFragmentNode;32;-721.7673,-404.4714;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;96;-1096.241,-815.1355;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;64;-658.556,-47.81969;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;66;-694.7673,-194.7486;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;43;-479.7516,-79.70787;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-1125.219,218.2467;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;97;-954.2413,-758.1356;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SignOpNode;91;-316.4309,-82.04448;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;98;-729.2418,-789.1357;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;101;-804.4938,480.4535;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.OneMinusNode;102;-662.494,537.4532;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;99;-548.9712,-820.4207;Inherit;False;Property;UNITY_UV_STARTS_AT_TOP;UNITY_UV_STARTS_AT_TOP;3;0;Create;False;0;0;0;False;0;False;0;0;0;False;UNITY_UV_STARTS_AT_TOP;Toggle;2;Key0;Key1;Fetch;False;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;89;-197.0923,-83.74127;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;49;4.79973,-84.86951;Inherit;False;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;18;-416.8339,-420.1366;Inherit;True;Property;_Decal;Decal;0;0;Create;True;0;0;0;False;0;False;-1;None;3207072c6b926894ca9955969b0ce8f0;True;0;False;white;Auto;False;Object;-1;MipLevel;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;39;-189.8542,-673.1726;Inherit;False;Property;_Color;Color;1;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;103;-437.494,506.4532;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;87;259.4811,-161.6906;Inherit;False;FLOAT4;4;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;100;-260.9267,476.4027;Inherit;False;Property;UNITY_UV_STARTS_AT_TOP1;UNITY_UV_STARTS_AT_TOP;3;0;Create;False;0;0;0;False;0;False;0;0;0;False;UNITY_UV_STARTS_AT_TOP;Toggle;2;Key0;Key1;Fetch;False;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;85;-945.6664,-153.8007;Inherit;False;FLOAT2;2;3;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;347.4732,-514.1842;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;8;-55.76405,138.1129;Inherit;False;FLOAT4;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;420.681,-221.4907;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;86;688.39,-537.597;Inherit;False;Property;_BACKFACECULLING;BACKFACECULLING;2;0;Create;True;0;0;0;True;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;136.6073,136.7019;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;2,-2,1,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;10;304.0794,137.444;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;-1,1,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;104;892.4716,-141.6931;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;1090.151,-127.8234;Float;False;True;-1;2;ASEMaterialInspector;100;18;Naelstrof/DecalProjector;928f6a5fbd2e6444ea9bb91fa46f1aa9;True;Unlit;0;0;Unlit;2;False;True;2;5;False;-1;10;False;-1;3;1;False;-1;10;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;2;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;7;False;-1;True;False;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;0;0;1;True;False;;False;0
WireConnection;38;0;37;0
WireConnection;83;0;38;0
WireConnection;34;0;83;0
WireConnection;32;0;34;0
WireConnection;96;0;32;0
WireConnection;43;0;66;0
WireConnection;43;1;64;0
WireConnection;97;0;96;1
WireConnection;91;0;43;0
WireConnection;98;0;96;0
WireConnection;98;1;97;0
WireConnection;101;0;7;0
WireConnection;102;0;101;1
WireConnection;99;1;98;0
WireConnection;99;0;32;0
WireConnection;89;0;91;0
WireConnection;49;0;89;0
WireConnection;18;1;99;0
WireConnection;103;0;101;0
WireConnection;103;1;102;0
WireConnection;87;3;49;0
WireConnection;100;1;103;0
WireConnection;100;0;7;0
WireConnection;85;0;38;0
WireConnection;40;0;39;0
WireConnection;40;1;18;0
WireConnection;8;0;100;0
WireConnection;8;2;85;0
WireConnection;90;0;40;0
WireConnection;90;1;87;0
WireConnection;86;1;40;0
WireConnection;86;0;90;0
WireConnection;9;0;8;0
WireConnection;10;0;9;0
WireConnection;104;0;86;0
WireConnection;42;0;104;0
WireConnection;42;1;10;0
ASEEND*/
//CHKSM=A7B420DF07750C5DEB164A6505083BA555BDAFFF