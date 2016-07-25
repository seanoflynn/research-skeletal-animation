using System.Collections.Generic;

namespace Graphics
{
    public enum RenderMode
    {
        None,
        Point,
        Edge,
        Face,
        Texture
    }

    public class Mesh
    {
        public string Name { get; set; }
        public Material Material { get; set; }

        public List<Vertex> Vertices { get; set; }
        public List<int> Elements { get; set; }

        public int ElementOffset { get; set; }
        public int ElementCount { get; set; }

        public RenderMode RenderMode { get; set; } = RenderMode.Texture;

        public override string ToString()
        {
            return string.Format("[Mesh: {0}, Elements=({1},{2})]", Name, ElementOffset, ElementCount);
        }
    }
}