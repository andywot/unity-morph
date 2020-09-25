// <copyright file="CameraFollowScript.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static PlayerEvents;

public class CameraFollowScript : MonoBehaviour
{
    private MovementController target;
    private PlayerController targetPlayer;

    [SerializeField] private PlayerFormSelect player;
    [SerializeField] private Vector2 focusAreaSize = new Vector2(30, 30);
    [SerializeField] private float verticalOffset = 10f;

    [SerializeField] private float lookAheadDistanceX = 5f;
    [SerializeField] private float lookAheadSmoothTimeX = 0.5f;
    [SerializeField] private float smoothTimeY = .2f;

    private FocusArea focusArea;

    private bool isLookingAhead;
    private float currentLookAheadX;
    private float targetLookAheadX;
    private float lookAheadDirectionX;
    private float smoothLookVelocityX;
    private float smoothVelocityY;

    private bool m_Start = false;

    private void Start()
    {
        m_Start = true;
        GetTrackTarget();
        playerMorphEvent.AddListener(GetTrackTarget);
    }

    private void LateUpdate()
    {
        focusArea.Update(target.boxCollider.bounds);
        Vector2 focusPosition = focusArea.Center + (Vector2.up * verticalOffset);

        if (focusArea.Displacement.x != 0)
        {
            lookAheadDirectionX = Mathf.Sign(focusArea.Displacement.x);
            if (Mathf.Sign(targetPlayer.Input.DirectionalInput.x) == Mathf.Sign(focusArea.Displacement.x) && targetPlayer.Input.DirectionalInput.x != 0)
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

    private void GetTrackTarget()
    {
        target = player.ActiveForm.GetComponent<MovementController>();
        targetPlayer = player.ActiveForm.GetComponent<PlayerController>();
        focusArea = new FocusArea(target.boxCollider.bounds, focusAreaSize);
    }

    private void OnDrawGizmos()
    {
        if (focusArea != null && m_Start)
        {
            Gizmos.color = new Color(1, 0, 0, .3f);
            Gizmos.DrawCube(focusArea.Center, focusAreaSize);
        }
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
