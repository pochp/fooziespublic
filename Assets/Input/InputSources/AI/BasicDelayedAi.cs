using FooziesConstants;
using Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Input.InputSources.AI
{
    public class BasicDelayedAi : BasicAi
    {
        //change between agressive and passive every 2 seconds
        private float timeSinceLastStanceChange = 0f;
        private float timeBetweenStanceChange = 2f;
        private int framesDelay = 5;

        private bool aggroMode = false;

        public BasicDelayedAi(bool _p1) : base(_p1)
        { 
        }

        protected override SinglePlayerInputs GetInputs(GameplayState gameState)
        {
            timeSinceLastStanceChange += Time.deltaTime;
            if (timeSinceLastStanceChange > timeBetweenStanceChange)
            {
                aggroMode = !aggroMode;
                timeSinceLastStanceChange = 0f;
            }
            //1. tries to maintain a distance of attack range +50%
            //2. if opponent whiffs an attack. hit him
            //3. if opponent is in throw range. throw

            var inputs = new SinglePlayerInputs();
            var pastState = gameState.GetPastState(framesDelay);
            var distanceBetweenCharacters = Math.Abs(pastState.P1_Position - pastState.P2_Position);
            var actualAttackRange = GameplayConstants.HITBOX_ACTIVE_LATE + GameplayConstants.CHARACTER_HURTBOX_WIDTH;

            GameplayEnums.CharacterState opponentState;
            if (p1)
            {
                opponentState = pastState.P2_CState.State;
            }
            else
            {
                opponentState = pastState.P1_CState.State;
            }

            if(aggroMode)
            {
                //move forward
                if (!p1)
                    inputs.JoystickDirection = 6;
                else
                    inputs.JoystickDirection = 4;

                int timeBeforeAttackFullyExtends_DelayConsidered = framesDelay + GameplayConstants.ATTACK_STARTUP + GameplayConstants.ATTACK_FULL_EXTEND;
                int distanceOpponentCanCoverBeforeAttackReachesHim = -(timeBeforeAttackFullyExtends_DelayConsidered * GameplayConstants.WALK_B_SPEED);

                if(distanceOpponentCanCoverBeforeAttackReachesHim + distanceBetweenCharacters < actualAttackRange)
                {
                    inputs.A = true;
                }
            }
            else
            {
                if (distanceBetweenCharacters > actualAttackRange)
                {
                    //move forward
                    if (!p1)
                        inputs.JoystickDirection = 6;
                    else
                        inputs.JoystickDirection = 4;
                }
                else
                {
                    if (opponentState == GameplayEnums.CharacterState.AttackRecovery)
                    {
                        inputs.A = true;
                    }
                    if (p1)
                        inputs.JoystickDirection = 6;
                    else
                        inputs.JoystickDirection = 4;
                    }
            }
            if (distanceBetweenCharacters < GameplayConstants.THROW_ACTIVE_RANGE + GameplayConstants.CHARACTER_HURTBOX_WIDTH)
            {
                inputs.B = true;
            }
            return inputs;
        }
    }
}
