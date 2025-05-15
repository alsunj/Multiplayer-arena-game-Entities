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

    /// <summary>
    /// Called when the system is created. Initializes the input actions and ensures the system
    /// only updates when the required components and entities are present in the world.
    /// </summary>
    protected override void OnCreate()
    {
        // Initialize and enable the input actions.
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();

        // Require specific components and entities for the system to update.
        RequireForUpdate<NetworkStreamInGame>();
        RequireForUpdate<GamePlayingTag>();
        RequireForUpdate<NetcodePlayerInput>();
    }

    /// <summary>
    /// Called every frame to process player input. Updates the `NetcodePlayerInput` component
    /// with the player's movement vector and sprinting state.
    /// </summary>
    protected override void OnUpdate()
    {
        // Iterate through all entities with a `NetcodePlayerInput` component and a `GhostOwnerIsLocal` tag.
        foreach (RefRW<NetcodePlayerInput> netcodePlayerInput in SystemAPI.Query<RefRW<NetcodePlayerInput>>()
                     .WithAll<GhostOwnerIsLocal>())
        {
            // Update the input vector and sprinting state from the input actions.
            netcodePlayerInput.ValueRW.inputVector = _inputActions.Player.Move.ReadValue<Vector2>();
            netcodePlayerInput.ValueRW.isSprinting = _inputActions.Player.Sprint.IsPressed();
        }
    }

    /// <summary>
    /// Called when the system is destroyed. Disables the input actions to clean up resources.
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        _inputActions.Disable();
    }
}