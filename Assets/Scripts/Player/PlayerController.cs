// <copyright file="PlayerController.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using UnityEngine;

[RequireComponent(typeof(MovementController))]
public class PlayerController : MonoBehaviour
{
    // Jumping Parameters
    protected virtual float JumpHeight { get; } = 11;
    protected virtual float MaxJumpDistance { get; } = 18;
    protected virtual float ApexRelativePosition { get; } = 0.6f;
    protected virtual float FallMultiplier { get; } = 2f;

    // Horizontal Movement Parameters
    protected virtual float WalkSpeed { get; } = 18;
    protected virtual float HorizontalAccTime { get; } = 0.08f;

    // Climbing Parameters
    protected virtual float LadderClimbSpeed { get; } = 10;

    private float timeToJumpApex;
    protected float jumpGravity;
    protected float fallGravity;
    protected float jumpVelocity;

    protected Vector2 displacement;
    protected Vector2 velocity;
    protected Vector2 acceleration;
    protected float verticalAcceleration;

    internal MovementController controller;
    protected PlayerInput Input;

    protected bool isJumping = false;

    protected float horizontalVelocitySmoothing;

    protected MovementInfo movementInfo = new MovementInfo();

    // Debug (Can be delete)
    private bool onGround;
    private bool collideLeft;
    private bool collideRight;
    private bool isWallHugging;
    private bool isJumpingFromGround;
    private bool canClimbLadder;
    private bool isClimbing;
    private bool onHorizontalSlope;
    private bool onVerticalSlope;
    private float xSlopeAngle;
    private float ySlopeAngle;
    private float slopeDirection;
    
    protected void Start()
    {
        controller = GetComponent<MovementController>();
        Input = GetComponent<PlayerInput>();
        CalculateJump();
    }

    protected void Update()
    {
        GetInput();
        DisplayDebugInfo();
        movementInfo.Update(controller);
        StopMotionOnCollision();

        MoveHorizontally();
        HandelWallSliding();
        ClimbLadder();
        
        if (isJumping)
        {
            Jump();
        }

        SetGravity();

        CalculateKinematics();
        Physics2D.SyncTransforms();
        controller.Move(displacement, Input.DirectionalInput);
    }

    private void CalculateKinematics()
    {
        displacement = (velocity * Time.deltaTime) + (0.5f * acceleration * Mathf.Pow(Time.deltaTime, 2));
        Vector2 newAcceleration = new Vector2(0, verticalAcceleration);
        velocity += 0.5f * (acceleration + acceleration) * Time.deltaTime;
        acceleration = newAcceleration;
    }

    private void MoveHorizontally()
    {
        float targetVelocityX = Input.DirectionalInput.x * WalkSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref horizontalVelocitySmoothing, HorizontalAccTime);
    }

    private void CalculateJump()
    {
        timeToJumpApex = MaxJumpDistance * ApexRelativePosition / WalkSpeed;
        jumpGravity = -2 * JumpHeight / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = -jumpGravity * timeToJumpApex;

        fallGravity = -2 * JumpHeight / Mathf.Pow(MaxJumpDistance * (1 - ApexRelativePosition) / WalkSpeed, 2);

        verticalAcceleration = jumpGravity;
    }

    internal virtual void Jump()
    {
        isJumping = false;

        verticalAcceleration = jumpGravity;
        velocity.y = jumpVelocity;
        movementInfo.IsJumpingFromGround = true;
    }

    private void SetGravity()
    {
        if (displacement.y < 0 && !movementInfo.IsClimbing)
        {
            verticalAcceleration = fallGravity;
        }
        else if (displacement.y > 0 && !Input.JumpKeyDown && movementInfo.IsJumpingFromGround)
        {
            verticalAcceleration = fallGravity * FallMultiplier;
        }
    }

    internal void OnJumpInputDown()
    {
        if ((controller.Collisions.Below || movementInfo.IsWallHugging || controller.Collisions.CanClimbLadder) && Input.DirectionalInput.y >= 0)
        {
            isJumping = true;
        }
    }

    protected virtual void HandelWallSliding() { }

    private void ClimbLadder()
    {
        if (controller.Collisions.CanClimbLadder)
        {
            verticalAcceleration = 0;
            if (Input.DirectionalInput.y != 0)
            {
                velocity.y = LadderClimbSpeed * Input.DirectionalInput.y;
                movementInfo.IsClimbing = true;
            }
            else
            {
                velocity.y = displacement.y = 0;
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

    private void GetInput()
    {
        if (Input.DirectionalInput.x > 0)
        {
            movementInfo.FaceDirection = 1;
        }
        else if (Input.DirectionalInput.x < 0)
        {
            movementInfo.FaceDirection = -1;
        }
        else
        {
            movementInfo.FaceDirection = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ladder")
        {
            controller.Collisions.CanClimbLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Ladder")
        {
            controller.Collisions.CanClimbLadder = false;
            verticalAcceleration = fallGravity;
        }
    }

    private void DisplayDebugInfo()
    {
        onGround = controller.Collisions.Below;
        collideLeft = controller.Collisions.Left;
        collideRight = controller.Collisions.Right;
        isWallHugging = movementInfo.IsWallHugging;
        isJumpingFromGround = movementInfo.IsJumpingFromGround;
        canClimbLadder = controller.Collisions.CanClimbLadder;
        isClimbing = movementInfo.IsClimbing;
    }

    protected class MovementInfo
    {
        internal bool IsWallHugging { get; set; } = false;

        internal bool IsJumpingFromGround { get; set; } = false;

        internal int FaceDirection { get; set; } = 0;

        internal int HorizontalCollisionDir { get; set; } = 0;

        internal bool IsClimbing { get; set; } = false;

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

            IsClimbing = false;
        }
    }
}