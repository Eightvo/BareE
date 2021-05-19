#version 450
layout (location=0) in vec3 pos;
layout (location=1) in vec3 norm;
layout (location=2) in vec3 tan;
layout (location=3) in vec3 bitan;
layout (location=4) in vec2 uv;

layout(location=0) out vec3 fpos;
layout(location=1) out vec3 fnorm;
layout(location=2) out vec3 ftan;
layout(location=3) out vec3 fbitan;
layout(location=4) out vec2 fuv;
layout(location=5) out mat3 TBN;

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

layout(set=2,binding=0) uniform texture2D diff_tex;
layout(set=2,binding=1) uniform sampler diff_sam;

layout(set=3,binding=0) uniform texture2D bump_tex;
layout(set=3,binding=1) uniform sampler bump_sam;

layout(set=4,binding=0) uniform texture2D spec_tex;
layout(set=4,binding=1) uniform sampler spec_sam;

layout(set=5,binding=0) uniform texture2D emissive_tex;
layout(set=5,binding=1) uniform sampler emissive_sam;



void main()
{
   
   fpos  =vec3(ModelMatrix*vec4(pos,1));
   fnorm =mat3(transpose(inverse(ModelMatrix))) * norm;
   ftan =mat3(transpose(inverse(ModelMatrix))) * tan;
   fbitan  =mat3(transpose(inverse(ModelMatrix))) * bitan;
   fuv=uv;

   
   vec3 T = normalize(tan);
   vec3 B = normalize(fbitan);
   vec3 N = normalize(fnorm);

     TBN = mat3(T, B, N);
   gl_Position = CamMatrix*vec4(fpos,1);
}