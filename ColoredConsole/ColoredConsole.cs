using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Bcd
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

        #endregion

        #region ──── FIELDS ────

        private readonly Theme       _theme;
        private readonly int         _width;
        private readonly int         _availableWidth;  // _width - 4 (2 border chars + 2 padding spaces)
        private          int         _autoNumber;
        private          StreamWriter _logWriter;
        private readonly object      _logLock = new object();
        private          bool        _disposed;

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
        public ColoredConsole(Theme theme = null, int width = 0)
        {
            _theme          = theme ?? Theme.Default;
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
        public void EnableLogging(string folder = "Log")
        {
            lock (_logLock)
            {
                _logWriter?.Dispose();

                var logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);
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
                _logWriter?.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
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
            Console.WriteLine(ls == LineStyle.Single
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
            Console.WriteLine(ls == LineStyle.Single
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
                int row = Console.IsOutputRedirected ? 0 : Console.CursorTop;
                var lr  = BorderChar(ls);
                ApplyColors(bc, lc);
                Console.Write($"{lr} ");
                Console.ForegroundColor = fc;
                Console.Write(FormatMessage(msg, tp, tab, ts));
                Console.ForegroundColor = lc;
                Console.WriteLine($" {lr}");
                Console.ResetColor();
                // Reposition to start of the written line for in-place overwrite
                if (!Console.IsOutputRedirected)
                    Console.SetCursorPosition(0, row);
            }
            else
            {
                foreach (var chunk in WordWrap(msg, _availableWidth))
                    RenderLine(chunk, ls, tp, tab, false, ts, bc, fc, lc);
            }
        }

        /// <summary>Renders a bordered line followed by a newline.</summary>
        private void RenderLine(string msg, LineStyle ls, TextPosition tp, int tab, bool an,
            TextStyle ts, ConsoleColor bc, ConsoleColor fc, ConsoleColor lc)
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
                Console.ForegroundColor = fc;
                Console.Write(FormatMessage(msg, tp, tab, ts));
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
                    RenderLine(chunk, ls, tp, first ? tab : tab + 1, false, ts, bc, fc, lc);
                    first = false;
                }
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

        private static char BorderChar(LineStyle ls)
            => ls == LineStyle.Double ? DBL_LR : SGL_LR;

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
    }
}
