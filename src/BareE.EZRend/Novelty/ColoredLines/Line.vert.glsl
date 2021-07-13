#version 450
layout (location=0) in vec4 pos;
layout (location=1) in vec4 Color;

layout(location =0) out vec4 vertColor;

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
	gl_Position = CamMatrix*ModelMatrix*vec4(pos.xyz,1);
	vertColor=Color;	
}