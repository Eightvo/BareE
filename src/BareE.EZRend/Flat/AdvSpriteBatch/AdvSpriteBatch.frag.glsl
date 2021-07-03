#version 450
layout(location=0) in vec2 uv;
layout(location=1) flat in vec4 tint1;
layout(location=2) flat in vec4 tint2;

layout(location=0) out vec4 FinalColor;

layout(set=1,binding=1) uniform texture2D Texture;
layout(set=1,binding=2) uniform sampler SurfaceSampler;

void main()
{

    vec4 iClr =texture(sampler2D(Texture,SurfaceSampler),uv);
	
	//FinalColor = 


    int iClrRG  = int(255.0*iClr.r);
	int iClrBA = int(255.0*iClr.g);
	bool owriteCl1 = iClr.b>0;// && (tint1.a>0 || tint1.g>0 || tint1.b>0);
	bool owriteCl2 = iClr.a>0;// && (tint2.r>0 || tint2.g>0 || tint2.b>0) && !owriteCl1;
	bool noOverwrite = !(owriteCl1 || owriteCl2);

    //15
	//240
	int iR = (iClrRG & 240)>>4;
	int iG = (iClrRG & 15);
	int iB = (iClrBA & 240)>>4;
	int iA = (iClrBA & 15);

	float oR = ((float(iR)/15.0));
	float oG = ((float(iG)/15.0));
	float oB = ((float(iB)/15.0));
	float oA = ((float(iA)/15.00));

	vec4 oClr = vec4(oR, oG, oB,oA);
	//oClr=vec4(1,0,0,1);

	FinalColor =  oClr
	* int(noOverwrite)
	           + tint1 * int(owriteCl1)
			   + tint2 * int(owriteCl2)
			   ;

	//FinalColor = oClr;tg
	//FinalColor = iClr;
	//FinalColor=vec4(1,0,0,1);
	//FinalColor=vec4(iClr.r, iClr.g,0,int(oA>0));
}