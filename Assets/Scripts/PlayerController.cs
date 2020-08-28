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

    private float wallSlideMaxSpeed = 8f;
    [SerializeField] private Vector2 wallJumpClimb = new Vector2(60, 55);
    [SerializeField] private float wallStickTime = 1f;
    private float timeToWallUnstick;

    private MovementController controller;
    private Vector2 input;
    private bool jumpKeyPressed;

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
        jumpKeyPressed = false;
        CalculateJump();
        gravity = jumpGravity;
        timeToWallUnstick = wallStickTime;
    }

    private void Update()
    {
        CalculateJump();
        GetInput();

        onGround = controller.Collisions.Below;
        collideLeft = controller.Collisions.Left;
        collideRight = controller.Collisions.Right;
        isWallHugging = movementInfo.IsWallHugging;
        isJumpingFromGround = movementInfo.IsJumpingFromGround;

        movementInfo.Update(controller);

        if (controller.Collisions.Below)
        {
            displacement.y = velocity.y = 0; // Cancel all speed if there's a vertical collision
            gravity = jumpGravity;
            movementInfo.IsJumpingFromGround = false;
        }

        if (controller.Collisions.Above)
        {
            displacement.y = velocity.y = 0;
        }

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref horizontalVelocitySmoothing, horizontalAccTime);

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
                if (input.x != movementInfo.HorizontalCollisionDir && input.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                    if (Input.GetButtonDown("Jump"))
                    {
                        velocity.x = -movementInfo.HorizontalCollisionDir * wallJumpClimb.x;
                        velocity.y = wallJumpClimb.y;
                        goto Finish;
                    }
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

        Finish:
            ;
        }

        if (jumpKeyPressed)
        {
            Jump();
        }

        if (displacement.y < 0)
        {
            gravity = fallGravity;
        }
        else if (displacement.y > 0 && !Input.GetButton("Jump") && movementInfo.IsJumpingFromGround)
        {
            gravity = fallGravity * 2;
        }

        displacement = (velocity * Time.deltaTime) + (0.5f * acceleration * Mathf.Pow(Time.deltaTime, 2));
        Vector3 newAcceleration = new Vector3(0, gravity, 0);
        velocity += 0.5f * (acceleration + acceleration) * Time.deltaTime;
        acceleration = newAcceleration;

        Physics2D.SyncTransforms();
        controller.Move(displacement, input);
    }

    private void CalculateJump()
    {
        timeToJumpApex = maxJumpDistance * apexRelativePosition / moveSpeed;
        jumpGravity = -2 * jumpHeight / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = -jumpGravity * timeToJumpApex;

        fallGravity = -2 * jumpHeight / Mathf.Pow(maxJumpDistance * (1 - apexRelativePosition) / moveSpeed, 2);
    }

    private void Jump()
    {
        jumpKeyPressed = false;
        if (movementInfo.IsWallHugging && !controller.Collisions.Below)
        {
            if (movementInfo.HorizontalCollisionDir == movementInfo.FaceDirection)
            {
                Debug.Log("Wall jumped");
                gravity = fallGravity;
                movementInfo.IsJumpingFromGround = false;
                velocity.x = -movementInfo.HorizontalCollisionDir * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
        }

        if (controller.Collisions.Below)
        {
            Debug.Log("Jumped from ground");
            velocity.y = jumpVelocity;
            movementInfo.IsJumpingFromGround = true;
        }
    }

    private void GetInput()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (Input.GetButtonDown("Jump") && (controller.Collisions.Below || movementInfo.IsWallHugging) && input.y >= 0)
        {
            jumpKeyPressed = true;
        }

        if (input.x > 0)
        {
            movementInfo.FaceDirection = 1;
        }
        else if (input.x < 0)
        {
            movementInfo.FaceDirection = -1;
        }
        else
        {
            movementInfo.FaceDirection = 0;
        }
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
        }
    }
}