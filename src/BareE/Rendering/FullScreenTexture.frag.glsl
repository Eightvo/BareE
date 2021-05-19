#version 450
layout(location=0) out vec4 FragColor;
layout(location=0) in vec2 uv;

layout(set=1,binding=1) uniform texture2D ColorTexture;
layout(set=1,binding=2) uniform sampler ColorSampler;


void main()
{
    vec4 pColor = texture(sampler2D(ColorTexture,ColorSampler), uv);
	if (pColor.a==0) discard;
	FragColor=pColor;
}