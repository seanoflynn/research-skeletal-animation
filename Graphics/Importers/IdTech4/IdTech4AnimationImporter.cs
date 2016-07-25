using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;

namespace Graphics.Importers.IdTech4
{
    public class IdTech4AnimationImporter : AssetImporter
    {
        private const string VersionFlag = "MD5Version";
        private const string FrameCountFlag = "numFrames";
        private const string BoneCountFlag = "numJoints";
        private const string FrameRateFlag = "frameRate";
        private const string AnimatedComponentsFlag = "numAnimatedComponents";
        private const string BonesStartFlag = "hierarchy";
        private const string BonesEndFlag = "}";
        private const string BoundsStartFlag = "bounds";
        private const string BoundsEndFlag = "}";
        private const string BasePoseStart = "baseframe";
        private const string BasePoseEnd = "}";
        private const string PoseStart = "frame";
        private const string PoseEnd = "}";

        public Type AssetType { get; } = typeof(Animation);
        public string[] FileExtensions { get; } = new string[] { ".md5anim" };

        public void Import(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string file = Path.GetFileName(path);

            bool collectingBoneInfo = false;
            bool collectingBounds = false;
            bool collectingBasePose = false;
            bool collectingPose = false;

            int expectedFrameCount = 0;
            int expectedBoneCount = 0;

            int frameRate = 0;

            List<SkeletalBone> bones = new List<SkeletalBone>();
            int boneIndex = 0;

            Skeleton basePose = null;
            int basePoseBoneIndex = 0;

            List<Pose> frames = new List<Pose>();
            int poseIndex = 0;

            SkeletalPose pose = null;
            int poseBoneIndex = 0;

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

                    if(parts[0] == FrameCountFlag)
                    {
                        expectedFrameCount = Convert.ToInt32(parts[1]);
                    }
                    else if(parts[0] == BoneCountFlag)
                    {
                        expectedBoneCount = Convert.ToInt32(parts[1]);
                        basePose = new Skeleton(expectedBoneCount);

                        if(expectedBoneCount > SkeletalModel.MaximumBonesPerModel)
                            throw new NotSupportedException($"only id Tech 4 (.md5mesh/.md5anim) models with less than {SkeletalModel.MaximumBonesPerModel + 1} are supported");
                    }
                    else if(parts[0] == FrameRateFlag)
                    {
                        frameRate = Convert.ToInt32(parts[1]);
                    }
                    else if(parts[0] == AnimatedComponentsFlag)
                    { }
                    // bones
                    else if(parts[0] == BonesStartFlag)
                    {
                        collectingBoneInfo = true;
                    }
                    else if(collectingBoneInfo && parts[0] == BonesEndFlag)
                    {
                        collectingBoneInfo = false;

                        if(bones.Count != expectedBoneCount)
                            throw new FormatException("incorrect number of bones/joints"); 
                    }
                    else if(collectingBoneInfo && parts[0] != BonesEndFlag)
                    {
                        // "name" parent flags startIndex
                        int parentBoneIndex = Convert.ToInt32(parts[1]);
                        bones.Add(new SkeletalBone()
                        {
                            Index = boneIndex,
                            Name = parts[0],
                            Parent = parentBoneIndex < 0 ? null : bones[parentBoneIndex]
                        });
                        boneIndex++;
                    }
                    // bounds
                    else if(parts[0] == BoundsStartFlag)
                    {
                        // start collecting bounds
                        collectingBounds = true;
                    }
                    else if(collectingBounds && parts[0] == BoundsEndFlag)
                    {
                        collectingBounds = false;
                    }
                    else if(collectingBounds)
                    { }
                    // poses
                    else if(parts[0] == BasePoseStart)
                    {
                        collectingBasePose = true;
                    }
                    else if(collectingBasePose && parts[0] == BasePoseEnd)
                    {
                        collectingBasePose = false;
                    }
                    else if(collectingBasePose)
                    {
                        // ( x y z ) ( rx ry rz )
                        basePose.Bones.Add(new SkeletalBone()
                        {
                            Parent = bones[basePoseBoneIndex].Parent,
                            Name = bones[basePoseBoneIndex].Name
                        });
                        basePoseBoneIndex++;
                    }
                    else if(parts[0] == PoseStart)
                    {
                        // start collecting another frame
                        collectingPose = true;
                        pose = new SkeletalPose(expectedBoneCount);
                        pose.Name = name + "." + poseIndex;
                        frames.Add(pose);
                        poseBoneIndex = 0;
                    }
                    else if(collectingPose && parts[0] == PoseEnd)
                    {                        
                        collectingPose = false;
                        poseIndex++;
                    }
                    else if(collectingPose)
                    {
                        // px py pz rz ry rz
                        var pos = new Vector3(Convert.ToSingle(parts[0]), Convert.ToSingle(parts[1]), Convert.ToSingle(parts[2]));
                        var rot = Util.Math.ComputeW(new Quaternion(Convert.ToSingle(parts[3]), Convert.ToSingle(parts[4]), Convert.ToSingle(parts[5]), 0.0f));
                        pose.Set(poseBoneIndex, pos, rot);

                        SkeletalBone parent = bones[poseBoneIndex].Parent;

                        // convert to model space
                        if(parent != null)
                            pose[poseBoneIndex] = pose[poseBoneIndex] * pose[parent.Index];

                        poseBoneIndex++;
                    }
                }
            }

            if(frames.Count != expectedFrameCount)
                throw new FormatException("incorrect number of frames");

            Assets.Register(new Animation(name, file)
            {
                FrameRate = frameRate,
                FrameCount = frames.Count,
                Frames = frames
            });
        }

        /*public void ExportAnimation(Animation model, string path)
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