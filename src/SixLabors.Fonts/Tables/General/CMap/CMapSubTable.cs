// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.Fonts.WellKnownIds;

namespace SixLabors.Fonts.Tables.General.CMap
{
    internal abstract class CMapSubTable
    {
        public ushort Format { get; }
        public PlatformEncodingID Platform { get; }
        public ushort Encoding { get; }

        public CMapSubTable()
        {
        }

        public CMapSubTable(PlatformEncodingID platform, ushort encoding, ushort format)
        {
            this.Platform = platform;
            this.Encoding = encoding;
            this.Format = format;
        }

        public abstract ushort GetGlyphId(int codePoint);
    }
}