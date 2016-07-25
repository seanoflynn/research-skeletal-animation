using System;
using OpenTK;

namespace Graphics.Util
{
    public static class Math
    {
        public static Quaternion ComputeW(Quaternion q)
        {
            float t = 1.0f - (q.X * q.X) - (q.Y * q.Y) - (q.Z * q.Z);
            float w = 0.0f;
            if(t >= 0.0f)
                w = -Convert.ToSingle(System.Math.Sqrt(t));

            return new Quaternion(q.Xyz, w);
        }

        public static Matrix4[] InterpolateMatrix(Matrix4[] prev, Matrix4[] next, float blend)
        {
            Matrix4[] result = new Matrix4[prev.Length];

            for(int i = 0; i < prev.Length; i++)
            {
                Vector3 positionInter = Vector3.Lerp(prev[i].ExtractTranslation(), next[i].ExtractTranslation(), blend);
                Vector3 scaleInter = Vector3.Lerp(prev[i].ExtractScale(), next[i].ExtractScale(), blend);
                Quaternion rotationInter = Quaternion.Slerp(prev[i].ExtractRotation(), next[i].ExtractRotation(), blend);

                result[i] = Matrix4.CreateFromQuaternion(rotationInter) * Matrix4.CreateTranslation(positionInter) * Matrix4.CreateScale(scaleInter);
            }
            return result;
        }
    }
}