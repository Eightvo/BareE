#version 450
layout(location =0) out vec4 vertColor;
layout(location =1) flat out vec2 DiameterAlpha;


layout (location=0) in vec2 pos;
layout (location=1) in vec3 DiameterAlphaHeight;
layout (location=2) in int  colorIndex;

layout(std140, set=0, binding=0) uniform ModelProjection
{
	mat4 ModelProjectionMatrix;
};
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
vec2 GetNDC(vec2 ipos){return ipos;}

void main()
{
	gl_Position = ModelProjectionMatrix*vec4(pos,DiameterAlphaHeight.z,1);
	
	float cI = float(colorIndex);
	float pS = float(PalletSize);
    vec4 pColor = texture(sampler2D(PalletTexture,PalletSampler), vec2(cI/pS,0.0));
	vertColor=vec4(pColor.rgb,DiameterAlpha.y);	
	DiameterAlpha=DiameterAlphaHeight.xy;
}