using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Input
{
    interface IInputSource
    {
        bool IsP1();
        SinglePlayerInputs GetInputs();
    }
}
