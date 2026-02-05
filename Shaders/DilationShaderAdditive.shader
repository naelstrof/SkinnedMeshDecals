// Made with Amplify Shader Editor v1.9.9.7
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/Naelstrof/DilationShaderAdditive"
{
	Properties
	{
		_MainTex ( "Screen", 2D ) = "black" {}
		_MainTex( "_MainTex", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		

		ZTest Always
		Cull Off
		ZWrite Off

		
		Pass
		{
			CGPROGRAM

			#define ASE_VERSION 19907


			#pragma vertex vert_img_custom
			#pragma fragment frag
			#pragma target 3.5
			#include "UnityCG.cginc"
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0


			struct appdata_img_custom
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				
			};

			struct v2f_img_custom
			{
				float4 pos : SV_POSITION;
				half2 uv   : TEXCOORD0;
				half2 stereoUV : TEXCOORD2;
		#if UNITY_UV_STARTS_AT_TOP
				half4 uv2 : TEXCOORD1;
				half4 stereoUV2 : TEXCOORD3;
		#endif
				
			};

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _MainTex_ST;

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
			


			v2f_img_custom vert_img_custom ( appdata_img_custom v  )
			{
				v2f_img_custom o;
				
				o.pos = UnityObjectToClipPos( v.vertex );
				o.uv = float4( v.texcoord.xy, 1, 1 );

				#if UNITY_UV_STARTS_AT_TOP
					o.uv2 = float4( v.texcoord.xy, 1, 1 );
					o.stereoUV2 = UnityStereoScreenSpaceUVAdjust ( o.uv2, _MainTex_ST );

					if ( _MainTex_TexelSize.y < 0.0 )
						o.uv.y = 1.0 - o.uv.y;
				#endif
				o.stereoUV = UnityStereoScreenSpaceUVAdjust ( o.uv, _MainTex_ST );
				return o;
			}

			half4 frag ( v2f_img_custom i ) : SV_Target
			{
				#ifdef UNITY_UV_STARTS_AT_TOP
					half2 uv = i.uv2;
					half2 stereoUV = i.stereoUV2;
				#else
					half2 uv = i.uv;
					half2 stereoUV = i.stereoUV;
				#endif

				half4 finalColor;

				// ase common template code
				float texelDist3 = 2.0;
				float2 texCoord5 = i.uv.xy * float2( 1,1 ) + float2( 0,0 );
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
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19907
Node;AmplifyShaderEditor.TexelSizeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;6;-621.5676,200.8369;Inherit;False;-1;Fetch;1;0;SAMPLER2D;;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2;-882.0396,19.80602;Inherit;True;Property;_MainTex;_MainTex;0;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;4;-477.2756,-130.8202;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;5;-787.266,-130.405;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;7;-379.5673,264.837;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3;-308.6451,-0.6990496;Inherit;False;float2 offsets[8] = {float2(-texelDist, 0), float2(texelDist,0),$float2(0, texelDist), float2(0,-texelDist),$float2(-texelDist, texelDist), float2(texelDist,texelDist),$float2(texelDist, -texelDist), float2(-texelDist,-texelDist)}@$float4 sample = tex2D(tex, uv)@$float4 sampleMax = sample@$for(int i=0@i<8@i++)$ {$	float2 curUV = uv + offsets[i]*texTexelSize.xy@$	float4 offsetSample = tex2D(tex, curUV)@$	sampleMax = max(offsetSample, sampleMax)@$}$sample = sampleMax@$return sample@;4;Create;4;True;texelDist;FLOAT;1;In;;Inherit;False;True;uv;FLOAT2;0,0;In;;Inherit;False;True;tex;SAMPLER2D;;In;;Inherit;False;True;texTexelSize;FLOAT2;0,0;In;;Inherit;False;DilateSample;True;False;0;;False;4;0;FLOAT;1;False;1;FLOAT2;0,0;False;2;SAMPLER2D;;False;3;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;9;0,0;Float;False;True;-1;3;AmplifyShaderEditor.MaterialInspector;0;16;Hidden/Naelstrof/DilationShaderAdditive;c71b220b631b6344493ea3cf87110c93;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;True;7;False;;False;False;True;0;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;6;0;2;0
WireConnection;7;0;6;1
WireConnection;7;1;6;2
WireConnection;3;0;4;0
WireConnection;3;1;5;0
WireConnection;3;2;2;0
WireConnection;3;3;7;0
WireConnection;9;0;3;0
ASEEND*/
//CHKSM=841EE41AC56AA339C0EEFA9CFDFC96AA076AD918