using System;

namespace bcd
{
    /// <summary>Visual style preset for <see cref="ColoredConsole.DrawTable"/>.</summary>
    public enum TableStyle
    {
        /// <summary>Double outer border with single inner column dividers (default, most readable).</summary>
        DoubleBorderSingleInner = 0,

        /// <summary>All double-line borders.</summary>
        AllDouble = 1,

        /// <summary>All single-line borders (lightweight).</summary>
        AllSingle = 2
    }

    /// <summary>Horizontal alignment of the table within the console box.</summary>
    public enum TableAlignment
    {
        /// <summary>Table starts 2 spaces from the left border (default).</summary>
        Left = 0,

        /// <summary>Table is centered horizontally within the box.</summary>
        Center = 1,

        /// <summary>Table ends 2 spaces from the right border.</summary>
        Right = 2,

        /// <summary>Table columns expand to fill the full width, leaving a 2-space gap on each side.</summary>
        Justified = 3
    }

    /// <summary>
    /// Configuration for <see cref="ColoredConsole.DrawTable"/>.
    /// Any <c>null</c> color falls back to the active <see cref="Theme"/>.
    /// </summary>
    public class TableOptions
    {
        /// <summary>Per-column text alignment. Missing entries default to <see cref="TextPosition.Left"/>.</summary>
        public TextPosition[] ColumnAlignments { get; set; }

        /// <summary>
        /// Explicit column widths in characters (excluding padding).
        /// <c>null</c> = auto-compute from the widest cell in each column.
        /// </summary>
        public int[] ColumnWidths { get; set; }

        /// <summary>Border and inner-line style. Default: <see cref="TableStyle.DoubleBorderSingleInner"/>.</summary>
        public TableStyle Style { get; set; } = TableStyle.DoubleBorderSingleInner;

        /// <summary>Draw a horizontal separator between every data row. Default: <c>false</c>.</summary>
        public bool ShowRowSeparators { get; set; } = false;

        /// <summary>Render the first row as a header with a heavier separator below it. Default: <c>true</c>.</summary>
        public bool ShowHeader { get; set; } = true;

        /// <summary>Table background color. <c>null</c> = theme default.</summary>
        public ConsoleColor? BackColor { get; set; }

        /// <summary>Header row text color. <c>null</c> = theme accent color.</summary>
        public ConsoleColor? HeaderForeColor { get; set; }

        /// <summary>Data row text color. <c>null</c> = theme foreground color.</summary>
        public ConsoleColor? DataForeColor { get; set; }

        /// <summary>Border and separator color. <c>null</c> = theme line color.</summary>
        public ConsoleColor? LineColor { get; set; }

        /// <summary>
        /// Horizontal placement of the table within the box.
        /// Default: <see cref="TableAlignment.Left"/> (2-space gap from the left border).
        /// </summary>
        public TableAlignment Alignment { get; set; } = TableAlignment.Left;
    }
}
