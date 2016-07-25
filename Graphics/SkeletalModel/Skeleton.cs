using System.Collections.Generic;
using OpenTK;

namespace Graphics
{    
    public class Skeleton
    {
        public List<SkeletalBone> Bones { get; set; }
        public SkeletalPose Identity { get; set; }
        public SkeletalPose BindPose { get; set; }
        public SkeletalPose InverseBindPose { get; set; }

        public Skeleton(int boneCount)
        {
            Bones = new List<SkeletalBone>(boneCount);
            Identity = new SkeletalPose(boneCount, Matrix4.Identity);
            BindPose = new SkeletalPose(boneCount);
            InverseBindPose = new SkeletalPose(boneCount);
        }
    }
}
