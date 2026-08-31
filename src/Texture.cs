using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using System;
using StbImageSharp;

namespace Tutorial
{
    public class Texture : IDisposable
    {
        private uint _handle;
        private GL _gl;

        public string Path { get; set; }
        public TextureType Type { get; set; }

        public unsafe Texture(){}

        public unsafe Texture(GL gl, string path, TextureType type = TextureType.None)
        {
            _gl = gl;
            Path = path;
            Type = type;
            _handle = _gl.GenTexture();
            Bind();


            string substr = path.Substring(path.Length-3);

            if (substr == ".hdr")
            {
                using (var stream = System.IO.File.OpenRead(path)){
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba32f, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.Float, image.Data);}
                SetParameters(TextureTarget.Texture2D); 
            }
            else
            {
                using (var stream = System.IO.File.OpenRead(path)){
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);}
                SetParameters(TextureTarget.Texture2D);
                
            }

            Console.WriteLine($"Hello {substr}",substr);


            
        }

        public unsafe void CubeTexture(GL gl, string[] path, TextureType type = TextureType.None)
        {
            _gl = gl;
            string[] Path = path;
            Type = type;
            _handle = _gl.GenTexture();
            Bind();

            for (int i = 0; i < Path.Length; i++)
            {
                using (var stream = System.IO.File.OpenRead(Path[i]))
                {
                    ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                    gl.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, InternalFormat.Rgba8, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
                }
            }
            SetParameters(TextureTarget.TextureCubeMap);
        }

        public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
        {
            _gl = gl;

            _handle = _gl.GenTexture();
            Bind();

            fixed (void* d = &data[0])
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
                SetParameters(TextureTarget.Texture2D);
            }
        }

        private void SetParameters(TextureTarget target)
        {
            _gl.TexParameter(target, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(target, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(target, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
            _gl.TexParameter(target, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            _gl.TexParameter(target, TextureParameterName.TextureBaseLevel, 0);
            _gl.TexParameter(target, TextureParameterName.TextureMaxLevel, 8);
            if (target == TextureTarget.TextureCubeMap)
            {
                _gl.TexParameter(target, TextureParameterName.TextureWrapR, (int)GLEnum.ClampToEdge);
            }
            _gl.GenerateMipmap(target);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            _gl.ActiveTexture(textureSlot);
            _gl.BindTexture(TextureTarget.Texture2D, _handle);
        }

        public void BindCubeMap(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            _gl.ActiveTexture(textureSlot);
            _gl.BindTexture(TextureTarget.TextureCubeMap, _handle);
        }

        public void Dispose()
        {
            _gl.DeleteTexture(_handle);
        }
    }
}