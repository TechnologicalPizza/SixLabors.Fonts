// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;

namespace SixLabors.Fonts.Tables.General.Glyphs
{
    internal readonly struct GlyphVector
    {
        public Vector2[] ControlPoints { get; }
        public bool[] OnCurves { get; }
        public ushort[] EndPoints { get; }
        public Bounds Bounds { get; }

        public int PointCount => this.ControlPoints.Length;

        internal GlyphVector(Vector2[] controlPoints, bool[] onCurves, ushort[] endPoints, Bounds bounds)
        {
            this.ControlPoints = controlPoints;
            this.OnCurves = onCurves;
            this.EndPoints = endPoints;
            this.Bounds = bounds;
        }
    }
}