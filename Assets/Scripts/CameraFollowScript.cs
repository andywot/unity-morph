// <copyright file="CameraScript.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    [SerializeField] private MovementController target;
    [SerializeField] private Vector2 focusAreaSize = new Vector2(30, 30);
    [SerializeField] private float verticalOffset = 10f;

    [SerializeField] private float lookAheadDistanceX = 5f;
    [SerializeField] private float lookAheadSmoothTimeX = .5f;
    [SerializeField] private float smoothTimeY = .1f;

    private FocusArea focusArea;

    private bool isLookingAhead;
    private float currentLookAheadX;
    private float targetLookAheadX;
    private float lookAheadDirectionX;
    private float smoothLookVelocityX;
    private float smoothVelocityY;

    private void Start()
    {
        focusArea = new FocusArea(target.Collider.bounds, focusAreaSize);
    }

    private void LateUpdate()
    {
        focusArea.Update(target.Collider.bounds);
        Vector2 focusPosition = focusArea.Center + (Vector2.up * verticalOffset);

        if (focusArea.Displacement.x != 0)
        {
            lookAheadDirectionX = Mathf.Sign(focusArea.Displacement.x);
            if (Mathf.Sign(target.PlayerInput.x) == Mathf.Sign(focusArea.Displacement.x) && target.PlayerInput.x != 0)
            {
                isLookingAhead = true;
                targetLookAheadX = lookAheadDistanceX * lookAheadDirectionX;
            }
            else
            {
                if (isLookingAhead)
                {
                    isLookingAhead = false;
                    targetLookAheadX = currentLookAheadX + ((lookAheadDistanceX * lookAheadDirectionX) - currentLookAheadX) / 4f;
                }
            }
        }

        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookAheadSmoothTimeX);
        focusPosition += Vector2.right * currentLookAheadX;
        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, smoothTimeY);

        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawCube(focusArea.Center, focusAreaSize);
    }

    internal class FocusArea
    {
        internal FocusArea(Bounds targetBounds, Vector2 size)
        {
            Left = targetBounds.center.x - (size.x / 2);
            Right = targetBounds.center.x + (size.x / 2);
            Bottom = targetBounds.min.y;
            Top = targetBounds.min.y + size.y;
            Center = new Vector2((Left + Right) / 2, (Top + Bottom) / 2);

            Displacement = Vector2.zero;
        }

        internal Vector2 Center { get; private set; }

        internal float Left { get; private set; }

        internal float Right { get; private set; }

        internal float Bottom { get; private set; }

        internal float Top { get; private set; }

        internal Vector2 Displacement { get; private set; }

        internal void Update(Bounds targetBounds)
        {
            float shiftAmountX = 0;
            float shiftAmountY = 0;

            if (targetBounds.min.x < Left)
            {
                shiftAmountX = targetBounds.min.x - Left;
            }
            else if (targetBounds.max.x > Right)
            {
                shiftAmountX = targetBounds.max.x - Right;
            }

            if (targetBounds.min.y < Bottom)
            {
                shiftAmountY = targetBounds.min.y - Bottom;
            }
            else if (targetBounds.max.y > Top)
            {
                shiftAmountY = targetBounds.max.y - Top;
            }

            Left += shiftAmountX;
            Right += shiftAmountX;
            Bottom += shiftAmountY;
            Top += shiftAmountY;
            Center = new Vector2((Left + Right) / 2, (Top + Bottom) / 2);
            Displacement = new Vector2(shiftAmountX, shiftAmountY);
        }
    }
}
