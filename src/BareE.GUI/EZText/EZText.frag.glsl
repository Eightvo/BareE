#version 450
layout(location=0) in vec2 uv;
layout(location=1) in vec3 clr;

layout(location=0) out vec4 FinalColor;

layout(set=2,binding=1) uniform texture2D Texture;
layout(set=2,binding=2) uniform sampler SurfaceSampler;

void main()
{
	vec4 actualData = texture(sampler2D(Texture,SurfaceSampler), uv);
	FinalColor= actualData*vec4(clr,1);
}