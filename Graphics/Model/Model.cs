using System;
using System.Linq;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Graphics
{
    public class Model : Asset
    {
        private static Vector4 TextureColor = Vector4.One;
        private static Vector4 PointColor = new Vector4(0.1f, 0.0f, 0.0f, 1.0f);
        private static Vector4 EdgeColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        private static Vector4 FaceColor = new Vector4(0.6f, 0.6f, 1.0f, 1.0f);

        #region Properties

        public string Name { get; private set; }
        public string File { get; private set; }

        public List<Mesh> Meshes { get; private set; } = new List<Mesh>();
        public List<Pose> Poses { get; private set; } = new List<Pose>();
        public List<Animation> Animations { get; private set; } = new List<Animation>();

        #endregion

        #region Fields

        protected ShaderProgram shaderProgram;

        private int vertexArrayObject = -1;
        private int vertexBufferId = -1;
        private int elementBufferId = -1;

        #endregion

        #region Constructor

        static Model()
        {
            Assets.Register(new ShaderProgram("Default", "Default", "Default"));
        }

        public Model(string name, string file, List<Mesh> meshes, string shaderProgramName = "Default")
        {
            Name = name;
            File = file;
            Meshes = meshes;
            shaderProgram = Assets.Retrieve<ShaderProgram>(shaderProgramName);
        }

        #endregion

        #region Setup

        public void Setup()
        {
            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            SetBuffers();
            SetShaderAttributes();

            GL.BindVertexArray(0);
        }

        private void SetBuffers()
        {
            float[] vertices = Meshes.SelectMany(x => x.Vertices).SelectMany(x => x.FloatArray()).ToArray();
            uint[] elements = Meshes.SelectMany(x => x.Elements).Select(x => Convert.ToUInt32(x)).ToArray();

            int vertexOffset = 0;
            int elementOffset = 0;
            foreach(var m in Meshes)
            {
                m.ElementOffset = elementOffset;
                m.ElementCount = m.Elements.Count;
                elementOffset += m.Elements.Count;

                for(int i = 0; i < m.Elements.Count; i++)
                    elements[m.ElementOffset + i] += Convert.ToUInt32(vertexOffset);

                vertexOffset += m.Vertices.Count;
            }

            GL.BindVertexArray(vertexArrayObject);

            vertexBufferId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            elementBufferId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(uint)), elements, BufferUsageHint.StaticDraw);
        }

        protected virtual void SetShaderAttributes()
        {
            int stride = 5 * sizeof(float);
            shaderProgram.AddAttribute("Position", 3, stride, 0);
            shaderProgram.AddAttribute("TextureCoordinates", 2, stride, 3 * sizeof(float));
        }

        #endregion

        #region Render

        public void Render(Matrix4 projection, Matrix4 view, Matrix4 model)
        {            
            GL.BindVertexArray(vertexArrayObject);

            shaderProgram.SetUniform("Projection", projection);
            shaderProgram.SetUniform("View", view);
            shaderProgram.SetUniform("Model", model);

            foreach(var m in Meshes)
            {
                if(m.RenderMode == RenderMode.None)
                    continue;
                
                if(m.RenderMode == RenderMode.Texture)
                {
                    shaderProgram.SetUniform("Color", TextureColor);
                    m.Material.DiffuseTexture.Bind();
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
                else if(m.RenderMode == RenderMode.Face)
                {
                    shaderProgram.SetUniform("Color", FaceColor);
                    Texture.Blank.Bind();
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
                else if(m.RenderMode == RenderMode.Edge)
                {
                    shaderProgram.SetUniform("Color", EdgeColor);
                    Texture.Blank.Bind();
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }
                else if(m.RenderMode == RenderMode.Point)
                {                    
                    shaderProgram.SetUniform("Color", PointColor);
                    Texture.Blank.Bind();
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
                }

                GL.DrawElements(BeginMode.Triangles, m.ElementCount, DrawElementsType.UnsignedInt, m.ElementOffset * sizeof(float));
            }

            GL.BindVertexArray(0);
        }

        #endregion

        #region Poses

        public virtual void AddPose(Pose pose)
        {
            Poses.Add(pose);
        }

        public virtual void AddPose(string pose)
        {
            AddPose(Assets.Retrieve<Pose>(pose));
        }

        public virtual void SetPose(string pose)
        {
            if(pose != "Default")
                throw new NotImplementedException();
        }

        #endregion

        #region Animation

        public virtual void AddAnimation(string animation)
        {
            AddAnimation(Assets.Retrieve<Animation>(animation));
        }

        public virtual void AddAnimation(Animation animation)
        {
            Animations.Add(animation);
        }

        public virtual void SetAnimationFrame(string animation, float frame)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}