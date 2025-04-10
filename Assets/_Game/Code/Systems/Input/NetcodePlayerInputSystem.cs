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
        RequireForUpdate<PlayerSprintData>();
    }

    protected override void OnUpdate()
    {
        foreach ((RefRW<NetcodePlayerInput> netcodePlayerInput, RefRW<PlayerSprintData> playerSprintData)
                 in SystemAPI.Query<RefRW<NetcodePlayerInput>, RefRW<PlayerSprintData>>()
                     .WithAll<GhostOwnerIsLocal>())
        {
            netcodePlayerInput.ValueRW.inputVector = _inputActions.Player.Move.ReadValue<Vector2>();
            playerSprintData.ValueRW.isSprinting = true;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _inputActions.Disable();
    }
}