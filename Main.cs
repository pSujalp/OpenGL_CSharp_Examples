using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using Silk.NET.Maths;

namespace Tutorial
{
    class Program
    {
        private static IWindow window;
        private static GL Gl;
        private static BufferObject<float> Vbo;
        private static BufferObject<uint> Ebo;
        private static VertexArrayObject<float, uint> Vao;
        public static Texture Texture;

        private static Shader Shader;

        private static readonly float[] Vertices = {

         0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 1.0f,
         0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f,
        -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 0.0f,
        -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f
    };


        private static readonly uint[] Indices = {
        0, 1, 3,
        1, 2, 3
    };


        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "LearnOpenGL with Silk.NET";
            options.WindowState = WindowState.Maximized | WindowState.Fullscreen;

            window = Window.Create(options);
            window.Load += OnLoad;
            window.Render += OnRender;
            window.FramebufferResize += OnFramebufferResize;
            window.Closing += OnClose;
            window.Run();
            window.Dispose();
        }


        private static void OnLoad()
        {
            IInputContext input = window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += KeyDown;
            }
            Gl = GL.GetApi(window);
            Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
            Vbo = new BufferObject<float>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);
            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 8, 0);
            Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 8, 3);
            Vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 8, 6);
            Shader = new Shader(Gl, "shaders/shader.vert", "shaders/shader.frag");
            Texture = new Texture(Gl, "assets/container.jpg");
        }


        private static unsafe void OnRender(double obj)
        {
            Gl.Clear((uint)ClearBufferMask.ColorBufferBit);
            Vao.Bind();
            Shader.Use();
            Texture.Bind(TextureUnit.Texture0);
            Shader.SetUniform("uTexture", 0);            
            Gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
        }

        private static void OnFramebufferResize(Vector2D<int> newSize)
        {
            Gl.Viewport(newSize);
        }

        private static void OnClose()
        {

            Vbo.Dispose();
            Ebo.Dispose();
            Vao.Dispose();
            Shader.Dispose();
            Texture.Dispose();

        }

        private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                window.Close();
            }
        }
    }
}
