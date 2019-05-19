// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SixLabors.Fonts
{
    /// <summary>
    /// Provides a collection of fonts.
    /// </summary>
    internal sealed class SystemFontCollection : IReadOnlyFontCollection
    { 
        private static readonly string[] paths = new[]
        {
            // windows directories
            "%SYSTEMROOT%\\Fonts",

            // linux directlty list
            "~/.fonts/",
            "/usr/local/share/fonts/",
            "/usr/share/fonts/",

            // mac fonts
            "~/Library/Fonts/",
            "/Library/Fonts/",
            "/Network/Library/Fonts/",
            "/System/Library/Fonts/",
            "/System Folder/Fonts/",
        };

        private FontCollection _collection;

        public SystemFontCollection()
        {
            Load();
        }

        public void Load()
        {
            _collection = new FontCollection();

            IEnumerable<string> expanded = paths.Select(x => Environment.ExpandEnvironmentVariables(x));
            IEnumerable<string> found = expanded.Where(x => Directory.Exists(x));

            // we do this to provide a consistent experience with case sensitive file systems.
            IEnumerable<string> files = found
                                .SelectMany(x => Directory.EnumerateFiles(x, "*.*", SearchOption.AllDirectories))
                                .Where(x => Path.GetExtension(x).Equals(".ttf", StringComparison.OrdinalIgnoreCase));

            foreach (string path in files)
            {
                try
                {
                    this._collection.Install(path);
                }
                catch
                {
                    // we swallow exceptions installing system fonts as we hold no garantees about permissions etc.
                }
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="FontFamily"/> objects associated with this <see cref="FontCollection"/>.
        /// </summary>
        public IEnumerable<FontFamily> Families => this._collection.Families;

        /// <inheritdoc />
        public int FamilyCount => _collection.FamilyCount;

        /// <inheritdoc />
        public int InstanceCount => _collection.InstanceCount;

        /// <inheritdoc />
        public FontFamily Find(string fontFamily) => this._collection.Find(fontFamily);

        /// <inheritdoc />
        public bool TryFind(string fontFamily, out FontFamily family) => this._collection.TryFind(fontFamily, out family);
    }
}
