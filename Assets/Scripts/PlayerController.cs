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

    private float gravity;
    private float jumpVelocity;
    private float horizontalAccTime = 0.08f;

    private Vector3 displacement;
    private Vector3 velocity;
    private Vector3 acceleration;

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

    private MovementInfo movementInfo = new MovementInfo();

    private void Start()
    {
        controller = GetComponent<MovementController>();
        CalculateJump();
        timeToWallUnstick = wallStickTime;
    }

    private void Update()
    {
        GetInput();
        DisplayDebugInfo();
        movementInfo.Update(controller);
        StopMotionOnCollision();

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
        Vector3 newAcceleration = new Vector3(0, gravity, 0);
        velocity += 0.5f * (acceleration + acceleration) * Time.deltaTime;
        acceleration = newAcceleration;
    }

    private void MoveHorizontally()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref horizontalVelocitySmoothing, horizontalAccTime);
    }

    private void CalculateJump()
    {
        timeToJumpApex = maxJumpDistance * apexRelativePosition / moveSpeed;
        jumpGravity = -2 * jumpHeight / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = -jumpGravity * timeToJumpApex;

        fallGravity = -2 * jumpHeight / Mathf.Pow(maxJumpDistance * (1 - apexRelativePosition) / moveSpeed, 2);

        gravity = jumpGravity;
    }

    private void Jump()
    {
        jumpKeyPressed = false;
        if (movementInfo.IsWallHugging && !controller.Collisions.Below)
        {
            if (movementInfo.HorizontalCollisionDir == directionalInput.x)
            {
                gravity = fallGravity;
                movementInfo.IsJumpingFromGround = false;
                velocity.x = -movementInfo.HorizontalCollisionDir * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (movementInfo.HorizontalCollisionDir == -directionalInput.x)
            {
                gravity = fallGravity;
                movementInfo.IsJumpingFromGround = false;
                velocity.x = -movementInfo.HorizontalCollisionDir * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else
            {
                gravity = fallGravity;
                movementInfo.IsJumpingFromGround = false;
                velocity.x = -movementInfo.HorizontalCollisionDir * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
        }

        if (controller.Collisions.Below)
        {
            gravity = jumpGravity;
            velocity.y = jumpVelocity;
            movementInfo.IsJumpingFromGround = true;
        }
    }

    private void VariableJump()
    {
        if (displacement.y < 0)
        {
            gravity = fallGravity;
        }
        else if (displacement.y > 0 && !Input.GetButton("Jump") && movementInfo.IsJumpingFromGround)
        {
            gravity = fallGravity * 2;
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

    private void StopMotionOnCollision()
    {
        if (controller.Collisions.Below)
        {
            displacement.y = velocity.y = 0;
        }

        if (controller.Collisions.Above)
        {
            displacement.y = velocity.y = 0;
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