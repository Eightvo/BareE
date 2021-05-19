#version 450
layout(location=0) in vec2 uv;
layout(location=1) flat in vec3 tint1;
layout(location=2) flat in vec3 tint2;

layout(location=0) out vec4 FinalColor;

layout(set=1,binding=1) uniform texture2D Texture;
layout(set=1,binding=2) uniform sampler SurfaceSampler;
layout(std140,set=2, binding = 0) uniform Settings
{
    float OutlineThreshold;
	float BlurOutDist;
	vec2 DropShadow;
	float GlowDist;
	vec4 GlowColor;
};
void main()
{

    //vec4 iClr =texture(sampler2D(Texture,SurfaceSampler),uv);
	vec4 actualData = texture(sampler2D(Texture,SurfaceSampler), uv);
	vec4 dropData = texture(sampler2D(Texture,SurfaceSampler), uv-DropShadow);

	FinalColor=vec4(0,0,0,0);

	float ot = OutlineThreshold;
	float bod = BlurOutDist;
	vec4 gc= GlowColor;
	float gd=GlowDist;
	ot=0.1;
	bod=0.2;
	gd=0.1;
	gc=vec4(1,0,0,1);

	if (actualData.b>0.5-bod && actualData.b<0.5+0.5-ot)
	{
		//FinalColor=vec4(1,0,1,1);
		FinalColor=vec4(tint1.rgb,smoothstep(0.5-bod,0.5,actualData.b)*1.0);
	}
	if (gd>0 && actualData.b>0.5-gd&& actualData.b<0.5)
	{
		FinalColor=vec4(gc.rgb,smoothstep(0.5-bod-gd,0.5+0.0,actualData.b)*gc.a);
	}
	if ((DropShadow.x>0 && DropShadow.y>0) && dropData.b>(0.5))
		FinalColor=vec4(0,0,0,1);
	if (FinalColor.a==0) discard;
}