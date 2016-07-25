using System;
using OpenTK;

namespace Graphics
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using(Viewer viewer = new Viewer())
            {
                var obj = new WorldObject("Crate");
                viewer.Objects.Add(obj);
                viewer.FocusObject = obj;

                viewer.Run(60.0);
            }
        }
    }
}