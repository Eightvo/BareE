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
	int FontType;
	int Flags;
	vec2 FontRes;
};


const float smoothing = 1.0/32.0;

vec4 STDFont(vec4 actualData, vec4 dropData)
{
    return actualData;
}

vec4 SDFFont(vec4 actualData, vec4 dropData)
{
    float dist = actualData.b;
	float distH = actualData.r;
	float distV = actualData.g;

	float alpha = 1-smoothstep(0.5, 0.5+BlurOutDist, dist);
	
	if (dist<0.5) return vec4(tint1,1);
	return vec4(tint2, alpha);
}

void main()
{
	vec4 actualData = texture(sampler2D(Texture,SurfaceSampler), uv);
	vec4 dropData = texture(sampler2D(Texture,SurfaceSampler), uv-DropShadow);

	if (FontType==1)
	   FinalColor =vec4(tint1,actualData.a);
	else 
	   FinalColor = SDFFont(actualData, dropData);

	if (FinalColor.a==0) discard;
}