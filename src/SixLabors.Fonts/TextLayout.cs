// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SixLabors.Fonts.Utilities;
using SixLabors.Primitives;

namespace SixLabors.Fonts
{
    /// <summary>
    /// Encapsulated logic for laying out text.
    /// </summary>
    public class TextLayout
    {
        public static TextLayout Default { get; set; } = new TextLayout();

        /// <summary>
        /// Generates the layout.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="options">The style.</param>
        /// <param name="output">The layout output.</param>
        /// <returns>
        /// A collection of layout that describe all thats 
        /// needed to measure or render a series of glyphs.
        /// </returns>
        public void GenerateLayout(
            ReadOnlySpan<char> text, RendererOptions options, IList<GlyphLayout> output)
        {
            var tmp = FontListPools.Char.Rent();
            try
            {
                for (int i = 0; i < text.Length; i++)
                    tmp.Add(text[i]);
                GenerateLayout(tmp, options, output);
            }
            finally
            {
                FontListPools.Char.Return(tmp);
            }
        }

        /// <summary>
        /// Generates the layout.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="options">The style.</param>
        /// <param name="output">The layout output.</param>
        /// <returns>
        /// A collection of layout that describe all thats 
        /// needed to measure or render a series of glyphs.
        /// </returns>
        public void GenerateLayout(
            StringBuilder text, RendererOptions options, IList<GlyphLayout> output)
        {
            var tmp = FontListPools.Char.Rent();
            try
            {
                for (int i = 0; i < text.Length; i++)
                    tmp.Add(text[i]);
                GenerateLayout(tmp, options, output);
            }
            finally
            {
                FontListPools.Char.Return(tmp);
            }
        }

        /// <summary>
        /// Generates the layout.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="options">The style.</param>
        /// <param name="output">The layout output.</param>
        /// <returns>
        /// A collection of layout that describe all thats 
        /// needed to measure or render a series of glyphs.
        /// </returns>
        public void GenerateLayout(
            char[] text, RendererOptions options, IList<GlyphLayout> output)
        {
            GenerateLayout((IReadOnlyList<char>)text, options, output);
        }

