#version 450
layout (location=0) in vec3 pos;
layout (location=1) in vec2 Uv;
layout (location=2) in vec3 normal;
layout (location=3) in vec3 Tangent;

layout(location =0) out vec2 vertUv;
layout(location = 1) out vec3 vertNormal;
layout(location = 2) out vec3 FragPos;
layout(location = 3) out vec3 vertTangent;

layout(std140, set=0, binding=0) uniform CameraMat
{
	mat4 CamMatrix;
};
layout(std140, set=0,binding=1) uniform ModelMat
{
   mat4 ModelMatrix;
};
layout(std140, set=1, binding=0) uniform LightData
{
	vec3 lightPosition;
	vec3 lightColor;
};

void main()
{
   
   FragPos = vec3(ModelMatrix*vec4(pos.x, pos.y, pos.z,1));
   vertNormal=vec3(ModelMatrix*vec4(normal.x, normal.y, normal.z,1));
   vertUv=Uv;
   vertTangent=Tangent;
   gl_Position = CamMatrix*vec4(FragPos,1);
}