using System;

namespace bcd
{
    /// <summary>
    /// Defines default visual settings for a <see cref="ColoredConsole"/> instance.
    /// All properties are mutable so the theme can be adjusted at runtime.
    /// </summary>
    public class Theme
    {
        // ── Colors ────────────────────────────────────────────────────────────

        /// <summary>Console background color.</summary>
        public ConsoleColor BackColor   { get; set; } = ConsoleColor.Black;

        /// <summary>Default text (foreground) color.</summary>
        public ConsoleColor ForeColor   { get; set; } = ConsoleColor.White;

        /// <summary>Box border and separator color.</summary>
        public ConsoleColor LineColor   { get; set; } = ConsoleColor.Yellow;

        /// <summary>Accent color used for box titles and section headers.</summary>
        public ConsoleColor AccentColor { get; set; } = ConsoleColor.DarkRed;

        // ── Line Styles ───────────────────────────────────────────────────────

        /// <summary>Default style for top/bottom box borders.</summary>
        public LineStyle LineStyle           { get; set; } = LineStyle.Double;

        /// <summary>Default vertical border style for separators.</summary>
        public LineStyle VerticalLineStyle   { get; set; } = LineStyle.Double;

        /// <summary>Default horizontal fill style for separators.</summary>
        public LineStyle HorizontalLineStyle { get; set; } = LineStyle.Single;

        // ── Text ──────────────────────────────────────────────────────────────

        /// <summary>Default text alignment.</summary>
        public TextPosition TextPosition { get; set; } = TextPosition.Left;

        /// <summary>Default text transformation.</summary>
        public TextStyle TextStyle { get; set; } = TextStyle.None;

        // ── Semantic Colors ───────────────────────────────────────────────────

        /// <summary>Color used by <c>WriteSuccess</c>.</summary>
        public ConsoleColor SuccessColor { get; set; } = ConsoleColor.Green;

        /// <summary>Color used by <c>WriteError</c>.</summary>
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

        /// <summary>Color used by <c>WriteWarning</c>.</summary>
        public ConsoleColor WarningColor { get; set; } = ConsoleColor.DarkYellow;

        /// <summary>Color used by <c>WriteInfo</c>.</summary>
        public ConsoleColor InfoColor { get; set; } = ConsoleColor.Cyan;

        // ── Preset Themes ─────────────────────────────────────────────────────

        /// <summary>Dark background with yellow borders (default).</summary>
        public static Theme Default => new Theme();

        /// <summary>Light terminal: white background with dark text.</summary>
        public static Theme Light => new Theme
        {
            BackColor   = ConsoleColor.White,
            ForeColor   = ConsoleColor.Black,
            LineColor   = ConsoleColor.DarkBlue,
            AccentColor = ConsoleColor.DarkRed
        };

        /// <summary>Hacker aesthetic: black background with green text and cyan borders.</summary>
        public static Theme Hacker => new Theme
        {
            BackColor   = ConsoleColor.Black,
            ForeColor   = ConsoleColor.Green,
            LineColor   = ConsoleColor.Cyan,
            AccentColor = ConsoleColor.DarkGreen
        };
    }
}
