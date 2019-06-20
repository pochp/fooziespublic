using Assets.Input.InputSources.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Input
{
    class AiPlayer : IInputSource
    {
        private IAiController _aiController;
        public AiPlayer(bool p1, Type aiType)
        {
            if(aiType == typeof(BasicDelayedAi))
                _aiController = new BasicDelayedAi(p1);
            else
                _aiController = new BasicAi(p1);

        }

        public SinglePlayerInputs GetInputs()
        {
            return _aiController.GetInputs();
        }

        public bool IsP1()
        {
            throw new NotImplementedException();
        }
    }
}
