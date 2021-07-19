#version 450
layout (location = 0) in vec3 Position; //Per Vertex
layout (location = 1) in vec3 Offset;   //PerInstance
layout (location = 2) in vec4 Color;    //PerInstance

layout (location = 0) out flat vec4 vertColor;

layout(std140,set=0, binding = 0) uniform CameraMat
{
    mat4 CamMatrix;
};
layout(std140,set=0, binding = 1) uniform ModelMat
{
    mat4 ModelMatrix;
};

void main()
{
	gl_Position = CamMatrix*ModelMatrix*vec4(Position.xyz,1);
	gl_Position+=vec4(Offset,0);
	vertColor=Color;	
}
