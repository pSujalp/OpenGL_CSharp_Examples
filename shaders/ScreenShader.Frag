#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D screenTexture;

const float renderWidth = 800;
const float renderHeight = 450;
float offset = 0.0;  

void main()
{
    float frequency = renderHeight/3.0;

    float globalPos = (TexCoords.y + offset)*frequency;
    float wavePos = cos((fract(globalPos) - 0.5)*3.14);

    
    vec4 texelColor = texture(screenTexture, TexCoords);

    FragColor = mix(vec4(0.0, 0.3, 0.0, 0.0), texelColor, wavePos);
}  