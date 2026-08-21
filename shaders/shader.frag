#version 330 core
out vec4 FragColor;


uniform sampler2D uTexture0;


in vec2 fUv;


void main()
{
    FragColor = texture(uTexture0, fUv);
}