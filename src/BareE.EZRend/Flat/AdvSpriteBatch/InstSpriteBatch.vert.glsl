#version 450
layout (location = 0) in vec3 Position; //Per Vertex
layout (location = 1) in vec2 Uv;       //Per Vertex
layout (location = 2) in vec4 UvBox;        //Per Instance
layout (location = 3) in vec4 Rotation;     //Per Instance

layout(std140,set=0, binding = 0) uniform CameraMat
{
    mat4 CamMatrix;
};
layout(std140,set=0, binding = 1) uniform ModelMat
{
    mat4 ModelMatrix;
};

layout (location = 0) out vec2 uv;

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





void main()
{
	mat2 rotMat = rotate2d(Rotation.z);
	vec2 pXY = Position.xy*Rotation.w;
	pXY=rotMat*pXY;
	vec4 worldPosition = vec4(pXY+Rotation.xy,Position.z, 1);

	vec4 clipPosition = CamMatrix * (worldPosition);
    gl_Position = clipPosition;
	uv=Uv*vec2(UvBox.zw)+vec2(UvBox.xy);
}  