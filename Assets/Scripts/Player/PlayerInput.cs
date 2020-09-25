// <copyright file="PlayerInput.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using System;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

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

        rPlayer.AddInputEventDelegate(OnJumpUpdate, UpdateLoopType.Update, "Jump");
        rPlayer.AddInputEventDelegate(player.OnJumpInputDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Jump");
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
        }

        if (data.GetButtonUp())
        {
            JumpKeyDown = false;
        }
    }
}