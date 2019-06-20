using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FooziesConstants;
using Assets.Match;

public class CharacterState
{
    public int Position;
    public int StateFrames;
    public GameplayEnums.CharacterState State;
    public List<Hitbox_Gameplay> Hitboxes;
    public int Gauge;
    public bool FacingRight;
    public Character SelectedCharacter;

    //atributes
    public bool Armor;
    public bool DisableThrowBreak;
    public bool AttackConnected;

    public bool P1
    {
        get { return FacingRight; }
    }

    public CharacterState(bool _p1, Character _selectedCharacter)
    {
        Position = 0;
        StateFrames = 0;
        State = GameplayEnums.CharacterState.Idle;
        Hitboxes = new List<Hitbox_Gameplay>();
        Gauge = 0;
        FacingRight = _p1;
        SelectedCharacter = _selectedCharacter;
        Armor = false;
        DisableThrowBreak = false;
        AttackConnected = false;
    }

    public CharacterState(CharacterState _toCopy)
    {
        Position = _toCopy.Position;
        StateFrames = _toCopy.StateFrames;
        State = _toCopy.State;
        Hitboxes = new List<Hitbox_Gameplay>();
        foreach(Hitbox_Gameplay hbox in _toCopy.Hitboxes)
        {
            Hitboxes.Add(new Hitbox_Gameplay(hbox));
        }
        Gauge = _toCopy.Gauge;
        FacingRight = _toCopy.FacingRight;
        SelectedCharacter = _toCopy.SelectedCharacter.CopyCharacter();
        Armor = _toCopy.Armor;
        DisableThrowBreak = _toCopy.DisableThrowBreak;
        AttackConnected = _toCopy.AttackConnected;
    }

    public void UpdateStateWithInputs(SinglePlayerInputs _inputs, SinglePlayerInputs _lastInputs, CharacterState _otherCharacter)
    {
        switch (State)
        {
            case GameplayEnums.CharacterState.Crouch:
            case GameplayEnums.CharacterState.Idle:
            case GameplayEnums.CharacterState.WalkBack:
            case GameplayEnums.CharacterState.WalkForward:
                SetCharacterState(GetCharacterAction(_inputs, _lastInputs));
                break;
            case GameplayEnums.CharacterState.BeingThrown:
                if (_inputs.B && !_lastInputs.B && !DisableThrowBreak)
                {
                    _otherCharacter.SetThrowBreak();
                    SetThrowBreak();
                }
                break;
            case GameplayEnums.CharacterState.Special:
                SelectedCharacter.HandleInputs(this, _inputs, _lastInputs);
                break;
        }
    }

    public string GetCharacterName()
    {
        return SelectedCharacter.GetCharacterName();
    }

    public GameplayEnums.CharacterState GetCharacterAction(SinglePlayerInputs _inputs, SinglePlayerInputs _previousInputs)
    {
        if (_inputs.C && !_previousInputs.C)
        {
            return GameplayEnums.CharacterState.Special;
        }
        if (_inputs.B && !_previousInputs.B)
        {
            return GameplayEnums.CharacterState.ThrowStartup;
        }
        if (_inputs.A && !_previousInputs.A)
        {
            return GameplayEnums.CharacterState.AttackStartup;
        }
        switch (_inputs.JoystickDirection)
        {
            case 9:
            case 6:
                return GameplayEnums.CharacterState.WalkForward;
            case 7:
            case 4:
                return GameplayEnums.CharacterState.WalkBack;
            case 1:
            case 2:
            case 3:
                return GameplayEnums.CharacterState.Crouch;
        }
        return GameplayEnums.CharacterState.Idle;
    }

