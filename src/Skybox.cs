using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using Silk.NET.Maths;
using Tutorial;

class Skybox
{
    
    private static readonly float[] SkyboxVertices= {        
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        -1.0f,  1.0f, -1.0f,
         1.0f,  1.0f, -1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
         1.0f, -1.0f,  1.0f
    };

    private BufferObject<float> Vbo;
    private VertexArrayObject<float, uint> Vao;
    public Tutorial.Texture Texture;

    public Tutorial.Shader shader;


    public Skybox(string[] path, GL gL)
    {
        Texture = new Tutorial.Texture();
        string[] paths = path;
        this.Texture.CubeTexture(gL,paths);
        Vbo = new BufferObject<float>(gL, SkyboxVertices, BufferTargetARB.ArrayBuffer);
        Vao = new VertexArrayObject<float, uint>(gL, Vbo);
        Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 3, 0);
        shader = new Tutorial.Shader(gl: gL, vertexPath:"shaders/skybox.vert",fragmentPath:"shaders/skybox.frag");
        shader.Use();
        shader.SetUniform("skybox",0);

    }

    public void Draw(GL gL,Matrix4x4 view , Matrix4x4 proj)
    {
        gL.DepthFunc(DepthFunction.Lequal);
        Vao.Bind();
        shader.Use();
        Texture.BindCubeMap(TextureUnit.Texture0);
        shader.SetUniform("skybox", 0);
        shader.SetUniform("view",view);
        shader.SetUniform("projection",proj);
        gL.DrawArrays(PrimitiveType.Triangles, 0, (uint)SkyboxVertices.Length);

        
        gL.BindVertexArray(0);
        gL.DepthFunc(DepthFunction.Less);
    }

    ~Skybox()
    {
        shader.Dispose();
        Texture.Dispose();
    }


}