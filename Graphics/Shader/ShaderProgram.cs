using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Graphics
{
    public class ShaderProgram : Asset
    {
        #region Fields

        public string Name { get; private set; }
        public string File { get; } = String.Empty;

        #endregion

        #region Fields

        private int id = -1;

        private Shader vertexShader;
        private Shader fragmentShader;

        private Dictionary<string, int> attributes = new Dictionary<string, int>();
        private Dictionary<string, int> uniforms = new Dictionary<string, int>();

        #endregion

        #region Constructor

        public ShaderProgram(string name, string vertex, string fragment)
        {
            Name = name;

            id = GL.CreateProgram();

            vertexShader = Assets.Retrieve<VertexShader>(vertex);
            fragmentShader = Assets.Retrieve<FragmentShader>(fragment);

            vertexShader.AttachToProgram(id);
            fragmentShader.AttachToProgram(id);
            GL.BindFragDataLocation(id, 0, "Color0");
            GL.LinkProgram(id);

            int status;
            GL.GetProgram(id, GetProgramParameterName.LinkStatus, out status);
            if(status != 1)
                throw new Exception("shader program link failed:\n" + GL.GetProgramInfoLog(id));
            
            AddUniform("Projection");
            AddUniform("View");
            AddUniform("Model");
            AddUniform("Color");

            SetUniform("Color", Vector4.One);
        }

        #endregion

        #region Activation

        public void Activate()
        {
            GL.UseProgram(id);
        }

        #endregion

        #region Attributes

        public void AddAttribute(string name, int size, int stride, int offset)
        {
            int attributeId = GL.GetAttribLocation(id, name);
            GL.EnableVertexAttribArray(attributeId);
            GL.VertexAttribPointer(attributeId, size, VertexAttribPointerType.Float, false, stride, offset);

            attributes.Add(name, attributeId);
        }

        #endregion

        #region Uniforms

        public void AddUniform(string name)
        {
            uniforms.Add(name, GL.GetUniformLocation(id, name));
        }

        public void SetUniform(string name, Vector4 vector)
        {
            Activate();
            GL.Uniform4(uniforms[name], ref vector);
        }

        public void SetUniform(string name, Matrix4 matrix)
        {
            Activate();
            GL.UniformMatrix4(uniforms[name], false, ref matrix);
        }

        public void SetUniform(string name, Matrix4[] matrices)
        {
            Activate();
            GL.UniformMatrix4(uniforms[name], matrices.Length, false, ref matrices[0].Row0.X);
        }

        #endregion
    }
}