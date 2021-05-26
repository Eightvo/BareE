#version 450
layout(location=0) in vec2 uv;
layout(location=1) flat in vec3 tint1;
layout(location=2) flat in vec3 tint2;

layout(location=0) out vec4 FinalColor;

layout(set=2,binding=1) uniform texture2D Texture;
layout(set=2,binding=2) uniform sampler SurfaceSampler;
layout(std140, set=1, binding = 0) uniform Settings
{
    float OutlineThreshold;
	float BlurOutDist;
	vec2 DropShadow;
	vec3 GlowColor;
	float GlowDist;
};


const float smoothing = 1.0/32.0;

void main()
{
	vec4 actualData = texture(sampler2D(Texture,SurfaceSampler), uv);
	vec4 dropData = texture(sampler2D(Texture,SurfaceSampler), uv-DropShadow);

    float dist = actualData.r;

	//FinalColor=vec4(tint1, dist);

	float alpha = smoothstep(0, 0.0+BlurOutDist, dist);

	if (dist==0) FinalColor=vec4(0,1,0,1);
	if (dist>0.25) FinalColor=vec4(0,0,1,1);


	if (FinalColor.a==0) discard;
}