using System;
using System.IO;
using System.Numerics;
using GlmSharp;
using Silk.NET.OpenGL;

namespace Tutorial
{
    public class Shader : IDisposable
    {
        private uint _handle;
        private GL _gl;

        public Shader(GL gl, string vertexPath, string fragmentPath)
        {
            _gl = gl;

            uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
            uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
            _handle = _gl.CreateProgram();
            _gl.AttachShader(_handle, vertex);
            _gl.AttachShader(_handle, fragment);
            _gl.LinkProgram(_handle);
            _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
            }
            _gl.DetachShader(_handle, vertex);
            _gl.DetachShader(_handle, fragment);
            _gl.DeleteShader(vertex);
            _gl.DeleteShader(fragment);
        }

        public void Use()
        {
            _gl.UseProgram(_handle);
        }

        public unsafe void SetUniformMat3(string name, Matrix4x4 value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1) return;

            float[] mat3 = new float[9]
            {
        value.M11, value.M12, value.M13,
        value.M21, value.M22, value.M23,
        value.M31, value.M32, value.M33
            };

            fixed (float* ptr = mat3)
            {
                _gl.UniformMatrix3(location, 1, false, ptr);
            }
        }

        public void SetUniform(string name, int value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                return;
            }
            _gl.Uniform1(location, value);
        }
        public void SetUniform(string name, Vector3 value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                return;
            }
            _gl.Uniform3(location, value);
        }
        public unsafe void SetUniform(string name, Matrix4x4 value)
        {
            //A new overload has been created for setting a uniform so we can use the transform in our shader.
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                return;
            }
            _gl.UniformMatrix4(location, 1, false, (float*)&value);
        }


        public void SetUniform(string name, float value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                return;
            }
            _gl.Uniform1(location, value);
        }

        public void Dispose()
        {
            _gl.DeleteProgram(_handle);
        }

        private uint LoadShader(ShaderType type, string path)
        {
            string src = File.ReadAllText(path);
            uint handle = _gl.CreateShader(type);
            _gl.ShaderSource(handle, src);
            _gl.CompileShader(handle);

            _gl.GetShader(handle, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string infoLog = _gl.GetShaderInfoLog(handle);
                _gl.DeleteShader(handle); // don't leak the failed shader object
                throw new Exception($"Error compiling shader of type {type}, failed with error: {infoLog}");
            }

            return handle;
        }
    }
}