#version 450
layout(location=0) out vec4 FragColor;

layout(location =0) in vec4 vertColor;
layout(location =1) flat in vec2 DiameterAlpha;

layout(std140,set=1,binding=0) uniform Config
{
   float time;
   float seed;
   int res_x;
   int res_y;
   int mpos_x;
   int mpos_y;
   int PalletSize;
};
layout(set=1,binding=1) uniform texture2D PalletTexture;
layout(set=1,binding=2) uniform sampler PalletSampler;

void main()
{
	
	FragColor=vertColor;
}