using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidController : PlayerController
{
    [SerializeField] protected override float JumpHeight => 5f;
    [SerializeField] protected override float MaxJumpDistance => 12f;
    [SerializeField] protected override float ApexRelativePosition => .6f;
    [SerializeField] protected override float FallMultiplier => 2f;

    [SerializeField] protected override float WalkSpeed => 18f;
    [SerializeField] protected override float HorizontalAccTime => .08f;

    [SerializeField] protected override float LadderClimbSpeed => 15f;

    private new void Start()
    {
        base.Start();
    }

    private new void Update()
    {
        base.Update();
    }
}
