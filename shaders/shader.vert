#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec2 vUv;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec2 fUv;
out vec3 Normal;
out vec3 Position;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
    fUv = vUv;
    Normal = mat3(transpose(inverse(uModel))) * vNormal;
    Position = vec3(uModel * vec4(vPos, 1.0));
}