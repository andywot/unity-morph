// <copyright file="GlobalSuppressions.cs" company="FruitDragons">
// Copyright (c) FruitDragons. All rights reserved.
// </copyright>

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1202:Elements should be ordered by access",
    Justification = "Start and Update Method should preceed all other methods.",
    Scope = "member",
    Target = "~M:MovementController.Move(UnityEngine.Vector3)")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1202:Elements should be ordered by access",
    Justification = "Start and Update Method should preceed all other methods.",
    Scope = "member",
    Target = "~M:RaycastController.UpdateRaycastOrigins")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.LayoutRules",
    "SA1516:Elements should be separated by blank line",
    Justification = "Readability",
    Scope = "member",
    Target = "~P:RaycastController.VerticalRayCount")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.LayoutRules",
    "SA1516:Elements should be separated by blank line",
    Justification = "Readability",
    Scope = "member",
    Target = "~P:RaycastController.VerticalRaySpacing")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1002:Semicolons should be spaced correctly",
    Justification = "Weird goto label",
    Scope = "member",
    Target = "~M:PlayerController.Update")]