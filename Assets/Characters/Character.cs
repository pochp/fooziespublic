using Assets.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class Character
{
    readonly public int MaxGauge;
    readonly public int MinCost;

    readonly int Startup;
    readonly int Active;
    readonly int Recovery;
    readonly string AttackType; //make an enum for this. mid, high, low, unblockable, nothing, 

    protected Character()
    {
        MaxGauge = 4;
        MinCost = 2;
    }

    public abstract MatchOutcome UpdateSpecial(CharacterState _character, ref int _positionOffset);
    public abstract void SetSpecial(CharacterState _character);

    /// <summary>
    /// clear any previous data
    /// </summary>
    public abstract void Initialize();

    public virtual GameplayEnums.Outcome GetOutcomeIfHit()
    {
        return GameplayEnums.Outcome.StrayHit;
    }
    public bool CanUseSpecial(CharacterState _character)
    {
        if(_character.Gauge >= MinCost)
        {
            _character.Gauge -= MinCost;
            return true;
        }
        return false;
    }
    public void AddGauge(CharacterState _character, int _toAdd)
    {
        _character.Gauge += _toAdd;
        if(_character.Gauge > MaxGauge)
        {
            _character.Gauge = MaxGauge;
        }
    }
    public virtual GameplayEnums.CharacterState GetEquivalentState()
    {
        return GameplayEnums.CharacterState.Idle;
    }

    public Character CopyCharacter()
    {
        return CreateCopy();
    }

    protected abstract Character CreateCopy();

    public virtual GameplayEnums.Outcome GetCurrentCharacterSpecialOutcome()
    {
        return GameplayEnums.Outcome.StrayHit;
    }

    public virtual void HandleInputs(CharacterState _cstate, SinglePlayerInputs _currentInputs, SinglePlayerInputs _previousInputs)
    {

    }

    public abstract string GetCharacterName();
}
