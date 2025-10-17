using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action<Vector2> OnPrimaryClick;

    public InputSystem_Actions _actions;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _actions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        _actions.Enable();
        _actions.Player.Primary.performed += OnPrimary;
    }

    void OnDisable()
    {
        _actions.Player.Primary.performed -= OnPrimary;
        _actions.Disable();
    }

    void OnPrimary(InputAction.CallbackContext ctx)
    {
        var pos = _actions.Player.Point.ReadValue<Vector2>();
        OnPrimaryClick?.Invoke(pos);
    }

    public Vector2 MousePosition
    {
        get
        {
            return Mouse.current?.position.ReadValue() ?? Vector2.zero;
        }
    }
}
