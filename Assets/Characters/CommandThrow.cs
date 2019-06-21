using Assets.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class CommandThrow : Character
{
    const int STARTUP = 3;
    const int ACTIVE = 2;
    const int RECOVERY = 60;

    const int ATTACK_RECOVERY_SHORTEN = 35;
    const int HURTBOX_WHIFF_EARLY = 500;
    const int HURTBOX_WHIFF_LATE = 500;
    const int HURTBOX_STARTUP = 500;
    const int HURTBOX_ACTIVE = 500; //hurtbox when active is shorter than the recovery so it favorises clashes
    const int HITBOX_ACTIVE = 1000;

    public enum CmdThrowState { Startup, Active, Recovery, Inactive }
    public CmdThrowState State { get { return m_state; } }
    private CmdThrowState m_state;

    public CommandThrow() : base()
    {
        Initialize();
    }

    protected override Character CreateCopy()
    {
        CommandThrow copy = new CommandThrow();
        copy.m_state = State;
        return copy;
    }

    public override void SetSpecial(CharacterState _character)
    {
        m_state = CmdThrowState.Startup;
        _character.Hitboxes.Add(_character.CreateHitbox(GameplayEnums.HitboxType.Hurtbox_Limb, HURTBOX_STARTUP));
        _character.SetCharacterHurtboxStanding(_character.Hitboxes);
        _character.DisableThrowBreak = true;
        _character.AttackConnected = false;
    }

    public override GameplayEnums.Outcome GetOutcomeIfHit()
    {
        switch (State)
        {
            case CmdThrowState.Active:
            case CmdThrowState.Startup:
                return GameplayEnums.Outcome.Counter;
            default:
                return GameplayEnums.Outcome.StrayHit;
        }
    }

    public override MatchOutcome UpdateSpecial(CharacterState _character, ref int _positionOffset)
    {
        switch (State)
        {

            case CmdThrowState.Active:
                if (_character.StateFrames > ACTIVE)
                {
                    m_state = CmdThrowState.Recovery;
                    _character.StateFrames = 0;
                    _character.Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw);
                    _character.ModifyHitbox(_character.Hitboxes, HURTBOX_WHIFF_EARLY);

                    _character.DisableThrowBreak = false;
                }
                break;
            case CmdThrowState.Recovery:
                if (_character.StateFrames > RECOVERY)
                {
                    m_state = CmdThrowState.Inactive;
                    _character.State = GameplayEnums.CharacterState.Idle;
                    _character.StateFrames = 0;
                    _character.Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                }
                if (_character.StateFrames == ATTACK_RECOVERY_SHORTEN)
                {
                    _character.ModifyHitbox(_character.Hitboxes, HURTBOX_WHIFF_LATE);
                }
                break;
            case CmdThrowState.Startup:
                if (_character.StateFrames > STARTUP)
                {
                    m_state = CmdThrowState.Active;
                    _character.StateFrames = 0;
                    Hitbox_Gameplay hbox = _character.CreateHitbox(GameplayEnums.HitboxType.Hitbox_Throw, HITBOX_ACTIVE);
                    hbox.AttackAttribute = GameplayEnums.AttackAttribute.UntechableThrow;
                    _character.Hitboxes.Add(hbox);
                    _character.ModifyHitbox(_character.Hitboxes, HURTBOX_ACTIVE);
                }
                break;
        }

        return new MatchOutcome();
    }

    public override GameplayEnums.CharacterState GetEquivalentState()
    {
        switch (State)
        {
            case CmdThrowState.Active:
            case CmdThrowState.Startup:
                return GameplayEnums.CharacterState.AttackStartup;
            default:
                return GameplayEnums.CharacterState.AttackRecovery;
        }
    }

    public override GameplayEnums.Outcome GetCurrentCharacterSpecialOutcome()
    {
        return GameplayEnums.Outcome.Throw;
    }

    public override void Initialize()
    {
        m_state = CmdThrowState.Inactive;
    }
    public override string GetCharacterName()
    {
        return "CmdThrow";
    }
}