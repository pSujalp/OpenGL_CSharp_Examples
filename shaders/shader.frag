#version 330 core
in vec2 fUv;
in vec3 Normal;
in vec3 Position;


uniform samplerCube skybox;


uniform vec3 cameraPos;


out vec4 FragColor;


void main()
{
    vec3 I = normalize(Position - cameraPos);
    vec3 R = reflect(I, normalize(Normal));
    FragColor = vec4(texture(skybox, R).rgb  , 1.0);

    // vec4 FragColor1 = ;
}