        /// <summary>
        /// Generates the layout.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="options">The style.</param>
        /// <param name="output">The layout output.</param>
        /// <returns>
        /// A collection of layout that describe all thats 
        /// needed to measure or render a series of glyphs.
        /// </returns>
        public void GenerateLayout(
            IReadOnlyList<char> text, RendererOptions options, IList<GlyphLayout> output)
        {
            int count = text.Count;
            Vector2 dpi = options.Dpi;
            Vector2 origin = options.Origin / dpi;

            float maxWidth = float.MaxValue;
            float originX = 0;
            if (options.WrappingWidth > 0)
            {
                maxWidth = options.WrappingWidth / dpi.X;

                switch (options.HorizontalAlignment)
                {
                    case HorizontalAlignment.Right:
                        originX = maxWidth;
                        break;

                    case HorizontalAlignment.Center:
                        originX = 0.5f * maxWidth;
                        break;

                    case HorizontalAlignment.Left:
                    default:
                        originX = 0;
                        break;
                }
            }

            AppliedFontStyle spanStyle = options.GetStyle(0, count);            
            float unscaledLineHeight = 0f;
            float lineHeight = 0f;
            float unscaledLineMaxAscender = 0f;
            float lineMaxAscender = 0f;
            var location = Vector2.Zero;
            float lineHeightOfFirstLine = 0;

            // Remember where the top of the layouted text is for accurate vertical alignment.
            // This is important because there is considerable space between the lineHeight at the glyph's ascender.
            float top = 0;

            bool firstLine = true;
            GlyphInstance previousGlyph = null;
            float scale = 0;
            int lastWrappableLocation = -1;
            bool startOfLine = true;
            float totalHeight = 0;

            for (int i = 0; i < count; i++)
            {
                char c = text[i];

                // four-byte characters are processed on the first char
                if (char.IsLowSurrogate(c))
                    continue;

                if (spanStyle.End < i)
                {
                    spanStyle = options.GetStyle(i, count);
                    previousGlyph = null;
                }

                if (spanStyle.Font.LineHeight > unscaledLineHeight)
                {
                    // get the larget lineheight thus far
                    unscaledLineHeight = spanStyle.Font.LineHeight;
                    scale = spanStyle.Font.UnitsPerEm * 72;
                    lineHeight = (unscaledLineHeight * spanStyle.PointSize) / scale;
                }

                if (spanStyle.Font.Ascender > unscaledLineMaxAscender)
                {
                    unscaledLineMaxAscender = spanStyle.Font.Ascender;
                    scale = spanStyle.Font.UnitsPerEm * 72;
                    lineMaxAscender = (unscaledLineMaxAscender * spanStyle.PointSize) / scale;
                }

                if (firstLine)
                {
                    if (lineHeight > lineHeightOfFirstLine)
                        lineHeightOfFirstLine = lineHeight;

                    top = lineHeightOfFirstLine - lineMaxAscender;
                }

                if (options.WrappingWidth > 0 && char.IsWhiteSpace(c))
                {
                    // keep a record of where to wrap text and ensure that no line starts with white space
                    for (int j = output.Count - 1; j >= 0; j--)
                    {
                        if (!output[j].IsWhiteSpace)
                        {
                            lastWrappableLocation = j + 1;
                            break;
                        }
                    }
                }

                bool hasFourBytes = char.IsHighSurrogate(c);
                int codePoint = hasFourBytes ?
                    (i + 1 < count ? char.ConvertToUtf32(c, text[i + 1]) : 0) : c;

                GlyphInstance glyph = spanStyle.Font.GetGlyph(codePoint);
                float glyphWidth = (glyph.AdvanceWidth * spanStyle.PointSize) / scale;
                float glyphHeight = (glyph.Height * spanStyle.PointSize) / scale;
                var g = new Glyph(glyph, spanStyle.PointSize);

                if (hasFourBytes || (c != '\r' && c != '\n' && c != '\t' && c != ' '))
                {
                    Vector2 glyphLocation = location;
                    if (spanStyle.ApplyKerning && previousGlyph != null)
                    {
                        // if there is special instructions for this glyph pair use that width
                        Vector2 scaledOffset = (spanStyle.Font.GetOffset(glyph, previousGlyph) * spanStyle.PointSize) / scale;

                        glyphLocation += scaledOffset;

                        // only fix the 'X' of the current tracked location but use the actual 'X'/'Y' of the offset
                        location.X = glyphLocation.X;
                    }

                    output.Add(new GlyphLayout(codePoint, g, glyphLocation, glyphWidth, glyphHeight, lineHeight, startOfLine, false, false));
                    startOfLine = false;

                    // move forward the actual width of the glyph, we are retaining the baseline
                    location.X += glyphWidth;

                    // if the word extended pass the end of the box, wrap it
                    if (location.X >= maxWidth && lastWrappableLocation > 0)
                    {
                        if (lastWrappableLocation < output.Count)
                        {
                            float wrappingOffset = output[lastWrappableLocation].Location.X;
                            startOfLine = true;

                            // move the characters to the next line
                            for (int j = lastWrappableLocation; j < output.Count; j++)
                            {
                                GlyphLayout currentLayout = output[j];
                                if (currentLayout.IsWhiteSpace)
                                {
                                    wrappingOffset += currentLayout.Width;
                                    output.RemoveAt(j);
                                    j--;
                                    continue;
                                }

                                Vector2 currentLoc = currentLayout.Location;
                                var newLayout = new GlyphLayout(
                                    currentLayout.CodePoint, currentLayout.Glyph, 
                                    new Vector2(currentLoc.X - wrappingOffset, currentLoc.Y + lineHeight),
                                    currentLayout.Width, currentLayout.Height,
                                    currentLayout.LineHeight, startOfLine,
                                    currentLayout.IsWhiteSpace, currentLayout.IsControlCharacter);

                                startOfLine = false;

                                location.X = newLayout.Location.X + newLayout.Width;
                                output[j] = newLayout;
                            }

                            location.Y += lineHeight;
                            firstLine = false;
                            lastWrappableLocation = -1;
                        }
                    }

                    float bottom = location.Y + lineHeight;
                    if (bottom > totalHeight)
                        totalHeight = bottom;

                    previousGlyph = glyph;
                }
                else if (c == '\r')
                {
                    // carriage return resets the XX coordinate to 0
                    location.X = 0;
                    previousGlyph = null;
                    startOfLine = true;

                    output.Add(new GlyphLayout(codePoint, g, location, 0, glyphHeight, lineHeight, startOfLine, true, true));
                    startOfLine = false;
                }
                else if (c == '\n')
                {
                    // carriage return resets the XX coordinate to 0
                    output.Add(new GlyphLayout(codePoint, g, location, 0, glyphHeight, lineHeight, startOfLine, true, true));
                    location.X = 0;
                    location.Y += lineHeight;
                    unscaledLineHeight = 0;
                    unscaledLineMaxAscender = 0;
                    previousGlyph = null;
                    firstLine = false;
                    lastWrappableLocation = -1;
                    startOfLine = true;
                }
                else if (text[i] == '\t')
                {
                    float tabStop = glyphWidth * spanStyle.TabWidth;
                    float finalWidth = 0;

                    if (tabStop > 0)
                        finalWidth = tabStop - (location.X % tabStop);
                    
                    if (finalWidth < glyphWidth)
                        // if we are not going to tab atleast a glyph width add another tabstop to it ???
                        // should I be doing this?
                        finalWidth += tabStop;

                    output.Add(new GlyphLayout(codePoint, g, location, finalWidth, glyphHeight, lineHeight, startOfLine, true, false));
                    startOfLine = false;

                    // advance to a position > width away that
                    location.X += finalWidth;
                    previousGlyph = null;
                }
                else if (text[i] == ' ')
                {
                    output.Add(new GlyphLayout(codePoint, g, location, glyphWidth, glyphHeight, lineHeight, startOfLine, true, false));
                    startOfLine = false;
                    location.X += glyphWidth;
                    previousGlyph = null;
                }
            }

            totalHeight -= top;
            var offset = new Vector2(0, lineHeightOfFirstLine - top);

            switch (options.VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    offset += new Vector2(0, -0.5f * totalHeight);
                    break;

                case VerticalAlignment.Bottom:
                    offset += new Vector2(0, -totalHeight);
                    break;

                case VerticalAlignment.Top:
                default:
                    // no change
                    break;
            }

            Vector2 lineOffset = offset;
            for (int i = 0; i < output.Count; i++)
            {
                GlyphLayout glyphLayout = output[i];
                if (glyphLayout.StartOfLine)
                {
                    // redundant
                    //lineOffset = offset;

                    // scan ahead measuring width
                    float width = glyphLayout.Width;
                    for (int j = i + 1; j < output.Count; j++)
                    {
                        if (output[j].StartOfLine)
                            break;

                        width = output[j].Location.X + output[j].Width; // rhs
                    }

                    switch (options.HorizontalAlignment)
                    {
                        case HorizontalAlignment.Right:
                            lineOffset = new Vector2(originX - width, 0) + offset;
                            break;

                        case HorizontalAlignment.Center:
                            lineOffset = new Vector2(originX - (width / 2f), 0) + offset;
                            break;

                        case HorizontalAlignment.Left:
                        default:
                            lineOffset = new Vector2(originX, 0) + offset;
                            break;
                    }
                }

                // TODO calculate an offset from the 'origin' based on TextAlignment for each line
                output[i] = new GlyphLayout(glyphLayout.CodePoint, glyphLayout.Glyph, glyphLayout.Location + lineOffset + origin, glyphLayout.Width, glyphLayout.Height, glyphLayout.LineHeight, glyphLayout.StartOfLine, glyphLayout.IsWhiteSpace, glyphLayout.IsControlCharacter);
            }
        }
    }
}
