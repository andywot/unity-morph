// <copyright file="CameraScript.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private MovementController target;
    private Vector2 focusAreaSize;

    private void Start()
    {
        
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
