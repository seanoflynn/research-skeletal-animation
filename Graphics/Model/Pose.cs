namespace Graphics
{
    public class Pose : Asset
    {
        public string Name { get; set; }
        public string File { get; set; }

        public virtual Pose Clone(string name)
        {
            return new Pose() { Name = name };
        }
    }
}
