using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bcd
{
    /// <summary>
    /// Richly-formatted console output: colored text, bordered boxes, section headers,
    /// separators, prompts, and optional file logging.
    /// <para>
    /// Implements <see cref="IDisposable"/> — wrap in a <c>using</c> block to ensure log
    /// files are flushed and closed on exit.
    /// </para>
    /// </summary>
    public class ColoredConsole : IDisposable
    {
        #region ──── BOX-DRAWING CHARACTER SET ────

        // Interpunct / dash fill characters
        private const char DOT_TB   = '·';
        private const char DSH_TB   = '-';

        // Double-line box characters
        private const char DBL_TL   = '╔';
        private const char DBL_TR   = '╗';
        private const char DBL_LR   = '║';
        private const char DBL_LJ   = '╠';
        private const char DBL_RJ   = '╣';
        private const char DBL_TB   = '═';
        private const char DBL_BL   = '╚';
        private const char DBL_BR   = '╝';

        // Single-line box characters
        private const char SGL_TL   = '┌';
        private const char SGL_TR   = '┐';
        private const char SGL_LR   = '│';
        private const char SGL_LJ   = '├';
        private const char SGL_RJ   = '┤';   // BUG FIX: original code used SGL_LJ here too
        private const char SGL_TB   = '─';
        private const char SGL_BL   = '└';
        private const char SGL_BR   = '┘';

        // Mixed-style joiners (double vertical + single horizontal, and vice-versa)
        private const char MIX_DLSJ = '╟';
        private const char MIX_DRSJ = '╢';
        private const char MIX_SLDJ = '╞';
        private const char MIX_SRDJ = '╡';

        // Table joiners
        private const char DBL_TJ   = '╦';  // double top joiner        (all-double tables)
        private const char DBL_BJ   = '╩';  // double bottom joiner
        private const char DBL_CJ   = '╬';  // double cross joiner
        private const char SGL_TJ   = '┬';  // single top joiner       (all-single tables)
        private const char SGL_BJ   = '┴';  // single bottom joiner
        private const char SGL_CJ   = '┼';  // single cross joiner
        private const char MIX_DTSC = '╤';  // double-H + single-V top joiner   (mixed tables)
        private const char MIX_DBSC = '╧';  // double-H + single-V bottom joiner
        private const char MIX_DHVC = '╪';  // double-H + single-V cross joiner

        #endregion

        #region ──── FIELDS ────

        private readonly Theme       _theme;
        private          int         _width;
        private          int         _availableWidth;  // _width - 4 (2 border chars + 2 padding spaces)
        private          int         _autoNumber;
        private          StreamWriter _logWriter;
        private readonly object      _logLock = new object();
        private          bool        _disposed;
        private          bool        _asciiMode;
        private          Func<string, string> _logFormatter;

        #endregion

        #region ──── PROPERTIES ────

        /// <summary>The active theme supplying default colors and line styles.</summary>
        public Theme Theme => _theme;

        /// <summary>Resolved render width in characters.</summary>
        public int Width => _width;

        /// <summary>Current auto-number counter value (thread-safe read).</summary>
        public int AutoNumberCounter => Volatile.Read(ref _autoNumber);

        /// <summary>Whether file logging is currently active.</summary>
        public bool LogEnabled => _logWriter != null;

        /// <summary>Whether ASCII fallback mode is active (replaces Unicode box-drawing with +, -, |).</summary>
        public bool AsciiMode => _asciiMode;

        #endregion

        #region ──── CONSTRUCTORS ────

        /// <summary>
        /// Creates a new <see cref="ColoredConsole"/> with an optional theme and width.
        /// </summary>
        /// <param name="theme">
        /// Visual theme. Defaults to <see cref="Theme.Default"/> (dark background, yellow borders).
        /// </param>
        /// <param name="width">
        /// Render width in characters. Pass <c>0</c> (default) to auto-detect from the current
        /// console window width (clamped 40–120), or supply a fixed value for redirected output.
        /// </param>
        /// <param name="asciiMode">
        /// When <c>true</c>, replaces Unicode box-drawing characters with plain ASCII
        /// (<c>+</c>, <c>-</c>, <c>|</c>). Useful for CI/CD logs, SSH sessions, or legacy terminals.
        /// </param>
        public ColoredConsole(Theme theme = null, int width = 0, bool asciiMode = false)
        {
            _theme          = theme ?? Theme.Default;
            _asciiMode      = asciiMode;
            _width          = ResolveWidth(width);
            _availableWidth = _width - 4;
        }

        #endregion

        #region ──── LOGGING ────

        /// <summary>
        /// Enables daily file logging. Each session appends to
        /// <c>&lt;appDir&gt;/&lt;folder&gt;/yy-MM-dd.log</c>.
        /// The log folder is created automatically if it does not exist.
        /// </summary>
        /// <param name="folder">Subfolder name relative to the application base directory. Default: <c>"Log"</c>.</param>
        /// <param name="formatter">
        /// Optional custom log line formatter. Receives the raw message and returns the string to write.
        /// Default: <c>[HH:mm:ss.fff] message</c>.
        /// Example: <c>msg => $"{DateTime.UtcNow:O} | {msg}"</c>
        /// </param>
        public void EnableLogging(string folder = "Log", Func<string, string> formatter = null)
        {
            _logFormatter = formatter;
            EnableLoggingCore(folder);
        }

        private void EnableLoggingCore(string folder)
        {
            lock (_logLock)
            {
                _logWriter?.Dispose();

                // Support both absolute paths (e.g. from tests) and relative subfolder names
                var logFolder = Path.IsPathRooted(folder)
                    ? folder
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);
                Directory.CreateDirectory(logFolder);

                var logPath = Path.Combine(logFolder, $"{DateTime.Now:yy-MM-dd}.log");

                _logWriter = new StreamWriter(logPath, append: true, encoding: Encoding.UTF8)
                {
                    AutoFlush = true
                };
                _logWriter.WriteLine($"===== SESSION {DateTime.Now:dd MMM yyyy HH:mm:ss} =====");
            }
        }

        /// <summary>
        /// Writes a timestamped message directly to the log file. No-op if logging is not enabled.
        /// </summary>
        public void WriteLog(string message)
        {
            if (_logWriter == null) return;
            lock (_logLock)
            {
                var line = _logFormatter != null
                    ? _logFormatter(message)
                    : $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
                _logWriter?.WriteLine(line);
            }
        }

        #endregion

        #region ──── DRAW: TOP / BOTTOM ────

        /// <summary>Draws the top border of a box.</summary>
        public void DrawTopLine(
            LineStyle?    lineStyle = null,
            ConsoleColor? backColor = null,
            ConsoleColor? lineColor = null)
        {
            var ls = lineStyle ?? _theme.LineStyle;
            var bc = backColor ?? _theme.BackColor;
            var lc = lineColor ?? _theme.LineColor;
            ApplyColors(bc, lc);
            Console.WriteLine(_asciiMode
                ? $"+{new string('-', _width - 2)}+"
                : ls == LineStyle.Single
                    ? $"{SGL_TL}{new string(SGL_TB, _width - 2)}{SGL_TR}"
                    : $"{DBL_TL}{new string(DBL_TB, _width - 2)}{DBL_TR}");
            Console.ResetColor();
        }

        /// <summary>Draws the bottom border of a box.</summary>
        public void DrawBottomLine(
            LineStyle?    lineStyle = null,
            ConsoleColor? backColor = null,
            ConsoleColor? lineColor = null)
        {
            var ls = lineStyle ?? _theme.LineStyle;
            var bc = backColor ?? _theme.BackColor;
            var lc = lineColor ?? _theme.LineColor;
            ApplyColors(bc, lc);
            Console.WriteLine(_asciiMode
                ? $"+{new string('-', _width - 2)}+"
                : ls == LineStyle.Single
                    ? $"{SGL_BL}{new string(SGL_TB, _width - 2)}{SGL_BR}"
                    : $"{DBL_BL}{new string(DBL_TB, _width - 2)}{DBL_BR}");
            Console.ResetColor();
        }

        #endregion

        #region ──── DRAW: SEPARATOR ────

        /// <summary>Draws a horizontal separator using the theme's default styles.</summary>
        public void DrawSeparator() => DrawSeparator(null, null);

        /// <summary>Draws a horizontal separator with the specified vertical and horizontal styles.</summary>
        public void DrawSeparator(
            LineStyle?    verticalLineStyle   = null,
            LineStyle?    horizontalLineStyle = null,
            ConsoleColor? backColor           = null,
            ConsoleColor? lineColor           = null)
        {
            var vls = verticalLineStyle   ?? _theme.VerticalLineStyle;
            var hls = horizontalLineStyle ?? _theme.HorizontalLineStyle;
            var bc  = backColor ?? _theme.BackColor;
            var lc  = lineColor ?? _theme.LineColor;
            ApplyColors(bc, lc);
            Console.WriteLine(BuildSeparatorLine(vls, hls));
            Console.ResetColor();
        }

        #endregion

        #region ──── DRAW: SECTION HEADER ────

        /// <summary>
        /// Draws a padded section-header line with embedded text and surrounding blank lines.
        /// Use this instead of <see cref="DrawSeparator()"/> when you need a labelled divider.
        /// </summary>
        public void DrawSectionHeader(
            string        message,
            LineStyle?    lineStyle    = null,
            TextPosition  textPosition = TextPosition.Left,
            int           tabStop      = 1,
            bool          autoNumber   = false,
            TextStyle     textStyle    = TextStyle.Caps,
            ConsoleColor? backColor    = null,
            ConsoleColor? foreColor    = null,
            ConsoleColor? lineColor    = null)
        {
            var ls = lineStyle ?? _theme.HorizontalLineStyle;
            var bc = backColor ?? _theme.BackColor;
            var fc = foreColor ?? _theme.AccentColor;
            var lc = lineColor ?? _theme.LineColor;

            var header = EmbedInLine(message, ls, tabStop);
            RenderBlankLine(bc, lc);
            RenderLine(header, _theme.LineStyle, textPosition, 0, autoNumber, textStyle, bc, fc, lc);
            RenderBlankLine(bc, lc);
            WriteLog($"[SECTION] {message}");
        }

        #endregion

        #region ──── DRAW: BOX ────

        /// <summary>Draws a complete box (top border + content + bottom border) around the given message.</summary>
        public void DrawBox(
            string        message,
            LineStyle?    lineStyle    = null,
            TextPosition  textPosition = TextPosition.Center,
            int           tabStop      = 0,
            bool          autoNumber   = false,
            TextStyle     textStyle    = TextStyle.SpacedCaps,
            ConsoleColor? backColor    = null,
            ConsoleColor? foreColor    = null,
            ConsoleColor? lineColor    = null)
        {
            var ls = lineStyle ?? _theme.LineStyle;
            var bc = backColor ?? _theme.BackColor;
            var fc = foreColor ?? _theme.AccentColor;
            var lc = lineColor ?? _theme.LineColor;

            DrawTopLine(ls, bc, lc);
            RenderLine(message, ls, textPosition, tabStop, autoNumber, textStyle, bc, fc, lc);
            DrawBottomLine(ls, bc, lc);
        }

        /// <summary>
        /// Draws a two-section box: a styled header, an inner separator, and one or more body lines.
        /// </summary>
        /// <param name="header">Title text for the top section.</param>
        /// <param name="body">Body lines displayed below the separator.</param>
        /// <param name="lineStyle">Outer border style (defaults to theme).</param>
        /// <param name="separatorStyle">Horizontal fill style for the inner separator (defaults to <paramref name="lineStyle"/>).</param>
        /// <param name="headerPosition">Alignment for the header text.</param>
        /// <param name="headerStyle">Text style for the header.</param>
        /// <param name="headerDecoration">ANSI decorations (Bold, Italic, Underline, …) for the header text.</param>
        /// <param name="headerGradientFrom">Start RGB color for a horizontal gradient on the header. Ignored when ANSI is unsupported.</param>
        /// <param name="headerGradientTo">End RGB color for the header gradient. Both <paramref name="headerGradientFrom"/> and this must be set to activate gradient.</param>
        /// <param name="bodyPosition">Alignment for body lines.</param>
        /// <param name="bodyStyle">Text style for body lines.</param>
        /// <param name="bodyDecoration">ANSI decorations for body text.</param>
        /// <param name="bodyTabStop">Tab indent for body lines.</param>
        /// <param name="backColor">Background color (defaults to theme).</param>
        /// <param name="headerForeColor">Header text color (defaults to theme accent). Ignored when gradient is active.</param>
        /// <param name="bodyForeColor">Body text color (defaults to theme foreground).</param>
        /// <param name="lineColor">Border color (defaults to theme).</param>
        public void DrawBox2(
            string                    header,
            string[]                  body,
            LineStyle?                lineStyle           = null,
            LineStyle?                separatorStyle      = null,
            TextPosition              headerPosition      = TextPosition.Center,
            TextStyle                 headerStyle         = TextStyle.SpacedCaps,
            AnsiDecoration            headerDecoration    = AnsiDecoration.None,
            (byte R, byte G, byte B)? headerGradientFrom  = null,
            (byte R, byte G, byte B)? headerGradientTo    = null,
            TextPosition              bodyPosition        = TextPosition.Left,
            TextStyle                 bodyStyle           = TextStyle.None,
            AnsiDecoration            bodyDecoration      = AnsiDecoration.None,
            int                       bodyTabStop         = 0,
            ConsoleColor?             backColor           = null,
            ConsoleColor?             headerForeColor     = null,
            ConsoleColor?             bodyForeColor       = null,
            ConsoleColor?             lineColor           = null)
        {
            var ls  = lineStyle       ?? _theme.LineStyle;
            var sep = separatorStyle  ?? ls;
            var bc  = backColor       ?? _theme.BackColor;
            var hfc = headerForeColor ?? _theme.AccentColor;
            var bfc = bodyForeColor   ?? _theme.ForeColor;
            var lc  = lineColor       ?? _theme.LineColor;

            var headerRenderer = BuildTextRenderer(hfc, headerDecoration, headerGradientFrom, headerGradientTo);
            var bodyRenderer   = BuildTextRenderer(bfc, bodyDecoration,   null,               null);

            DrawTopLine(ls, bc, lc);
            RenderLine(header, ls, headerPosition, 0, false, headerStyle, bc, hfc, lc, headerRenderer);

            // Inner separator — vertical joiners match outer box style; horizontal fill is configurable
            ApplyColors(bc, lc);
            Console.WriteLine(_asciiMode
                ? $"+{new string('-', _width - 2)}+"
                : BuildSeparatorLine(ls, sep));
            Console.ResetColor();

            if (body != null)
                foreach (var line in body)
                    RenderLine(line ?? string.Empty, ls, bodyPosition, bodyTabStop, false, bodyStyle, bc, bfc, lc, bodyRenderer);

            DrawBottomLine(ls, bc, lc);
        }

        #endregion

        #region ──── PROMPT ────

        /// <summary>
        /// Displays a prompt message inside the box border and reads a line of user input.
        /// Returns the input string (may be null if redirected).
        /// </summary>
        public string Prompt(string message, WriteOptions options = null)
        {
            var o = Resolve(options);
            RenderLine(message, o.LineStyle, o.TextPosition, o.TabStop,
                o.AutoNumber, o.TextStyle, o.BackColor, o.ForeColor, o.LineColor);
            var input = RenderReadLine(o.LineStyle, o.TabStop, o.BackColor, ConsoleColor.Green, o.LineColor);
            WriteLog(message);
            return input;
        }

        #endregion

        #region ──── WRITE / WRITELINE ────

        /// <summary>
        /// Writes text inside the box borders and repositions the cursor to the start of that line.
        /// Designed for in-place animation (e.g. progress bars). For regular output use <see cref="WriteLine"/>.
        /// </summary>
        public void Write(string message, WriteOptions options = null)
        {
            var o = Resolve(options);
            RenderWrite(message, o.LineStyle, o.TextPosition, o.TabStop,
                o.AutoNumber, o.TextStyle, o.BackColor, o.ForeColor, o.LineColor);
            WriteLog(message);
        }

        /// <summary>
        /// Writes text inside the box borders followed by a newline.
        /// Call with no arguments to output a blank bordered line.
        /// </summary>
        public void WriteLine(string message = "", WriteOptions options = null)
        {
            var o = Resolve(options);
            RenderLine(message, o.LineStyle, o.TextPosition, o.TabStop,
                o.AutoNumber, o.TextStyle, o.BackColor, o.ForeColor, o.LineColor);
            WriteLog(message);
        }

        /// <summary>Resets the auto-number counter to zero.</summary>
        public void ResetAutoNumber() => Interlocked.Exchange(ref _autoNumber, 0);

        #endregion

        #region ──── RESIZE / ASCII MODE ────

        /// <summary>
        /// Re-reads <see cref="Console.WindowWidth"/> and updates the internal render width.
        /// Call this after the user resizes the terminal window.
        /// </summary>
        /// <param name="explicitWidth">Override value. Pass <c>0</c> to auto-detect.</param>
        public void Resize(int explicitWidth = 0)
        {
            _width          = ResolveWidth(explicitWidth);
            _availableWidth = _width - 4;
        }

        /// <summary>
        /// Enables or disables ASCII fallback mode at runtime.
        /// When enabled, Unicode box-drawing characters are replaced with <c>+</c>, <c>-</c>, <c>|</c>.
        /// </summary>
        public void SetAsciiMode(bool enabled) => _asciiMode = enabled;

        #endregion

        #region ──── DRAW: KEY-VALUE / LIST ────

        /// <summary>
        /// Writes a single key-value pair with the key in accent color and the value in foreground color.
        /// </summary>
        public void DrawKeyValue(string key, string value,
            int tabStop = 0,
            ConsoleColor? keyColor   = null,
            ConsoleColor? valueColor = null,
            WriteOptions  options    = null)
        {
            var o  = Resolve(options);
            var kc = keyColor   ?? _theme.AccentColor;
            var vc = valueColor ?? _theme.ForeColor;
            var lr = BorderChar(o.LineStyle);

            ApplyColors(o.BackColor, o.LineColor);
            Console.Write($"{lr} ");
            Console.Write(new string(' ', tabStop * 4));

            Console.ForegroundColor = kc;
            Console.Write(key);
            Console.ForegroundColor = o.LineColor;
            Console.Write(": ");
            Console.ForegroundColor = vc;

            // Fill rest of line
            var content      = $"{key}: {value}";
            var padded       = value.PadRight(_availableWidth - tabStop * 4 - content.Length + value.Length);
            Console.Write(padded);

            Console.ForegroundColor = o.LineColor;
            Console.WriteLine($" {lr}");
            Console.ResetColor();
            WriteLog($"{key}: {value}");
        }

        /// <summary>
        /// Writes a labelled bullet list, each item prefixed with a bullet character.
        /// </summary>
        /// <param name="header">Optional section header drawn above the list. Pass <c>null</c> to skip.</param>
        /// <param name="items">List items.</param>
        /// <param name="bullet">Bullet character. Default: <c>•</c>. Use <c>'-'</c> for ASCII mode.</param>
        /// <param name="tabStop">Indentation level for list items.</param>
        public void DrawList(string header, string[] items,
            char   bullet  = '•',
            int    tabStop = 1,
            ConsoleColor? foreColor = null)
        {
            if (header != null)
                DrawSectionHeader(header);

            if (items == null) return;

            var color = foreColor ?? _theme.ForeColor;
            var effectiveBullet = _asciiMode ? '-' : bullet;

            foreach (var item in items)
                WriteLine($"{effectiveBullet} {item}", new WriteOptions { ForeColor = color, TabStop = tabStop });
        }

        #endregion

        #region ──── WRITE: SEMANTIC ────

        /// <summary>Writes a success message prefixed with ✓ in the theme's success color.</summary>
        public void WriteSuccess(string message, int tabStop = 0)
            => WriteLine($"✓ {message}", new WriteOptions { ForeColor = _theme.SuccessColor, TabStop = tabStop });

        /// <summary>Writes an error message prefixed with ✗ in the theme's error color.</summary>
        public void WriteError(string message, int tabStop = 0)
            => WriteLine($"✗ {message}", new WriteOptions { ForeColor = _theme.ErrorColor, TabStop = tabStop });

        /// <summary>Writes a warning message prefixed with ⚠ in the theme's warning color.</summary>
        public void WriteWarning(string message, int tabStop = 0)
            => WriteLine($"⚠ {message}", new WriteOptions { ForeColor = _theme.WarningColor, TabStop = tabStop });

        /// <summary>Writes an informational message prefixed with ℹ in the theme's info color.</summary>
        public void WriteInfo(string message, int tabStop = 0)
            => WriteLine($"ℹ {message}", new WriteOptions { ForeColor = _theme.InfoColor, TabStop = tabStop });

        #endregion

        #region ──── WRITE: ASYNC ────

        /// <summary>
        /// Asynchronously writes text inside the box borders followed by a newline.
        /// Thread-pool offload — safe to <c>await</c> from async contexts.
        /// </summary>
        public Task WriteLineAsync(string message = "", WriteOptions options = null)
            => Task.Run(() => WriteLine(message, options));

        /// <summary>
        /// Asynchronously writes text inside the box borders (animation-safe overwrite mode).
        /// </summary>
        public Task WriteAsync(string message, WriteOptions options = null)
            => Task.Run(() => Write(message, options));

        #endregion

        #region ──── DRAW: TABLE ────

        /// <summary>
        /// Renders a formatted, bordered table with an optional header row.
        /// Column widths are auto-computed from cell content unless overridden via <see cref="TableOptions.ColumnWidths"/>.
        /// </summary>
        /// <param name="headers">Header labels. Pass <c>null</c> or set <see cref="TableOptions.ShowHeader"/> to <c>false</c> to omit.</param>
        /// <param name="rows">Data rows. Each row is an array of cell strings.</param>
        /// <param name="options">Table rendering options.</param>
        public void DrawTable(string[] headers, IEnumerable<string[]> rows, TableOptions options = null)
        {
            var opt = options ?? new TableOptions();

            // Materialise rows so we can iterate twice (column width + render)
            var data = new List<string[]>();
            if (rows != null)
                foreach (var r in rows) data.Add(r);

            int colCount = headers != null ? headers.Length
                         : data.Count > 0  ? data[0].Length
                         : 0;
            if (colCount == 0) return;

            // innerWidth = space between the outer box's left and right borders
            const int Gap        = 2;
            int       innerWidth = _width - 2; // outer ║ on each side

            // Maximum table width the alignment permits
            int maxTableW;
            switch (opt.Alignment)
            {
                case TableAlignment.Justified: maxTableW = innerWidth - Gap * 2; break; // 2-gap each side
                case TableAlignment.Center:    maxTableW = innerWidth;            break; // centers itself
                default:                       maxTableW = innerWidth - Gap;      break; // Left/Right: one gap
            }

            var colWidths = ComputeColumnWidths(headers, data, opt, colCount, maxTableW);

            // Justified: expand columns proportionally to fill the target width
            if (opt.Alignment == TableAlignment.Justified)
            {
                int overhead    = 3 * colCount + 1;
                int targetCols  = Math.Max(colCount, maxTableW - overhead);
                int currentCols = 0;
                for (int i = 0; i < colCount; i++) currentCols += colWidths[i];

                if (targetCols > currentCols)
                {
                    int extra = targetCols - currentCols, distributed = 0;
                    for (int i = 0; i < colCount - 1; i++)
                    {
                        int add = extra * colWidths[i] / currentCols;
                        colWidths[i] += add;
                        distributed  += add;
                    }
                    colWidths[colCount - 1] += extra - distributed;
                }
            }

            // Actual table width: border + N*(sp+col+sp) + (N-1)*inner + border  →  3N+1+Σcol
            int tableWidth = 3 * colCount + 1;
            for (int i = 0; i < colCount; i++) tableWidth += colWidths[i];

            // Left indent (space between outer ║ and the table's own left border)
            int indent;
            switch (opt.Alignment)
            {
                case TableAlignment.Right:
                    indent = Math.Max(0, innerWidth - tableWidth - Gap);
                    break;
                case TableAlignment.Center:
                    indent = Math.Max(0, (innerWidth - tableWidth) / 2);
                    break;
                default: // Left and Justified
                    indent = Gap;
                    break;
            }
            // Space between the table's right border and the outer ║
            int rightPad = Math.Max(0, innerWidth - indent - tableWidth);

            // Outer border character (the cc box that surrounds the table)
            char        outerChar = _asciiMode ? '|'
                                  : _theme.LineStyle == LineStyle.Double ? DBL_LR : SGL_LR;
            ConsoleColor outerLc  = _theme.LineColor;

            var chars = GetTableCharSet(opt.Style);
            var bc    = opt.BackColor       ?? _theme.BackColor;
            var hfc   = opt.HeaderForeColor ?? _theme.AccentColor;
            var dfc   = opt.DataForeColor   ?? _theme.ForeColor;
            var lc    = opt.LineColor       ?? _theme.LineColor;

            // Writes a horizontal structural line (border/separator) wrapped in the outer ║ chars
            void WriteBorderLine(string tableLine)
            {
                ApplyColors(bc, outerLc);
                Console.Write(outerChar);
                if (indent   > 0) Console.Write(new string(' ', indent));
                ApplyColors(bc, lc);
                Console.Write(tableLine);
                ApplyColors(bc, outerLc);
                if (rightPad > 0) Console.Write(new string(' ', rightPad));
                Console.Write(outerChar);
                Console.ResetColor();
                Console.WriteLine();
            }

            // Top border
            WriteBorderLine(BuildTableLine(colWidths, chars.TopLeft, chars.TopFill, chars.TopJoin, chars.TopRight));

            // Header
            if (opt.ShowHeader && headers != null && headers.Length > 0)
            {
                RenderTableRow(headers, colWidths, opt.ColumnAlignments, chars, bc, hfc, lc,
                    indent, rightPad, outerChar, outerLc);
                WriteBorderLine(BuildTableLine(colWidths, chars.HdrLeft, chars.HdrFill, chars.HdrJoin, chars.HdrRight));
            }

            // Data rows
            for (int r = 0; r < data.Count; r++)
            {
                RenderTableRow(data[r], colWidths, opt.ColumnAlignments, chars, bc, dfc, lc,
                    indent, rightPad, outerChar, outerLc);

                if (opt.ShowRowSeparators && r < data.Count - 1)
                    WriteBorderLine(BuildTableLine(colWidths, chars.RowLeft, chars.RowFill, chars.RowJoin, chars.RowRight));
            }

            // Bottom border
            WriteBorderLine(BuildTableLine(colWidths, chars.BotLeft, chars.BotFill, chars.BotJoin, chars.BotRight));
        }

        #endregion

        #region ──── DRAW: COLUMNS ────

        /// <summary>
        /// Writes an array of values side-by-side in equal-width columns inside the box border.
        /// </summary>
        /// <param name="values">Cell values, one per column.</param>
        /// <param name="alignments">Per-column alignment. Missing entries default to <see cref="TextPosition.Left"/>.</param>
        /// <param name="foreColors">Per-column text colors. Missing entries use theme foreground.</param>
        /// <param name="options">Base write options (line style, back/line color, tab stop).</param>
        public void WriteColumns(string[] values,
            TextPosition[] alignments = null,
            ConsoleColor[] foreColors = null,
            WriteOptions   options    = null)
        {
            if (values == null || values.Length == 0) return;

            var o        = Resolve(options);
            int colCount = values.Length;

            // Line structure: ║ [sp] col0 [sp│sp] col1 … colN-1 [sp] ║
            // Total = 2(left) + N*colW + (N-1)*3(separators) + 2(right) = _width
            // → sum of colW = _availableWidth − (N−1)*3
            // Distribute any integer-division remainder to the last column so the
            // line is always exactly _width characters wide.
            int totalData = Math.Max(colCount, _availableWidth - (colCount - 1) * 3);
            int baseColW  = totalData / colCount;
            int remainder = totalData - baseColW * colCount;

            // colWidths[i]: last column absorbs the remainder
            int[] colWidths = new int[colCount];
            for (int i = 0; i < colCount; i++)
                colWidths[i] = baseColW + (i == colCount - 1 ? remainder : 0);

            var lr = BorderChar(o.LineStyle);
            ApplyColors(o.BackColor, o.LineColor);
            Console.Write($"{lr} ");

            for (int i = 0; i < colCount; i++)
            {
                if (i > 0)
                {
                    Console.ForegroundColor = o.LineColor;
                    Console.Write($" {SGL_LR} ");
                }

                int colW  = colWidths[i];
                var tp    = alignments != null && i < alignments.Length ? alignments[i] : TextPosition.Left;
                var color = foreColors != null && i < foreColors.Length  ? foreColors[i]  : o.ForeColor;
                var text  = values[i] ?? string.Empty;
                if (text.Length > colW) text = text.Substring(0, colW);

                Console.ForegroundColor = color;
                Console.Write(PadColumn(text, tp, colW));
            }

            Console.ForegroundColor = o.LineColor;
            Console.WriteLine($" {lr}");
            Console.ResetColor();
            WriteLog(string.Join(" | ", values));
        }

        /// <summary>
        /// Writes label-value pairs as side-by-side columns, each formatted as <c>"Label: Value"</c>.
        /// </summary>
        public void WriteColumns(params (string Label, string Value)[] pairs)
        {
            if (pairs == null || pairs.Length == 0) return;
            var values = new string[pairs.Length];
            for (int i = 0; i < pairs.Length; i++)
                values[i] = $"{pairs[i].Label}: {pairs[i].Value}";
            WriteColumns(values);
        }

        #endregion

        #region ──── IDISPOSABLE ────

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_logLock)
            {
                _logWriter?.Dispose();
                _logWriter = null;
            }
        }

        #endregion

        #region ──── PRIVATE: RENDERING ────

        /// <summary>
        /// Renders a bordered line and repositions the cursor to the start of that line
        /// so the next call overwrites it in place (animation-safe).
        /// </summary>
        private void RenderWrite(string msg, LineStyle ls, TextPosition tp, int tab, bool an,
            TextStyle ts, ConsoleColor bc, ConsoleColor fc, ConsoleColor lc)
        {
            msg = msg?.Trim() ?? string.Empty;

            if (msg.Length <= _availableWidth)
            {
                var lr = BorderChar(ls);
                ApplyColors(bc, lc);
                Console.Write($"{lr} ");
                Console.ForegroundColor = fc;
                Console.Write(FormatMessage(msg, tp, tab, ts));
                Console.ForegroundColor = lc;
                Console.Write($" {lr}");
                Console.ResetColor();
                // '\r' returns to column 0 without advancing the line — universally supported
                if (!Console.IsOutputRedirected)
                    Console.Write('\r');
            }
            else
            {
                foreach (var chunk in WordWrap(msg, _availableWidth))
                    RenderLine(chunk, ls, tp, tab, false, ts, bc, fc, lc);
            }
        }

        /// <summary>Renders a bordered line followed by a newline.</summary>
        /// <param name="textRenderer">
        /// Optional custom text writer. When non-null, called with the formatted string instead of the
        /// default <c>Console.ForegroundColor + Write</c> path — use this to inject ANSI decorations or gradients.
        /// The renderer is responsible for writing the text; color reset is handled by the caller.
        /// </param>
        private void RenderLine(string msg, LineStyle ls, TextPosition tp, int tab, bool an,
            TextStyle ts, ConsoleColor bc, ConsoleColor fc, ConsoleColor lc,
            Action<string> textRenderer = null)
        {
            msg = msg?.Trim() ?? string.Empty;

            if (an && tp == TextPosition.Left)
            {
                var n = Interlocked.Increment(ref _autoNumber);
                msg = $"{n}.".PadRight(4) + msg;
            }

            if (msg.Length <= _availableWidth)
            {
                var lr = BorderChar(ls);
                ApplyColors(bc, lc);
                Console.Write($"{lr} ");
                if (textRenderer != null)
                    textRenderer(FormatMessage(msg, tp, tab, ts));
                else
                {
                    Console.ForegroundColor = fc;
                    Console.Write(FormatMessage(msg, tp, tab, ts));
                }
                Console.ForegroundColor = lc;
                Console.WriteLine($" {lr}");
                Console.ResetColor();
            }
            else
            {
                // Word-wrap: first chunk stays at caller's tab, subsequent chunks indent one extra level
                var chunks = WordWrap(msg, _availableWidth - tab * 4);
                bool first = true;
                foreach (var chunk in chunks)
                {
                    RenderLine(chunk, ls, tp, first ? tab : tab + 1, false, ts, bc, fc, lc, textRenderer);
                    first = false;
                }
            }
        }

        /// <summary>
        /// Builds an <c>Action&lt;string&gt;</c> text renderer that applies ANSI decorations and/or a
        /// gradient to the formatted text string inside a bordered line.
        /// Returns <c>null</c> when ANSI is unsupported or no effects are requested, causing
        /// <see cref="RenderLine"/> to fall back to the standard <c>Console.ForegroundColor</c> path.
        /// </summary>
        private static Action<string> BuildTextRenderer(
            ConsoleColor                    fc,
            AnsiDecoration                  decoration,
            (byte R, byte G, byte B)?       gradientFrom,
            (byte R, byte G, byte B)?       gradientTo)
        {
            bool hasDecoration = decoration != AnsiDecoration.None;
            bool hasGradient   = gradientFrom.HasValue && gradientTo.HasValue;

            if ((!hasDecoration && !hasGradient) || !AnsiConsole.IsSupported)
                return null;

            return text =>
            {
                if (hasDecoration)
                {
                    if ((decoration & AnsiDecoration.Bold)          != 0) AnsiConsole.Bold();
                    if ((decoration & AnsiDecoration.Italic)        != 0) AnsiConsole.Italic();
                    if ((decoration & AnsiDecoration.Underline)     != 0) AnsiConsole.Underline();
                    if ((decoration & AnsiDecoration.Dim)           != 0) AnsiConsole.Dim();
                    if ((decoration & AnsiDecoration.Blink)         != 0) AnsiConsole.Blink();
                    if ((decoration & AnsiDecoration.Strikethrough) != 0) AnsiConsole.Strikethrough();
                }

                if (hasGradient)
                {
                    // WriteGradient sets per-char RGB via ANSI (doesn't clear prior decorations)
                    // and calls AnsiConsole.Reset() at the end
                    AnsiConsole.WriteGradient(text, gradientFrom.Value, gradientTo.Value);
                }
                else
                {
                    // Use ANSI to set color — Console.ForegroundColor (Win32 API) would wipe ANSI state
                    AnsiConsole.SetForeground(ConsoleColorToAnsi256(fc));
                    Console.Write(text);
                    AnsiConsole.Reset();  // clear decorations; caller re-sets line color afterwards
                }
            };
        }

        /// <summary>
        /// Maps a <see cref="ConsoleColor"/> to the equivalent ANSI 256-color palette index (0–15).
        /// Used when applying decorations so the color stays in ANSI space and isn't cleared by
        /// the Win32 <c>Console.ForegroundColor</c> setter.
        /// </summary>
        private static int ConsoleColorToAnsi256(ConsoleColor c)
        {
            switch (c)
            {
                case ConsoleColor.Black:       return 0;
                case ConsoleColor.DarkRed:     return 1;
                case ConsoleColor.DarkGreen:   return 2;
                case ConsoleColor.DarkYellow:  return 3;
                case ConsoleColor.DarkBlue:    return 4;
                case ConsoleColor.DarkMagenta: return 5;
                case ConsoleColor.DarkCyan:    return 6;
                case ConsoleColor.Gray:        return 7;
                case ConsoleColor.DarkGray:    return 8;
                case ConsoleColor.Red:         return 9;
                case ConsoleColor.Green:       return 10;
                case ConsoleColor.Yellow:      return 11;
                case ConsoleColor.Blue:        return 12;
                case ConsoleColor.Magenta:     return 13;
                case ConsoleColor.Cyan:        return 14;
                default:                       return 15; // White
            }
        }

        /// <summary>Renders a blank bordered line using background and border colors.</summary>
        private void RenderBlankLine(ConsoleColor bc, ConsoleColor lc)
            => RenderLine(string.Empty, _theme.LineStyle, TextPosition.Left, 0, false,
                TextStyle.None, bc, _theme.ForeColor, lc);

        /// <summary>Renders a bordered "? " prompt line and captures user input inline.</summary>
        private string RenderReadLine(LineStyle ls, int tab,
            ConsoleColor bc, ConsoleColor fc, ConsoleColor lc)
        {
            var lr = BorderChar(ls);
            ApplyColors(bc, lc);
            Console.Write($"{lr} ");
            Console.ForegroundColor = fc;
            Console.Write(FormatMessage("? ", TextPosition.Left, tab + 1, TextStyle.None));
            Console.ForegroundColor = lc;
            Console.WriteLine($" {lr}");

            // Place cursor at the "?" input position inside the border
            if (!Console.IsOutputRedirected)
                Console.SetCursorPosition((tab + 1) * 4 + 4, Console.CursorTop - 1);

            Console.ForegroundColor = fc;
            var result = Console.ReadLine();
            Console.ResetColor();
            return result;
        }

        #endregion

        #region ──── PRIVATE: SEPARATOR CONSTRUCTION ────

        private string BuildSeparatorLine(LineStyle vls, LineStyle hls)
        {
            if (_asciiMode) return $"+{new string('-', _width - 2)}+";
            GetSeparatorChars(vls, hls, out char left, out char fill, out char right);
            return $"{left}{new string(fill, _width - 2)}{right}";
        }

        private static void GetSeparatorChars(LineStyle vls, LineStyle hls,
            out char left, out char fill, out char right)
        {
            if      (vls == LineStyle.Double && hls == LineStyle.Double) { left = DBL_LJ;   fill = DBL_TB; right = DBL_RJ;   }
            else if (vls == LineStyle.Double && hls == LineStyle.Single) { left = MIX_DLSJ; fill = SGL_TB; right = MIX_DRSJ; }
            else if (vls == LineStyle.Double && hls == LineStyle.Dotted) { left = DBL_LR;   fill = DOT_TB; right = DBL_LR;   }
            else if (vls == LineStyle.Double && hls == LineStyle.Dashed) { left = DBL_LR;   fill = DSH_TB; right = DBL_LR;   }
            else if (vls == LineStyle.Single && hls == LineStyle.Double) { left = MIX_SLDJ; fill = DBL_TB; right = MIX_SRDJ; }
            else if (vls == LineStyle.Single && hls == LineStyle.Single) { left = SGL_LJ;   fill = SGL_TB; right = SGL_RJ;   } // BUG FIX: was SGL_LJ on right
            else if (vls == LineStyle.Single && hls == LineStyle.Dotted) { left = SGL_LR;   fill = DOT_TB; right = SGL_LR;   }
            else                                                          { left = SGL_LR;   fill = DSH_TB; right = SGL_LR;   } // Single + Dashed
        }

        #endregion

        #region ──── PRIVATE: TEXT FORMATTING ────

        /// <summary>
        /// Embeds text into a full-width line filled with the style's fill character.
        /// Example (Single, tabStop=1): "──── MY HEADER ─────────────────────────────────"
        /// </summary>
        private string EmbedInLine(string text, LineStyle ls, int tabStop)
        {
            char fill = ls == LineStyle.Double ? DBL_TB
                      : ls == LineStyle.Dotted ? DOT_TB
                      : ls == LineStyle.Dashed ? DSH_TB
                      : SGL_TB;

            var prefix = new string(fill, tabStop * 4);
            var body   = $"{prefix} {text} ";
            var padLen = _availableWidth - body.Length;

            // BUG FIX: use padLen as additional chars, guard against negative
            return padLen > 0
                ? body + new string(fill, padLen)
                : body.Substring(0, Math.Min(body.Length, _availableWidth));
        }

        private string FormatMessage(string msg, TextPosition tp, int tab, TextStyle ts)
        {
            switch (ts)
            {
                case TextStyle.Spaced:     msg = SpaceOut(msg);                    break;
                case TextStyle.Caps:       msg = msg.ToUpperInvariant();            break;
                case TextStyle.SpacedCaps: msg = SpaceOut(msg).ToUpperInvariant(); break;
            }
            return PadToWidth(msg, tp, tab);
        }

        private string PadToWidth(string str, TextPosition tp, int tab)
        {
            switch (tp)
            {
                case TextPosition.Center:
                    int pad = _availableWidth - str.Length;
                    return str.PadLeft(str.Length + pad / 2).PadRight(_availableWidth);
                case TextPosition.Right:
                    return str.PadLeft(_availableWidth);
                default: // Left
                    return str.PadLeft(str.Length + tab * 4).PadRight(_availableWidth);
            }
        }

        private static string SpaceOut(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var sb = new StringBuilder(s.Length * 2);
            foreach (char c in s) { sb.Append(c); sb.Append(' '); }
            sb.Length--; // remove trailing space
            return sb.ToString();
        }

        /// <summary>
        /// Word-aware text wrapping. Splits at word boundaries; force-splits only when a
        /// single word exceeds <paramref name="maxWidth"/>.
        /// </summary>
        private static IEnumerable<string> WordWrap(string text, int maxWidth)
        {
            if (maxWidth <= 0) maxWidth = 1;

            var line = new StringBuilder();

            foreach (var word in text.Split(' '))
            {
                if (word.Length == 0) continue;

                // Force-split words that exceed maxWidth on their own
                if (word.Length > maxWidth)
                {
                    if (line.Length > 0) { yield return line.ToString(); line.Clear(); }
                    for (int i = 0; i < word.Length; i += maxWidth)
                        yield return word.Substring(i, Math.Min(maxWidth, word.Length - i));
                    continue;
                }

                int needed = line.Length == 0 ? word.Length : line.Length + 1 + word.Length;
                if (needed > maxWidth)
                {
                    yield return line.ToString();
                    line.Clear();
                }
                else if (line.Length > 0)
                {
                    line.Append(' ');
                }
                line.Append(word);
            }

            if (line.Length > 0) yield return line.ToString();
        }

        #endregion

        #region ──── PRIVATE: HELPERS ────

        private ResolvedOptions Resolve(WriteOptions o) => new ResolvedOptions
        {
            LineStyle    = o?.LineStyle    ?? _theme.LineStyle,
            TextPosition = o?.TextPosition ?? _theme.TextPosition,
            TabStop      = o?.TabStop      ?? 0,
            AutoNumber   = o?.AutoNumber   ?? false,
            TextStyle    = o?.TextStyle    ?? _theme.TextStyle,
            BackColor    = o?.BackColor    ?? _theme.BackColor,
            ForeColor    = o?.ForeColor    ?? _theme.ForeColor,
            LineColor    = o?.LineColor    ?? _theme.LineColor,
        };

        private char BorderChar(LineStyle ls)
        {
            if (_asciiMode) return '|';
            return ls == LineStyle.Double ? DBL_LR : SGL_LR;
        }

        private static void ApplyColors(ConsoleColor back, ConsoleColor fore)
        {
            Console.BackgroundColor = back;
            Console.ForegroundColor = fore;
        }

        private static int ResolveWidth(int requested)
        {
            if (requested > 0) return requested;
            if (Console.IsOutputRedirected) return 79;
            try   { return Math.Max(40, Math.Min(Console.WindowWidth - 1, 120)); }
            catch { return 79; }
        }

        // Internal struct avoids repeated heap allocation for resolved options
        private struct ResolvedOptions
        {
            public LineStyle    LineStyle;
            public TextPosition TextPosition;
            public int          TabStop;
            public bool         AutoNumber;
            public TextStyle    TextStyle;
            public ConsoleColor BackColor;
            public ConsoleColor ForeColor;
            public ConsoleColor LineColor;
        }

        #endregion

        #region ──── PRIVATE: TABLE HELPERS ────

        private void RenderTableRow(string[] cells, int[] colWidths, TextPosition[] alignments,
            TableCharSet chars, ConsoleColor bc, ConsoleColor fc, ConsoleColor lc,
            int indent, int rightPad, char outerChar, ConsoleColor outerLc)
        {
            // Outer left border
            ApplyColors(bc, outerLc);
            Console.Write(outerChar);

            // Indent gap between outer border and table
            if (indent > 0) Console.Write(new string(' ', indent));

            // Table left border
            ApplyColors(bc, lc);
            Console.Write(chars.Side);

            for (int i = 0; i < colWidths.Length; i++)
            {
                if (i > 0)
                {
                    Console.ForegroundColor = lc;
                    Console.Write(chars.InnerSide);
                }

                var text = (cells != null && i < cells.Length ? cells[i] : null) ?? string.Empty;
                if (text.Length > colWidths[i]) text = text.Substring(0, colWidths[i]);

                var tp = alignments != null && i < alignments.Length ? alignments[i] : TextPosition.Left;

                Console.ForegroundColor = lc;
                Console.Write(' ');
                Console.ForegroundColor = fc;
                Console.Write(PadColumn(text, tp, colWidths[i]));
                Console.ForegroundColor = lc;
                Console.Write(' ');
            }

            // Table right border
            Console.ForegroundColor = lc;
            Console.Write(chars.Side);

            // Right padding gap + outer right border
            ApplyColors(bc, outerLc);
            if (rightPad > 0) Console.Write(new string(' ', rightPad));
            Console.Write(outerChar);
            Console.ResetColor();
            Console.WriteLine();
        }

        private static string BuildTableLine(int[] colWidths, char left, char fill, char join, char right)
        {
            var sb = new StringBuilder();
            sb.Append(left);
            for (int i = 0; i < colWidths.Length; i++)
            {
                if (i > 0) sb.Append(join);
                sb.Append(new string(fill, colWidths[i] + 2)); // +2 for the padding spaces
            }
            sb.Append(right);
            return sb.ToString();
        }

        private int[] ComputeColumnWidths(string[] headers, List<string[]> data, TableOptions opt, int colCount, int maxTableWidth)
        {
            var widths = new int[colCount];

            if (opt.ColumnWidths != null)
            {
                for (int i = 0; i < colCount; i++)
                    widths[i] = i < opt.ColumnWidths.Length ? Math.Max(1, opt.ColumnWidths[i]) : 3;
            }
            else
            {
                // Auto-compute from content
                if (headers != null)
                    for (int i = 0; i < Math.Min(headers.Length, colCount); i++)
                        widths[i] = Math.Max(widths[i], headers[i]?.Length ?? 0);

                foreach (var row in data)
                    for (int i = 0; i < Math.Min(row.Length, colCount); i++)
                        widths[i] = Math.Max(widths[i], row[i]?.Length ?? 0);

                for (int i = 0; i < colCount; i++)
                    widths[i] = Math.Max(3, widths[i]);
            }

            // Clamp total to available width.
            // Line structure: [border] + N * ([sp][col][sp]) + (N-1) * [inner] + [border]
            // = 2 + N*(w+2) + (N-1)   →  overhead = 3*N + 1
            int maxData = maxTableWidth - (3 * colCount + 1);
            int total   = 0;
            for (int i = 0; i < colCount; i++) total += widths[i];

            if (total > maxData && maxData > 0)
            {
                double scale = (double)maxData / total;
                for (int i = 0; i < colCount; i++)
                    widths[i] = Math.Max(1, (int)(widths[i] * scale));
            }

            return widths;
        }

        private static string PadColumn(string text, TextPosition tp, int width)
        {
            switch (tp)
            {
                case TextPosition.Center:
                    int pad = width - text.Length;
                    return text.PadLeft(text.Length + pad / 2).PadRight(width);
                case TextPosition.Right:
                    return text.PadLeft(width);
                default:
                    return text.PadRight(width);
            }
        }

        private static TableCharSet GetTableCharSet(TableStyle style)
        {
            switch (style)
            {
                case TableStyle.AllDouble:
                    return new TableCharSet
                    {
                        Side      = DBL_LR,  InnerSide = DBL_LR,
                        TopLeft   = DBL_TL,  TopFill   = DBL_TB,  TopJoin   = DBL_TJ,  TopRight  = DBL_TR,
                        HdrLeft   = DBL_LJ,  HdrFill   = DBL_TB,  HdrJoin   = DBL_CJ,  HdrRight  = DBL_RJ,
                        RowLeft   = DBL_LJ,  RowFill   = DBL_TB,  RowJoin   = DBL_CJ,  RowRight  = DBL_RJ,
                        BotLeft   = DBL_BL,  BotFill   = DBL_TB,  BotJoin   = DBL_BJ,  BotRight  = DBL_BR
                    };

                case TableStyle.AllSingle:
                    return new TableCharSet
                    {
                        Side      = SGL_LR,  InnerSide = SGL_LR,
                        TopLeft   = SGL_TL,  TopFill   = SGL_TB,  TopJoin   = SGL_TJ,  TopRight  = SGL_TR,
                        HdrLeft   = SGL_LJ,  HdrFill   = SGL_TB,  HdrJoin   = SGL_CJ,  HdrRight  = SGL_RJ,
                        RowLeft   = SGL_LJ,  RowFill   = SGL_TB,  RowJoin   = SGL_CJ,  RowRight  = SGL_RJ,
                        BotLeft   = SGL_BL,  BotFill   = SGL_TB,  BotJoin   = SGL_BJ,  BotRight  = SGL_BR
                    };

                default: // DoubleBorderSingleInner
                    return new TableCharSet
                    {
                        Side      = DBL_LR,   InnerSide = SGL_LR,
                        TopLeft   = DBL_TL,   TopFill   = DBL_TB,  TopJoin   = MIX_DTSC, TopRight  = DBL_TR,
                        HdrLeft   = DBL_LJ,   HdrFill   = DBL_TB,  HdrJoin   = MIX_DHVC, HdrRight  = DBL_RJ,
                        RowLeft   = MIX_DLSJ, RowFill   = SGL_TB,  RowJoin   = SGL_CJ,   RowRight  = MIX_DRSJ,
                        BotLeft   = DBL_BL,   BotFill   = DBL_TB,  BotJoin   = MIX_DBSC, BotRight  = DBL_BR
                    };
            }
        }

        // Holds all border/joiner characters for a given table style
        private struct TableCharSet
        {
            public char Side, InnerSide;
            public char TopLeft, TopFill, TopJoin, TopRight;
            public char HdrLeft, HdrFill, HdrJoin, HdrRight;
            public char RowLeft, RowFill, RowJoin, RowRight;
            public char BotLeft, BotFill, BotJoin, BotRight;
        }

        #endregion
    }
}
