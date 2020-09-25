using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MovementController))]
public class PlayerController : MonoBehaviour
{
    public PlayerControllerParameters player;
    public MovementController controller;
    public PlayerInput Input;

    public Vector2Variable displacement;
    public Vector2Variable velocity;
    public Vector2Variable acceleration;

    private float timeToJumpApex;
    protected float jumpGravity;
    protected float fallGravity;
    protected float jumpVelocity;

    private float horizontalVelocitySmoothing;
    private bool isJumping;

    private PlayerState currentState;

    private void Awake()
    {
        CalculateJump();
        controller.OnTriggerEnterEvent += OnTriggerEnterEvent;
        controller.OnTriggerExitEvent += OnTriggerExitEvent;
        controller.OnControllerCollidedEvent += OnControllerCollidedEvent;
    }

    #region Event listeners

    private void OnTriggerEnterEvent(Collider2D col)
    {
    }

    private void OnTriggerExitEvent(Collider2D col)
    {
    }

    private void OnControllerCollidedEvent(RaycastHit2D hit)
    {
    }

    #endregion

    private void Update()
    {
        StopMotionOnCollision();

        currentState.Execute();

        CalculateKinematics();
        Physics2D.SyncTransforms();
        controller.Move(displacement.Value);

        velocity.Value = controller.velocity;
    }

    private void CalculateKinematics()
    {
        velocity.Value += acceleration.Value * Time.deltaTime;
        displacement.Value = velocity.Value * Time.deltaTime;
    }

    private void SetGravity()
    {
        if (velocity.Value.y < 0)
        {
            acceleration.Value = new Vector2(0, fallGravity);
        }
        else if (displacement.Value.y > 0 && !Input.JumpKeyDown)
        {
            acceleration.Value = new Vector2(0, fallGravity * player.FallMultiplier);
        }
    }

    private void Jump()
    {
        isJumping = false;

        acceleration.Value = new Vector2(0, jumpGravity);
        velocity.Value.y = jumpVelocity;
    }

    public void OnJumpInputDown(InputActionEventData data)
    {
        if (controller.isGrounded && Input.DirectionalInput.y >= 0)
        {
            isJumping = true;
        }
    }

    private void StopMotionOnCollision()
    {
        if (controller.isGrounded)
        {
            velocity.Value.y = 0;
        }
    }

    private void MoveHorizontally()
    {
        float targetVelocityX = Input.DirectionalInput.x * player.WalkSpeed;
        velocity.Value.x = Mathf.SmoothDamp(velocity.Value.x, targetVelocityX, ref horizontalVelocitySmoothing, player.HorizontalAccelerationTime);
    }

    private void HandelWallSliding()
    {
    }

    private void ClimbLadder()
    {
    }

    private void CalculateJump()
    {
        timeToJumpApex = player.MaxJumpDistance * player.ApexRelativePosition / player.WalkSpeed;
        jumpGravity = -2 * player.JumpHeight / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = -jumpGravity * timeToJumpApex;

        fallGravity = -2 * player.JumpHeight / Mathf.Pow(player.MaxJumpDistance * (1 - player.ApexRelativePosition) / player.WalkSpeed, 2);

        acceleration.Value = new Vector2(0, jumpGravity);
    }

    public void SetState(PlayerState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;

        if (currentState != null)
            currentState.Enter();
    }
}
