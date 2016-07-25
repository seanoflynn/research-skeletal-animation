using OpenTK;

namespace Graphics
{
    public class SkeletalPose : Pose
    {
        private Matrix4[] boneTransformations;

        public Matrix4 this[int i]
        {
            get { return boneTransformations[i]; }
            set { boneTransformations[i] = value; }
        }

        public Matrix4[] MatrixArray { get { return boneTransformations; } }

        #region Constructors

        public SkeletalPose(int count)
        {
            boneTransformations = new Matrix4[count];
        }

        public SkeletalPose(int count, Matrix4 template)
        {
            boneTransformations = new Matrix4[count];
            for(int i = 0; i < count; i++)
                boneTransformations[i] = template;
        }

        public SkeletalPose(Matrix4[] matrices)
        {
            boneTransformations = matrices;
        }

        #endregion

        #region Conversion

        public Vector3 Position(int boneIndex)
        {
            return boneTransformations[boneIndex].ExtractTranslation();
        }

        public Quaternion Rotation(int boneIndex)
        {
            return boneTransformations[boneIndex].ExtractRotation();
        }

        public void Set(int boneIndex, Vector3 position, Quaternion rotation)
        {
            boneTransformations[boneIndex] = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(position);
        }

        #endregion

        #region Clone

        public override Pose Clone(string name)
        {
            return new SkeletalPose((Matrix4[])this.boneTransformations.Clone()) { Name = name };
        }

        #endregion
    }
}

