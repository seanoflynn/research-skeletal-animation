using System;
using System.IO;

namespace Graphics.Importers
{
    public class DefaultVertexShaderImporter : AssetImporter
    {
        public Type AssetType { get; } = typeof(VertexShader);
        public string[] FileExtensions { get; } = new string[] { ".vs" };

        public void Import(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string file = Path.GetFileName(path);

            Assets.Register(new VertexShader(name, file, File.ReadAllText(path)));
        }
    }
}