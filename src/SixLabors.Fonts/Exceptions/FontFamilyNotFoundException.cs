// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.Fonts.Exceptions
{
    /// <summary>
    /// The exception that is thrown when trying to get a font family that could not be found.
    /// </summary>
    public class FontFamilyNotFoundException : FontException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FontFamilyNotFoundException"/> class.
        /// </summary>
        /// <param name="family">The name of the missing font family.</param>
        public FontFamilyNotFoundException(string family)
            : base($"{family} could not be found")
        {
            this.FontFamily = family;
        }

        /// <summary>
        /// Gets the name of the font familiy that could not be found.
        /// </summary>
        public string FontFamily { get; }
    }
}