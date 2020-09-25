#define DEBUG_CC2D_RAYS
using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class MovementController : RaycastController
{
    #region Internal types

    struct CharacterRaycastOrigins
    {
        public Vector3 topLeft;
        public Vector3 bottomRight;
        public Vector3 bottomLeft;
    }

    public class CharacterCollisionState2D
    {
        public bool right;
        public bool left;
        public bool above;
        public bool below;
        public bool becameGroundedThisFrame;
        public bool wasGroundedLastFrame;
        public bool movingDownSlope;
        public float slopeAngle;


        public bool hasCollision()
        {
            return below || right || left || above;
        }


        public void reset()
        {
            right = left = above = below = becameGroundedThisFrame = movingDownSlope = false;
            slopeAngle = 0f;
        }


        public override string ToString()
        {
            return string.Format("[CharacterCollisionState2D] r: {0}, l: {1}, a: {2}, b: {3}, movingDownSlope: {4}, angle: {5}, wasGroundedLastFrame: {6}, becameGroundedThisFrame: {7}",
                                 right, left, above, below, movingDownSlope, slopeAngle, wasGroundedLastFrame, becameGroundedThisFrame);
        }
    }

    #endregion


    #region Events, properties and fields

    public event Action<RaycastHit2D> OnControllerCollidedEvent;
    public event Action<Collider2D> OnTriggerEnterEvent;
    public event Action<Collider2D> OnTriggerStayEvent;
    public event Action<Collider2D> OnTriggerExitEvent;

    internal bool ignoreOneWayPlatformsThisFrame;

    [Header("Collision Masks")]
    public LayerMask platformMask = 0;
    public LayerMask triggerMask = 0;
    public LayerMask oneWayPlatformMask = 0;

    [Header("Slope Settings")]
    [Range(0f, 90f)]
    public float slopeLimit = 30f;
    public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));

    [Header("Raycast Settings")]
    [SerializeField]
    [Range(0.001f, 0.3f)]
    float _skinWidth = 0.02f;

    public float skinWidth
    {
        get { return _skinWidth; }
        set
        {
            _skinWidth = value;
            recalculateDistanceBetweenRays();
        }
    }

    [Range(2, 20)]
    public int totalHorizontalRays = 8;
    [Range(2, 20)]
    public int totalVerticalRays = 4;

    float _slopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);

    internal BoxCollider2D boxCollider;
    internal Rigidbody2D rigidBody2D;
    internal PlayerInput playerInput;


    public CharacterCollisionState2D collisionState = new CharacterCollisionState2D();
    internal Vector3 velocity;
    public bool isGrounded { get { return collisionState.below; } }

    const float kSkinWidthFloatFudgeFactor = 0.001f;

    #endregion
    
    CharacterRaycastOrigins _raycastOrigins;
    RaycastHit2D _raycastHit;
    List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>(2);

    float _verticalDistanceBetweenRays;
    float _horizontalDistanceBetweenRays;

    bool _isGoingUpSlope = false;


    #region Monobehaviour

    protected override void Awake()
    {
        platformMask |= oneWayPlatformMask;

        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody2D = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        skinWidth = _skinWidth;

        // we want to set our CC2D to ignore all collision layers except what is in our triggerMask
        for (var i = 0; i < 32; i++)
        {
            // see if our triggerMask contains this layer and if not ignore it
            if ((triggerMask.value & 1 << i) == 0)
                Physics2D.IgnoreLayerCollision(gameObject.layer, i);
        }
    }


    public void OnTriggerEnter2D(Collider2D col)
    {
        OnTriggerEnterEvent?.Invoke(col);
    }


    public void OnTriggerStay2D(Collider2D col)
    {
        OnTriggerStayEvent?.Invoke(col);
    }


    public void OnTriggerExit2D(Collider2D col)
    {
        OnTriggerExitEvent?.Invoke(col);
    }

    #endregion


    [System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
    void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }


    #region Public

    public void Move(Vector3 deltaMovement)
    {
        // save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame
        collisionState.wasGroundedLastFrame = collisionState.below;

        // clear our state
        collisionState.reset();
        _raycastHitsThisFrame.Clear();
        _isGoingUpSlope = false;

        primeRaycastOrigins();


        // first, we check for a slope below us before moving
        // only check slopes if we are going down and grounded
        if (deltaMovement.y < 0f && collisionState.wasGroundedLastFrame)
            handleVerticalSlope(ref deltaMovement);

        // now we check movement in the horizontal dir
        if (deltaMovement.x != 0f)
            moveHorizontally(ref deltaMovement);

        // next, check movement in the vertical dir
        if (deltaMovement.y != 0f)
        {
            moveVertically(ref deltaMovement);
        }

        // move then update our state
        deltaMovement.z = 0;
        transform.Translate(deltaMovement, Space.World);

        // only calculate velocity if we have a non-zero deltaTime
        if (Time.deltaTime > 0f)
            velocity = deltaMovement / Time.deltaTime;

        // set our becameGrounded state based on the previous and current collision state
        if (!collisionState.wasGroundedLastFrame && collisionState.below)
            collisionState.becameGroundedThisFrame = true;

        // if we are going up a slope we artificially set a y velocity so we need to zero it out here
        if (_isGoingUpSlope)
            velocity.y = 0;

        // send off the collision events if we have a listener
        if (OnControllerCollidedEvent != null)
        {
            for (var i = 0; i < _raycastHitsThisFrame.Count; i++)
                OnControllerCollidedEvent(_raycastHitsThisFrame[i]);
        }

        ignoreOneWayPlatformsThisFrame = false;
    }


    /// <summary>
    /// moves directly down until grounded
    /// </summary>
    public void warpToGrounded()
    {
        do
        {
            Move(new Vector3(0, -1f, 0));
        } while (!isGrounded);
    }


    /// <summary>
    /// this should be called anytime you have to modify the BoxCollider2D at runtime. It will recalculate the distance between the rays used for collision detection.
    /// It is also used in the skinWidth setter in case it is changed at runtime.
    /// </summary>
    public void recalculateDistanceBetweenRays()
    {
        // figure out the distance between our rays in both directions
        // horizontal
        var colliderUseableHeight = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2f * _skinWidth);
        _verticalDistanceBetweenRays = colliderUseableHeight / (totalHorizontalRays - 1);

        // vertical
        var colliderUseableWidth = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2f * _skinWidth);
        _horizontalDistanceBetweenRays = colliderUseableWidth / (totalVerticalRays - 1);
    }

    #endregion


    #region Movement Methods

    /// <summary>
    /// resets the raycastOrigins to the current extents of the box collider inset by the skinWidth. It is inset
    /// to avoid casting a ray from a position directly touching another collider which results in wonky normal data.
    /// </summary>
    /// <param name="futurePosition">Future position.</param>
    /// <param name="deltaMovement">Delta movement.</param>
    void primeRaycastOrigins()
    {
        // our raycasts need to be fired from the bounds inset by the skinWidth
        var modifiedBounds = boxCollider.bounds;
        modifiedBounds.Expand(-2f * _skinWidth);

        _raycastOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
        _raycastOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
        _raycastOrigins.bottomLeft = modifiedBounds.min;
    }


    /// <summary>
    /// we have to use a bit of trickery in this one. The rays must be cast from a small distance inside of our
    /// collider (skinWidth) to avoid zero distance rays which will get the wrong normal. Because of this small offset
    /// we have to increase the ray distance skinWidth then remember to remove skinWidth from deltaMovement before
    /// actually moving the player
    /// </summary>
    void moveHorizontally(ref Vector3 deltaMovement)
    {
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + _skinWidth;
        var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
        var initialRayOrigin = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;

        for (var i = 0; i < totalHorizontalRays; i++)
        {
            var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays);

            DrawRay(ray, rayDirection * rayDistance, Color.red);

            // if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one). this will allow us to
            // walk up sloped oneWayPlatforms
            if (i == 0 && collisionState.wasGroundedLastFrame)
                _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);
            else
                _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask & ~oneWayPlatformMask);

            if (_raycastHit)
            {
                // the bottom ray can hit a slope but no other ray can so we have special handling for these cases
                if (i == 0 && handleHorizontalSlope(ref deltaMovement, Vector2.Angle(_raycastHit.normal, Vector2.up)))
                {
                    _raycastHitsThisFrame.Add(_raycastHit);
                    // if we weren't grounded last frame, that means we're landing on a slope horizontally.
                    // this ensures that we stay flush to that slope
                    if (!collisionState.wasGroundedLastFrame)
                    {
                        float flushDistance = Mathf.Sign(deltaMovement.x) * (_raycastHit.distance - skinWidth);
                        transform.Translate(new Vector2(flushDistance, 0));
                    }
                    break;
                }

                // set our new deltaMovement and recalculate the rayDistance taking it into account
                deltaMovement.x = _raycastHit.point.x - ray.x;
                rayDistance = Mathf.Abs(deltaMovement.x);

                // remember to remove the skinWidth from our deltaMovement
                if (isGoingRight)
                {
                    deltaMovement.x -= _skinWidth;
                    collisionState.right = true;
                }
                else
                {
                    deltaMovement.x += _skinWidth;
                    collisionState.left = true;
                }

                _raycastHitsThisFrame.Add(_raycastHit);

                // we add a small fudge factor for the float operations here. if our rayDistance is smaller
                // than the width + fudge bail out because we have a direct impact
                if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
                    break;
            }
        }
    }


    /// <summary>
    /// handles adjusting deltaMovement if we are going up a slope.
    /// </summary>
    /// <returns><c>true</c>, if horizontal slope was handled, <c>false</c> otherwise.</returns>
    /// <param name="deltaMovement">Delta movement.</param>
    /// <param name="angle">Angle.</param>
    bool handleHorizontalSlope(ref Vector3 deltaMovement, float angle)
    {
        // disregard 90 degree angles (walls)
        if (Mathf.RoundToInt(angle) == 90)
            return false;

        // if we can walk on slopes and our angle is small enough we need to move up
        if (angle < slopeLimit)
        {
            // we only need to adjust the deltaMovement if we are not jumping
            // TODO: this uses a magic number which isn't ideal! The alternative is to have the user pass in if there is a jump this frame
            if (!playerInput.IsJumpKeyDown)
            {
                // apply the slopeModifier to slow our movement up the slope
                var slopeModifier = slopeSpeedMultiplier.Evaluate(angle);
                deltaMovement.x *= slopeModifier;

                // we dont set collisions on the sides for this since a slope is not technically a side collision.
                // smooth y movement when we climb. we make the y movement equivalent to the actual y location that corresponds
                // to our new x location using our good friend Pythagoras
                deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);
                var isGoingRight = deltaMovement.x > 0;

                // safety check. we fire a ray in the direction of movement just in case the diagonal we calculated above ends up
                // going through a wall. if the ray hits, we back off the horizontal movement to stay in bounds.
                var ray = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
                RaycastHit2D raycastHit;
                if (collisionState.wasGroundedLastFrame)
                    raycastHit = Physics2D.Raycast(ray, deltaMovement.normalized, deltaMovement.magnitude, platformMask);
                else
                    raycastHit = Physics2D.Raycast(ray, deltaMovement.normalized, deltaMovement.magnitude, platformMask & ~oneWayPlatformMask);

                if (raycastHit)
                {
                    // we crossed an edge when using Pythagoras calculation, so we set the actual delta movement to the ray hit location
                    deltaMovement = (Vector3)raycastHit.point - ray;
                    if (isGoingRight)
                        deltaMovement.x -= _skinWidth;
                    else
                        deltaMovement.x += _skinWidth;
                }

                _isGoingUpSlope = true;
                collisionState.below = true;
                collisionState.slopeAngle = -angle;
            }
        }
        else // too steep. get out of here
        {
            deltaMovement.x = 0;
        }

        return true;
    }


    void moveVertically(ref Vector3 deltaMovement)
    {
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y) + _skinWidth;
        var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
        var initialRayOrigin = isGoingUp ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;

        // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
        initialRayOrigin.x += deltaMovement.x;

        // if we are moving up, we should ignore the layers in oneWayPlatformMask
        var mask = platformMask;
        if ((isGoingUp && !collisionState.wasGroundedLastFrame) || ignoreOneWayPlatformsThisFrame)
            mask &= ~oneWayPlatformMask;

        for (var i = 0; i < totalVerticalRays; i++)
        {
            var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

            DrawRay(ray, rayDirection * rayDistance, Color.red);
            _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);
            if (_raycastHit)
            {
                // set our new deltaMovement and recalculate the rayDistance taking it into account
                deltaMovement.y = _raycastHit.point.y - ray.y;
                rayDistance = Mathf.Abs(deltaMovement.y);

                // remember to remove the skinWidth from our deltaMovement
                if (isGoingUp)
                {
                    deltaMovement.y -= _skinWidth;
                    collisionState.above = true;
                }
                else
                {
                    deltaMovement.y += _skinWidth;
                    collisionState.below = true;
                }

                _raycastHitsThisFrame.Add(_raycastHit);

                // this is a hack to deal with the top of slopes. if we walk up a slope and reach the apex we can get in a situation
                // where our ray gets a hit that is less then skinWidth causing us to be ungrounded the next frame due to residual velocity.
                if (!isGoingUp && deltaMovement.y > 0.00001f)
                    _isGoingUpSlope = true;

                // we add a small fudge factor for the float operations here. if our rayDistance is smaller
                // than the width + fudge bail out because we have a direct impact
                if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
                    break;
            }
        }
    }


    /// <summary>
    /// checks the center point under the BoxCollider2D for a slope. If it finds one then the deltaMovement is adjusted so that
    /// the player stays grounded and the slopeSpeedModifier is taken into account to speed up movement.
    /// </summary>
    /// <param name="deltaMovement">Delta movement.</param>
    private void handleVerticalSlope(ref Vector3 deltaMovement)
    {
        // slope check from the center of our collider
        var centerOfCollider = (_raycastOrigins.bottomLeft.x + _raycastOrigins.bottomRight.x) * 0.5f;
        var rayDirection = -Vector2.up;

        // the ray distance is based on our slopeLimit
        var slopeCheckRayDistance = _slopeLimitTangent * (_raycastOrigins.bottomRight.x - centerOfCollider);

        var slopeRay = new Vector2(centerOfCollider, _raycastOrigins.bottomLeft.y);
        DrawRay(slopeRay, rayDirection * slopeCheckRayDistance, Color.yellow);
        _raycastHit = Physics2D.Raycast(slopeRay, rayDirection, slopeCheckRayDistance, platformMask);
        if (_raycastHit)
        {
            // bail out if we have no slope
            var angle = Vector2.Angle(_raycastHit.normal, Vector2.up);
            if (angle == 0)
                return;

            // we are moving down the slope if our normal and movement direction are in the same x direction
            var isMovingDownSlope = Mathf.Sign(_raycastHit.normal.x) == Mathf.Sign(deltaMovement.x);
            if (isMovingDownSlope)
            {
                // going down we want to speed up in most cases so the slopeSpeedMultiplier curve should be > 1 for negative angles
                var slopeModifier = slopeSpeedMultiplier.Evaluate(-angle);
                // we add the extra downward movement here to ensure we "stick" to the surface below
                deltaMovement.y += _raycastHit.point.y - slopeRay.y - skinWidth;
                deltaMovement = new Vector3(0, deltaMovement.y, 0) +
                                (Quaternion.AngleAxis(-angle, Vector3.forward) * new Vector3(deltaMovement.x * slopeModifier, 0, 0));
                collisionState.movingDownSlope = true;
                collisionState.slopeAngle = angle;
            }
        }
    }

    #endregion

}