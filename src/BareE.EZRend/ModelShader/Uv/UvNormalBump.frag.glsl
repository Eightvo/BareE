#version 450
layout(location=0) out vec4 FragColor;

layout(location=0) in vec3 fpos;
layout(location=1) in vec3 fnorm;
layout(location=2) in vec3 ftan;
layout(location=3) in vec3 fbitan;
layout(location=4) in vec2 fuv;
layout(location=5) in mat3 TBN;

layout(std140, set=0, binding=0) uniform CameraMat
{
	mat4 CamMatrix;
};
layout(std140, set=0,binding=1) uniform ModelMat
{
   mat4 ModelMatrix;
};
layout(std140, set=0, binding=2) uniform CommonData
{
   int time;
   int flags;
   int pad1;
   int pad2;
   vec2 u_resolution;
   vec2 u_mouse;
   vec3 u_campos;
   float seed;
};


//Ambient lighting: https://learnopengl.com/ somewhere.
//SpotLights and Point Lights: https://learnopengl.com/Lighting/Light-casters
layout(std140, set=1, binding=0) uniform AmbientLightData
{
	vec4[4] ALDR;
};

layout(std140, set=1, binding=1) uniform PointLightData
{
	vec4  PLDR[32];
};
//Difus
layout(set=2,binding=0) uniform texture2D Texture0;
layout(set=2,binding=1) uniform sampler Sampler0;
//Normal
layout(set=3,binding=0) uniform texture2D Texture1;
layout(set=3,binding=1) uniform sampler Sampler1;

//Spec
layout(set=4,binding=0) uniform texture2D Texture2;
layout(set=4,binding=1) uniform sampler Sampler2;

//Emissive
layout(set=5,binding=0) uniform texture2D Texture3;
layout(set=5,binding=1) uniform sampler Sampler3;


float calcAttenuation(vec3 a, float d)
{
	return 1.0/(a.x*a.y*d+a.z*d*d);
}
vec3 calcPointLight(vec3 p, vec3 n, vec3 lp, vec3 lc, vec3 la)
{
	vec3 ld = normalize(lp-p); 
	float diff = max(dot(n,ld),0);

	float dist = length(lp-p);
	float att = calcAttenuation(la,dist);

	return lc*diff*att;

}

float calcTOffset(float t)
{
  return -((2*t-1)*(2*t-1))+1;
}

void main()
{
	vec3  alc = ALDR[0].xyz;                //Ambient Light Color  -- There is no Ambient Light Color... ALDR[0].xyz can be used for flags?
	float ali = ALDR[0].w;                  //Ambient Light Intensity 

	vec3  dlp = ALDR[1].xyz;                 //Diffuse Light Position
	                                        
	vec3 slp = vec3(ALDR[1].w,ALDR[2].xy);  //Spotlight Position
	vec3 slc = vec3(ALDR[2].zw, ALDR[2].x);//Spotlight Color.
	vec3 slt = vec3(ALDR[3].yzw);          //SpotLight Threshold.


    vec4  diffuseS = texture(sampler2D(Texture0,Sampler0), fuv);
	vec3    snorm = texture(sampler2D(Texture1,Sampler1), fuv).xyz;
	vec4 specSample     = texture(sampler2D(Texture2,Sampler2),fuv);
	vec4 emmisive = texture(sampler2D(Texture3,Sampler3), fuv);


	vec3 trueNorm = normalize(TBN*snorm);


	

	//ambient
    vec3 ambient = ali * diffuseS.rgb;
  	
    // diffuse 
    vec3 norm = normalize(trueNorm);
    vec3 lightDir = normalize(fpos-dlp);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * diffuseS.rgb;
    

    // specular
    vec3 viewDir = normalize( u_campos-fpos);
    vec3 reflectDir = reflect(-lightDir, norm);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specSample.a * spec * specSample.rgb;  

	vec3 emm = emmisive.rgb*(1-calcTOffset(float(time)/1000));

    vec3 result = (ambient + diffuse + specular + emm);


	//result = ffwd;

    FragColor = vec4(result, 1.0);
	
}