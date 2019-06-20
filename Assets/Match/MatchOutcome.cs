using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Match
{
    public class MatchOutcome
    {
        public bool P1_Scores;
        public bool P2_Scores;
        public GameplayEnums.Outcome Outcome;

        public MatchOutcome()
        {
            P1_Scores = false;
            P2_Scores = false;
            Outcome = GameplayEnums.Outcome.StillGoing;
        }

        public MatchOutcome(bool _p1Scores, bool _p2Scores, GameplayEnums.Outcome _outcome)
        {
            P1_Scores = _p1Scores;
            P2_Scores = _p2Scores;
            Outcome = _outcome;
        }

        public bool IsSame(MatchOutcome _other)
        {
            if (P1_Scores != _other.P1_Scores)
                return false;
            if (P2_Scores != _other.P2_Scores)
                return false;
            if (Outcome != _other.Outcome)
                return false;
            return true;
        }

        public bool IsEnd()
        {
            MatchOutcome notDone = new MatchOutcome();
            return !IsSame(notDone);
        }
    }
}
