using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Inputs
{
    public SinglePlayerInputs P1_Inputs { get; set; }
    public SinglePlayerInputs P2_Inputs { get; set; }
    public CommonInputs Common_Inputs { get; set; }

    public Inputs(SinglePlayerInputs _p1Inputs, SinglePlayerInputs _p2Inputs, CommonInputs _commonInputs)
    {
        P1_Inputs = _p1Inputs;
        P2_Inputs = _p2Inputs;
        Common_Inputs = _commonInputs;
    }
}


/// <summary>
/// Mostly debugging or temp solutions
/// </summary>
public class CommonInputs
{
    public bool F4;//remap controllers
    public bool F3;//set AI player 2
    public bool F2;//set AI player 2
    public bool F5;//set as online match as Host
    public bool F6;//set as online match, but not as host
    public bool F12;//create config file
    public CommonInputs()
    {
        F4 = false;
        F3 = false;
        F2 = false;
        F12 = false;
        F5 = false;
        F6 = false;
    }
}