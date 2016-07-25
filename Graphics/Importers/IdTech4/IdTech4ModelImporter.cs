using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OpenTK;

namespace Graphics.Importers.IdTech4
{
    public class IdTech4ModelImporter : AssetImporter
    {
        private const string VersionFlag = "MD5Version";
        private const string MeshCountFlag = "numMeshes";
        private const string BoneCountFlag = "numJoints";
        private const string BonesStartFlag = "joints";
        private const string BonesEndFlag = "}";
        private const string MeshStartFlag = "mesh";
        private const string MeshEndFlag = "}";
        private const string VertexCountFlag = "numverts";
        private const string TriangleCountFlag = "numtris";
        private const string WeightCountFlag = "numweights";
        private const string TriangleFlag = "tri";
        private const string WeightFlag = "weight";
        private const string ShaderFlag = "shader";
        private const string VertexFlag = "vert";
        private const string DiffuseTextureSuffix = "_d";
        private const string SpecularTextureSuffix = "_s";
        private const string NormalTextureSuffix = "_local";
        private const string HeighTextureSuffix = "_h";

        public Type AssetType { get; } = typeof(Model);
        public string[] FileExtensions { get; } = new string[] { ".md5mesh" };

        public void Import(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string file = Path.GetFileName(path);

            // skeleton
            int expectedBoneCount = 0;
            bool collectingBones = false;
            Skeleton skeleton = null;
            int boneIndex = 0;

            // meshes
            int expectedMeshCount = 0;
            bool collectingMesh = false;
            List<Mesh> meshes = null;

            // mesh
            Mesh currentMesh = null;
            List<SkeletalVertex> weightToVertex = new List<SkeletalVertex>();
            int expectedVertexCount = 0;
            int expectedFaceCount = 0;
            int expectedWeightCount = 0;

            using(StreamReader reader = new StreamReader(path))
            {
                while(reader.Peek() >= 0)
                {
                    string line = reader.ReadLine().TrimStart();

                    // split on whitespace
                    string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                    if(parts.Length < 1)
                        continue;

                    if(parts[0] == VersionFlag && parts[1] != "10")
                    {
                        throw new NotSupportedException("only id Tech 4 (.md5mesh) version 10 supported");
                    }

                    if(parts[0] == BoneCountFlag)
                    {
                        expectedBoneCount = Convert.ToInt32(parts[1]);
                        skeleton = new Skeleton(expectedBoneCount);

                        if(expectedBoneCount > SkeletalModel.MaximumBonesPerModel)
                            throw new NotSupportedException($"only id Tech 4 (.md5mesh/.md5anim) models with less than {SkeletalModel.MaximumBonesPerModel + 1} are supported");
                    }
                    else if(parts[0] == MeshCountFlag)
                    {
                        expectedMeshCount = Convert.ToInt32(parts[1]);
                        meshes = new List<Mesh>(expectedMeshCount);
                    }
                    // bones
                    else if(parts[0] == BonesStartFlag)
                    {
                        collectingBones = true;
                    }
                    else if(collectingBones && parts[0] == BonesEndFlag)
                    {
                        collectingBones = false;

                        if(skeleton.Bones.Count != expectedBoneCount)
                            throw new FormatException("incorrect number of bones/joints");                        
                    }
                    else if(collectingBones)
                    {
                        // "name" parent ( px py pz ) ( rx ry rz )
                        int parentBoneIndex = Convert.ToInt32(parts[1]);
                        skeleton.Bones.Add(new SkeletalBone()
                        {
                            Index = boneIndex,
                            Name = parts[0].Replace("\"",""),
                            Parent = parentBoneIndex < 0 ? null : skeleton.Bones[parentBoneIndex]
                        });

                        var position = new Vector3(Convert.ToSingle(parts[3]), Convert.ToSingle(parts[4]), Convert.ToSingle(parts[5]));
                        var rotation = Util.Math.ComputeW(new Quaternion(Convert.ToSingle(parts[8]), Convert.ToSingle(parts[9]), Convert.ToSingle(parts[10]), 0.0f));

                        skeleton.BindPose.Set(boneIndex, position, rotation);
                        skeleton.InverseBindPose[boneIndex] = skeleton.BindPose[boneIndex].Inverted();
                        boneIndex++;
                    }
                    // mesh
                    else if(parts[0] == MeshStartFlag)
                    {
                        collectingMesh = true;

                        currentMesh = new Mesh();
                        meshes.Add(currentMesh);

                        weightToVertex.Clear();
                    }

                    else if(collectingMesh && parts[0] == MeshEndFlag)
                    {
                        collectingMesh = false;

                        if(expectedVertexCount != currentMesh.Vertices.Count)
                            throw new FormatException("incorrect number of vertices for mesh '" + currentMesh.Name + "', expected=" + expectedVertexCount + ", actual=" + currentMesh.Vertices.Count);
                        if(expectedFaceCount * 3 != currentMesh.Elements.Count)
                            throw new FormatException("incorrect number of faces for mesh '" + currentMesh.Name + "', expected=" + expectedFaceCount + ", actual=" + currentMesh.Elements.Count);
                        if(expectedWeightCount != currentMesh.Vertices.Sum(x => ((SkeletalVertex)x).Weights.Count))
                            throw new FormatException("incorrect number of weights for mesh '" + currentMesh.Name + "', expected=" + expectedWeightCount + ", actual=" + currentMesh.Vertices.Sum(x => ((SkeletalVertex)x).Weights.Count));
                    }
                    else if(parts[0] == VertexCountFlag)
                    {
                        expectedVertexCount = Convert.ToInt32(parts[1]);
                        currentMesh.Vertices = new List<Vertex>(expectedVertexCount);
                    }
                    else if(parts[0] == TriangleCountFlag)
                    {
                        expectedFaceCount = Convert.ToInt32(parts[1]);
                        currentMesh.Elements = new List<int>(expectedFaceCount * 3);
                    }
                    else if(parts[0] == WeightCountFlag)
                    {
                        expectedWeightCount = Convert.ToInt32(parts[1]);
                    }
                    else if(parts[0] == VertexFlag)
                    {
                        // vert index ( u v ) startWeight weightCount
                        SkeletalVertex v = new SkeletalVertex()
                        {
                            TextureCoordinates = new Vector2(Convert.ToSingle(parts[3]), Convert.ToSingle(parts[4])),
                            Weights = new List<SkeletalWeight>()
                        };
                        currentMesh.Vertices.Add(v);

                        int weightCount = Convert.ToInt32(parts[7]);
                        for(int i = 0; i < weightCount; i++)
                            weightToVertex.Add(v);
                    }
                    else if(parts[0] == TriangleFlag)
                    {
                        // tri index v0 v1 v2
                        currentMesh.Elements.AddRange(new[] { Convert.ToInt32(parts[2]), Convert.ToInt32(parts[3]), Convert.ToInt32(parts[4]) });
                    }
                    else if(parts[0] == WeightFlag)
                    {
                        // weight index joint bias ( x y z )
                        SkeletalWeight w = new SkeletalWeight()
                        {
                            BoneIndex = Convert.ToInt32(parts[2]),
                            Bias = Convert.ToSingle(parts[3]),
                            Position = new Vector3(Convert.ToSingle(parts[5]), Convert.ToSingle(parts[6]), Convert.ToSingle(parts[7]))
                        };

                        int id = Convert.ToInt32(parts[1]);

                        SkeletalVertex v = weightToVertex[id];
                        v.Weights.Add(w);

                        var rotpos = Vector3.Transform(w.Position, skeleton.BindPose.Rotation(w.BoneIndex));
                        v.Position += (skeleton.BindPose.Position(w.BoneIndex) + rotpos) * w.Bias;
                    }
                    else if(parts[0] == ShaderFlag)
                    {
                        // shader "file"
                        string materialFile = parts[1].Replace("\"", "");
                        bool isDiffuseOnly = Path.GetExtension(materialFile) != String.Empty;
                        currentMesh.Name = Path.GetFileNameWithoutExtension(materialFile);
                        currentMesh.Material = new Material(currentMesh.Name, "");

                        if(isDiffuseOnly)
                        {
                            currentMesh.Material.DiffuseTexture = Assets.RetrieveFile<Texture>(materialFile);
                        }
                        else
                        {   
                            currentMesh.Material.DiffuseTexture = Assets.RetrieveFile<Texture>(materialFile + DiffuseTextureSuffix + ".png");
                            currentMesh.Material.SpecularTexture = Assets.RetrieveFile<Texture>(materialFile + SpecularTextureSuffix + ".png");
                            currentMesh.Material.NormalTexture = Assets.RetrieveFile<Texture>(materialFile + NormalTextureSuffix + ".png");
                            currentMesh.Material.HeightTexture = Assets.RetrieveFile<Texture>(materialFile + HeighTextureSuffix + ".png");
                        }
                    }
                }
            }

            if(meshes.Count != expectedMeshCount)
                throw new FormatException("incorrect number of meshes");

            Model model = new SkeletalModel(name, file, skeleton, meshes);
            model.Setup();
            Assets.Register(model);
        }

