using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingState : PlayerState
{
    public JumpingState(PlayerController player, PlayerInput input) : base(player, input) { }

    private float horizontalVelocitySmoothing;

    public override void Enter()
    {
        player.acceleration.Value = new Vector2(0, player.jumpGravity);
        player.velocity.Value.y = player.jumpVelocity;
    }

    public override void Execute()
    {
        HandleLogic();

        if (player.velocity.Value.y < 0 && !player.controller.isGrounded)
            player.stateMachine.SetState(new FallingState(player, input));

        if (player.velocity.Value.y > 0 && !input.IsJumpKeyDown)
            player.stateMachine.SetState(new SmallJumpFallingState(player, input));
    }

    public void HandleLogic()
    {
        float targetVelocityX = input.DirectionalInput.x * player.playerMovementData.WalkSpeed;
        player.velocity.Value.x = Mathf.SmoothDamp(player.velocity.Value.x, targetVelocityX, ref horizontalVelocitySmoothing, player.playerMovementData.HorizontalAccelerationTime);
    }
}
