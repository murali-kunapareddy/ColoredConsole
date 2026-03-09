using System;

namespace Bcd
{
    /// <summary>
    /// Configures how a single <see cref="ColoredConsole"/> write call is rendered.
    /// Any <c>null</c> value falls back to the instance's active <see cref="Theme"/> setting,
    /// allowing per-call overrides without affecting defaults.
    /// </summary>
    public class WriteOptions
    {
        /// <summary>Border style. <c>null</c> = theme default.</summary>
        public LineStyle? LineStyle { get; set; }

        /// <summary>Text alignment. <c>null</c> = theme default.</summary>
        public TextPosition? TextPosition { get; set; }

        /// <summary>Number of 4-space indents from the inner left border. Default: <c>0</c>.</summary>
        public int TabStop { get; set; }

        /// <summary>Prefix the line with an auto-incrementing counter. Default: <c>false</c>.</summary>
        public bool AutoNumber { get; set; }

        /// <summary>Text transformation. <c>null</c> = theme default.</summary>
        public TextStyle? TextStyle { get; set; }

        /// <summary>Background color. <c>null</c> = theme default.</summary>
        public ConsoleColor? BackColor { get; set; }

        /// <summary>Foreground (text) color. <c>null</c> = theme default.</summary>
        public ConsoleColor? ForeColor { get; set; }

        /// <summary>Border color. <c>null</c> = theme default.</summary>
        public ConsoleColor? LineColor { get; set; }
    }
}
