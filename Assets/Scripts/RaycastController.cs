// <copyright file="RaycastController.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    protected const float SkinWidth = .03f;
    private new BoxCollider2D collider;

    internal RaycastOrigins RaycastStartingPts { get; } = new RaycastOrigins();

    protected int HorizontalRayCount { get; private set; } = 3;
    protected int VerticalRayCount { get; private set; } = 3;

    protected float HorizontalRaySpacing { get; private set; }
    protected float VerticalRaySpacing { get; private set; }

    protected LayerMask CollisionMask { get; private set; }

    protected virtual void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CollisionMask = LayerMask.GetMask("Ground");
        CalculateRaySpacing();
    }

    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(SkinWidth * -2);

        RaycastStartingPts.Update(
            new Vector2(bounds.min.x, bounds.min.y),
            new Vector2(bounds.max.x, bounds.min.y),
            new Vector2(bounds.min.x, bounds.max.y),
            new Vector2(bounds.max.x, bounds.max.y));
    }

    private void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(SkinWidth * -2);

        HorizontalRayCount = Mathf.Clamp(HorizontalRayCount, 2, int.MaxValue);
        VerticalRayCount = Mathf.Clamp(VerticalRayCount, 2, int.MaxValue);

        HorizontalRaySpacing = bounds.size.y / (HorizontalRayCount - 1);
        VerticalRaySpacing = bounds.size.x / (VerticalRayCount - 1);
    }

    internal class RaycastOrigins
    {
        internal Vector2 TopLeft { get; private set; }

        internal Vector2 TopRight { get; private set; }

        internal Vector2 BottomLeft { get; private set; }

        internal Vector2 BottomRight { get; private set; }

        internal void Update(Vector2 bottomLeft, Vector2 bottomRight, Vector2 topLeft, Vector2 topRight)
        {
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            TopLeft = topLeft;
            TopRight = topRight;
        }
    }
}
