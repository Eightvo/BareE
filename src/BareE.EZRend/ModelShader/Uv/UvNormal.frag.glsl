#version 450
layout(location=0) out vec4 FragColor;

layout(location =0) in vec2 vertUv;
layout(location = 1) in vec3 vertNormal;
layout(location = 2) in vec3 FragPos;

layout(std140, set=0, binding=0) uniform CameraMat
{
	mat4 CamMatrix;
};
layout(std140, set=0,binding=1) uniform ModelMat
{
   mat4 ModelMatrix;
};
layout(set=1,binding=1) uniform texture2D ColorTexture;
layout(set=1,binding=2) uniform sampler ColorSampler;

layout(std140, set=2, binding=0) uniform LightData
{
	vec3 lightPosition;
	vec3 lightColor;
};

void main()
{
    vec4 vertColor = texture(sampler2D(ColorTexture,ColorSampler), vertUv);

	vec3 lp=lightPosition;
	vec3 lc=lightColor;

	vec3 alc = vec3(1,1,1);

	lp = vec3(0,0,-10);
	lc =vec3(1,1,1);

	//ambient
	float ambientStrength=0.75;
	vec3 ambient=(ambientStrength*alc);

	//diffuse
	vec3 norm = normalize(vertNormal);
	vec3 lightDir = normalize(lp-FragPos);
	float diff = max(dot(norm,lightDir),0.0);
	vec3 diffuse = diff*lc;

	vec3 result = (diffuse+ambient)*vec3(vertColor.xyz);

	FragColor=vec4(result,1.0);
	
	//FragColor=vec4(vertColor.xyz,1);


}