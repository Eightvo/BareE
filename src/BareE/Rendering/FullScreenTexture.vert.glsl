#version 450
layout(location =0) out vec2 uv;

layout (location=0) in vec3 pos;
layout (location=1) in vec2 UV;

layout(std140, set=0, binding=0) uniform CameraMat
{
	mat4 CamMatrix;
};
layout(std140, set=0,binding=1) uniform ModelMat
{
   mat4 ModelMatrix;
};
void main()
{
	gl_Position = CamMatrix*ModelMatrix*vec4(pos,1);
	uv=UV;
}