#version 450
layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 Uv;
layout (location = 2) in vec3 Color;

layout(std140,set=0, binding = 0) uniform CameraMat
{
    mat4 CamMatrix;
};
layout(std140,set=0, binding = 1) uniform ModelMat
{
    mat4 ModelMatrix;
};

layout (location = 0) out vec2 uv;
layout (location = 1) out vec3 clr;

void main()
{
	gl_Position = CamMatrix*ModelMatrix*vec4(Position,1);
	uv=Uv;
	clr=Color;
}  
