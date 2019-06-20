using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.States
{
    public abstract class SystemState
    {
        public int FrameNumber { get; set; }

        public SystemState(int frameNumber) { FrameNumber = frameNumber; }
    }
}
