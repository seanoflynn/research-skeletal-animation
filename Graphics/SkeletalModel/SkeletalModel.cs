using System;
using System.Collections.Generic;
using OpenTK;

namespace Graphics
{
    public class SkeletalModel : Model
    {
        public const int MaximumWeightsPerVertex = 4;
        public const int MaximumBonesPerModel = 50;

        public Skeleton Skeleton { get; private set; }

        private Dictionary<string, List<SkeletalPose>> animationPoses = new Dictionary<string, List<SkeletalPose>>();
        private Dictionary<string, SkeletalPose> posePoses = new Dictionary<string, SkeletalPose>();

        static SkeletalModel()
        {
            Assets.Register(new ShaderProgram("Skeletal", "Skeletal", "Default"));
        }

        public SkeletalModel(string name, string file, Skeleton skeleton, List<Mesh> meshes) 
            : base(name, file, meshes, "Skeletal")
        {
            Skeleton = skeleton;
            AddPose(skeleton.BindPose.Clone("Default"));

            shaderProgram.AddUniform("Bones");

            Meshes.Add(Util.Mesh.GenerateSkeletonMesh(skeleton));
        }

        #region Setup

        protected override void SetShaderAttributes()
        {
            int stride = (5 + (2 * MaximumWeightsPerVertex)) * sizeof(float);
            shaderProgram.AddAttribute("Position", 3, stride, 0);
            shaderProgram.AddAttribute("TextureCoordinates", 2, stride, 3 * sizeof(float));
            shaderProgram.AddAttribute("Index", MaximumWeightsPerVertex, stride, 5 * sizeof(float));
            shaderProgram.AddAttribute("Weight", MaximumWeightsPerVertex, stride, (5 + MaximumWeightsPerVertex) * sizeof(float));
        }

        #endregion

        #region Animation

        public override void AddPose(Pose pose)
        {
            Poses.Add(pose);

            // precompute the matrix for each joint for each animation frame 
            SkeletalPose p = pose as SkeletalPose;
            SkeletalPose calculatedPose = new SkeletalPose(p.MatrixArray.Length);

            // multiply each animation joint matrix by its relevant inverse bind pose joint matrix
            for(int i = 0; i < calculatedPose.MatrixArray.Length; i++)
                calculatedPose[i] = Skeleton.InverseBindPose[i] * p[i];

            posePoses.Add(pose.Name, calculatedPose);
        }

        public override void AddAnimation(Animation animation)
        {            
            Animations.Add(animation);

            // precompute the matrix for each joint for each animation frame 
            List<SkeletalPose> poses = new List<SkeletalPose>();
            foreach(var a in animation.Frames)
            {
                SkeletalPose f = a as SkeletalPose;
                SkeletalPose pose = new SkeletalPose(f.MatrixArray.Length);

                // multiply each animation joint matrix by its relevant inverse bind pose joint matrix
                for(int i = 0; i < f.MatrixArray.Length; i++)
                    pose[i] = Skeleton.InverseBindPose[i] * f[i];

                poses.Add(pose);
            }

            animationPoses.Add(animation.Name, poses);
        }

        public override void SetPose(string pose)
        {
            shaderProgram.SetUniform("Bones", posePoses[pose].MatrixArray);
        }

        public override void SetAnimationFrame(string animation, float frame)
        {
            int prevFrame = Convert.ToInt32(Math.Floor(frame));
            int nextFrame = Convert.ToInt32(Math.Ceiling(frame));

            // we're sitting on an exact frame
            if(prevFrame == nextFrame)
            {
                shaderProgram.SetUniform("Bones", animationPoses[animation][nextFrame].MatrixArray);
                return;
            }

            if(nextFrame >= animationPoses[animation].Count)
                nextFrame = 0;

            // we need to interpolate between framese
            float blend = Convert.ToSingle(frame % 1.0);

            Matrix4[] prev = animationPoses[animation][prevFrame].MatrixArray;
            Matrix4[] next = animationPoses[animation][nextFrame].MatrixArray;

            Matrix4[] inter = Util.Math.InterpolateMatrix(prev, next, blend);

            shaderProgram.SetUniform("Bones", inter);
        }

        #endregion
    }    
}