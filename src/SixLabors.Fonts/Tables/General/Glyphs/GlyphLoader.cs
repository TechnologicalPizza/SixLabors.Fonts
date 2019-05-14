// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace SixLabors.Fonts.Tables.General.Glyphs
{
    internal abstract class GlyphLoader : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public abstract GlyphVector CreateGlyph(GlyphTable table);

        public static GlyphLoader Load(BinaryReader reader)
        {
            short contoursCount = reader.ReadInt16();
            Bounds bounds = Bounds.Load(reader);

            if (contoursCount >= 0)
            {
                return SimpleGlyphLoader.LoadSimpleGlyph(reader, contoursCount, bounds);
            }
            else
            {
                return CompositeGlyphLoader.LoadCompositeGlyph(reader, bounds);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
                IsDisposed = true;
        }

        ~GlyphLoader()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }
    }
}