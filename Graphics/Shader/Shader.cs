using System;
using OpenTK.Graphics.OpenGL4;

namespace Graphics
{
    public class Shader : Asset
    {
        public string Name { get; private set; }
        public string File { get; private set; }

        private int id = -1;

        public Shader(string name, string file, ShaderType type, string code)
        {
            Name = name;
            File = file;
            id = GL.CreateShader(type);

            GL.ShaderSource(id, code);
            GL.CompileShader(id);

            int status;
            GL.GetShader(id, ShaderParameter.CompileStatus, out status);
            if(status != 1)
                throw new Exception(type + " compilation failed:\n" + GL.GetShaderInfoLog(id));
        }

        public void AttachToProgram(int programId)
        {
            GL.AttachShader(programId, id);
        }
    }

    public class VertexShader : Shader
    {
        public VertexShader(string name, string file, string code)
            : base(name, file, ShaderType.VertexShader, code)
        { }
    }

    public class FragmentShader : Shader
    {
        public FragmentShader(string name, string file, string code)
            : base(name, file, ShaderType.FragmentShader, code)
        { }
    }
}