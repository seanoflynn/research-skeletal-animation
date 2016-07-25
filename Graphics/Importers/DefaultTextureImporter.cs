using System;
using System.IO;
using System.Drawing;

namespace Graphics.Importers
{
    public class DefaultTextureImporter : AssetImporter
    {
        public Type AssetType { get; } = typeof(Texture);
        public string[] FileExtensions { get; } = new string[] { ".bmp", ".exif", ".tiff", ".png", ".gif", ".jpg", ".jpeg" };

        public void Import(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string file = Path.GetFileName(path);

            Assets.Register(new Texture(name, file, new Bitmap(path)));
        }
    }
}