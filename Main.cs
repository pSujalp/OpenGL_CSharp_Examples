using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
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
        // positions         // colors
         0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,  // bottom right
        -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,  // bottom left
         0.0f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f   // top 
    };


        private static readonly uint[] Indices ={0, 1, 2};

        private static Transform[] Transforms = new Transform[4];


        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "LearnOpenGL with Silk.NET";
            options.WindowState = WindowState.Normal;

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
            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 6, 0);
            Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 6, 3);
            Shader = new Shader(Gl, "shaders/shader.vert", "shaders/shader.frag");


            Transforms[0] = new Transform();
            Transforms[0].Position = new Vector3(0.5f, 0.5f, 0f);
            //Rotation.
            Transforms[1] = new Transform();
            Transforms[1].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1f);
            //Scaling.
            Transforms[2] = new Transform();
            Transforms[2].Scale = 0.5f;
            //Mixed transformation.
            Transforms[3] = new Transform();
            Transforms[3].Position = new Vector3(-0.5f, 0.5f, 0f);
            Transforms[3].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1f);
            Transforms[3].Scale = 0.5f;
            
        }

        private static unsafe void OnRender(double obj)
        {
            Gl.Clear((uint)ClearBufferMask.ColorBufferBit);
            Vao.Bind();
            Shader.Use();



            for (int i = 0; i < Transforms.Length; i++)
            {
                //Using the transformations.
                Shader.SetUniform("uModel", Transforms[i].ViewMatrix);

                Gl.DrawElements(PrimitiveType.Triangles, (uint) Indices.Length, DrawElementsType.UnsignedInt, null);
            }

           
           
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
