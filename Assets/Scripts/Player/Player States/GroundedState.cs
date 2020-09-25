using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class GroundedState : PlayerState
{
    public GroundedState(PlayerController player, PlayerInput input) : base(player, input) { }
    
    public override void Execute()
    {
        if (input.DirectionalInput.x != 0)
            player.stateMachine.SetState(new WalkingState(player, input));

        if (input.IsJumpKeyDown && player.controller.isGrounded)
            player.stateMachine.SetState(new JumpingState(player, input));

        if (player.velocity.Value.y < 0 && !player.controller.isGrounded)
            player.stateMachine.SetState(new FallingState(player, input));
    }
}
