using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public class SinglePlayerInputs
{
    //789
    //456
    //123
    public short JoystickDirection;
    public bool A, B, C, Start;
    public SinglePlayerInputs()
    {
        JoystickDirection = 5;
        A = false;
        B = false;
        C = false;
        Start = false;
    }
    public SinglePlayerInputs(SinglePlayerInputs other)
    {
        JoystickDirection = other.JoystickDirection;
        A = other.A;
        B = other.B;
        C = other.C;
        Start = other.Start;
    }

    public bool Equals(SinglePlayerInputs other)
    {
        return JoystickDirection == other.JoystickDirection &&
        A == other.A &&
        B == other.B &&
        C == other.C &&
        Start == other.Start;
    }
}

