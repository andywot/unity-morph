using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    protected struct RaycastOrigins
    {
        internal Vector2 topLeft;
        internal Vector2 topRight;
        internal Vector2 bottomLeft;
        internal Vector2 bottomRight;
    }


    protected const float SkinWidth = 0.03f;
    private const float DistanceBetweenRays = 1f;

    protected BoxCollider2D Collider2D;
    protected RaycastOrigins _RaycastOrigins;

    protected int HorizontalRayCount { get; private set; }
    protected int VerticalRayCount { get; private set; }

    protected float HorizontalRaySpacing { get; private set; }
    protected float VerticalRaySpacing { get; private set; }

    protected virtual void Awake()
    {
        Collider2D = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    protected void UpdateRaycastOrigins()
    {
        Bounds modifiedBounds = Collider2D.bounds;
        modifiedBounds.Expand(SkinWidth * -2);

        _RaycastOrigins.bottomLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.min.y);
        _RaycastOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
        _RaycastOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
        _RaycastOrigins.topRight = new Vector2(modifiedBounds.max.x, modifiedBounds.max.y);
    }

    protected void CalculateRaySpacing()
    {
        Bounds modifiedBounds = Collider2D.bounds;
        modifiedBounds.Expand(SkinWidth * -2);

        HorizontalRayCount = Mathf.RoundToInt(modifiedBounds.size.y / DistanceBetweenRays) + 1;
        VerticalRayCount = Mathf.RoundToInt(modifiedBounds.size.x / DistanceBetweenRays) + 1;

        HorizontalRaySpacing = modifiedBounds.size.y / (HorizontalRayCount - 1);
        VerticalRaySpacing = modifiedBounds.size.x / (VerticalRayCount - 1);
    }

}
