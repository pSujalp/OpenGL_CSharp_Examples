//Specifying the version like in our vertex shader.
#version 330 core
out vec4 FragColor;

in vec3 outColor;
in vec2 outTexCoord;

uniform sampler2D uTexture;


void main()
{
   
    FragColor =  vec4(outColor,1.0f) * texture(uTexture,outTexCoord);
}
