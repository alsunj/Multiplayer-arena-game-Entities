using System;
using UnityEngine;

public enum TeamType : byte
{
    None = 0,
    Player = 1,
    Enemy = 2,

    AutoAssign = Byte.MaxValue,
}