// <copyright file="PlayerInput.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using System;
using Rewired;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerInput : MonoBehaviour
{
    internal bool JumpKeyDown { get; set; }
    internal Vector2 DirectionalInput { get; set; }
    
    private PlayerController player;
    private Rewired.Player rPlayer;

    private int playerId = 0;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        rPlayer = ReInput.players.GetPlayer(playerId);

        //controls.Player.Move.performed += ctx => DirectionalInput = ctx.ReadValue<Vector2>();
        //controls.Player.Move.canceled += _ => DirectionalInput = Vector2.zero;

        //controls.Player.Jump.performed += _ => { JumpKeyDown = true; player.OnJumpInputDown(); };
        //controls.Player.Jump.canceled += _ => JumpKeyDown = false;

        rPlayer.AddInputEventDelegate(OnJumpUpdate, UpdateLoopType.Update, "Jump");
        rPlayer.AddInputEventDelegate(OnMoveHorizontal, UpdateLoopType.Update, InputActionEventType.AxisActiveOrJustInactive, "Move Horizontal");
        rPlayer.AddInputEventDelegate(OnMoveVertical, UpdateLoopType.Update, InputActionEventType.AxisActiveOrJustInactive, "Move Vertical");
    }

    private void OnMoveHorizontal(InputActionEventData data)
    {
        DirectionalInput = new Vector2(data.GetAxisRaw(), DirectionalInput.y);
    }

    private void OnMoveVertical(InputActionEventData data)
    {
        DirectionalInput = new Vector2(DirectionalInput.x, data.GetAxisRaw());
    }

    private void OnJumpUpdate(InputActionEventData data)
    {
        if (data.GetButtonDown())
        {
            JumpKeyDown = true;
            player.OnJumpInputDown();
        }

        if (data.GetButtonUp())
        {
            JumpKeyDown = false;
        }
    }
}