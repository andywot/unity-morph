using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerControllerParameters : ScriptableObject
{
    public float JumpHeight;
    public float MaxJumpDistance;
    public float ApexRelativePosition;
    public float FallMultiplier;

    public float WalkSpeed;
    public float HorizontalAccelerationTime;

    public float LadderClimbSpeed;

    public float WallSlideSpeed;
    public Vector2 WallJump;
    public float WallStickTime;

    public PlayerForm playerForm;
}
