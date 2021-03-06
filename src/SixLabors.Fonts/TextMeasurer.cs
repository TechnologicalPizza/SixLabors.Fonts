﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SixLabors.Fonts.Utilities;
using SixLabors.Primitives;

namespace SixLabors.Fonts
{
    /// <summary>
    /// Encapulated logic for laying out and measuring text.
    /// </summary>
    public static class TextMeasurer
    {
        private static readonly GlyphMetric[] EmptyGlyphMetricArray = new GlyphMetric[0];

        /// <summary>
        /// Measures the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="options">The style.</param>
        /// <returns>The size of the text if it was to be rendered.</returns>
        public static SizeF Measure(string text, RendererOptions options)
            => TextMeasurerInt.Default.Measure(text.AsSpan(), options);

        /// <summary>
        /// Measures the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="options">The style.</param>
        /// <returns>The size of the text if it was to be rendered.</returns>
        public static SizeF Measure(ReadOnlySpan<char> text, RendererOptions options)
            => TextMeasurerInt.Default.Measure(text, options);

        /// <summary>
        /// Measures the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="options">The style.</param>
        /// <returns>The size of the text if it was to be rendered.</returns>
        public static RectangleF MeasureBounds(string text, RendererOptions options)
            => TextMeasurerInt.Default.MeasureBounds(text.AsSpan(), options);

        /// <summary>
        /// Measures the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="options">The style.</param>
        /// <returns>The size of the text if it was to be rendered.</returns>
        public static RectangleF MeasureBounds(ReadOnlySpan<char> text, RendererOptions options)
            => TextMeasurerInt.Default.MeasureBounds(text, options);

        /// <summary>
        /// Measures the character bounds of the text. For each control character the list contains a <c>null</c> element.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="options">The style.</param>
        /// <param name="characterBounds">The list of character bounds of the text if it was to be rendered.</param>
        /// <returns>Whether any of the characters had non-empty bounds.</returns>
        public static bool TryMeasureCharacterBounds(ReadOnlySpan<char> text, RendererOptions options, out GlyphMetric[] characterBounds)
            => TextMeasurerInt.Default.TryMeasureCharacterBounds(text, options, out characterBounds);

        internal static SizeF GetSize(IReadOnlyList<GlyphLayout> glyphLayouts, Vector2 dpi)
        {
            if (glyphLayouts.Count == 0)
                return Size.Empty;

            float left = glyphLayouts.FastMin(x => x.Location.X);
            float right = glyphLayouts.FastMax(x => x.Location.X + x.Width);

            // location is bottom left of the line
            float top = glyphLayouts.FastMin(x => x.Location.Y - x.LineHeight);
            float bottom = glyphLayouts.FastMax(x => x.Location.Y - x.LineHeight + x.Height);

            var topLeft = new Vector2(left, top) * dpi;
            var bottomRight = new Vector2(right, bottom) * dpi;

            Vector2 size = bottomRight - topLeft;
            return new RectangleF(topLeft.X, topLeft.Y, size.X, size.Y).Size;
        }

        internal static RectangleF GetBounds(IReadOnlyList<GlyphLayout> glyphLayouts, Vector2 dpi)
        {
            if (glyphLayouts.Count == 0)
            {
                return RectangleF.Empty;
            }

            bool hasSize = false;

            float left = int.MaxValue;
            float top = int.MaxValue;
            float bottom = int.MinValue;
            float right = int.MinValue;

            for (int i = 0; i < glyphLayouts.Count; i++)
            {
                GlyphLayout c = glyphLayouts[i];
                if (!c.IsControlCharacter)
                {
                    hasSize = true;
                    RectangleF box = c.BoundingBox(dpi);
                    if (left > box.Left)
                    {
                        left = box.Left;
                    }

                    if (top > box.Top)
                    {
                        top = box.Top;
                    }

                    if (bottom < box.Bottom)
                    {
                        bottom = box.Bottom;
                    }

                    if (right < box.Right)
                    {
                        right = box.Right;
                    }
                }
            }

            if (!hasSize)
            {
                return RectangleF.Empty;
            }

            float width = right - left;
            float height = bottom - top;

            return new RectangleF(left, top, width, height);
        }

        internal static bool TryGetCharacterBounds(IReadOnlyList<GlyphLayout> glyphLayouts, Vector2 dpi, out GlyphMetric[] characterBounds)
        {
            bool hasSize = false;
            if (glyphLayouts.Count == 0)
            {
                characterBounds = EmptyGlyphMetricArray;
                return hasSize;
            }

            var characterBoundsList = new GlyphMetric[glyphLayouts.Count];

            for (int i = 0; i < glyphLayouts.Count; i++)
            {
                GlyphLayout c = glyphLayouts[i];

                // TODO: This sets the hasSize value to the last layout... is this correct?
                if (!c.IsControlCharacter)
                {
                    hasSize = true;
                }

                characterBoundsList[i] = new GlyphMetric(c.CodePoint, c.BoundingBox(dpi), c.IsControlCharacter);
            }

            characterBounds = characterBoundsList;
            return hasSize;
        }

        internal class TextMeasurerInt
        {
            private readonly TextLayout layoutEngine;

            public TextMeasurerInt(TextLayout layoutEngine)
            {
                this.layoutEngine = layoutEngine;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TextMeasurerInt"/> class.
            /// </summary>
            public TextMeasurerInt() : this(TextLayout.Default)
            {
            }

            public static TextMeasurerInt Default { get; set; } = new TextMeasurerInt();

            /// <summary>
            /// Measures the text.
            /// </summary>
            /// <param name="text">The text.</param>
            /// <param name="options">The style.</param>
            /// <returns>The size of the text if it was to be rendered.</returns>
            internal RectangleF MeasureBounds(ReadOnlySpan<char> text, RendererOptions options)
            {
                // TODO: add pooling
                var glyphsToRender = FontListPools.Layout.Rent();
                try
                {
                    this.layoutEngine.GenerateLayout(text, options, glyphsToRender);
                    return GetBounds(glyphsToRender, options.Dpi);
                }
                finally
                {
                    FontListPools.Layout.Return(glyphsToRender);
                }
            }

            /// <summary>
            /// Measures the text.
            /// </summary>
            /// <param name="text">The text.</param>
            /// <param name="options">The style.</param>
            /// <param name="characterBounds">The character bounds list.</param>
            /// <returns>The size of the text if it was to be rendered.</returns>
            internal bool TryMeasureCharacterBounds(ReadOnlySpan<char> text, RendererOptions options, out GlyphMetric[] characterBounds)
            {
                var glyphsToRender = FontListPools.Layout.Rent();
                try
                {
                    this.layoutEngine.GenerateLayout(text, options, glyphsToRender);
                    return TryGetCharacterBounds(glyphsToRender, options.Dpi, out characterBounds);
                }
                finally
                {
                    FontListPools.Layout.Return(glyphsToRender);
                }
            }

            /// <summary>
            /// Measures the text.
            /// </summary>
            /// <param name="text">The text.</param>
            /// <param name="options">The style.</param>
            /// <returns>The size of the text if it was to be rendered.</returns>
            internal SizeF Measure(ReadOnlySpan<char> text, RendererOptions options)
            {
                // TODO: add pooling
                var glyphsToRender = FontListPools.Layout.Rent();
                try
                {
                    this.layoutEngine.GenerateLayout(text, options, glyphsToRender);
                    return GetSize(glyphsToRender, options.Dpi);
                }
                finally
                {
                    FontListPools.Layout.Return(glyphsToRender);
                }
            }
        }
    }
}