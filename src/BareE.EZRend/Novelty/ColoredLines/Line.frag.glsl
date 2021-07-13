#version 450
layout(location=0) out vec4 FragColor;
layout(location =0) in vec4 vertColor;

void main()
{
	float ambientStrength=1.0;
	vec3 lightColor = vec3(1.0,1.0,1.0);
	FragColor=vec4(vertColor.xyz*(ambientStrength*lightColor),vertColor.z);
}