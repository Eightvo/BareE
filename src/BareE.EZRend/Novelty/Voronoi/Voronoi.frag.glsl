#version 450
layout(location=0) flat in vec4 color;

layout(location=0) out vec4 FinalColor;


void main()
{
	FinalColor= color;
	//FinalColor=vec4(1,0,0,1);
}