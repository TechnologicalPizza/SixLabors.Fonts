using SixLabors.Memory;
using static SixLabors.Fonts.Tables.General.Glyphs.CompositeGlyphLoader;

namespace SixLabors.Fonts.Utilities
{
    /// <summary>
    /// Commonly used pools with "font parts" that are used within the library.
    /// </summary>
    internal static class FontListPools
    {
        /// <summary> A pool for <see cref="Composite"/>s.</summary>
        internal readonly static ListPool<Composite> Composite = new ListPool<Composite>(32);
    }
}
