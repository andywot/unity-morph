// <copyright file="MovementController.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using UnityEngine;

public class MovementController : RaycastController
{
    private Vector3 displacementOld;
    private float directionXOld;

    private float maxClimbAngle = 50;
    private float maxDescendAngle = 45;

    internal Vector2 PlayerInput { get; set; }

    internal CollisionInfo Collisions { get; } = new CollisionInfo();

    protected override void Start()
    {
        base.Start();
    }

    internal void Move(Vector3 displacement)
    {
        Move(displacement, Vector2.zero);
    }

    internal void Move(Vector3 displacement, Vector2 input)
    {
        UpdateRaycastOrigins();
        Collisions.Reset();
        PlayerInput = input;
        displacementOld = displacement;
        if (displacement.x != 0)
        {
            directionXOld = Mathf.Sign(displacement.x);
        }

        if (displacement.y < 0)
        {
            DescendSlope(ref displacement);
        }

        HorizontalCollisions(ref displacement);

        if (displacement.y != 0)
        {
            VerticalCollisions(ref displacement);
        }

        transform.Translate(displacement);
    }

    private void HorizontalCollisions(ref Vector3 displacement)
    {
        float directionX = Mathf.Sign(displacement.x);
        float rayLength = Mathf.Abs(displacement.x) + SkinWidth;

        if (Mathf.Abs(displacement.x) < SkinWidth)
        {
            rayLength = 2 * SkinWidth;
        }

        if (displacement.x == 0)
        {
            directionX = directionXOld;
        }

        for (int i = 0; i < HorizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? RaycastStartingPts.BottomLeft : RaycastStartingPts.BottomRight;
            rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                if (hit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    if (Collisions.DescendingSlope)
                    {
                        Collisions.DescendingSlope = false;
                        displacement = displacementOld;
                    }

                    float distanceToSlopeStart = 0;
                    if (slopeAngle != Collisions.SlopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - SkinWidth;
                        displacement.x -= distanceToSlopeStart * directionX;
                    }

                    ClimbSlope(ref displacement, slopeAngle);
                    displacement.x += distanceToSlopeStart * directionX;
                }

                if (!Collisions.ClimbingSlope || slopeAngle > maxClimbAngle)
                {
                    displacement.x = Mathf.Min(Mathf.Abs(displacement.x), hit.distance - SkinWidth) * directionX;
                    rayLength = Mathf.Min(Mathf.Abs(displacement.x) + SkinWidth, hit.distance);
                    if (Collisions.ClimbingSlope)
                    {
                        displacement.y = displacement.x * Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad);
                    }

                    Collisions.Left = directionX == -1;
                    Collisions.Right = directionX == 1;
                }
            }
        }
    }

    private void ClimbSlope(ref Vector3 displacement, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(displacement.x);
        float climbDistanceY = moveDistance * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);

        if (displacement.y <= climbDistanceY)
        {
            displacement.x = moveDistance * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(displacement.x);
            displacement.y = climbDistanceY;
            Collisions.Below = true;
            Collisions.ClimbingSlope = true;
            Collisions.SlopeAngle = slopeAngle;
        }
    }

    private void DescendSlope(ref Vector3 displacement)
    {
        float directionX = Mathf.Sign(displacement.x);
        Vector2 rayOrigin = directionX == 1 ? RaycastStartingPts.BottomLeft : RaycastStartingPts.BottomRight;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, CollisionMask); // Not sure with infintie length ray

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.x))
                    {
                        float moveDistance = Mathf.Abs(displacement.x);
                        float descendDistanceY = moveDistance * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);

                        displacement.x = moveDistance * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(displacement.x);
                        displacement.y -= descendDistanceY;

                        Collisions.Below = true;
                        Collisions.DescendingSlope = true;
                        Collisions.SlopeAngle = slopeAngle;
                    }
                }
            }
        }
    }

    private void VerticalCollisions(ref Vector3 displacement)
    {
        float directionY = Mathf.Sign(displacement.y);
        float rayLength = Mathf.Abs(displacement.y) + SkinWidth;

        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? RaycastStartingPts.BottomLeft : RaycastStartingPts.TopLeft;
            rayOrigin += Vector2.right * ((VerticalRaySpacing * i) + displacement.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, CollisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                if (hit.collider.tag == "Semisolid")
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue; // Skip the collision check for current ray
                    }

                    if (PlayerInput.y < 0 && Input.GetButton("Jump"))
                    {
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
            rayLength = Mathf.Abs(displacement.x + SkinWidth);
            Vector2 rayOrigin = (directionX == -1 ? RaycastStartingPts.BottomLeft : RaycastStartingPts.BottomRight) + (Vector2.up * displacement.y);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, directionX * Vector2.right, rayLength, CollisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != Collisions.SlopeAngle)
                {
                    displacement.x = (hit.distance - SkinWidth) * directionX;
                    Collisions.SlopeAngle = slopeAngle;
                }
            }
        }
    }

    internal class CollisionInfo
    {
        internal bool Above { get; set; }

        internal bool Below { get; set; }

        internal bool Left { get; set; }

        internal bool Right { get; set; }

        internal bool ClimbingSlope { get; set; }

        internal bool DescendingSlope { get; set; }

        internal float SlopeAngle { get; set; }

        internal float SlopeAngleOld { get; set; }

        internal void Reset()
        {
            Above = Below = Left = Right = ClimbingSlope = false;
            SlopeAngleOld = SlopeAngle;
            SlopeAngle = 0;
        }
    }
}