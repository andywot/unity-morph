using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxController : PlayerController
{
    private float wallSlideMaxSpeed = 1f;
    private Vector2 wallJumpClimb = new Vector2(60, 55);
    private float wallStickTime = .05f;
    private float timeToWallUnstick;

    private new void Start()
    {
        base.Start();
        timeToWallUnstick = wallStickTime;
    }

    private new void Update()
    {
        base.Update();
    }

    internal override void Jump()
    {
        isJumping = false;

        if (movementInfo.IsWallHugging && !controller.Collisions.Below)
        {
            if (movementInfo.HorizontalCollisionDir == Input.DirectionalInput.x)
            {
                verticalAcceleration = fallGravity;
                movementInfo.IsJumpingFromGround = false;
                velocity.x = -movementInfo.HorizontalCollisionDir * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (movementInfo.HorizontalCollisionDir == -Input.DirectionalInput.x)
            {
                verticalAcceleration = fallGravity;
                movementInfo.IsJumpingFromGround = false;
                velocity.x = -movementInfo.HorizontalCollisionDir * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else
            {
                verticalAcceleration = fallGravity;
                movementInfo.IsJumpingFromGround = false;
                velocity.x = -movementInfo.HorizontalCollisionDir * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
        }

        if (controller.Collisions.Below || controller.Collisions.CanClimbLadder)
        {
            verticalAcceleration = jumpGravity;
            velocity.y = jumpVelocity;
            movementInfo.IsJumpingFromGround = true;
        }
    }

    protected override void HandelWallSliding()
    {
        if ((movementInfo.HorizontalCollisionDir == movementInfo.FaceDirection) && !controller.Collisions.Below && (movementInfo.FaceDirection != 0))
        {
            movementInfo.IsWallHugging = true;
            if (velocity.y < -wallSlideMaxSpeed)
            {
                velocity.y = -wallSlideMaxSpeed;
            }
        }

        if (movementInfo.HorizontalCollisionDir != 0 && !controller.Collisions.Below)
        {
            if (timeToWallUnstick > 0)
            {
                horizontalVelocitySmoothing = velocity.x = 0;
                if (Input.DirectionalInput.x != movementInfo.HorizontalCollisionDir && Input.DirectionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }
}
