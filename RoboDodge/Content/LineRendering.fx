float4x4 worldViewProj : WorldViewProjection;

struct VertexInput
{
	float3 pos   : POSITION;
	float4 color : COLOR;
};

struct VertexOutput 
{
   float4 pos   : POSITION;
   float4 color : COLOR;
};

VertexOutput LineRenderingVS(VertexInput In)
{
	VertexOutput Out;

	Out.pos = mul(float4(In.pos, 1), worldViewProj);
	Out.color = In.color;
	
	return Out;
} 

float4 LineRenderingPS(VertexOutput In) : Color
{
	return In.color;
} 

technique LineRendering3D
{
	pass pass1
	{
		VertexShader = compile vs_1_1 LineRenderingVS();
		PixelShader = compile ps_1_1 LineRenderingPS();
	} 
}
