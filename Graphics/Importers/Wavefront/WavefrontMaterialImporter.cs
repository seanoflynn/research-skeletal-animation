using System;
using System.IO;
using System.Linq;
using OpenTK;

namespace Graphics.Importers.Wavefront
{    
    public class WavefrontMaterialImporter : AssetImporter
    {
        private const string CommentFlag = "#";
        private const string AmbientColorFlag = "Ka";
        private const string DiffuseColorFlag = "Kd";
        private const string SpecularColorFlag = "Ks";
        private const string AlphaFlag = "d";
        private const string InverseAlphaFlag = "Tr";
        private readonly string[] ShininessFlags = { "Ns", "Ni" };
        private const string IlluminationModeFlag = "illum";
        private const string AmbientTexture = "map_Ka";
        private const string DiffuseTexture = "map_Kd";
        private const string SpecularTexture = "map_Ks";
        private const string AlphaTexture = "map_d";
        private const string NewMaterialFlag = "newmtl";
        private readonly string[] BumpTextureFlags = { "map_bump", "bump" };

        public Type AssetType { get; } = typeof(Material);
        public string[] FileExtensions { get; } = new string[] { ".mtl" };

        public void Import(string path)
        {
            string file = Path.GetFileNameWithoutExtension(path);

            Material currentMaterial = null;

            using(StreamReader reader = new StreamReader(path))
            {
                while(reader.Peek() >= 0)
                {
                    string line = reader.ReadLine().TrimStart();

                    if(line.StartsWith(CommentFlag, StringComparison.InvariantCulture))
                        continue;

                    // split on whitespace
                    string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                    if(parts.Length < 1)
                        continue;

                    if(parts[0] == NewMaterialFlag)
                    {
                        currentMaterial = new Material(parts[1], file);
                        Assets.Register(currentMaterial);
                    }
                    else if(parts[0] == AmbientColorFlag)
                        currentMaterial.AmbientColor = new Vector4(Convert.ToSingle(parts[1]), Convert.ToSingle(parts[2]), Convert.ToSingle(parts[3]), 1.0F);
                    else if(parts[0] == DiffuseColorFlag)
                        currentMaterial.DiffuseColor = new Vector4(Convert.ToSingle(parts[1]), Convert.ToSingle(parts[2]), Convert.ToSingle(parts[3]), 1.0F);
                    else if(parts[0] == SpecularColorFlag)
                        currentMaterial.SpecularColor = new Vector4(Convert.ToSingle(parts[1]), Convert.ToSingle(parts[2]), Convert.ToSingle(parts[3]), 1.0F);
                    else if(parts[0] == AlphaFlag)
                        currentMaterial.Alpha = Convert.ToSingle(parts[1]);
                    else if(parts[0] == InverseAlphaFlag)
                        currentMaterial.Alpha = 1.0f - Convert.ToSingle(parts[1]);
                    else if(ShininessFlags.Contains(parts[0]))
                        currentMaterial.Shininess = Convert.ToSingle(parts[1]);
                    else if(parts[0] == IlluminationModeFlag)
                        currentMaterial.IlluminationMode = Convert.ToInt32(parts[1]);
                    else if(parts[0] == AmbientTexture)
                        currentMaterial.AmbientTexture = Assets.RetrieveFile<Texture>(parts[1]);
                    else if(parts[0] == DiffuseTexture)
                        currentMaterial.DiffuseTexture = Assets.RetrieveFile<Texture>(parts[1]);
                    else if(parts[0] == SpecularTexture)
                        currentMaterial.SpecularTexture = Assets.RetrieveFile<Texture>(parts[1]);
                    else if(parts[0] == AlphaTexture)
                        currentMaterial.AlphaTexture = Assets.RetrieveFile<Texture>(parts[1]);
                    else if(BumpTextureFlags.Contains(parts[0]))
                        currentMaterial.BumpTexture = Assets.RetrieveFile<Texture>(parts[1]);
                }
            }
        }
    }
}