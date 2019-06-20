using Assets.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    class Armor : Character
    {

    const int FORWARD_MOVEMENT = 15;
    const int RECOVERY = 10;
    const int FORWARD_SPEED = 150;

    public enum DashState { InitialMovement, Recovery, Inactive }
    public DashState State { get { return m_state; } }
    private DashState m_state;

    public Armor() : base()
    {
        Initialize();
    }

    protected override Character CreateCopy()
    {
        Armor copy = new Armor();
        copy.m_state = State;
        return copy;
    }

    public override void SetSpecial(CharacterState _character)
    {
        m_state = DashState.InitialMovement;
        _character.SetCharacterHurtboxStanding(_character.Hitboxes);
        _character.Armor = true;
        _character.DisableThrowBreak = true;
    }

    public override GameplayEnums.Outcome GetOutcomeIfHit()
    {
        return GameplayEnums.Outcome.Counter;
    }

    public override MatchOutcome UpdateSpecial(CharacterState _character, ref int _positionOffset)
    {
        switch (State)
        {

            case DashState.InitialMovement:
                _character.Armor = true;
                if (_character.StateFrames > FORWARD_MOVEMENT)
                {
                    m_state = DashState.Recovery;
                    _character.StateFrames = 0;
                    _character.Armor = false;
                    _character.DisableThrowBreak = false;
                }
                _positionOffset = FORWARD_SPEED;
                break;
            case DashState.Recovery:
                if (_character.StateFrames > RECOVERY)
                {
                    m_state = DashState.Inactive;
                    _character.State = GameplayEnums.CharacterState.Idle;
                    _character.StateFrames = 0;
                }
                break;
        }

        return new MatchOutcome();
    }

    public override GameplayEnums.CharacterState GetEquivalentState()
    {
        switch (State)
        {
            case DashState.InitialMovement:
                return GameplayEnums.CharacterState.AttackStartup;
            default:
                return GameplayEnums.CharacterState.AttackRecovery;
        }
    }

    //public override void HandleInputs(CharacterState _cstate, SinglePlayerInputs _currentInputs, SinglePlayerInputs _previousInputs)
    //{
    //    //can't block but can cancel into attack or throw
    //    GameplayEnums.CharacterState potentialCancelState = _cstate.GetCharacterAction(_currentInputs, _previousInputs);
    //    switch (potentialCancelState)
    //    {
    //        case GameplayEnums.CharacterState.AttackStartup:
    //        case GameplayEnums.CharacterState.ThrowStartup:
    //            _cstate.SetCharacterState(potentialCancelState);
    //            break;
    //        default:
    //            break;
    //    }
    //}

    public override void Initialize()
    {
        m_state = DashState.Inactive;
    }

    public override string GetCharacterName()
    {
        return "Armor";
    }
}