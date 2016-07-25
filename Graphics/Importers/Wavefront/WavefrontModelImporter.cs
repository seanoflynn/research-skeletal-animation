using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OpenTK;

namespace Graphics.Importers.Wavefront
{
    public class WavefrontModelImporter : AssetImporter
    {
        private const string CommentFlag = "#";
        private const string ObjectFlag = "o";
        private const string PositionFlag = "v";
        private const string TextureCoordinatesFlag = "vt";
        private const string NormalFlag = "vn";
        private const string FaceFlag = "f";
        private const char FacePartsFlag = '/';
        private const string ShadingModeFlag = "s";
        private const string MaterialLibraryFlag = "mtllib";
        private const string MaterialFlag = "usemtl";

        public Type AssetType { get; } = typeof(Model);
        public string[] FileExtensions { get; } = new string[] { ".obj" };

        public void Import(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string file = Path.GetFileName(path);

            List<Mesh> meshes = new List<Mesh>();
            Mesh currentMesh = null;

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> textureCoordinates = new List<Vector2>();

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

                    if(parts[0] == ObjectFlag)
                    {
                        currentMesh = new Mesh()
                        {
                            Name = parts[1],
                            Vertices = new List<Vertex>(),
                            Elements = new List<int>()
                        };
                        meshes.Add(currentMesh);
                    }
                    else if(parts[0] == PositionFlag)
                    {
                        positions.Add(new Vector3(Convert.ToSingle(parts[1]), Convert.ToSingle(parts[2]), Convert.ToSingle(parts[3])));
                    }
                    else if(parts[0] == TextureCoordinatesFlag)
                    {
                        textureCoordinates.Add(new Vector2(Convert.ToSingle(parts[1]), 1.0f - Convert.ToSingle(parts[2])));
                    }
                    else if(parts[0] == NormalFlag)
                    {
                        normals.Add(new Vector3(Convert.ToSingle(parts[1]), Convert.ToSingle(parts[2]), Convert.ToSingle(parts[3])));
                    }
                    else if(parts[0] == FaceFlag)
                    {
                        for(int i = 1; i < parts.Length; i++)
                        {
                            string[] subParts = parts[i].Split(FacePartsFlag);

                            currentMesh.Vertices.Add(new Vertex()
                            {
                                Position = positions[Convert.ToInt32(subParts[0]) - 1],
                                Normal = normals[Convert.ToInt32(subParts[2]) - 1],
                                TextureCoordinates = textureCoordinates[Convert.ToInt32(subParts[1]) - 1]
                            });
                        }
                    }
                    else if(parts[0] == ShadingModeFlag)
                    { }
                    else if(parts[0] == MaterialLibraryFlag)
                    {
                        Assets.ImportFile<Material>(parts[1]);
                    }
                    else if(parts[0] == MaterialFlag)
                    {
                        currentMesh.Material = Assets.Retrieve<Material>(parts[1]);
                    }
                }
            }
        
            foreach(var mesh in meshes)
            {
                mesh.Elements = Enumerable.Range(0, mesh.Vertices.Count).ToList();

                // remove duplicates
                for(int i = mesh.Vertices.Count - 1; i >= 0; i--)
                {
                    // check if this is the first instance of this vertex
                    var current = mesh.Vertices[i];
                    int firstIndex = mesh.Vertices.FindIndex(x => x.Position == current.Position && 
                                                                  x.Normal == current.Normal && 
                                                                  x.TextureCoordinates == current.TextureCoordinates);
                    if(i == firstIndex)
                        continue;

                    // remove duplicate vertex
                    mesh.Vertices.RemoveAt(i);
                    // remove references to duplicate
                    mesh.Elements = mesh.Elements.Select(x => x == i ? firstIndex : x).ToList();
                    // adjust references to indexes greater than the removed item
                    mesh.Elements = mesh.Elements.Select(x => (x > i ? x - 1 : x)).ToList();
                }
            }

            Model model = new Model(name, file, meshes);
            model.Setup();
            Assets.Register(model);
        }

        /*public void ExportModel(Model model, string path)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> textureCoordinates = new List<Vector2>();

            string output = "";

            output += "# Model Viewer v0.1 OBJ File: ''\n";
            output += "mtllib " + model.Meshes[0].Material.Library + ".mtl\n";

            string f = "0.0000000";

            foreach(var x in model.Meshes)
            {
                Mesh m = x as Mesh;

                output += "o " + m.Name + "\n";

                foreach(var e in m.Elements)
                {
                    Vector3 v = m.Vertices[Convert.ToInt32(e)].Position;
                    if(positions.Contains(v))
                        continue;

                    positions.Add(v);
                    output += "v " + v.X.ToString(f) + " " + v.Y.ToString(f) + " " + v.Z.ToString(f) + "\n";
                }

                foreach(var e in m.Elements)
                {
                    Vector2 v = m.Vertices[Convert.ToInt32(e)].TextureCoordinates;
                    if(textureCoordinates.Contains(v))
                        continue;

                    textureCoordinates.Add(v);
                    output += "vt " + v.X.ToString(f) + " " + v.Y.ToString(f) + "\n";
                }

                foreach(var e in m.Elements)
                {
                    Vector3 v = m.Vertices[Convert.ToInt32(e)].Normal;
                    if(normals.Contains(v))
                        continue;

                    normals.Add(v);
                    output += "vn " + v.X.ToString(f) + " " + v.Y.ToString(f) + " " + v.Z.ToString(f) + "\n";
                }

                output += "usemat " + m.Material.Name + "\n";

                for(int i = 0; i < m.Elements.Count; i += 3)
                {
                    output += "f ";
                    for(int j = 0; j < 3; j++)
                    {
                        Vertex v = m.Vertices[Convert.ToInt32(m.Elements[i + j])];
                        int pi = positions.IndexOf(v.Position) + 1;
                        int ti = textureCoordinates.IndexOf(v.TextureCoordinates) + 1;
                        int ni = normals.IndexOf(v.Normal) + 1;
                        output += pi + "/" + ti + "/" + ni + " ";
                    }
                    output += "\n";
                }
            }

            File.WriteAllText(path + model.Name + ".obj", output);
        }*/
    }
}