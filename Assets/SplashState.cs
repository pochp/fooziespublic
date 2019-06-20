using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SplashState
{
    public enum State { RoundOver_ShowResult, RoundStart_3, RoundStart_2, RoundStart_1, RoundStart_F, GameOver, None}
    public State CurrentState;
    public int FramesRemaining;

    public SplashState()
    {
        CurrentState = State.None;
        FramesRemaining = 0;
    }
    public SplashState(SplashState _other)
    {
        CurrentState = _other.CurrentState;
        FramesRemaining = _other.FramesRemaining;
    }
}
