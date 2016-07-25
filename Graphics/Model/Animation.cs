using System;
using System.Linq;
using System.Collections.Generic;

namespace Graphics
{
    public class Animation : Asset
    {
        public string Name { get; private set; }
        public string File { get; private set; }

        public int FrameRate { get; set; }
        public int FrameCount { get; set; }

        public List<Pose> Frames { get; set; } = new List<Pose>();

        public Animation(string name, string file)
        {
            Name = name;
            File = file;
        }

        public Pose ExtractPose(string name, int frame)
        {
            Pose pose = Frames[frame].Clone(name);

            Assets.Register(pose);

            return pose;
        }

        public Animation ExtractAnimation(string name, int startFrame, int count)
        {
            Animation animation = new Animation(name, "")
            {
                FrameRate = FrameRate,
                FrameCount = count,
                Frames = Frames.Skip(startFrame).Take(count).Select((x, i) => x.Clone(name + "." + i)).ToList()
            };

            Assets.Register(animation);

            return animation;
        }
    }       
}

