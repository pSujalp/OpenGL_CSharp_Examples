using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using Silk.NET.Maths;
using Tutorial;

using GlmSharp;



namespace FreeCam
{

    class Camera
    {
        public Camera() { }

        public Camera(IKeyboard primaryKeyboard1, IInputContext input)
        {
            primaryKeyboard = primaryKeyboard1;
            for (int i = 0; i < input.Mice.Count; i++)
            {
                input.Mice[i].Cursor.CursorMode = CursorMode.Raw;
                input.Mice[i].MouseMove += OnMouseMove;
                input.Mice[i].Scroll += OnMouseWheel;
            }
            
        }

        private static IKeyboard primaryKeyboard;
        public static Vector3 CameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
        public static Vector3 CameraFront = new Vector3(0.0f, 0.0f, -1.0f);
        public static Vector3 CameraUp = Vector3.UnitY;
        public static Vector3 CameraDirection = Vector3.Zero;
        public static float CameraYaw = -90f;
        public static float CameraPitch = 0f;
        public static float CameraZoom = 45f;
        private static Vector2 LastMousePosition;


        private static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            var lookSensitivity = 0.1f;
            if (LastMousePosition == default) { LastMousePosition = position; }
            else
            {
                var xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
                var yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
                LastMousePosition = position;

                CameraYaw += xOffset;
                CameraPitch -= yOffset;

                //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
                CameraPitch = Math.Clamp(CameraPitch, -89.0f, 89.0f);

                CameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(CameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch));
                CameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(CameraPitch));
                CameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(CameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch));
                CameraFront = Vector3.Normalize(CameraDirection);
            }
        }

        private static unsafe void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            //We don't want to be able to zoom in too close or too far away so clamp to these values
            CameraZoom = Math.Clamp(CameraZoom - scrollWheel.Y, 1.0f, 45f);
        }

        public static unsafe mat4 GetViewMatrix()
        {
            return mat4.LookAt(new vec3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z), 
            new vec3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z) + 
            new vec3(CameraFront.X, CameraFront.Y, CameraFront.Z), new vec3(CameraUp.X, CameraUp.Y, CameraUp.Z));
        }

    }


}