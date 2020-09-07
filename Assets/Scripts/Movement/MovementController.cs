// <copyright file="MovementController.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using UnityEngine;
using System.Collections;

public class MovementController : RaycastController
{

    private float maxSlopeAngle = 45;

    internal CollisionInfo Collisions;
    internal Vector2 PlayerInput;

    protected override void Start()
    {
        base.Start();
        Collisions.FaceDir = 1;
    }

    internal void Move(Vector2 displacement, bool standingOnPlatform)
    {
        Move(displacement, Vector2.zero, standingOnPlatform);
    }

    internal void Move(Vector2 displacement, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();

        Collisions.Reset();
        Collisions.DisplacementOld = displacement;
        PlayerInput = input;

        if (displacement.y < 0)
        {
            DescendSlope(ref displacement);
        }

        if (displacement.x != 0)
        {
            Collisions.FaceDir = (int)Mathf.Sign(displacement.x);
        }

        HorizontalCollisions(ref displacement);
        if (displacement.y != 0)
        {
            VerticalCollisions(ref displacement);
        }

        transform.Translate(displacement);

        if (standingOnPlatform)
        {
            Collisions.Below = true;
        }
    }

    void HorizontalCollisions(ref Vector2 displacement)
    {
        float directionX = Collisions.FaceDir;
        float rayLength = Mathf.Abs(displacement.x) + SkinWidth;

        if (Mathf.Abs(displacement.x) < SkinWidth)
        {
            rayLength = 2 * SkinWidth;
        }

        for (int i = 0; i < HorizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? RaycastStartingPts.BottomLeft : RaycastStartingPts.BottomRight;
            rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (hit)
            {

                if (hit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Collisions.DescendingSlope)
                    {
                        Collisions.DescendingSlope = false;
                        displacement = Collisions.DisplacementOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != Collisions.SlopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - SkinWidth;
                        displacement.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref displacement, slopeAngle, hit.normal);
                    displacement.x += distanceToSlopeStart * directionX;
                }

                if (!Collisions.ClimbingSlope || slopeAngle > maxSlopeAngle)
                {
                    displacement.x = (hit.distance - SkinWidth) * directionX;
                    rayLength = hit.distance;

                    if (Collisions.ClimbingSlope)
                    {
                        displacement.y = Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.x);
                    }

                    Collisions.Left = directionX == -1;
                    Collisions.Right = directionX == 1;
                }
            }
        }
    }

    void VerticalCollisions(ref Vector2 displacement)
    {
        float directionY = Mathf.Sign(displacement.y);
        float rayLength = Mathf.Abs(displacement.y) + SkinWidth;

        for (int i = 0; i < VerticalRayCount; i++)
        {

            Vector2 rayOrigin = (directionY == -1) ? RaycastStartingPts.BottomLeft : RaycastStartingPts.TopLeft;
            rayOrigin += Vector2.right * (VerticalRaySpacing * i + displacement.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, CollisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if (hit)
            {
                if (hit.collider.tag == "Semisolid")
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                    if (Collisions.FallingThroughPlatform)
                    {
                        continue;
                    }
                    if (PlayerInput.y == -1 && Input.GetButton("Jump"))
                    {
                        Collisions.FallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", .5f);
                        continue;
                    }
                }

                displacement.y = (hit.distance - SkinWidth) * directionY;
                rayLength = hit.distance;

                if (Collisions.ClimbingSlope)
                {
                    displacement.x = displacement.y / Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(displacement.x);
                }

                Collisions.Below = directionY == -1;
                Collisions.Above = directionY == 1;
            }
        }

        if (Collisions.ClimbingSlope)
        {
            float directionX = Mathf.Sign(displacement.x);
            rayLength = Mathf.Abs(displacement.x) + SkinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? RaycastStartingPts.BottomLeft : RaycastStartingPts.BottomRight) + Vector2.up * displacement.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != Collisions.SlopeAngle)
                {
                    displacement.x = (hit.distance - SkinWidth) * directionX;
                    Collisions.SlopeAngle = slopeAngle;
                    Collisions.SlopeNormal = hit.normal;
                }
            }
        }
    }

    void ClimbSlope(ref Vector2 displacement, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(displacement.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (displacement.y <= climbmoveAmountY)
        {
            displacement.y = climbmoveAmountY;
            displacement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(displacement.x);
            Collisions.Below = true;
            Collisions.ClimbingSlope = true;
            Collisions.SlopeAngle = slopeAngle;
            Collisions.SlopeNormal = slopeNormal;
        }
    }

    void DescendSlope(ref Vector2 displacement)
    {

        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(RaycastStartingPts.BottomLeft, Vector2.down, Mathf.Abs(displacement.y) + SkinWidth, CollisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(RaycastStartingPts.BottomRight, Vector2.down, Mathf.Abs(displacement.y) + SkinWidth, CollisionMask);
        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownMaxSlope(maxSlopeHitLeft, ref displacement);
            SlideDownMaxSlope(maxSlopeHitRight, ref displacement);
        }

        if (!Collisions.SlidingDownMaxSlope)
        {
            float directionX = Mathf.Sign(displacement.x);
            Vector2 rayOrigin = (directionX == -1) ? RaycastStartingPts.BottomRight : RaycastStartingPts.BottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, CollisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.x))
                        {
                            float moveDistance = Mathf.Abs(displacement.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            displacement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(displacement.x);
                            displacement.y -= descendmoveAmountY;

                            Collisions.SlopeAngle = slopeAngle;
                            Collisions.DescendingSlope = true;
                            Collisions.Below = true;
                            Collisions.SlopeNormal = hit.normal;
                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 displacement)
    {

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                displacement.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(displacement.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                Collisions.SlopeAngle = slopeAngle;
                Collisions.SlidingDownMaxSlope = true;
                Collisions.SlopeNormal = hit.normal;
            }
        }

    }

    void ResetFallingThroughPlatform()
    {
        Collisions.FallingThroughPlatform = false;
    }

    internal struct CollisionInfo
    {
        internal bool Above, Below;
        internal bool Left, Right;

        internal bool ClimbingSlope;
        internal bool DescendingSlope;
        internal bool SlidingDownMaxSlope;

        internal float SlopeAngle, SlopeAngleOld;
        internal Vector2 SlopeNormal;
        internal Vector2 DisplacementOld;
        internal int FaceDir;
        internal bool FallingThroughPlatform;

        internal bool CanClimbLadder;

        internal void Reset()
        {
            Above = Below = false;
            Left = Right = false;
            ClimbingSlope = false;
            DescendingSlope = false;
            SlidingDownMaxSlope = false;
            SlopeNormal = Vector2.zero;

            SlopeAngleOld = SlopeAngle;
            SlopeAngle = 0;
        }
    }

}