    public void SetThrowBreak()
    {
        State = GameplayEnums.CharacterState.ThrowBreak;
        StateFrames = 0;
        Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
        Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw);
    }

    public void SetCharacterState(GameplayEnums.CharacterState _state)
    {
        ResetAttributes();
        State = _state;
        StateFrames = 0;
        switch (_state)
        {
            case GameplayEnums.CharacterState.AttackStartup:
                Hitboxes.Add(CreateHitbox(GameplayEnums.HitboxType.Hurtbox_Limb, GameplayConstants.HURTBOX_STARTUP));
                SetCharacterHurtboxStanding(Hitboxes);
                DisableThrowBreak = true;
                AttackConnected = false;
                //play sfx
                GameManager.Instance.SoundManager.PlaySfx(SoundManager.SFX.Whiff);
                break;
            case GameplayEnums.CharacterState.ThrowStartup:
                Hitboxes.Add(CreateHitbox(GameplayEnums.HitboxType.Hurtbox_Limb, GameplayConstants.THROW_STARTUP_HURTBOX));
                SetCharacterHurtboxStanding(Hitboxes);

                //play sfx
                GameManager.Instance.SoundManager.PlaySfx(SoundManager.SFX.Whiff);
                break;
            case GameplayEnums.CharacterState.WalkBack:
            case GameplayEnums.CharacterState.WalkForward:
            case GameplayEnums.CharacterState.Idle:
                SetCharacterHurtboxStanding(Hitboxes);
                break;
            case GameplayEnums.CharacterState.Crouch:
                SetCharacterHurtboxCrouching(Hitboxes);
                break;
            case GameplayEnums.CharacterState.Special:
                if (SelectedCharacter.CanUseSpecial(this))
                    SelectedCharacter.SetSpecial(this);
                else
                    State = GameplayEnums.CharacterState.Idle;
                break;
        }
    }

    //updates this character's state by one frame
    public MatchOutcome UpdateCharacterState()
    {
        int positionOffset = 0;
        ++StateFrames;
        switch (State)
        {
            case GameplayEnums.CharacterState.AttackActive:
                if (StateFrames > GameplayConstants.ATTACK_ACTIVE)
                {
                    State = GameplayEnums.CharacterState.AttackRecovery;
                    StateFrames = 0;
                    Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack);
                    ModifyHitbox(Hitboxes, GameplayConstants.HURTBOX_WHIFF_EARLY);
                    DisableThrowBreak = false;
                }
                if (StateFrames == GameplayConstants.ATTACK_FULL_EXTEND)
                {
                    ModifyHitbox(Hitboxes, GameplayConstants.HITBOX_ACTIVE_LATE, GameplayEnums.HitboxType.Hitbox_Attack);
                }
                break;
            case GameplayEnums.CharacterState.AttackRecovery:
                if (StateFrames > GameplayConstants.ATTACK_RECOVERY_TOTAL)
                {
                    State = GameplayEnums.CharacterState.Idle;
                    StateFrames = 0;
                    Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                }
                if (StateFrames == GameplayConstants.ATTACK_RECOVERY_SHORTEN)
                {
                    ModifyHitbox(Hitboxes, GameplayConstants.HURTBOX_WHIFF_LATE);
                }
                break;
            case GameplayEnums.CharacterState.AttackStartup:
                if (StateFrames > GameplayConstants.ATTACK_STARTUP)
                {
                    State = GameplayEnums.CharacterState.AttackActive;
                    StateFrames = 0;
                    Hitboxes.Add(CreateHitbox(GameplayEnums.HitboxType.Hitbox_Attack, GameplayConstants.HITBOX_ACTIVE_EARLY));
                    ModifyHitbox(Hitboxes, GameplayConstants.HURTBOX_ACTIVE);
                }
                break;
            case GameplayEnums.CharacterState.BeingThrown:
                SetCharacterHurtboxStanding(Hitboxes);
                if (StateFrames > GameplayConstants.THROW_BREAK_WINDOW)
                {
                    if (P1)
                        return new MatchOutcome(false, true, GameplayEnums.Outcome.Throw);
                    else
                        return new MatchOutcome(true, false, GameplayEnums.Outcome.Throw);
                }
                break;//this is handled in checking if a player wins
            case GameplayEnums.CharacterState.Blockstun:
                SetCharacterHurtboxStanding(Hitboxes);
                if (StateFrames < GameplayConstants.ATTACK_PUSHBACK_DURATION)
                {
                    positionOffset = GameplayConstants.ATTACK_PUSHBACK_SPEED;
                }
                if (StateFrames > GameplayConstants.ATTACK_BLOCKSTUN)
                {
                    State = GameplayEnums.CharacterState.Idle;
                    StateFrames = 0;
                }
                break;
            case GameplayEnums.CharacterState.Clash:
                if (StateFrames > GameplayConstants.CLASH_PUSHBACK_DURATION)
                {
                    State = GameplayEnums.CharacterState.Idle;
                    StateFrames = 0;
                }
                positionOffset = GameplayConstants.CLASH_PUSHBACK_SPEED;
                break;
            case GameplayEnums.CharacterState.Crouch:
                SetCharacterHurtboxCrouching(Hitboxes);
                break;
            case GameplayEnums.CharacterState.Idle:
                SetCharacterHurtboxStanding(Hitboxes);
                break;
            case GameplayEnums.CharacterState.Inactive:
                throw new System.Exception("character state was inactive. not supposed to happen???");
            case GameplayEnums.CharacterState.Special:
                MatchOutcome specialOutcome = SelectedCharacter.UpdateSpecial(this, ref positionOffset);
                if (specialOutcome.IsEnd())
                    return specialOutcome;
                break;
            case GameplayEnums.CharacterState.ThrowActive:
                if (StateFrames > GameplayConstants.THROW_ACTIVE)
                {
                    State = GameplayEnums.CharacterState.ThrowRecovery;
                    StateFrames = 0;
                    Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hitbox_Throw);
                    ModifyHitbox(Hitboxes, GameplayConstants.THROW_RECOVERY_HURTBOX);
                }
                break;
            case GameplayEnums.CharacterState.ThrowBreak:
                if (StateFrames > GameplayConstants.BREAK_DURATION)
                {
                    State = GameplayEnums.CharacterState.Idle;
                    StateFrames = 0;
                }
                positionOffset = GameplayConstants.BREAK_PUSHBACK;
                break;
            case GameplayEnums.CharacterState.ThrowingOpponent:
                if (StateFrames > GameplayConstants.THROW_BREAK_WINDOW)
                {
                    if (P1)
                        return new MatchOutcome(true, false, GameplayEnums.Outcome.Throw);
                    else
                        return new MatchOutcome(false, true, GameplayEnums.Outcome.Throw);
                }
                break;//this is handled in checking if a player wins
            case GameplayEnums.CharacterState.ThrowRecovery:
                if (StateFrames > GameplayConstants.THROW_RECOVERY)
                {
                    State = GameplayEnums.CharacterState.Idle;
                    StateFrames = 0;
                    Hitboxes.RemoveAll(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Limb);
                }
                break;
            case GameplayEnums.CharacterState.ThrowStartup:
                SetCharacterHurtboxStanding(Hitboxes);
                if (StateFrames > GameplayConstants.THROW_STARTUP)
                {
                    State = GameplayEnums.CharacterState.ThrowActive;
                    StateFrames = 0;
                    Hitboxes.Add(CreateHitbox(GameplayEnums.HitboxType.Hitbox_Throw, GameplayConstants.THROW_ACTIVE_RANGE));
                    ModifyHitbox(Hitboxes, GameplayConstants.THROW_STARTUP_HURTBOX);
                }
                break;
            case GameplayEnums.CharacterState.WalkBack:
                SetCharacterHurtboxStanding(Hitboxes);
                positionOffset = GameplayConstants.WALK_B_SPEED;
                break;
            case GameplayEnums.CharacterState.WalkForward:
                SetCharacterHurtboxStanding(Hitboxes);
                positionOffset = GameplayConstants.WALK_F_SPEED;
                break;
            default:
                throw new System.Exception("Ooops looks like I forgot to handle state : " + State.ToString());
        }

        if(FacingRight)
            Position += positionOffset;
        else
            Position -= positionOffset;


        return new MatchOutcome();
    }
    public Hitbox_Gameplay CreateHitbox(GameplayEnums.HitboxType _boxType, int _width)
    {
        Hitbox_Gameplay box = new Hitbox_Gameplay();
        box.HitboxType = _boxType;
        box.Width = _width;
        if (FacingRight)
            box.Position = GameplayConstants.CHARACTER_HURTBOX_WIDTH / 2 + _width / 2;
        else
            box.Position = GameplayConstants.CHARACTER_HURTBOX_WIDTH / -2 - _width / 2;
        return box;
    }

    public void ModifyHitbox(List<Hitbox_Gameplay> _hitboxes, int _length, GameplayEnums.HitboxType _hitboxType = GameplayEnums.HitboxType.Hurtbox_Limb)
    {
        Hitbox_Gameplay hbox = _hitboxes.Find(o => o.HitboxType == _hitboxType);
        hbox.Width = _length;
        if (FacingRight)
        {
            hbox.Position = GameplayConstants.CHARACTER_HURTBOX_WIDTH / 2 + _length / 2;
        }
        else
        {
            hbox.Position = GameplayConstants.CHARACTER_HURTBOX_WIDTH / 2 - _length / 2;
        }
    }

    public void SetCharacterHurtboxStanding(List<Hitbox_Gameplay> _hitboxes)
    {
        Hitbox_Gameplay hbox = _hitboxes.Find(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Main);
        hbox.Width = GameplayConstants.CHARACTER_HURTBOX_WIDTH;
    }

    public void SetCharacterHurtboxCrouching(List<Hitbox_Gameplay> _hitboxes)
    {
        Hitbox_Gameplay hbox = _hitboxes.Find(o => o.HitboxType == GameplayEnums.HitboxType.Hurtbox_Main);
        hbox.Width = GameplayConstants.CROUCHING_HURTBOX_WIDTH;
    }

    public void AddGauge(int _amount)
    {
        SelectedCharacter.AddGauge(this, _amount);
    }

    public bool HasArmor()
    {
        return Armor;
    }

    public bool CanBreakThrow()
    {
        return !DisableThrowBreak;
    }

    public void ResetAttributes()
    {
        Armor = false;
        DisableThrowBreak = false;
    }
}