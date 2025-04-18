using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;


[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class NetcodePlayerInputSystem : SystemBase
{
    private InputSystem_Actions _inputActions;

    protected override void OnCreate()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();
        RequireForUpdate<NetworkStreamInGame>();
        RequireForUpdate<GamePlayingTag>();
        RequireForUpdate<NetcodePlayerInput>();
    }

    protected override void OnUpdate()
    {
        foreach (RefRW<NetcodePlayerInput> netcodePlayerInput in SystemAPI.Query<RefRW<NetcodePlayerInput>>()
                     .WithAll<GhostOwnerIsLocal>())

        {
            netcodePlayerInput.ValueRW.inputVector = _inputActions.Player.Move.ReadValue<Vector2>();
            netcodePlayerInput.ValueRW.isSprinting = _inputActions.Player.Sprint.IsPressed();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _inputActions.Disable();
    }
}