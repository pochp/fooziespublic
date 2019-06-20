using Assets.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Vanilla : Character
{
    /// <summary>
    /// vanilla class with no special, not sure what will happen if special is executed
    /// </summary>
    public Vanilla() : base()
    {
        Initialize();
    }

    protected override Character CreateCopy()
    {
        Vanilla copy = new Vanilla();
        return copy;
    }

    public override void SetSpecial(CharacterState _character)
    {

    }

    public override GameplayEnums.Outcome GetOutcomeIfHit()
    {
        return GameplayEnums.Outcome.StrayHit;
    }

    public override MatchOutcome UpdateSpecial(CharacterState _character, ref int _positionOffset)
    {
        return new MatchOutcome();
    }

    public override GameplayEnums.CharacterState GetEquivalentState()
    {
         return GameplayEnums.CharacterState.AttackRecovery;
    }

    public override GameplayEnums.Outcome GetCurrentCharacterSpecialOutcome()
    {
        return GameplayEnums.Outcome.StrayHit;
    }
    public override void Initialize()
    {

    }
    public override string GetCharacterName()
    {
        return "Vanilla";
    }
}
