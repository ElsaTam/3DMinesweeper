using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public EventHandler OnLeftClick;
    public EventHandler OnRightClick;
    public EventHandler OnDoubleClick;
    public EventHandler OnCubeMovement;
    public EventHandler OnKeyTabPressed;
    public EventHandler OnKeyEscapePressed;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one InputManager. " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    private void Start()
    {
        playerInputActions.Player.LeftClick.performed += _ => OnLeftClick?.Invoke(this, EventArgs.Empty);
        playerInputActions.Player.RightClick.performed += _ => OnRightClick?.Invoke(this, EventArgs.Empty);
        playerInputActions.Player.DoubleClick.performed += _ => OnDoubleClick?.Invoke(this, EventArgs.Empty);
        playerInputActions.Player.KeyTab.performed += _ => OnKeyTabPressed?.Invoke(this, EventArgs.Empty);
        playerInputActions.Player.KeyEscape.performed += _ => OnKeyEscapePressed?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMouseScreenPosition()
    {
        return Mouse.current.position.ReadValue();
    }

    public Vector2 GetCameraRotateVector()
    {
        return playerInputActions.Player.CameraMovement.ReadValue<Vector2>();
    }

    public int GetCubeMovement()
    {
        return (int) playerInputActions.Player.CubeMovement.ReadValue<float>();
    }
}
