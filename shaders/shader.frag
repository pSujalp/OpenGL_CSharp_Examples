#version 330 core
out vec4 FragColor;


uniform sampler2D uTexture0;
uniform int enableOutline;

in vec2 fUv;




void main()
{
    if(enableOutline==1){
        FragColor = vec4(1.0f,1.0f,1.0f,1.0f);
    }
    else FragColor = texture(uTexture0, fUv);
}