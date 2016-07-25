using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Graphics
{
    public class Texture : Asset
    {
        public static Texture Blank = new Texture("", "", Util.Bitmap.CreateBlank(1, 1, Vector4.One));

        public string Name { get; private set; }
        public string File { get; private set; }

        private int id = -1;

        public Texture(string name, string file, Bitmap bitmap)
        {
            Name = name;
            File = file;
            id = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, 
                                              System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                          OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, this.id);
        }
    }
}