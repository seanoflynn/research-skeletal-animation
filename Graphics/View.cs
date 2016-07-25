using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Graphics
{
    public enum ProjectionMode
    {
        Orthographic,
        Perspective
    }

    public class View
    {
        public Camera Camera { get; private set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float AspectRatio { get { return (float)Width / (float)Height; } }

        public Matrix4 projectionMatrix;
        public Matrix4 ProjectionMatrix { get { return projectionMatrix; } }
        private ProjectionMode projectionMode = ProjectionMode.Perspective;
        public ProjectionMode ProjectionMode
        {
            get { return projectionMode; }
            set
            {
                projectionMode = value;
                if(ProjectionMode == ProjectionMode.Perspective)
                    Matrix4.CreatePerspectiveFieldOfView(0.78f, AspectRatio, 0.1f, 1000.0f, out projectionMatrix);
                else
                    Matrix4.CreateOrthographic(40.0F * AspectRatio, 40.0F, -1.0f, 1000.0f, out projectionMatrix);
            }
        }

        static View()
        {
            GL.Enable(EnableCap.ScissorTest);
            GL.Enable(EnableCap.DepthTest);

            GL.ClearColor(Color4.White);
        }

        public View(int x, int y, int width, int height, ProjectionMode mode = ProjectionMode.Perspective, Camera camera = null)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            ProjectionMode = mode;
            Camera = camera ?? new Camera();
        }

        public View(double x, double y, double width, double height, ProjectionMode mode = ProjectionMode.Perspective, Camera camera = null)
            : this(Convert.ToInt32(x),Convert.ToInt32(y), Convert.ToInt32(width), Convert.ToInt32(height), mode, camera)
        {
        }

        public void Resize(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public void Resize(double x, double y, double width, double height)
        {
            Resize(Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(width), Convert.ToInt32(height));
        }

        public void Render(IEnumerable<WorldObject> objects)
        {
            GL.Viewport(X, Y, Width, Height);
            GL.Scissor(X, Y, Width, Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            foreach(var obj in objects)
                obj.Render(this.projectionMatrix, Camera.Transformation);            
        }
    }
}

