using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Match
{
    public class MatchResult
    {
        public enum Result { P1_Win, P2_Win}
        public Result Match_Result;
        public MatchResult(Result _res)
        {
            Match_Result = _res;
        }
    }
}
