// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.Primitives;

namespace SixLabors.Fonts
{
    /// <summary>
    /// A glyph from a particular font face.
    /// </summary>
    public partial class GlyphInstance
    {
        internal GlyphInstance(
            FontInstance font, Vector2[] controlPoints, bool[] onCurves, ushort[] endPoints, 
            Bounds bounds, ushort advanceWidth, short leftSideBearing, ushort unitsPerEm, ushort index)
        {
            Font = font;
            UnitsPerEm = unitsPerEm;
            ControlPoints = controlPoints;
            OnCurves = onCurves;
            EndPoints = endPoints;
            Bounds = bounds;
            AdvanceWidth = advanceWidth;
            Index = index;
            Height = UnitsPerEm - Bounds.Min.Y;

            LeftSideBearing = leftSideBearing;
            ScaleFactor = (float)(UnitsPerEm * 72f);
        }

        /// <summary>
        /// Gets the Font.
        /// </summary>
        /// <value>
        /// The Font.
        /// </value>
        internal FontInstance Font { get; }

        /// <summary>
        /// Gets the bounds.
        /// </summary>
        /// <value>
        /// The bounds.
        /// </value>
        internal Bounds Bounds { get; }

        /// <summary>
        /// Gets the width of the advance.
        /// </summary>
        /// <value>
        /// The width of the advance.
        /// </value>
        public ushort AdvanceWidth { get; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public float Height { get; }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        internal ushort Index { get; }

        private static readonly Vector2 Scale = new Vector2(1, -1);

        internal RectangleF BoundingBox(Vector2 origin, Vector2 scaledPointSize)
        {
            Vector2 size = (Bounds.Size() * scaledPointSize) / ScaleFactor;
            Vector2 loc = ((new Vector2(Bounds.Min.X, Bounds.Max.Y) * scaledPointSize) / ScaleFactor) * Scale;

            loc = origin + loc;
            return new RectangleF(loc.X, loc.Y, size.X, size.Y);
        }

        /// <summary>
        /// Renders the glyph to the render surface in font units relative to a bottom left origin at (0,0)
        /// </summary>
        /// <param name="surface">The surface.</param>
        /// <param name="pointSize">Size of the point.</param>
        /// <param name="location">The location.</param>
        /// <param name="dpi">The dpi.</param>
        /// <param name="lineHeight">The lineHeight the current glyph was draw agains to offset topLeft while calling out to IGlyphRenderer.</param>
        /// <exception cref="NotSupportedException">Too many control points</exception>
        public void RenderTo(
            IGlyphRenderer surface, float pointSize, Vector2 location, Vector2 dpi, float lineHeight)
        {
            location *= dpi;

            //Vector2 firstPoint = Vector2.Zero;
            Vector2 scaledPoint = dpi * pointSize;
            RectangleF box = BoundingBox(location, scaledPoint);

            var paramaters = new GlyphRendererParameters(this, pointSize, dpi);

            if (surface.BeginGlyph(box, paramaters))
            {
                int endOfContor = -1;
                for (int i = 0; i < EndPoints.Length; i++)
                {
                    surface.BeginFigure();
                    int startOfContor = endOfContor + 1;
                    endOfContor = EndPoints[i];

                    Vector2 curr = GetPoint(ref scaledPoint, endOfContor) + location;
                    Vector2 next = GetPoint(ref scaledPoint, startOfContor) + location;

                    if (OnCurves[endOfContor])
                    {
                        surface.MoveTo(curr);
                    }
                    else
                    {
                        if (OnCurves[startOfContor])
                        {
                            surface.MoveTo(next);
                        }
                        else
                        {
                            // If both first and last points are off-curve, start at their middle.
                            Vector2 startPoint = (curr + next) / 2;
                            surface.MoveTo(startPoint);
                        }
                    }

                    int length = (endOfContor - startOfContor) + 1;
                    for (int p = 0; p < length; p++)
                    {
                        Vector2 prev = curr;
                        curr = next;
                        int currentIndex = startOfContor + p;
                        int nextIndex = startOfContor + ((p + 1) % length);
                        int prevIndex = startOfContor + (((length + p) - 1) % length);
                        next = GetPoint(ref scaledPoint, nextIndex) + location;

                        if (OnCurves[currentIndex])
                        {
                            // This is a straight line.
                            surface.LineTo(curr);
                        }
                        else
                        {
                            Vector2 prev2 = prev;
                            Vector2 next2 = next;

                            if (!OnCurves[prevIndex])
                            {
                                prev2 = (curr + prev) / 2;
                                surface.LineTo(prev2);
                            }

                            if (!OnCurves[nextIndex])
                                next2 = (curr + next) / 2;
                            
                            surface.LineTo(prev2);
                            surface.QuadraticBezierTo(curr, next2);
                        }
                    }

                    surface.EndFigure();
                }
            }

            surface.EndGlyph();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2 GetPoint(ref Vector2 scaledPoint, int pointIndex)
        {
            // scale each point as we go, w will now have the correct relative point size
            return Scale * ((ControlPoints[pointIndex] * scaledPoint) / ScaleFactor);
        }

        //private static void AlignToGrid(ref Vector2 point)
        //{
        //    var floorPoint = new Vector2(MathF.Floor(point.X), MathF.Floor(point.Y));
        //    Vector2 decimalPart = point - floorPoint;
        //
        //    decimalPart.X = decimalPart.X < 0.5f ? 0 : 1f;
        //    decimalPart.Y = decimalPart.Y < 0.5f ? 0 : 1f;
        //
        //    point = floorPoint + decimalPart;
        //}

        private static ControlPointCollection DrawPoints(IGlyphRenderer surface, ControlPointCollection points, Vector2 point)
        {
            switch (points.Count)
            {
                case 0:
                    break;

                case 1:
                    surface.QuadraticBezierTo(
                        points.SecondControlPoint,
                        point);
                    break;

                case 2:
                    surface.CubicBezierTo(
                        points.SecondControlPoint,
                        points.ThirdControlPoint,
                        point);
                    break;

                default:
                    throw new NotSupportedException("Too many control points.");
            }

            points.Clear();
            return points;
        }

        private struct ControlPointCollection
        {
            public Vector2 SecondControlPoint;
            public Vector2 ThirdControlPoint;
            public int Count;

            public void Add(Vector2 point)
            {
                switch (Count++)
                {
                    case 0:
                        SecondControlPoint = point;
                        break;

                    case 1:
                        ThirdControlPoint = point;
                        break;

                    default:
                        throw new NotSupportedException("Too many control points.");
                }
            }

            public void ReplaceLast(Vector2 point)
            {
                Count--;
                Add(point);
            }

            public void Clear()
            {
                Count = 0;
            }
        }

        public ushort UnitsPerEm { get; }

        /// <summary>
        /// The points defining the shape of this glyph
        /// </summary>
        public Vector2[] ControlPoints { get; }

        /// <summary>
        /// Wether or not the corresponding control point is on a curve
        /// </summary>
        public bool[] OnCurves { get; }

        /// <summary>
        /// The end points
        /// </summary>
        public ushort[] EndPoints { get; }

        /// <summary>
        /// The distance from the bounding box start
        /// </summary>
        public short LeftSideBearing { get; }

        /// <summary>
        /// The scale factor that is applied to the glyph
        /// </summary>
        public float ScaleFactor { get; }
    }
}