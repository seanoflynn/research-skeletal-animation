using System;
using System.Collections.Generic;
using OpenTK;

namespace Graphics.Util
{
    public static class Mesh
    {
        public static Graphics.Mesh GenerateSkeletonMesh(Skeleton skeleton)
        {
            List<Vertex> verts = new List<Vertex>();
            List<int> els = new List<int>();

            for(int i = 0; i < skeleton.Bones.Count; i++)
            {
                int pi = skeleton.Bones[i].Parent?.Index ?? 0;

                verts.AddRange(new[] { CreateBoneVertex(skeleton, i), CreateBoneVertex(skeleton, i, 0.5f), CreateBoneVertex(skeleton, pi) });
                els.AddRange(new[] { i * 3, i * 3 + 1, i * 3 + 2 });
            }

            return new Graphics.Mesh()
            {
                Name = "Skeleton",
                Vertices = verts,
                Elements = els,
                RenderMode = RenderMode.None,
                Material = Material.Blank
            };
        }

        private static SkeletalVertex CreateBoneVertex(Skeleton skeleton, int boneIndex, float offset = 0.0f)
        {
            return new SkeletalVertex()
            {
                Position = skeleton.BindPose.Position(boneIndex) + (Vector3.One * offset),
                Weights = new List<SkeletalWeight>() { new SkeletalWeight() { Bias = 1.0f, BoneIndex = boneIndex, Position = Vector3.Zero } }
            };
        }
    }
}