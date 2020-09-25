using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingState : PlayerState
{
    public FallingState(PlayerController player, PlayerInput input) : base(player, input) { }
    
    private float horizontalVelocitySmoothing;

    public override void Enter()
    {
        player.acceleration.Value = new Vector2(0, player.fallGravity);
    }

    public override void Execute()
    {
        HandleLogic();

        if (player.controller.isGrounded)
            player.stateMachine.SetState(new GroundedState(player, input));
    }

    public void HandleLogic()
    {
        float targetVelocityX = input.DirectionalInput.x * player.playerMovementData.WalkSpeed;
        player.velocity.Value.x = Mathf.SmoothDamp(player.velocity.Value.x, targetVelocityX, ref horizontalVelocitySmoothing, player.playerMovementData.HorizontalAccelerationTime);
    }

    public override void Exit()
    {
        player.acceleration.Value = new Vector2(0, player.jumpGravity);
    }
}
