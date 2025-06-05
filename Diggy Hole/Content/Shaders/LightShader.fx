#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif



Texture2D SourceTex;
sampler2D TexSampler = sampler_state
{
    Texture = <SourceTex>;
};

Texture2D MaskTex;
sampler2D MaskSampler = sampler_state
{
    Texture = <MaskTex>;
};



struct ImageData
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
    float2 TextureCoord : TEXCOORD0;
};



float4 LightMask(ImageData input) : COLOR
{
    float4 sourceColour = tex2D(TexSampler, input.TextureCoord);
    float4 maskColour = tex2D(MaskSampler, input.TextureCoord);
	
    return sourceColour * maskColour;
}



technique
{
	pass
	{
		PixelShader = compile PS_SHADERMODEL LightMask();
	}
};