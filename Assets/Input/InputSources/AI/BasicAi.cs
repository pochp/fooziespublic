using Assets.Menus;
using FooziesConstants;
using Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Input.InputSources.AI
{
    public class BasicAi : IAiController
    {
        //just as a test, he'll mash A every 100 ms or so
        private float timeSinceLastPress;
        private float timeBetweenPresses;
        protected bool p1;

        public BasicAi(bool _p1)
        {
            timeSinceLastPress = 0f;
            timeBetweenPresses = 0.1f;
            p1 = _p1;
        }

        public virtual SinglePlayerInputs GetInputs()
        {
            var gameplayState = ApplicationStateManager.GetInstance().GetApplicationState();
            return GetInputsDispatchState(gameplayState);
        }

        private SinglePlayerInputs GetInputsDispatchState(ApplicationState applicationState)
        {
            if (applicationState is GameplayState)
            {
                return GetInputs(applicationState as GameplayState);
            }
            if (applicationState is MainMenu)
            {
                return GetInputs(applicationState as MainMenu);
            }
            if (applicationState is CharacterSelectScreen)
            {
                return GetInputs(applicationState as CharacterSelectScreen);
            }
            throw new NotSupportedException("application state " + applicationState.GetType().ToString() + " not supported by this AI yet");
        }

        //todo : place these in a strategy pattern
        protected virtual SinglePlayerInputs GetInputs(GameplayState gameState)
        {
            //1. tries to maintain a distance of attack range +50%
            //2. if opponent whiffs an attack. hit him
            //3. if opponent is in throw range. throw

            var inputs = new SinglePlayerInputs();
            var pastState = gameState.GetPastState(0);
            var distanceBetweenCharacters = Math.Abs(pastState.P1_Position - pastState.P2_Position);
            var attackRangeAndAHalf = GameplayConstants.HITBOX_ACTIVE_LATE * 3 / 2;

            GameplayEnums.CharacterState opponentState;
            if(p1)
            {
                opponentState = pastState.P2_CState.State;
            }
            else
            {
                opponentState = pastState.P1_CState.State;
            }

            if (distanceBetweenCharacters > attackRangeAndAHalf)
            {
                //move forward
                inputs.JoystickDirection = 6;
            }
            else
            {
                if (opponentState == GameplayEnums.CharacterState.AttackRecovery)
                {
                    inputs.A = true;
                }
                inputs.JoystickDirection = 4;
            }
            if(distanceBetweenCharacters < GameplayConstants.THROW_ACTIVE_RANGE + GameplayConstants.CHARACTER_HURTBOX_WIDTH)
            {
                inputs.B = true;
            }
            return inputs;
        }
        protected virtual SinglePlayerInputs GetInputs(MainMenu gameState)
        {
            return new SinglePlayerInputs();
        }
        protected virtual SinglePlayerInputs GetInputs(CharacterSelectScreen gameState)
        {
            SinglePlayerInputs inputs = new SinglePlayerInputs();
            timeSinceLastPress += Time.deltaTime;
            if (timeSinceLastPress > timeBetweenPresses)
            {
                timeSinceLastPress = 0;
                inputs.A = true;
            }
            return inputs;
        }
    }
}
