#version 450
layout(location=0) in vec2 uv;

layout(location=0) out vec4 FinalColor;

layout(set=1,binding=1) uniform texture2D Texture;
layout(set=1,binding=2) uniform sampler SurfaceSampler;

void main()
{

    vec4 iClr =texture(sampler2D(Texture,SurfaceSampler),uv);
	
	if (iClr.a==0.0) discard;
	FinalColor=iClr;
}