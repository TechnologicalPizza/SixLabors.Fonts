// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Text;

using SixLabors.Fonts.Utilities;
using SixLabors.Fonts.WellKnownIds;

namespace SixLabors.Fonts.Tables.General.Name
{
    internal class NameRecord
    {
        private readonly string value;

        public NameRecord(PlatformEncodingID platform, ushort languageId, NameID nameId, string value)
        {
            this.Platform = platform;
            this.LanguageID = languageId;
            this.NameID = nameId;
            this.value = value;
        }

        public PlatformEncodingID Platform { get; }

        public ushort LanguageID { get; }

        public NameID NameID { get; }

        internal StringLoader StringReader { get; private set; }

        public string Value => this.StringReader?.Value ?? this.value;

        public static NameRecord Read(BinaryReader reader)
        {
            PlatformEncodingID platform = reader.ReadUInt16<PlatformEncodingID>();
            EncodingID encodingId = reader.ReadUInt16<EncodingID>();
            Encoding encoding = encodingId.AsEncoding();
            ushort languageID = reader.ReadUInt16();
            NameID nameID = reader.ReadUInt16<NameID>();

            StringLoader stringReader = StringLoader.Create(reader, encoding);

            return new NameRecord(platform, languageID, nameID, null)
            {
                StringReader = stringReader
            };
        }
    }
}
