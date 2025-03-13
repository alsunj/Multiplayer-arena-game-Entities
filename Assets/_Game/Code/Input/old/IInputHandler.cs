using System;
using UnityEngine;

public interface IInputHandler
{
    event Action<Vector2> MoveEvent;
    event Action<Vector2> LookEvent;
    event Action InteractEvent;
    event Action JumpEvent;
    event Action AttackEvent;
    event Action<bool> SprintEvent;
    event Action<bool> CrouchEvent;
    event Action<bool> DefenceEvent;

    void SimulateMove(Vector2 movement);
    void SimulateInteract();
    void SimulateSprint(bool isSprinting);
}