// <copyright file="RaycastController.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    private const float distanceBetweenRays = 1f;
    protected const float SkinWidth = .03f;

    internal BoxCollider2D Collider { get; set; }

    internal RaycastOrigins RaycastStartingPts { get; } = new RaycastOrigins();

    protected int HorizontalRayCount { get; private set; }
    protected int VerticalRayCount { get; private set; }

    protected float HorizontalRaySpacing { get; private set; }
    protected float VerticalRaySpacing { get; private set; }

    protected LayerMask CollisionMask { get; private set; }

    private void Awake()
    {
        Collider = GetComponent<BoxCollider2D>();
    }

    protected virtual void Start()
    {
        CollisionMask = LayerMask.GetMask("Ground");
        CalculateRaySpacing();
    }

    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = Collider.bounds;
        bounds.Expand(SkinWidth * -2);

        RaycastStartingPts.Update(
            new Vector2(bounds.min.x, bounds.min.y),
            new Vector2(bounds.max.x, bounds.min.y),
            new Vector2(bounds.min.x, bounds.max.y),
            new Vector2(bounds.max.x, bounds.max.y));
    }

    private void CalculateRaySpacing()
    {
        Bounds bounds = Collider.bounds;
        bounds.Expand(SkinWidth * -2);

        HorizontalRayCount = Mathf.RoundToInt(bounds.size.y / distanceBetweenRays) + 1;
        VerticalRayCount = Mathf.RoundToInt(bounds.size.x / distanceBetweenRays) + 1;

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
