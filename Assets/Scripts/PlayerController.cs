// <copyright file="PlayerController.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using UnityEngine;

[RequireComponent(typeof(MovementController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 11;
    [SerializeField] private float moveSpeed = 18;
    [SerializeField] private float maxJumpDistance = 18;
    [SerializeField] private float apexRelativePosition = 0.6f;
    private float timeToJumpApex;
    private float jumpGravity;
    private float fallGravity;

    private float jumpVelocity;
    private float horizontalAccTime = 0.08f;

    private Vector2 displacement;
    private Vector2 velocity;
    private Vector2 acceleration;
    private float verticalAcceleration = 0;
    private float horizontalAcceleration = 0;

    private float wallSlideMaxSpeed = 1f;
    [SerializeField] private Vector2 wallJumpClimb = new Vector2(60, 55);
    [SerializeField] private float wallStickTime = 1f;
    private float timeToWallUnstick;

    internal MovementController controller;
    private Vector2 directionalInput;
    private bool jumpKeyPressed = false;

    private float horizontalVelocitySmoothing;

    // Debug (Can be delete)
    private bool onGround;
    private bool collideLeft;
    private bool collideRight;
    private bool isWallHugging;
    private bool isJumpingFromGround;
    private bool isSliding;
    private float slopeAngle;

    private MovementInfo movementInfo = new MovementInfo();

    private void Start()
    {
        controller = GetComponent<MovementController>();
        CalculateJump();
        timeToWallUnstick = wallStickTime;
    }

    private void Update()
    {
        DisplayDebugInfo();
        movementInfo.Update(controller);
        HandelVerticalVelocity();

        MoveHorizontally();
        HandelWallSliding();
        if (jumpKeyPressed)
        {
            Jump();
        }

        VariableJump();

        CalculateKinematics();
        Physics2D.SyncTransforms();
        controller.Move(displacement, directionalInput);
    }

    private void CalculateKinematics()
    {
        displacement = (velocity * Time.deltaTime) + (0.5f * acceleration * Mathf.Pow(Time.deltaTime, 2));
        Vector2 newAcceleration = new Vector2(horizontalAcceleration, verticalAcceleration);
        velocity += 0.5f * (acceleration + newAcceleration) * Time.deltaTime;
        acceleration = newAcceleration;
    }

    private void MoveHorizontally()
    {
        if (!controller.Collisions.SlidingDownMaxSlope)
        {
            float targetVelocityX = directionalInput.x * moveSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref horizontalVelocitySmoothing, horizontalAccTime);
        }
    }

    private void CalculateJump()
    {
        timeToJumpApex = maxJumpDistance * apexRelativePosition / moveSpeed;
        jumpGravity = -2 * jumpHeight / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = -jumpGravity * timeToJumpApex;

        fallGravity = -2 * jumpHeight / Mathf.Pow(maxJumpDistance * (1 - apexRelativePosition) / moveSpeed, 2);

        verticalAcceleration = jumpGravity;
    }

    private void Jump()
    {
        jumpKeyPressed = false;
        if (movementInfo.IsWallHugging && !controller.Collisions.Below)
        {
            if (movementInfo.HorizontalCollisionDir == directionalInput.x)
            {
                verticalAcceleration = fallGravity;
                movementInfo.IsJumpingFromGround = false;
                velocity.x = -movementInfo.HorizontalCollisionDir * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (movementInfo.HorizontalCollisionDir == -directionalInput.x)
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

        if (controller.Collisions.Below)
        {
            verticalAcceleration = jumpGravity;
            velocity.y = jumpVelocity;
            movementInfo.IsJumpingFromGround = true;
        }
    }

    private void VariableJump()
    {
        if (displacement.y < 0 && !controller.Collisions.SlidingDownMaxSlope)
        {
            verticalAcceleration = fallGravity;
        }
        else if (displacement.y > 0 && !Input.GetButton("Jump") && movementInfo.IsJumpingFromGround)
        {
            verticalAcceleration = fallGravity * 2;
        }
    }

    private void HandelWallSliding()
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
                if (directionalInput.x != movementInfo.HorizontalCollisionDir && directionalInput.x != 0)
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

    private void HandelVerticalVelocity()
    {
        if (controller.Collisions.Below || controller.Collisions.Above)
        {
            if (controller.Collisions.SlidingDownMaxSlope)
            {
                verticalAcceleration = Mathf.Pow(Mathf.Sin(controller.Collisions.SlopeAngle * Mathf.Deg2Rad), 2) * fallGravity;
                horizontalAcceleration = Mathf.Sin(controller.Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Cos(controller.Collisions.SlopeAngle * Mathf.Deg2Rad) * fallGravity;
            }
            else
            {
                velocity.y = displacement.y = 0;
                horizontalAcceleration = 0;
            }
        }
    }

    internal void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    internal void OnJumpInputDown()
    {
        if ((controller.Collisions.Below || movementInfo.IsWallHugging) && directionalInput.y >= 0)
        {
            jumpKeyPressed = true;
        }
    }

    private void GetInput()
    {
        if (directionalInput.x > 0)
        {
            movementInfo.FaceDirection = 1;
        }
        else if (directionalInput.x < 0)
        {
            movementInfo.FaceDirection = -1;
        }
        else
        {
            movementInfo.FaceDirection = 0;
        }
    }

    private void DisplayDebugInfo()
    {
        onGround = controller.Collisions.Below;
        collideLeft = controller.Collisions.Left;
        collideRight = controller.Collisions.Right;
        isWallHugging = movementInfo.IsWallHugging;
        isJumpingFromGround = movementInfo.IsJumpingFromGround;
        isSliding = controller.Collisions.SlidingDownMaxSlope;
        slopeAngle = controller.Collisions.SlopeAngle;
    }

    private class MovementInfo
    {
        internal bool IsWallHugging { get; set; } = false;

        internal bool IsJumpingFromGround { get; set; } = false;

        internal int FaceDirection { get; set; } = 0;

        internal int HorizontalCollisionDir { get; set; } = 0;

        internal void Update(MovementController controller)
        {
            if (controller.Collisions.Left)
            {
                HorizontalCollisionDir = -1;
            }
            else if (controller.Collisions.Right)
            {
                HorizontalCollisionDir = 1;
            }
            else
            {
                HorizontalCollisionDir = 0;
                IsWallHugging = false;
            }

            if (controller.Collisions.Below)
            {
                IsJumpingFromGround = false;
            }
        }
    }
}