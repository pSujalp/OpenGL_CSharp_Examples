using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using Silk.NET.Maths;
using FreeCam;

using GlmSharp;


namespace Tutorial
{
    class Program
    {
        private static IWindow window;

        private static IKeyboard primaryKeyboard;
        private static GL Gl;


        private static BufferObject<float> Vbo;
        private static VertexArrayObject<float, uint> Vao;

        public static Texture Texture;
        private static Shader Shader;
        private static Shader ScreenShader;

        private static Model Model;

        private static uint fbo;

        private static BufferObject<float> VboScreen;
        private static VertexArrayObject<float, uint> VaoScreen;

        private static uint textureColorBuffer;



        private static readonly float[] quadVertices = { // vertex attributes for a quad that fills the entire screen in Normalized Device Coordinates.
        // positions   // texCoords
        -1.0f,  1.0f,  0.0f, 1.0f,
        -1.0f, -1.0f,  0.0f, 0.0f,
         1.0f, -1.0f,  1.0f, 0.0f,

        -1.0f,  1.0f,  0.0f, 1.0f,
         1.0f, -1.0f,  1.0f, 0.0f,
         1.0f,  1.0f,  1.0f, 1.0f
    };



        private static readonly float[] Vertices =
        {

            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
        };



        private static readonly uint[] Indices = { 0, 1, 2 };


        private static Camera camera;


        private static void Main(string[] args)
        {
            var options = WindowOptions.Default ;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "LearnOpenGL with Silk.NET";
            options.PreferredDepthBufferBits = 24;
            options.PreferredStencilBufferBits = 8;
            options.API = new GraphicsAPI(
                ContextAPI.OpenGL,
                ContextProfile.Core,
                ContextFlags.ForwardCompatible,
                new APIVersion(3, 3));
            options.WindowState = WindowState.Maximized ;
            options.WindowBorder = WindowBorder.Resizable;

            
            window = Window.Create(options);
            window.Load += OnLoad;
            window.Render += OnRender;
            window.Update += OnUpdate;
            window.FramebufferResize += OnFramebufferResize;
            window.Closing += OnClose;
            window.Run();
            window.Dispose();

        }


        private static unsafe void OnLoad()
        {

            IInputContext input = window.CreateInput();
            primaryKeyboard = input.Keyboards.FirstOrDefault();
            if (primaryKeyboard != null)
            {
                primaryKeyboard.KeyDown += KeyDown;
                camera = new Camera(primaryKeyboard, input);
            }

            Gl = GL.GetApi(window);
            Gl.DepthFunc(DepthFunction.Less);
            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.CullFace);


            var size = window.FramebufferSize;
            fbo = Gl.GenFramebuffer();
            Gl.BindFramebuffer(GLEnum.Framebuffer, fbo);


