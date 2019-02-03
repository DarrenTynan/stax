Shader "Custom/1_FlatCustom"
{
	Properties
	{
		_Color ("Color", Color) = (1,0,1,0)
	}
	SubShader {
		Pass
		{
			CGPROGRAM

			// Pragmas.
			#pragma vertex vert
			#pragma fragment frag

			// User defined variables.
			float4 _Color;

			// Base input struct.
			struct vertexInput
			{
				float4 vertex : POSITION;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
			};

			// Vertex functions.
			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

				return o;
			}

			// Fragment functions.

			float4 frag(vertexOutput i) : COLOR
			{
				return _Color;
			}

			ENDCG
		}
	}
	// Fallback commented out during development.
//	Fallback "Diffuse"
}
