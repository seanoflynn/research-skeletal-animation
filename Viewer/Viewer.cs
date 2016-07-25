using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace Graphics
{
    public class Viewer : GameWindow
    {
        private const float MovementSpeed = 0.4f;
        private const float RotationSpeed = 0.05f;
        private const float CameraRotationSpeed = 0.002f;

        private View mainView;
        private View topOrtho;
        private View rightOrtho;
        private View frontOrtho;

        private bool isDebugMeshMode = false;
        private bool isMultiMode = true;

        public List<WorldObject> Objects = new List<WorldObject>();
        public WorldObject FocusObject = null;

        public Viewer()
            : base(640, 480, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 16), "Viewer",
                   GameWindowFlags.Default, DisplayDevice.Default, 4, 1, GraphicsContextFlags.Debug)
        {
            frontOrtho = new View(0, 0, 0.5 * Width, 0.5 * Height, ProjectionMode.Orthographic);
            frontOrtho.Camera.Position = Direction.Forward * 100.0f;
            frontOrtho.Camera.LookAt(Vector3.Zero);

            topOrtho = new View(0, 0.5 * Height, 0.5 * Width, Height - 0.5 * Height, ProjectionMode.Orthographic);
            topOrtho.Camera.Position = Direction.Up * 100.0f;
            topOrtho.Camera.LookAt(Vector3.Zero, Direction.Forward);

            rightOrtho = new View(0.5 * Width, 0, Width - 0.5 * Width, 0.5 * Height, ProjectionMode.Orthographic);
            rightOrtho.Camera.Position = Direction.Right * 100.0f;
            rightOrtho.Camera.LookAt(Vector3.Zero);

            mainView = new View(0.5 * Width, 0.5 * Height, Width - 0.5 * Width, Height - 0.5 * Height);
            mainView.Camera.Position = new Vector3(50.0F, 100.0F, -100.0F);
            mainView.Camera.LookAt(Vector3.Zero);

            Keyboard.KeyDown += OnKeyDown;
            Mouse.Move += OnMouseMove;
        }

        protected override void OnLoad(EventArgs e)
        {
            VSync = VSyncMode.On;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateViews();
        }

        private void UpdateViews()
        {
            if(isMultiMode)
            {
                frontOrtho.Resize(0, 0, 0.5 * Width, 0.5 * Height);
                topOrtho.Resize(0, 0.5 * Height, 0.5 * Width, Height - 0.5 * Height);
                rightOrtho.Resize(0.5 * Width, 0, Width - 0.5 * Width, 0.5 * Height);
                mainView.Resize(0.5 * Width, 0.5 * Height, Width - 0.5 * Width, Height - 0.5 * Height);
            }
            else
            {
                mainView.Resize(0, 0, Width, Height);
            }
        }

        #region Input

        private void CheckKeyboard()
        {
            if(Keyboard[Key.Escape])
                Exit();

            if(Keyboard[Key.T] && FocusObject != null)
            {
                if(Keyboard[Key.Up])
                    FocusObject.Move(Direction.Forward * MovementSpeed);
                if(Keyboard[Key.Down])
                    FocusObject.Move(Direction.Backward * MovementSpeed);
                if(Keyboard[Key.Left])
                    FocusObject.Move(Direction.Left * MovementSpeed);
                if(Keyboard[Key.Right])
                    FocusObject.Move(Direction.Right * MovementSpeed);
                if(Keyboard[Key.O])
                    FocusObject.Move(Direction.Up * MovementSpeed);
                if(Keyboard[Key.L])
                    FocusObject.Move(Direction.Down * MovementSpeed);
                if(Keyboard[Key.P])
                    FocusObject.Position = Vector3.Zero;
            }
            else if(Keyboard[Key.R] && FocusObject != null)
            {
                if(Keyboard[Key.Up])
                    FocusObject.Rotate(Direction.Forward, RotationSpeed);
                if(Keyboard[Key.Down])
                    FocusObject.Rotate(Direction.Backward, RotationSpeed);
                if(Keyboard[Key.Left])
                    FocusObject.Rotate(Direction.Left, RotationSpeed);
                if(Keyboard[Key.Right])
                    FocusObject.Rotate(Direction.Right, RotationSpeed);
                if(Keyboard[Key.O])
                    FocusObject.Rotate(Direction.Up, RotationSpeed);
                if(Keyboard[Key.L])
                    FocusObject.Rotate(Direction.Down, RotationSpeed);
                if(Keyboard[Key.P])
                    FocusObject.Rotation = Quaternion.Identity;
            }
            else
            {
                if(Keyboard[Key.Up])
                    mainView.Camera.Move(Direction.Forward * MovementSpeed);
                if(Keyboard[Key.Down])
                    mainView.Camera.Move(Direction.Backward * MovementSpeed);
                if(Keyboard[Key.Right])
                    mainView.Camera.Move(Direction.Right * MovementSpeed);
                if(Keyboard[Key.Left])
                    mainView.Camera.Move(Direction.Left * MovementSpeed);
                if(Keyboard[Key.O])
                    mainView.Camera.Move(Direction.Up * MovementSpeed, Space.World);
                if(Keyboard[Key.L])
                    mainView.Camera.Move(Direction.Down * MovementSpeed, Space.World);
            }

            if(Keyboard[Key.C] && FocusObject != null)
            {
                mainView.Camera.LookAt(FocusObject.Position);
            }
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if(e.Key == Key.Number1)
                ToggleMeshMode();
            else if(e.Key == Key.Number2)
                ToggleMultiMode();
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            if(Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                mainView.Camera.Rotate(Vector3.UnitY, CameraRotationSpeed * e.XDelta, Space.Local);
                mainView.Camera.Rotate(Vector3.UnitX, CameraRotationSpeed * e.YDelta, Space.World);
            }
        }

        #endregion

        #region Modes

        private void ToggleMeshMode()
        {
            foreach(var m in Assets.RetrieveAll<Model>())
            {
                foreach(var n in m.Meshes)
                {
                    if(isDebugMeshMode)
                        n.RenderMode = (n.Name != "Skeleton" ? RenderMode.Texture : RenderMode.None);
                    else
                        n.RenderMode = (n.Name != "Skeleton" ? RenderMode.Edge : RenderMode.Face);
                }
            }
            isDebugMeshMode = !isDebugMeshMode;
        }

        private void ToggleMultiMode()
        {
            isMultiMode = !isMultiMode;
            UpdateViews();
        }

        #endregion

        #region Updating

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            CheckKeyboard();
        }

        #endregion

        #region Render

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            mainView.Render(Objects);

            if(isMultiMode)
            {
                frontOrtho.Render(Objects);
                rightOrtho.Render(Objects);
                topOrtho.Render(Objects);
            }

            SwapBuffers();
        }

        #endregion
    }
}