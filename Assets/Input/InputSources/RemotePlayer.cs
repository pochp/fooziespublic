using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Input
{
    class RemotePlayer : IInputSource
    {
        public SinglePlayerInputs GetInputs()
        {
            return new SinglePlayerInputs();
        }

        public bool IsP1()
        {
            throw new NotImplementedException();
        }
    }
}
