// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.Primitives;

namespace SixLabors.Fonts
{
    /// <summary>
    /// Encapsulated logic for laying out and then rendering text to a <see cref="IGlyphRenderer"/> surface.
    /// </summary>
    public static class TextRenderer
    {
        /// <summary>
        /// Renders the text to the <paramref name="renderer"/>.
        /// </summary>
        /// <param name="renderer">The target renderer.</param>
        /// <param name="text">The text.</param>
        /// <param name="options">The style.</param>
        public static void RenderText(
            IGlyphRenderer renderer, ReadOnlySpan<char> text, RendererOptions options)
        {
            RenderText(renderer, text, options, TextLayout.Default);
        }

        internal static void RenderText(
            IGlyphRenderer renderer, ReadOnlySpan<char> text, RendererOptions options, TextLayout layoutEngine)
        {
            List<GlyphLayout> glyphsToRender = layoutEngine.GenerateLayout(text, options);

            Vector2 dpi = new Vector2(options.DpiX, options.DpiY);
            RectangleF rect = TextMeasurer.GetBounds(glyphsToRender, dpi);

            renderer.BeginText(rect);

            foreach (GlyphLayout g in glyphsToRender)
            {
                if (g.IsWhiteSpace)
                    continue;
                
                g.Glyph.RenderTo(renderer, g.Location, options.DpiX, options.DpiY, g.LineHeight);
            }

            renderer.EndText();
        }
    }
}
