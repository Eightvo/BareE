#version 450
layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 Uv;
layout (location = 2) in vec3 T1;
layout (location = 3) in vec3 T2;

layout(std140,set=0, binding = 0) uniform CameraMat
{
    mat4 CamMatrix;
};
layout(std140,set=0, binding = 1) uniform ModelMat
{
    mat4 ModelMatrix;
};

layout (location = 0) out vec2 uv;
layout (location = 1) out flat vec3 tint1;
layout (location = 2) out flat vec3 tint2;

mat4 rotationMatrix(vec3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}

mat4 translationMatrix(vec3 tran)
{
	return mat4(1.0, 0.0, 0.0, 0.0,  0.0, 1.0, 0.0, 0.0,  0.0, 0.0, 1.0, 0.0,  tran.x, tran.y, tran.z, 1.0);
}


mat2 rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

 vec3 COLOR_MASKS[4] = vec3[]( 
	vec3( 1.0, 0.0, 0.0 ),
	vec3( 0.0, 1.0, 1.0 ),
	vec3( 0.70, 0.5, 0.755 ),
	vec3( 0.5, 0.75, 0.25 )
);

void main()
{
	gl_Position = CamMatrix*ModelMatrix*vec4(Position,1);
    //gl_Position = vec4(Position,1);
	uv=Uv;
	tint1 = T1;
	tint2= T2;
}  
