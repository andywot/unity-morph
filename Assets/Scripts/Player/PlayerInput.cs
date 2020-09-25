// <copyright file="PlayerInput.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using System;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour
{
    #region Exposed fields and properties

    public Rewired.Player rPlayer;

    public bool IsJumpKeyDown { get; private set; }
    public Vector2 DirectionalInput { get; private set; }

    #endregion

    #region Internal fields

    private PlayerController player;
    private int playerId = 0;

    #endregion

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        rPlayer = ReInput.players.GetPlayer(playerId);

        rPlayer.AddInputEventDelegate(OnJumpUpdate, UpdateLoopType.Update, "Jump");
        rPlayer.AddInputEventDelegate(OnMoveHorizontal, UpdateLoopType.Update, InputActionEventType.AxisActiveOrJustInactive, "Move Horizontal");
        rPlayer.AddInputEventDelegate(OnMoveVertical, UpdateLoopType.Update, InputActionEventType.AxisActiveOrJustInactive, "Move Vertical");
    }

    #region Input events

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
        IsJumpKeyDown = data.GetButton();
    }

    #endregion
}