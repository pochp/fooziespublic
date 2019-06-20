using Assets.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Sweep : Character
{
    const int STARTUP = 12;
    const int ACTIVE = 3;
    const int RECOVERY = 60;

    const int ATTACK_RECOVERY_SHORTEN = 35;
    const int HURTBOX_WHIFF_EARLY = 2800;
    const int HURTBOX_WHIFF_LATE = 1000;
    const int HURTBOX_STARTUP = 2000;
    const int HURTBOX_ACTIVE = 2500; //hurtbox when active is shorter than the recovery so it favorises clashes
    const int HITBOX_ACTIVE = 3000;

    public enum SweepState{ Startup, Active, Recovery, Inactive}
    public SweepState State { get { return m_state; } }
    private SweepState m_state;

    public Sweep() : base()
    {
        Initialize();
    }

    protected override Character CreateCopy()
    {
        Sweep copy = new Sweep();
        copy.m_state = State;
        return copy;
    }

    public override void SetSpecial(CharacterState _character)
    {
        m_state = SweepState.Startup;
        _character.Hitboxes.Add(_character.CreateHitbox(GameplayEnums.HitboxType.Hurtbox_Limb, HURTBOX_STARTUP));
        _character.SetCharacterHurtboxStanding(_character.Hitboxes);
        _character.DisableThrowBreak = true;
        _character.AttackConnected = false;
    }

    public override GameplayEnums.Outcome GetOutcomeIfHit()
    {
        switch(State)
        {
            case SweepState.Active:
            case SweepState.Startup:
                return GameplayEnums.Outcome.Counter;
            default:
                return GameplayEnums.Outcome.StrayHit;
        }
    }

    public override MatchOutcome UpdateSpecial(CharacterState _character, ref int _positionOffset)
    {
        switch(State)
        {

            case SweepState.Active:
                if (_character.StateFrames > ACTIVE)
            {
                    m_state = SweepState.Recovery;
                _character.StateFrames = 0;
                _character.Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack);
                _character.ModifyHitbox(_character.Hitboxes, HURTBOX_WHIFF_EARLY);

                    _character.DisableThrowBreak = false;
                }
            break;
            case SweepState.Recovery:
                if (_character.StateFrames > RECOVERY)
            {
                    m_state = SweepState.Inactive;
                    _character.State = GameplayEnums.CharacterState.Idle;
                    _character.StateFrames = 0;
                    _character.Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
            }
            if (_character.StateFrames == ATTACK_RECOVERY_SHORTEN)
            {
                    _character.ModifyHitbox(_character.Hitboxes, HURTBOX_WHIFF_LATE);
            }
            break;
            case SweepState.Startup:
            if (_character.StateFrames > STARTUP)
            {
                    m_state = SweepState.Active;
                    _character.StateFrames = 0;
                    Hitbox_Gameplay hbox = _character.CreateHitbox(GameplayEnums.HitboxType.Hitbox_Attack, HITBOX_ACTIVE);
                    hbox.AttackAttribute = GameplayEnums.AttackAttribute.Low;
                    _character.Hitboxes.Add(hbox);
                    _character.ModifyHitbox(_character.Hitboxes, HURTBOX_ACTIVE);
            }
            break;
        }

        return new MatchOutcome();
    }

    public override GameplayEnums.CharacterState GetEquivalentState()
    {
        switch(State)
        {
            case SweepState.Active:
            case SweepState.Startup:
                return GameplayEnums.CharacterState.AttackStartup;
            default:
                return GameplayEnums.CharacterState.AttackRecovery;
        }
    }

    public override GameplayEnums.Outcome GetCurrentCharacterSpecialOutcome()
    {
        return GameplayEnums.Outcome.Sweep;
    }

    public override void Initialize()
    {
        m_state = SweepState.Inactive;
    }
    public override string GetCharacterName()
    {
        return "Sweep";
    }
}
