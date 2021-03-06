﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using SixLabors.Fonts.Exceptions;

namespace SixLabors.Fonts
{
    /// <summary>
    /// A readonly collection of fonts.
    /// </summary>
    public interface IReadOnlyFontCollection
    {
        /// <summary>
        /// Gets the collection of <see cref="FontFamily"/> objects associated with this <see cref="FontCollection"/>.
        /// </summary>
        /// <value>
        /// The families.
        /// </value>
        IEnumerable<FontFamily> Families { get; }

        /// <summary>
        /// Gets the number of font families.
        /// </summary>
        int FamilyCount { get; }

        /// <summary>
        /// Gets the number of font instances.
        /// </summary>
        int InstanceCount { get; }

        /// <summary>
        /// Finds the specified font family.
        /// </summary>
        /// <param name="fontFamily">The font family.</param>
        /// <returns>The family if installed otherwise throws <see cref="FontFamilyNotFoundException"/></returns>
        FontFamily Find(string fontFamily);

        /// <summary>
        /// Finds the specified font family.
        /// </summary>
        /// <param name="fontFamily">The font family to find.</param>
        /// <param name="family">The found family.</param>
        /// <returns>true if a font of that family has been installed into the font collection.</returns>
        bool TryFind(string fontFamily, out FontFamily family);
    }
}
