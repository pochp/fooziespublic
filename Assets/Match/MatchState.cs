using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Match
{
    public class MatchState
    {
        public int P1_Score;
        //{
        //    get
        //    {
        //        int sum = 0;
        //        foreach(MatchOutcome mo in Outcomes)
        //        {
        //            if (mo.P1_Scores)
        //                ++sum;
        //        }
        //        return sum;
        //    }
        //}
        public int P2_Score;

        public List<MatchOutcome> Outcomes;
        public bool GameOver;

        public MatchState()
        {
            P1_Score = 0;
            P2_Score = 0;
            GameOver = false;
            Outcomes = new List<MatchOutcome>();
        }
    }
}
