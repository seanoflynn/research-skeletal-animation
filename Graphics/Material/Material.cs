using OpenTK;

namespace Graphics
{    
    public class Material : Asset
    {
        public string Name { get; private set; }
        public string File { get; private set; }

        public Vector4 AmbientColor;
        public Vector4 DiffuseColor;
        public Vector4 SpecularColor;
        public Vector4 EmissionColor;

        public float Alpha;
        public float Shininess;

        public int IlluminationMode;

        public Texture AmbientTexture;
        public Texture DiffuseTexture;
        public Texture SpecularTexture;
        public Texture AlphaTexture;
        public Texture BumpTexture;
        public Texture NormalTexture;
        public Texture HeightTexture;

        public Material(string name, string file)
        {
            Name = name;
            File = file;
        }

        public static Material Blank = new Material("", "") { DiffuseTexture = Texture.Blank };
    }
}

