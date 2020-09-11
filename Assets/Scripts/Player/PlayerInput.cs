using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerInput : MonoBehaviour
{
    internal bool JumpKeyDown { get; set; }
    internal Vector2 DirectionalInput { get; set; }
    
    private PlayerControls controls;
    private PlayerController player;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => DirectionalInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => DirectionalInput = Vector2.zero;

        controls.Player.Jump.performed += _ => { JumpKeyDown = true; player.OnJumpInputDown(); };
        controls.Player.Jump.canceled += _ => JumpKeyDown = false;
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }
}