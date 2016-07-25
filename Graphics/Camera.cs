using OpenTK;

namespace Graphics
{
    public class Camera : WorldObject
    {
        protected override void RecalculateTransformation()
        {
            transformation = Matrix4.CreateTranslation(position * -1.0f) * Matrix4.CreateScale(scale) * Matrix4.CreateFromQuaternion(rotation);
        }
    }
}

