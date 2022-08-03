// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/Naelstrof/DilationShader"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}

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

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float4 DilateSample3( float texelDist, float2 uv, sampler2D tex, float2 texTexelSize )
			{
				float2 offsets[8] = {float2(-texelDist, 0), float2(texelDist,0),
				float2(0, texelDist), float2(0,-texelDist),
				float2(-texelDist, texelDist), float2(texelDist,texelDist),
				float2(texelDist, -texelDist), float2(-texelDist,-texelDist)};
				float4 sample = tex2D(tex, uv);
				float4 sampleMax = sample;
				for(int i=0;i<8;i++)
				 {
					float2 curUV = uv + offsets[i]*texTexelSize.xy;
					float4 offsetSample = tex2D(tex, curUV);
					sampleMax = max(offsetSample, sampleMax);
				}
				sample = sampleMax;
				return sample;
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float texelDist3 = 1.0;
				float2 texCoord5 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv3 = texCoord5;
				sampler2D tex3 = _MainTex;
				float2 appendResult7 = (float2(_MainTex_TexelSize.x , _MainTex_TexelSize.y));
				float2 texTexelSize3 = appendResult7;
				float4 localDilateSample3 = DilateSample3( texelDist3 , uv3 , tex3 , texTexelSize3 );
				
				
				finalColor = localDilateSample3;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18900
182;353;1800;736;1680.494;262.7452;1;True;False
Node;AmplifyShaderEditor.TexturePropertyNode;2;-882.0396,19.80602;Inherit;True;Property;_MainTex;_MainTex;0;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexelSizeNode;6;-621.5676,200.8369;Inherit;False;-1;1;0;SAMPLER2D;;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;4;-477.2756,-130.8202;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-787.266,-130.405;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;7;-379.5673,264.837;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CustomExpressionNode;3;-308.6451,-0.6990496;Inherit;False;float2 offsets[8] = {float2(-texelDist, 0), float2(texelDist,0),$float2(0, texelDist), float2(0,-texelDist),$float2(-texelDist, texelDist), float2(texelDist,texelDist),$float2(texelDist, -texelDist), float2(-texelDist,-texelDist)}@$float4 sample = tex2D(tex, uv)@$float4 sampleMax = sample@$for(int i=0@i<8@i++)$ {$	float2 curUV = uv + offsets[i]*texTexelSize.xy@$	float4 offsetSample = tex2D(tex, curUV)@$	sampleMax = max(offsetSample, sampleMax)@$}$sample = sampleMax@$return sample@;4;False;4;True;texelDist;FLOAT;1;In;;Inherit;False;True;uv;FLOAT2;0,0;In;;Inherit;False;True;tex;SAMPLER2D;;In;;Inherit;False;True;texTexelSize;FLOAT2;0,0;In;;Inherit;False;DilateSample;True;False;0;4;0;FLOAT;1;False;1;FLOAT2;0,0;False;2;SAMPLER2D;;False;3;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;0,0;Float;False;True;-1;2;ASEMaterialInspector;100;1;Hidden/Naelstrof/DilationShader;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;6;0;2;0
WireConnection;7;0;6;1
WireConnection;7;1;6;2
WireConnection;3;0;4;0
WireConnection;3;1;5;0
WireConnection;3;2;2;0
WireConnection;3;3;7;0
WireConnection;1;0;3;0
ASEEND*/
//CHKSM=2B567EDB65B1F0B97425F615BD3460B7850FA431