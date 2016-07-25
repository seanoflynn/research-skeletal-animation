using System;
using System.IO;

namespace Graphics.Importers
{    
    public class DefaultFragmentShaderImporter : AssetImporter
    {
        public Type AssetType { get; } = typeof(FragmentShader);
        public string[] FileExtensions { get; } = new string[] { ".fs" };

        public void Import(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string file = Path.GetFileName(path);

            Assets.Register(new FragmentShader(name, file, File.ReadAllText(path)));
        }
    }
}