            fixed (uint* ptr = &textureColorBuffer)
            {
                Gl.GenTextures(1, ptr);
            }
            Gl.BindTexture(GLEnum.Texture2D, textureColorBuffer);
            Gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgb, (uint)size.X, (uint)size.Y, 0, GLEnum.Rgb, GLEnum.UnsignedByte, null);
            Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            Gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, textureColorBuffer, 0);



            uint rbo;

            rbo = Gl.GenRenderbuffer();
            Gl.BindRenderbuffer(GLEnum.Renderbuffer, rbo);
            Gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth24Stencil8, (uint)size.X, (uint)size.Y);
            Gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Renderbuffer, rbo);

            if (Gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
            {
                Console.WriteLine("Failed to create a framebuffer");
                return;
            }
            Gl.BindFramebuffer(GLEnum.Framebuffer, 0);




            Vbo = new BufferObject<float>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<float, uint>(Gl, Vbo);
            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);
            Shader = new Shader(Gl, "shaders/shader.vert", "shaders/shader.frag");
            Texture = new Texture(Gl, "assets/backpack/diffuse.jpg");
            Model = new Model(Gl, "assets/backpack/backpack.obj");

            VboScreen = new BufferObject<float>(Gl, quadVertices, BufferTargetARB.ArrayBuffer);
            VaoScreen = new VertexArrayObject<float, uint>(Gl, VboScreen);
            VaoScreen.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 4, 0);
            VaoScreen.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 4, 2);
            ScreenShader = new Shader(Gl, "shaders/ScreenShader.vert", "shaders/ScreenShader.frag");







        }

        private static unsafe void OnUpdate(double deltaTime)
        {
            var moveSpeed = 2.5f * (float)deltaTime;

            if (primaryKeyboard.IsKeyPressed(Key.W))
            {

                Camera.CameraPosition += moveSpeed * Camera.CameraFront;
            }
            if (primaryKeyboard.IsKeyPressed(Key.S))
            {

                Camera.CameraPosition -= moveSpeed * Camera.CameraFront;
            }
            if (primaryKeyboard.IsKeyPressed(Key.A))
            {

                Camera.CameraPosition -= Vector3.Normalize(Vector3.Cross(Camera.CameraFront, Camera.CameraUp)) * moveSpeed;
            }
            if (primaryKeyboard.IsKeyPressed(Key.D))
            {

                Camera.CameraPosition += Vector3.Normalize(Vector3.Cross(Camera.CameraFront, Camera.CameraUp)) * moveSpeed;
            }
        }

        private static unsafe void OnRender(double obj)
        {




            var difference = (float)(window.Time * 100);

            var size = window.FramebufferSize;

            Gl.BindFramebuffer(GLEnum.Framebuffer, fbo);
            Gl.Enable(EnableCap.DepthTest);
            Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));



            var model = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(difference)) * Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(difference));
            var view = Matrix4x4.CreateLookAt(Camera.CameraPosition, Camera.CameraPosition + Camera.CameraFront, Camera.CameraUp);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.CameraZoom), (float)size.X / size.Y, 0.1f, 3000.0f);

            Vector3 scaleVector = new Vector3(1.0f, 1.0f, 1.0f);
            Quaternion rotationQuaternion = Quaternion.Identity;
            Vector3 translationVector = new Vector3(0.0f, 0.0f, 0.0f);

            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(scaleVector);
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(rotationQuaternion);
            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(translationVector);
            Matrix4x4 worldMatrix = scaleMatrix * rotationMatrix * translationMatrix;


            Shader.SetUniform("uModel", worldMatrix);
            Shader.SetUniform("uView", view);
            Shader.SetUniform("uProjection", projection);


            foreach (var mesh in Model.Meshes)
            {
                mesh.Bind();
                Shader.Use();
                Texture.Bind();
                Shader.SetUniform("uTexture0", 0);
                Shader.SetUniform("uModel", worldMatrix);
                Shader.SetUniform("uView", view);
                Shader.SetUniform("uProjection", projection);
                Gl.DrawElements(PrimitiveType.Triangles, (UInt32)mesh.Indices.Length, DrawElementsType.UnsignedInt, null);
            }

            Gl.BindVertexArray(0);



            Gl.BindFramebuffer(GLEnum.Framebuffer, 0);
            Gl.Disable(EnableCap.DepthTest);
            Gl.ClearColor(1, 1, 1, 1);
            Gl.Clear((UInt16)(ClearBufferMask.ColorBufferBit));

            VaoScreen.Bind();
            ScreenShader.Use();
            ScreenShader.SetUniform("screenTexture", 0);

            Gl.ActiveTexture(GLEnum.Texture0);
            Gl.BindTexture(GLEnum.Texture2D, textureColorBuffer);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);



        }


        private static unsafe void OnFramebufferResize(Vector2D<int> newSize)
        {
            Gl.Viewport(newSize);

            fixed (uint* ptr = &textureColorBuffer)
            {
                Gl.GenTextures(1, ptr);
            }
            Gl.BindTexture(GLEnum.Texture2D, textureColorBuffer);
            Gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgb, (uint)newSize.X, (uint)newSize.Y, 0, GLEnum.Rgb, GLEnum.UnsignedByte, null);
            Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            Gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, textureColorBuffer, 0);
            uint rbo;
            rbo = Gl.GenRenderbuffer();
            Gl.BindRenderbuffer(GLEnum.Renderbuffer, rbo);
            Gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth24Stencil8, (uint)newSize.X, (uint)newSize.Y);
            Gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Renderbuffer, rbo);

            if (Gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
            {
                Console.WriteLine("Failed to create a framebuffer");
                return;
            }
            Gl.BindFramebuffer(GLEnum.Framebuffer, 0);
        }

        private static void OnClose()
        {
            Vbo.Dispose();
            Vao.Dispose();
            Shader.Dispose();
            Texture.Dispose();
            VboScreen.Dispose();
            VaoScreen.Dispose();
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
