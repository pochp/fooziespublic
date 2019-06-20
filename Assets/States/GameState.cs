using Match;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.States
{
    public class GameState : SystemState
    {

        public int P1_Position
        {
            get
            {
                return P1_CState.Position;
            }
            set
            {
                P1_CState.Position = value;
            }
        }
        public int P2_Position
        {
            get
            {
                return P2_CState.Position;
            }
            set
            {
                P2_CState.Position = value;
            }
        }
        public int P1_StateFrames
        {
            get
            {
                return P1_CState.StateFrames;
            }
            set
            {
                P1_CState.StateFrames = value;
            }
        }
        public int P2_StateFrames
        {
            get
            {
                return P2_CState.StateFrames;
            }
            set
            {
                P2_CState.StateFrames = value;
            }
        }
        public GameplayEnums.CharacterState P1_State
        {
            get
            {
                return P1_CState.State;
            }
            set
            {
                P1_CState.State = value;
            }
        }
        public GameplayEnums.CharacterState P2_State
        {
            get
            {
                return P2_CState.State;
            }
            set
            {
                P2_CState.State = value;
            }
        }
        public List<Hitbox_Gameplay> P1_Hitboxes
        {
            get
            {
                return P1_CState.Hitboxes;
            }
            set
            {
                P1_CState.Hitboxes = value;
            }
        }
        public List<Hitbox_Gameplay> P2_Hitboxes
        {
            get
            {
                return P2_CState.Hitboxes;
            }
            set
            {
                P2_CState.Hitboxes = value;
            }
        }
        public int P1_Gauge
        {
            get
            {
                return P1_CState.Gauge;
            }
            set
            {
                P1_CState.Gauge = value;
            }
        }
        public int P2_Gauge
        {
            get
            {
                return P2_CState.Gauge;
            }
            set
            {
                P2_CState.Gauge = value;
            }
        }
        public int RemainingHitstop;
        public int RemainingTime;

        public CharacterState P1_CState;
        public CharacterState P2_CState;

        public GameState(MatchInitializationData _initData, int frameNumber) : base(frameNumber)
        {
            _initData.InitCharacters();
            P1_CState = new CharacterState(true, _initData.P1_Character);
            P2_CState = new CharacterState(false, _initData.P2_Character);


            P1_Position = 0;
            P2_Position = 0;
            P1_StateFrames = 0;
            P2_StateFrames = 0;
            P1_State = GameplayEnums.CharacterState.Idle;
            P2_State = GameplayEnums.CharacterState.Idle;
            P1_Hitboxes = new List<Hitbox_Gameplay>();
            P2_Hitboxes = new List<Hitbox_Gameplay>();
            P1_Gauge = 0;
            P2_Gauge = 0;
            RemainingHitstop = 0;
            RemainingTime = 0;
        }

        public GameState(GameState _other, int frameNumber) : base(frameNumber)
        {
            P1_CState = new CharacterState(_other.P1_CState);
            P2_CState = new CharacterState(_other.P2_CState);


            RemainingHitstop = _other.RemainingHitstop;
            RemainingTime = _other.RemainingTime;
        }
    }
}





