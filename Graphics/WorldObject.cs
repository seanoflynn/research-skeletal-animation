using System;
using OpenTK;

namespace Graphics
{
    public static class Direction
    {
        public static Vector3 Up = Vector3.UnitY;
        public static Vector3 Down = -Vector3.UnitY;

        public static Vector3 Right = Vector3.UnitX;
        public static Vector3 Left = -Vector3.UnitX;

        public static Vector3 Backward = Vector3.UnitZ;
        public static Vector3 Forward = -Vector3.UnitZ;
    }

    public enum Space
    {
        Local,
        World
    }

    public class WorldObject
    {
        #region Fields

        protected Vector3 position = Vector3.Zero;
        protected Vector3 scale = Vector3.One;
        protected Quaternion rotation = Quaternion.Identity;
        protected Matrix4 transformation = Matrix4.Identity;

        public Vector3 Position { get { return position; } set { position = value; RecalculateTransformation(); } }
        public Vector3 Scale { get { return scale; } set { scale = value; RecalculateTransformation(); } }
        public Quaternion Rotation { get { return rotation; } set { rotation = value; RecalculateTransformation(); } }
        public Matrix4 Transformation { get { return transformation; } }

        public Model Model { get; set; }
        private DateTime animationStartTime;
        private Animation currentAnimation;
        private string currentPose;

        #endregion

        #region Constructor

        public WorldObject()
        { }

        public WorldObject(string modelName)
        {
            Model = Assets.Retrieve<Model>(modelName);
        }

        #endregion

        #region Transformations

        protected virtual void RecalculateTransformation()
        {
            transformation = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position);
        }

        public void Move(Vector3 delta, Space space = Space.Local)
        {
            if(space == Space.World)
                position += delta;
            else
                position += rotation.Inverted() * delta;
            RecalculateTransformation();
        }

        public void Rotate(Quaternion rot, Space space = Space.Local)
        {
            if(space == Space.Local)
                rotation = rotation * rot;
            else
                rotation = rot * rotation;
            RecalculateTransformation();
        }

        public void Rotate(Vector3 angles, Space space = Space.Local)
        {
            Rotate(Quaternion.FromEulerAngles(angles), space);
        }

        public void Rotate(Vector3 direction, float angle, Space space = Space.Local)
        {
            Rotate(Quaternion.FromAxisAngle(direction, angle), space);
        }

        public void LookAt(Vector3 position, Vector3 up)
        {
            rotation = Matrix4.LookAt(this.position, position, up).ExtractRotation();
            RecalculateTransformation();
        }

        public void LookAt(Vector3 position)
        {
            LookAt(position, Direction.Up);
        }

        #endregion

        #region Poses & Animation

        public void Pose(string pose)
        {
            currentAnimation = null;
            currentPose = pose;
        }

        public void Animate(string animation, TimeSpan offset = default(TimeSpan))
        {
            currentPose = null;
            currentAnimation = Assets.Retrieve<Animation>(animation);
            animationStartTime = DateTime.UtcNow - offset;
        }

        #endregion

        #region Render

        public void Render(Matrix4 projection, Matrix4 view)
        {
            if(Model == null)
                return;

            if(currentAnimation != null)
            {
                float seconds = Convert.ToSingle((DateTime.UtcNow - animationStartTime).TotalSeconds);
                float currentAnimationFrame = (seconds * currentAnimation.FrameRate) % currentAnimation.FrameCount;
                Model.SetAnimationFrame(currentAnimation.Name, currentAnimationFrame);
            }
            else if(currentPose != null)
                Model.SetPose(currentPose);
            else
                Model.SetPose("Default");
            
            Model.Render(projection, view, transformation);
        }

        #endregion
    }
}