#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D screenTexture;

const float renderWidth = 800;
const float renderHeight = 450;
float offset = 0.0;  

void main()
{
    const float gamma = 2.2;
    vec3 hdrColor = texture(screenTexture, TexCoords).rgb;
    vec3 mapped = vec3(1.0) - exp(-hdrColor * 4.5f);
    mapped = pow(mapped, vec3(1.0 / gamma));
    FragColor = vec4(mapped, 1.0);
}  