using System.Numerics;
using System.Text;
using SixLabors.Primitives;

namespace SixLabors.Fonts
{
    /// <summary>
    /// Glyphs layout and location.
    /// </summary>
    public readonly struct GlyphLayout
    {
        /// <summary>
        /// Gets the glyph that this layout describes.
        /// </summary>
        public Glyph Glyph { get; }

        /// <summary>
        /// Gets the location.
        /// </summary>
        public Vector2 Location { get; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        public float Height { get; }

        public float LineHeight { get; }

        /// <summary>
        /// Gets a value indicating whether this glyph is the first glyph on a new line.
        /// </summary>
        public bool StartOfLine { get; }

        /// <summary>
        /// Gets the Unicode code point of the character.
        /// </summary>
        public int CodePoint { get; }

        /// <summary>
        /// Gets a value indicating whether gets the glyph represents a whitespace character.
        /// </summary>
        public bool IsWhiteSpace { get; }

        public bool IsControlCharacter { get; }

        public GlyphLayout(int codePoint, Glyph glyph, Vector2 location, float width, float height, float lineHeight, bool startOfLine, bool isWhiteSpace, bool isControlCharacter)
        {
            this.CodePoint = codePoint;
            this.Glyph = glyph;
            this.Location = location;
            this.Width = width;
            this.Height = height;
            this.LineHeight = lineHeight;
            this.StartOfLine = startOfLine;
            this.IsWhiteSpace = isWhiteSpace;
            this.IsControlCharacter = isControlCharacter;
        }

        public RectangleF BoundingBox(Vector2 dpi)
        {
            RectangleF box = this.Glyph.BoundingBox(this.Location * dpi, dpi);

            if (this.IsWhiteSpace)
                box.Width = this.Width * dpi.X;

            return box;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (this.StartOfLine)
            {
                sb.Append('@');
                sb.Append(' ');
            }

            if (this.IsWhiteSpace)
                sb.Append('!');

            sb.Append('\'');
            switch (this.CodePoint)
            {
                case '\t': sb.Append("\\t"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case ' ': sb.Append(" "); break;
                default:
                    sb.Append(char.ConvertFromUtf32(this.CodePoint));
                    break;
            }

            sb.Append('\'');
            sb.Append(' ');

            sb.Append(this.Location.X);
            sb.Append(',');
            sb.Append(this.Location.Y);
            sb.Append(' ');
            sb.Append(this.Width);
            sb.Append('x');
            sb.Append(this.Height);

            return sb.ToString();
        }
    }
}