        /*public void Export(Asset model, string path)
        {
            string f = "0.000000";

            string output = "";

            output += "MD5Version 10\n";
            output += "commandline \"\"\n\n";

            output += "numJoints " + model.Joints.Count + "\n";
            output += "numMeshes " + model.Meshes.Where(x => !x.IsSkeleton).Count() + "\n\n";

            output += "joints {\n";

            foreach(var j in model.Joints)
                output += "\t\"" + j.Name + "\"\t" + model.Joints.IndexOf(j.Parent) + " ( " + j.Position.X.ToString(f) + " " + j.Position.Y.ToString(f) + " " + j.Position.Z.ToString(f) + " ) ( " + j.Rotation.X.ToString(f) + " " + j.Rotation.Y.ToString(f) + " " + j.Rotation.Z.ToString(f) + " )\n";            

            output += "}\n\n";

            foreach(var m in model.Meshes)
            {
                if(m.IsSkeleton)
                    continue;

                output += "mesh {\n";
                output += "\tshader \"" + m.Material.DiffuseTexture.FilePath + "\"\n\n";

                output += "\tnumverts " + m.Vertices.Count + "\n";
                int weightCount = 0;
                for(int i = 0; i < m.Vertices.Count; i++)
                {
                    FixedVertex v = m.Vertices[i];
                    output += "\tvert " + i + " ( " + v.TextureCoordinate.X.ToString(f) + " " + v.TextureCoordinate.Y.ToString(f) + " ) " + weightCount + " " + v.Weights.Count + "\n";
                    weightCount += v.Weights.Count;
                }

                output += "\n\tnumtris " + m.ElementCount / 3 + "\n";
                for(int i = 0; i < m.ElementCount / 3; i++)
                    output += "\ttri " + i + " " + m.Elements[i * 3] + " " + m.Elements[(i * 3) + 1] + " " + m.Elements[(i * 3) + 2] + "\n";

                output += "\n\tnumweights " + weightCount + "\n";
                int wcount = 0;
                for(int i = 0; i < m.Vertices.Count; i++)
                {
                    foreach(var w in m.Vertices[i].Weights)
                    {
                        output += "\tweight " + wcount + " " + model.Joints.IndexOf(w.Joint) + " " + w.Bias.ToString(f) + " ( " + w.Position.X.ToString(f) + " " + w.Position.Y.ToString(f) + " " + w.Position.Z.ToString(f) + " )\n";
                        wcount++;
                    }
                }

                output += "}\n\n";
            }

            string file = (path != "" ? path : model.Name + ".md5mesh");
            File.WriteAllText( file, output);
        }*/
    }
}