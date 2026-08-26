using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using Silk.NET.Maths;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;


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
        
        private static readonly  float[] transparentVertices = {
        // positions         // texture Coords (swapped y coordinates because texture is flipped upside down)
        0.0f,  0.5f,  0.0f,  0.0f,  0.0f,
        0.0f, -0.5f,  0.0f,  0.0f,  1.0f,
        1.0f, -0.5f,  0.0f,  1.0f,  1.0f,

        0.0f,  0.5f,  0.0f,  0.0f,  0.0f,
        1.0f, -0.5f,  0.0f,  1.0f,  1.0f,
        1.0f,  0.5f,  0.0f,  1.0f,  0.0f
    };

        private static List<vec3> windowsPos = new List<vec3>();


        private static readonly uint[] Indices ={0, 1, 2, 3 ,0};

        private static Vector3 CameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
        private static Vector3 CameraTarget = Vector3.Zero;
        private static Vector3 CameraDirection = Vector3.Normalize(CameraPosition - CameraTarget);
        private static Vector3 CameraRight = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, CameraDirection));
        private static Vector3 CameraUp = Vector3.Cross(CameraDirection, CameraRight);


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
            Gl.Enable(EnableCap.Blend );
            Gl.Enable(EnableCap.DepthTest);
            Gl.BlendFunc(0,GLEnum.SrcAlpha,GLEnum.OneMinusSrcAlpha);


            Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
            Vbo = new BufferObject<float>(Gl, transparentVertices, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);
            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);
            Shader = new Shader(Gl, "shaders/shader.vert", "shaders/shader.frag");

            Texture = new Texture(Gl,"assets/window.png");    

            windowsPos.Add(new vec3(-1.5f, 0.0f, -0.48f)); 
            windowsPos.Add(new vec3(1.5f, 0.0f, 0.51f)); 
            windowsPos.Add(new vec3( 0.0f, 0.0f, 0.7f));
            windowsPos.Add(new vec3( -0.3f, 0.0f, -2.3f)); 
            windowsPos.Add(new vec3( 0.5f, 0.0f, -0.6f)); 
        }

        private static unsafe void OnRender(double obj)
        {
            Gl.Enable(EnableCap.DepthTest);
            Gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            Vao.Bind();
            Texture.Bind();
            Shader.Use();
            Shader.SetUniform("uTexture0", 0);
            var difference = (float) (window.Time * 100);

            var size = window.FramebufferSize;


            mat4 model1 = mat4.Identity;
            model1 = mat4.RotateY(MathHelper.DegreesToRadians(difference));
            model1 = model1 * mat4.Scale(.4f,.4f,.4f);
            mat4 proj = mat4.Perspective(MathHelper.DegreesToRadians(45.0f),(float)size.X / size.Y, 0.1f, 1000.0f);
            mat4 view1 = mat4.LookAt(new vec3(CameraPosition.X,CameraPosition.Y,CameraPosition.Z),
                                               new vec3(CameraTarget.X,CameraTarget.Y,CameraTarget.Z),
                                               new vec3(CameraUp.X,CameraUp.Y,CameraUp.Z));


            Dictionary<float, vec3> sorted = new Dictionary<float, vec3>();

            for (int i = 0; i < windowsPos.Count; i++)
            {
                float dist = vec3.Distance(new vec3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z), windowsPos[i]);
                sorted.Add(dist, windowsPos[i]);
            }

            var byKeyDesc = sorted.OrderByDescending(kvp => kvp.Key);


            foreach (var kvp in byKeyDesc){
            model1 = mat4.Identity;
            model1 = mat4.Translate(kvp.Value.x,kvp.Value.y,kvp.Value.z);
            Shader.SetUniform("uModel", model1);
            Shader.SetUniform("uView", view1);
            Shader.SetUniform("uProjection", proj);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);

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
