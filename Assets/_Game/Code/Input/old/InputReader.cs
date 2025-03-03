using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions, IInputHandler, ISwitchPlayerMap
{
    private InputSystem_Actions inputActions;
    public event Action<Vector2> MoveEvent;
    public event Action<Vector2> LookEvent;
    public event Action InteractEvent;
    public event Action JumpEvent;
    public event Action AttackEvent;
    public event Action<bool> SprintEvent;
    public event Action<bool> CrouchEvent;

    public event Action<bool> DefenceEvent;


    public void InitializeInput()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Player.SetCallbacks(this);
        }

        inputActions.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
            inputActions.Player.RemoveCallbacks(this);
            inputActions.Dispose();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }
        else
        {
            MoveEvent?.Invoke(Vector2.zero);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LookEvent?.Invoke(context.ReadValue<Vector2>());
        }
        else
        {
            LookEvent?.Invoke(new Vector2(0, 0));
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AttackEvent?.Invoke();
        }
    }

    public void OnDefence(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DefenceEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            DefenceEvent?.Invoke(false);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractEvent?.Invoke();
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CrouchEvent?.Invoke(true);
        }
        else
        {
            CrouchEvent?.Invoke(false);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpEvent?.Invoke();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintEvent?.Invoke(true);
        }
        else
        {
            SprintEvent?.Invoke(false);
        }
    }

    public void SimulateMove(Vector2 movement)
    {
        throw new NotImplementedException();
    }

    public void SimulateInteract()
    {
        throw new NotImplementedException();
    }

    public void SimulateSprint(bool isSprinting)
    {
        throw new NotImplementedException();
    }

    public void TurnOffPlayerControls()
    {
        inputActions.Player.Disable();
    }

    public void TurnOnPlayerControls()
    {
        inputActions.Player.Enable();
    }
}