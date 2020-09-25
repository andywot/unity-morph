using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MovementController))]
public class PlayerController : MonoBehaviour
{
    #region Class references

    [Header("Class references")]
    public PlayerControllerParameters playerMovementData;
    public MovementController controller;
    public PlayerInput playerInput;
    public StateMachine stateMachine = new StateMachine();

    #endregion

    #region Exposed fields

    [Header("Exposed fields")]
    public Vector2Variable displacement;
    public Vector2Variable velocity;
    public Vector2Variable acceleration;

    [Header("Jumping parameters")]
    public float timeToJumpApex;

    public float jumpGravity;
    public float fallGravity;
    public float jumpVelocity;

    #endregion

    #region Internal fields



    #endregion

    #region MonoBehaviours

    private void Awake()
    {
        CalculateJump();
        controller.OnTriggerEnterEvent += OnTriggerEnterEvent;
        controller.OnTriggerExitEvent += OnTriggerExitEvent;
        controller.OnControllerCollidedEvent += OnControllerCollidedEvent;
    }

    private void Start()
    {
        stateMachine.SetState(new GroundedState(this, playerInput));
    }

    private void Update()
    {
        StopMotionOnCollision();

        stateMachine.currentState.Execute();

        CalculateKinematics();
        Physics2D.SyncTransforms();
        controller.Move(displacement.Value);

        velocity.Value = controller.velocity;
    }

    #endregion

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

    private void CalculateKinematics()
    {
        velocity.Value += acceleration.Value * Time.deltaTime;
        displacement.Value = velocity.Value * Time.deltaTime;
    }

    private void StopMotionOnCollision()
    {
        if (controller.isGrounded)
        {
            velocity.Value.y = 0;
        }
    }

    private void CalculateJump()
    {
        timeToJumpApex = playerMovementData.MaxJumpDistance * playerMovementData.ApexRelativePosition / playerMovementData.WalkSpeed;
        jumpGravity = -2 * playerMovementData.JumpHeight / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = -jumpGravity * timeToJumpApex;

        fallGravity = -2 * playerMovementData.JumpHeight / Mathf.Pow(playerMovementData.MaxJumpDistance * (1 - playerMovementData.ApexRelativePosition) / playerMovementData.WalkSpeed, 2);

        acceleration.Value = new Vector2(0, jumpGravity);
    }
}
