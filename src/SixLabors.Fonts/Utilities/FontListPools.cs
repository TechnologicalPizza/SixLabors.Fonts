using SixLabors.Memory;
using static SixLabors.Fonts.Tables.General.Glyphs.CompositeGlyphLoader;

namespace SixLabors.Fonts
{
    /// <summary>
    /// Commonly used pools with "font parts" that are used within the library.
    /// </summary>
    public static class FontListPools
    {
        /// <summary> A pool for <see cref="Composite"/>s.</summary>
        internal readonly static ListPool<Composite> Composite = new ListPool<Composite>(32);

        /// <summary> A pool for <see cref="GlyphLayout"/>s.</summary>
        public readonly static ListPool<GlyphLayout> Layout = new ListPool<GlyphLayout>(512);

        /// <summary> A pool for <see langword="char"/>s.</summary>
        public readonly static ListPool<char> Char = new ListPool<char>(512);
    }
}
