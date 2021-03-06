﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using SixLabors.Fonts.WellKnownIds;

namespace SixLabors.Fonts.Tables.General.CMap
{
    internal sealed class Format4SubTable : CMapSubTable
    {
        public ushort Language { get; }
        public ushort[] GlyphIDs { get; }
        public Segment[] Segments { get; }

        public Format4SubTable(ushort language, PlatformEncodingID platform, ushort encoding, Segment[] segments, ushort[] glyphIds)
            : base(platform, encoding, 4)
        {
            this.Language = language;
            this.GlyphIDs = glyphIds;
            this.Segments = segments;
        }

        public override ushort GetGlyphId(int codePoint)
        {
            uint charAsInt = (uint)codePoint;
            if (charAsInt > ushort.MaxValue)
                return 0;

            for (int i = 0; i < this.Segments.Length; i++)
            {
                Segment seg = this.Segments[i];
                if (seg.End >= charAsInt && seg.Start <= charAsInt)
                {
                    if (seg.Offset == 0)
                    {
                        return (ushort)((charAsInt + seg.Delta) % ushort.MaxValue); // TODO: bitmask instead?
                    }
                    else
                    {
                        long offset = (seg.Offset / 2) + (charAsInt - seg.Start);
                        return this.GlyphIDs[offset - this.Segments.Length + seg.Index];
                    }
                }
            }

            return 0;
        }

        public static IEnumerable<Format4SubTable> Load(IEnumerable<EncodingRecord> encodings, BinaryReader reader)
        {
            // 'cmap' Subtable Format 4:
            // Type   | Name                       | Description
            // -------|----------------------------|------------------------------------------------------------------------
            // uint16 | format                     | Format number is set to 4.
            // uint16 | length                     | This is the length in bytes of the subtable.
            // uint16 | language                   | Please see “Note on the language field in 'cmap' subtables“ in this document.
            // uint16 | segCountX2                 | 2 x segCount.
            // uint16 | searchRange                | 2 x (2**floor(log2(segCount)))
            // uint16 | entrySelector              | log2(searchRange/2)
            // uint16 | rangeShift                 | 2 x segCount - searchRange
            // uint16 | endCount[segCount]         | End characterCode for each segment, last=0xFFFF.
            // uint16 | reservedPad                | Set to 0.
            // uint16 | startCount[segCount]       | Start character code for each segment.
            // int16  | idDelta[segCount]          | Delta for all character codes in segment.
            // uint16 | idRangeOffset[segCount]    | Offsets into glyphIdArray or 0
            // uint16 | glyphIdArray[ ]            | Glyph index array (arbitrary length)
            
            // format has already been read by this point skip it
            ushort length = reader.ReadUInt16();
            ushort language = reader.ReadUInt16();
            ushort segCountX2 = reader.ReadUInt16();
            ushort searchRange = reader.ReadUInt16();
            ushort entrySelector = reader.ReadUInt16();
            ushort rangeShift = reader.ReadUInt16();
            int segCount = segCountX2 / 2;
            ushort[] endCounts = reader.ReadUInt16Array(segCount);
            ushort reserved = reader.ReadUInt16();

            ushort[] startCounts = reader.ReadUInt16Array(segCount);
            short[] idDelta = reader.ReadInt16Array(segCount);
            ushort[] idRangeOffset = reader.ReadUInt16Array(segCount);

            // table length thus far
            int headerLength = 16 + (segCount * 8);
            int glyphIDCount = (length - headerLength) / 2;
            ushort[] glyphIDs = reader.ReadUInt16Array(glyphIDCount);

            Segment[] segments = Segment.Create(endCounts, startCounts, idDelta, idRangeOffset);
            foreach (EncodingRecord encoding in encodings)
                yield return new Format4SubTable(language, encoding.PlatformID, encoding.EncodingID, segments, glyphIDs);
        }

        internal readonly struct Segment
        {
            public ushort Index { get; }
            public short Delta { get; }
            public ushort End { get; }
            public ushort Offset { get; }
            public ushort Start { get; }

            public Segment(ushort index, ushort end, ushort start, short delta, ushort offset)
            {
                this.Index = index;
                this.End = end;
                this.Start = start;
                this.Delta = delta;
                this.Offset = offset;
            }

            public static Segment[] Create(ushort[] endCounts, ushort[] startCode, short[] idDelta, ushort[] idRangeOffset)
            {
                if (endCounts.Length == 0)
                    return Array.Empty<Segment>();

                var segments = new Segment[endCounts.Length];
                for (ushort i = 0; i < segments.Length; i++)
                {
                    ushort start = startCode[i];
                    ushort end = endCounts[i];
                    short delta = idDelta[i];
                    ushort offset = idRangeOffset[i];
                    segments[i] = new Segment(i, end, start, delta, offset);
                }
                return segments;
            }
        }
    }
}
