using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Input.InputSources.AI
{
    public interface IAiController
    {
        SinglePlayerInputs GetInputs();
    }
}
