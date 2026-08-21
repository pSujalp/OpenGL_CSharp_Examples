#version 330 core
layout (location = 0) in vec3 vPos;
layout (location=1) in vec3 inColor;


out vec3 outColor;

uniform mat4 uModel;

void main()
{
    
    gl_Position = uModel * vec4(vPos, 1.0);
    outColor = inColor;
    
    
}
