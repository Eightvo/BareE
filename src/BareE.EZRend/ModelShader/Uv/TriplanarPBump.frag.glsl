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

vec3 saturate(vec3 v)
{
   return clamp(v.rgb, 0.0, 1.0);
}

vec2 modvec(vec2 a, vec2 b)
{
   return a - (b * floor(a/b));
}

void main()
{
	//for each axis (x,y,z) a 1 means to 'flip' that axis
	vec3 flip = vec3(vertNormal.x < 0.0, vertNormal.y >= 0.0, vertNormal.z < 0.0);

	// Raise each component of normal vector to 4th power.
	vec3 blend = saturate(abs(normalize(abs(vertNormal))) - 0.5);
	blend *= blend;
	blend *= blend;
	// Normalize result by dividing by the sum of its components.
	blend /= dot(blend, vec3(1.0, 1.0, 1.0)); 
   
     //Three sets of UVs based on World Position.
     vec2 yzS = modvec(abs(FragPos.yz),vec2(1));
     vec2 xzS = modvec(abs(FragPos.xz),vec2(1));
     vec2 xyS = modvec(abs(FragPos.xy),vec2(1));

	 //Three samples from the ColorTexture. One for each axis.
	                                                                                  //Only use the left texture on the top
    vec4 vertColor2 = texture(sampler2D(ColorTexture,ColorSampler), xzS*vec2(0.25,1.0)+vec2(0.0+0.5*flip.y,0.0));
    vec4 vertColor1 = texture(sampler2D(ColorTexture,ColorSampler), yzS*vec2(0.25,1.0)+vec2(0.5,0.0));
    vec4 vertColor3 = texture(sampler2D(ColorTexture,ColorSampler), xyS*vec2(0.25,1.0)+vec2(0.5,0.0));

	//Blend the color
	vec4 vertColor = vertColor1*blend.x+vertColor2*blend.y+vertColor3*blend.z;

	//Three Surface Normals from the Normal Map. One for each axis.
    vec3 vertN2 = texture(sampler2D(ColorTexture,ColorSampler), xzS*vec2(0.25,1.0)+vec2(0.25+0.5*flip.y,0.0)).rgb*(flip.x*-1);
    vec3 vertN1 = texture(sampler2D(ColorTexture,ColorSampler), yzS*vec2(0.25,1.0)+vec2(0.75,0.0)).rgb*(flip.x*-1);
    vec3 vertN3 = texture(sampler2D(ColorTexture,ColorSampler), xyS*vec2(0.25,1.0)+vec2(0.75,0.0)).rgb*(flip.z*-1);
    
	vertN2 = normalize(vertN2*2.0-1.0);
	vertN1 = normalize(vertN2*2.0-1.0);
	vertN3 = normalize(vertN2*2.0-1.0);


	//Blend the surface normal
	vec3 vNormal = vertNormal;
	// vNormal = vertN1*blend.x+vertN2*blend.y+vertN3*blend.z;

	//A) Is the vertNormal Supposed to be blended like this? I would suppose it would be
	//B) How to rotate the vertNormal to the correct orientation?

    

	vec3 lp=lightPosition;
	vec3 lc=lightColor;

	//override input light for testing
	vec3 alc = vec3(1,1,1);

	lp = vec3(0,0,-10);
	lc =vec3(1,1,1);
	float ambientStrength=0.50;

	//ambient
	vec3 ambient=(ambientStrength*alc);

	//diffuse
	vec3 norm = normalize(vNormal);
	vec3 lightDir = normalize(lp-FragPos);
	float diff = max(dot(norm,lightDir),0.0);
	vec3 diffuse = diff*lc;

	vec3 result = (diffuse+ambient)*vec3(vertColor.xyz);

	FragColor=vec4(result,1.0);
}