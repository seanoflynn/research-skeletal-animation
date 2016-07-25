using OpenTK;

namespace Graphics
{
    public class Vertex
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Normal = Vector3.Zero;
        public Vector2 TextureCoordinates = Vector2.Zero;

        public virtual float[] FloatArray()
        {
            return new float[] {
                    Position.X, Position.Y, Position.Z,
                    TextureCoordinates.X, TextureCoordinates.Y
            };
        }

        public override string ToString()
        {
            return string.Format("[Vertex: P({0:D2},{1},{2}), N({3},{4},{5}), TC({6},{7})]",
                                 Position.X, Position.Y, Position.Z,
                                 Normal.X, Normal.Y, Normal.Z,
                                 TextureCoordinates.X, TextureCoordinates.Y);
        }
    }
}