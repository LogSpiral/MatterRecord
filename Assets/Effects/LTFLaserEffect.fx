sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float uTime;
float uXFactor;
float4 PSFunction(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float g = tex2D(uImage0, coord * float2(uXFactor, 1.0) + float2(uTime, 0)).x;
	return tex2D(uImage1, float2(g, 0.5)) * color;
}
technique Technique1
{
	pass Laser
	{
		PixelShader = compile ps_3_0 PSFunction();
	}
}