using System;
using System.Collections.Generic;

namespace Graphics
{
    public class SkeletalVertex : Vertex
    {
        public List<SkeletalWeight> Weights;

        public override float[] FloatArray()
        {
            float[] ret = new float[5 + SkeletalModel.MaximumWeightsPerVertex * 2];
            ret[0] = Position.X;
            ret[1] = Position.Y;
            ret[2] = Position.Z;
            ret[3] = TextureCoordinates.X;
            ret[4] = TextureCoordinates.Y;

            for(int i = 0; i < Weights.Count; i++)
            {
                ret[5 + i] = Weights[i].BoneIndex;
                ret[5 + SkeletalModel.MaximumWeightsPerVertex + i] = Weights[i].Bias;
            }

            return ret;
        }
    }
}