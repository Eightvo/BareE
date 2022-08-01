#version 450
layout(location =0) out vec3 uvt;
layout(location=1) out vec4 clr;

layout (location=0) in vec3 POS;
layout (location=1) in vec3 UVT;
layout (location=2) in vec4 COLOR;

layout(std140, set=0, binding=0) uniform CameraMat
{
	mat4 CamMatrix;
};
layout(std140, set=0,binding=1) uniform ModelMat
{
   mat4 ModelMatrix;
};

layout(std140, set=1, binding=0) uniform CommonData
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

//Style
layout(set=2,binding=0) uniform texture2D Texture0;
layout(set=2,binding=1) uniform sampler Sampler0;
//Font
layout(set=3,binding=0) uniform texture2D Texture1;
layout(set=3,binding=1) uniform sampler Sampler1;


void main()
{
	gl_Position = CamMatrix*ModelMatrix*vec4(POS*vec3(1,-1,1),1);
	uvt=UVT;
	clr=COLOR;
}