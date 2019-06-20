using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Hitbox_Gameplay
{
    public GameplayEnums.HitboxType HitboxType;
    public GameplayEnums.AttackAttribute AttackAttribute;
    public GameplayEnums.HurtboxAttribute HurtboxAttribute;
    public int Position;
    public int Width;
    public int RemainingTime;
    public bool HasStruck;
    public readonly long ID;

    static int ID_Counter = 0;

    public Hitbox_Gameplay()
    {
        HitboxType = GameplayEnums.HitboxType.Hitbox_Attack;
        Position = 0;
        Width = 0;
        RemainingTime = -1;
        HasStruck = false;
        ID = ID_Counter++;
        AttackAttribute = GameplayEnums.AttackAttribute.NotAttack;
        HurtboxAttribute = GameplayEnums.HurtboxAttribute.Normal;
    }

    public Hitbox_Gameplay(GameplayEnums.HitboxType _boxType, int _position, int _width, int _remainingTime = -1) : this()
    {
        HitboxType = _boxType;
        Position = _position;
        Width = _width;
        RemainingTime = _remainingTime;
    }

    public Hitbox_Gameplay(Hitbox_Gameplay _other)
    {
        HitboxType = _other.HitboxType;
        Position = _other.Position;
        Width = _other.Width;
        RemainingTime = _other.RemainingTime;
        HasStruck = _other.HasStruck;
        ID = ID_Counter++;
        AttackAttribute = _other.AttackAttribute;
        HurtboxAttribute = _other.HurtboxAttribute;
    }
}