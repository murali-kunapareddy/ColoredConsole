using System;

namespace bcd
{
    /// <summary>
    /// Configures how a single <see cref="ColoredConsole"/> write call is rendered.
    /// Any <c>null</c> value falls back to the instance's active <see cref="Theme"/> setting,
    /// allowing per-call overrides without affecting defaults.
    /// <para>
    /// Use object-initializer syntax or the fluent builder methods (e.g. <c>new WriteOptions().Green().Tabbed(1)</c>).
    /// </para>
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

        // ── Fluent Builder ────────────────────────────────────────────────────

        /// <summary>Sets the foreground color and returns this instance for chaining.</summary>
        public WriteOptions WithColor(ConsoleColor color)   { ForeColor = color; return this; }

        /// <summary>Sets the background color and returns this instance for chaining.</summary>
        public WriteOptions WithBack(ConsoleColor color)    { BackColor = color; return this; }

        /// <summary>Sets the border color and returns this instance for chaining.</summary>
        public WriteOptions WithBorder(ConsoleColor color)  { LineColor = color; return this; }

        /// <summary>Sets the tab stop and returns this instance for chaining.</summary>
        public WriteOptions Tabbed(int stops)               { TabStop = stops; return this; }

        /// <summary>Sets text alignment and returns this instance for chaining.</summary>
        public WriteOptions Aligned(TextPosition pos)       { TextPosition = pos; return this; }

        /// <summary>Sets the text style and returns this instance for chaining.</summary>
        public WriteOptions Styled(TextStyle style)         { TextStyle = style; return this; }

        /// <summary>Enables auto-numbering and returns this instance for chaining.</summary>
        public WriteOptions Numbered()                      { AutoNumber = true; return this; }

        // ── Color Shorthand ───────────────────────────────────────────────────

        /// <summary>Green foreground.</summary>
        public WriteOptions Green()       => WithColor(ConsoleColor.Green);

        /// <summary>Red foreground.</summary>
        public WriteOptions Red()         => WithColor(ConsoleColor.Red);

        /// <summary>Yellow foreground.</summary>
        public WriteOptions Yellow()      => WithColor(ConsoleColor.Yellow);

        /// <summary>Cyan foreground.</summary>
        public WriteOptions Cyan()        => WithColor(ConsoleColor.Cyan);

        /// <summary>White foreground.</summary>
        public WriteOptions White()       => WithColor(ConsoleColor.White);

        /// <summary>DarkGray foreground.</summary>
        public WriteOptions Dimmed()      => WithColor(ConsoleColor.DarkGray);

        /// <summary>Centers the text.</summary>
        public WriteOptions Centered()    => Aligned(bcd.TextPosition.Center);

        /// <summary>Right-aligns the text.</summary>
        public WriteOptions RightAligned() => Aligned(bcd.TextPosition.Right);
    }
}

