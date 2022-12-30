using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace Engine
{
    public class TextureAtlas
    {
        public readonly float[] Coordinates;
        public readonly int Handle;
        public int Height;
        public int Width;

        public TextureAtlas(List<string> fileNames)
        {
            throw new NotImplementedException();
        }

        private void FlipImageX(byte[] pixelBytes, int x, int y)
        {
            for (int i = 0; i < y; i += 1)
            {
                for (int j = 0; j < x; j++)
                {
                    byte t = pixelBytes[i * x + j];
                    pixelBytes[i * x + j] = pixelBytes[i * x + x - j - 1];
                    pixelBytes[i * x + x - j - 1] = t;
                }
            }
        }


        public TextureAtlas(Image<Rgba32> atlas, int xCount, int yCount)
        {
            Width = xCount;
            Height = yCount;
            var handle = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            atlas.Mutate(x => x.Flip(FlipMode.Vertical));
            byte[] pixelBytes = new byte[atlas.Width * atlas.Height * 4];
            atlas.CopyPixelDataTo(pixelBytes);
            atlas.Mutate(x => x.Flip(FlipMode.Vertical));

            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                atlas.Width,
                atlas.Height,
                0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixelBytes);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            //atlas.UnlockBits(data);

            Coordinates = new float[xCount * yCount * 8];
            FillCoords(xCount, yCount);

            Handle = handle;
        }

        public void Use(TextureUnit unit)
        {
            if (Handle != GlobalOptions.lastTextureUsed[unit - TextureUnit.Texture0])
            {
                GL.ActiveTexture(unit);
                GL.BindTexture(TextureTarget.Texture2D, Handle);
                if (GlobalOptions.full_debug)
                    Misc.Log("Texture " + Convert.ToString(Handle) + " loaded to slot " +
                             Convert.ToString((int)unit - 33984));

                GlobalOptions.lastTextureUsed[unit - TextureUnit.Texture0] = Handle;
            }
        }

        private void FillCoords(int x, int y)
        {
            for (var i = 0; i < x; i++)
                for (var j = 0; j < y; j++)
                {
                    // top right x
                    Coordinates[(x * j + i) * 8] = (i + 1) / (float)x;
                    // top right y
                    Coordinates[(x * j + i) * 8 + 1] = (j + 1) / (float)y;
                    // bottom right x
                    Coordinates[(x * j + i) * 8 + 2] = (i + 1) / (float)x;
                    // bottom right y
                    Coordinates[(x * j + i) * 8 + 3] = j / (float)y;
                    // bottom left x
                    Coordinates[(x * j + i) * 8 + 4] = i / (float)x;
                    // bottom left y
                    Coordinates[(x * j + i) * 8 + 5] = j / (float)y;
                    // top left x
                    Coordinates[(x * j + i) * 8 + 6] = i / (float)x;
                    // top left y
                    Coordinates[(x * j + i) * 8 + 7] = (j + 1) / (float)y;
                }
        }
    }
}