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

        public static Texture NorTexture;
        public static Texture RoughTexture;

        public static Texture EmissiveTexture;

        public static Texture MetallicTexture;
        public static Texture AOTexture;
        private static Shader Shader;

        private static Model Model;

    


        private static readonly uint[] Indices = { 0, 1, 2 };


        private static Camera camera;

        public static Skybox skybox;

        public static List<Vector3> lightPositions = new List<Vector3>
        {
            new Vector3(0.0f,  1.0f, 0.0f),
            new Vector3( 0.0f,  -1.0f, 0.0f),
            new Vector3(0.0f,  1.0f, 2.0f),
            new Vector3( 2.0f,  1.0f, 0.0f)
        };


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
            Gl.ClearColor(0.1f, 0.1f, 0.12f, 1.0f);
            Gl.DepthFunc(DepthFunction.Less);
            Gl.Enable(EnableCap.DepthTest);
            Gl.Disable(EnableCap.CullFace);

            Shader = new Shader(Gl, "shaders/shader.vert", "shaders/shader.frag");
            Texture = new Texture(Gl, "assets/gun/Cerberus_by_Andrew_Maximov 2/Textures/Cerberus_A.tga", Silk.NET.Assimp.TextureType.None, true);
            NorTexture = new Texture(Gl, "assets/gun/Cerberus_by_Andrew_Maximov 2/Textures/Cerberus_N.tga", Silk.NET.Assimp.TextureType.None, true);
            RoughTexture = new Texture(Gl, "assets/gun/Cerberus_by_Andrew_Maximov 2/Textures/Cerberus_R.tga", Silk.NET.Assimp.TextureType.None, true);
            EmissiveTexture = new Texture(Gl, "assets/gun/Cerberus_by_Andrew_Maximov 2/Textures/Cerberus_R.tga", Silk.NET.Assimp.TextureType.None, true);
            AOTexture = new Texture(Gl, "assets/gun/Cerberus_by_Andrew_Maximov 2/Textures/Raw/Cerberus_AO.tga", Silk.NET.Assimp.TextureType.None, true);
            Model = new Model(Gl, "assets/gun/Cerberus_by_Andrew_Maximov 2/Cerberus_LP.FBX");
            MetallicTexture = new Texture(Gl, "assets/gun/Cerberus_by_Andrew_Maximov 2/Textures/Cerberus_M.tga", Silk.NET.Assimp.TextureType.None, true);


            string[] faces =
            {
                "assets/skybox/right.jpg",
                "assets/skybox/left.jpg",
                "assets/skybox/top.jpg",
                "assets/skybox/bottom.jpg",
                "assets/skybox/front.jpg",
                "assets/skybox/back.jpg"
            };
            skybox = new Skybox(faces, Gl);
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

            
            Texture.Bind();
            Shader.Use();
            Shader.SetUniform("uTexture0", 0);
            var difference = (float)(window.Time * 100);

            var size = window.FramebufferSize;
            var model = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(difference)) * Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(90));
            var view = Matrix4x4.CreateLookAt(Camera.CameraPosition, Camera.CameraPosition + Camera.CameraFront, Camera.CameraUp);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.CameraZoom), (float)size.X / size.Y, 0.1f, 3000.0f);

            


            mat4 worldMatrix = mat4.Identity;
            worldMatrix = worldMatrix * mat4.RotateX(MathHelper.DegreesToRadians(-90));

            worldMatrix = worldMatrix * mat4.Scale(0.01f, 0.01f, 0.01f);

            mat4 viewMatrix = Camera.GetViewMatrix();
            mat4 projectionMatrix = mat4.Perspective(MathHelper.DegreesToRadians(Camera.CameraZoom), (float)size.X / size.Y, 0.1f, 3000.0f);



            Shader.SetUniform("uModel", worldMatrix);
            Shader.SetUniform("uView", viewMatrix);
            Shader.SetUniform("uProjection", projectionMatrix);


            foreach (var mesh in Model.Meshes)
            {
                mesh.Bind();
                Shader.Use();
                
                
                Shader.SetUniform("uModel", worldMatrix);
                Shader.SetUniform("uView", viewMatrix);
                Shader.SetUniform("uProjection", projectionMatrix);
                
                

                
                Shader.SetUniform("camPos", Camera.CameraPosition);


                for(int i = 0; i < lightPositions.Count; i++)
                {
                    Shader.SetUniform($"lightPositions[{i}]", lightPositions[i]);
                    Shader.SetUniform($"lightColors[{i}]", new Vector3(1.0f, 1.0f, 1.0f));
                }

                Texture.Bind();
                Shader.SetUniform("uTexture0", 0);

                skybox.Texture.BindCubeMap(TextureUnit.Texture1);
                Shader.SetUniform("equiRecMap", 1);

                NorTexture.Bind(TextureUnit.Texture2);
                Shader.SetUniform("uNormalMap", 2);
                RoughTexture.Bind(TextureUnit.Texture3);
                Shader.SetUniform("uRoughnessMap", 3);
                EmissiveTexture.Bind(TextureUnit.Texture4);
                Shader.SetUniform("uEmissiveMap", 4);
                AOTexture.Bind(TextureUnit.Texture5);
                Shader.SetUniform("uAOMap", 5);
                MetallicTexture.Bind(TextureUnit.Texture6);
                Shader.SetUniform("uMetallicMap", 6);

                
                
        
                Gl.DrawElements(PrimitiveType.Triangles, (UInt32)mesh.Indices.Length, DrawElementsType.UnsignedInt, null);
            }

            skybox.Draw(Gl, view, projection);
        }


        private static void OnFramebufferResize(Vector2D<int> newSize)
        {
            Gl.Viewport(newSize);
        }

        private static void OnClose()
        {
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
