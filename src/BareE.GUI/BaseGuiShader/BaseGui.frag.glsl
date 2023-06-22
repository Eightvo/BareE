#version 450
layout(location=0) out vec4 FragColor;
layout(location=0) in vec3 uvt;
layout(location=1) in vec4 color;


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
    vec2 uv=uvt.xy;
	float t=uvt.z;
    vec4 pColor = (texture(sampler2D(Texture0,Sampler0), uv)*(1-t))
		         +(texture(sampler2D(Texture1,Sampler1), uv)*(t));
	pColor=pColor*color;
	//if (pColor.a==0) discard;
	FragColor=pColor;
	//FragColor=vec4(0,1,1,1);
}