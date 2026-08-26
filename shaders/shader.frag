#version 330 core
out vec4 FragColor;


uniform sampler2D uTexture0;
in vec2 fUv;


void main()
{
    vec4 texColor = texture(uTexture0, fUv);
    // if(texColor.a < 0.1)
    //     discard;
    FragColor = texColor;
}