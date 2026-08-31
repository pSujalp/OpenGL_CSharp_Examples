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

        public static Texture Texture;

        public static Texture WoodTexture;
        private static Shader Shader;

        private static Shader DepthShader;

        private static Model Model;

        private static Model PlaneModel;

        private static uint depthMap;

        private static uint depthMapFBO;


        private const uint SHADOW_WIDTH = 1024;
        private const uint SHADOW_HEIGHT = 1024;


        private static Vector3 lightPos = new Vector3(-4.0f, 14.0f, -1.0f);

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

        private static Camera camera;

        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "LearnOpenGL with Silk.NET";
            options.PreferredDepthBufferBits = 24;
            options.PreferredStencilBufferBits = 8;
            options.API = new GraphicsAPI(
                ContextAPI.OpenGL,
                ContextProfile.Core,
                ContextFlags.ForwardCompatible,
                new APIVersion(3, 3));

            window = Window.Create(options);
            window.Load += OnLoad;
            window.Render += OnRender;
            window.Update += OnUpdate;
            window.FramebufferResize += OnFramebufferResize;
            window.Closing += OnClose;
            window.Run();
            window.Dispose();

        }


        private unsafe static void OnLoad()
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



            Shader = new Shader(Gl, "shaders/shader.vert", "shaders/shader.frag");
            DepthShader = new Shader(Gl, "shaders/depthShader.vert", "shaders/depthShader.frag");

            Texture = new Texture(Gl, "assets/backpack/diffuse.jpg");
            WoodTexture = new Texture(Gl, "assets/wood.png");
            Model = new Model(Gl, "assets/backpack/backpack.obj");
            PlaneModel = new Model(Gl, "assets/plane.fbx");

            depthMapFBO = Gl.GenFramebuffer();

            depthMap = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2D, depthMap);
            Gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.DepthComponent, SHADOW_WIDTH, SHADOW_HEIGHT, 0, GLEnum.DepthComponent, GLEnum.Float, null);

            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToBorder);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToBorder);
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            Gl.BindFramebuffer(GLEnum.Framebuffer, depthMapFBO);
            Gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.DepthAttachment, TextureTarget.Texture2D, depthMap, 0);
            Gl.DrawBuffer(GLEnum.None);
            Gl.ReadBuffer(GLEnum.None);

            var fboStatus = Gl.CheckFramebufferStatus(GLEnum.Framebuffer);
            if (fboStatus != GLEnum.FramebufferComplete)
            {
                Console.WriteLine($"Shadow FBO incomplete: {fboStatus}");
            }

            Gl.BindFramebuffer(GLEnum.Framebuffer, 0);


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
            var size = window.FramebufferSize;

            var difference = (float)(window.Time * 100);

            var view = Matrix4x4.CreateLookAt(Camera.CameraPosition, Camera.CameraPosition + Camera.CameraFront, Camera.CameraUp);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.CameraZoom), (float)size.X / size.Y, 0.1f, 3000.0f);


            float nearPlane = 1.0f, farPlane = 25.0f;
            var lightProjection = Matrix4x4.CreateOrthographic(20.0f, 20.0f, nearPlane, farPlane);
            var lightView = Matrix4x4.CreateLookAt(lightPos, Vector3.Zero, Vector3.UnitY);
            var lightSpaceMatrix = lightView * lightProjection;


            var cubeModel = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(difference)) * Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(difference));

            Vector3 planeScale = new Vector3(10.0f, 10.0f, 10.0f);
            Quaternion planeRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(-Math.PI / 2));
            Vector3 planeTranslation = new Vector3(0.0f, -3.5f, 0.0f);
            var planeModelMatrix = Matrix4x4.CreateScale(planeScale) * Matrix4x4.CreateFromQuaternion(planeRotation) * Matrix4x4.CreateTranslation(planeTranslation);


            Gl.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT);
            Gl.BindFramebuffer(GLEnum.Framebuffer, depthMapFBO);
            Gl.Clear((uint)ClearBufferMask.DepthBufferBit);

            DepthShader.Use();
            DepthShader.SetUniform("lightSpaceMatrix", lightSpaceMatrix);


            Gl.CullFace(TriangleFace.Front);
            DepthShader.SetUniform("model", cubeModel);
            foreach (var mesh in Model.Meshes)
            {
                mesh.Bind();
                Gl.DrawElements(PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt, null);
            }

            DepthShader.SetUniform("model", planeModelMatrix);
            foreach (var mesh in PlaneModel.Meshes)
            {
                mesh.Bind();
                Gl.DrawElements(PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt, null);
            }

            Gl.CullFace(TriangleFace.Back);
            Gl.BindFramebuffer(GLEnum.Framebuffer, 0);

            Gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Shader.Use();
            Shader.SetUniform("uView", view);
            Shader.SetUniform("uProjection", projection);
            Shader.SetUniform("lightSpaceMatrix", lightSpaceMatrix);
            Shader.SetUniform("lightPos", lightPos);
            Shader.SetUniform("viewPos", Camera.CameraPosition);
            Shader.SetUniform("uTexture0", 0);
            Shader.SetUniform("shadowMap", 1);
            Gl.ActiveTexture(TextureUnit.Texture1);
            Gl.BindTexture(TextureTarget.Texture2D, depthMap);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Texture.Bind();
            Shader.SetUniform("uModel", cubeModel);
            foreach (var mesh in Model.Meshes)
            {
                mesh.Bind();
                Gl.DrawElements(PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt, null);
            }
            Gl.ActiveTexture(TextureUnit.Texture0);
            WoodTexture.Bind();
            Shader.SetUniform("uModel", planeModelMatrix);
            foreach (var mesh in PlaneModel.Meshes)
            {
                mesh.Bind();
                Gl.DrawElements(PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt, null);
            }
        }

        private static void OnFramebufferResize(Vector2D<int> newSize)
        {
            Gl.Viewport(newSize);
        }

        private static void OnClose()
        {
            Shader.Dispose();
            DepthShader.Dispose();
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