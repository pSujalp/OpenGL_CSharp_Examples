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

        private static Model Model;

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


        private static void OnLoad()
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
            Gl.CullFace(GLEnum.Back);


            Vbo = new BufferObject<float>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<float, uint>(Gl, Vbo);
            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);
            Shader = new Shader(Gl, "shaders/shader.vert", "shaders/shader.frag");
            Texture = new Texture(Gl, "assets/backpack/diffuse.jpg");
            Model = new Model(Gl, "assets/backpack/backpack.obj");



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
            
            Gl.Clear((UInt16)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            

            Vao.Bind();
            Texture.Bind();
            Shader.Use();
            Shader.SetUniform("uTexture0", 0);
            var difference = (float)(window.Time * 100);

            var size = window.FramebufferSize;





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
                Gl.DrawElements(PrimitiveType.Triangles, (UInt32)mesh.Indices.Length , DrawElementsType.UnsignedInt, null);
            }
        }


        private static void OnFramebufferResize(Vector2D<int> newSize)
        {
            Gl.Viewport(newSize);
        }

        private static void OnClose()
        {
            Vbo.Dispose();
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
