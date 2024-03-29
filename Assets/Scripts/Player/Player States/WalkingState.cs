﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingState : PlayerState
{
    public WalkingState(PlayerController player, PlayerInput input) : base(player, input) { }
    
    private float horizontalVelocitySmoothing;

    public override void Execute()
    {
        HandleLogic();

        if (input.IsJumpKeyDown)
            player.stateMachine.SetState(new JumpingState(player, input));

        if (player.velocity.Value.x == 0 && player.controller.isGrounded)
            player.stateMachine.SetState(new GroundedState(player, input));
    }

    public void HandleLogic()
    {
        float targetVelocityX = input.DirectionalInput.x * player.playerMovementData.WalkSpeed;
        player.velocity.Value.x = Mathf.SmoothDamp(player.velocity.Value.x, targetVelocityX, ref horizontalVelocitySmoothing, player.playerMovementData.HorizontalAccelerationTime);
    }
}
