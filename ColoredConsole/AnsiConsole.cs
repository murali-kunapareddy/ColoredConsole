using System;
using System.Runtime.InteropServices;

namespace bcd
{
    /// <summary>
    /// Provides ANSI escape-code helpers for terminals that support 256-color and text decoration.
    /// Supported on: Windows 10 version 1511+, Linux, macOS.
    /// <para>
    /// Use <see cref="IsSupported"/> to guard calls in environments that may not support ANSI
    /// (older Windows, some CI runners, redirected output).
    /// </para>
    /// </summary>
    public static class AnsiConsole
    {
        // ── ANSI escape sequences ─────────────────────────────────────────────
        private const string ESC = "\x1B[";

        // ── Support detection ─────────────────────────────────────────────────

        private static readonly bool _supported = DetectSupport();

        /// <summary>
        /// <c>true</c> when the current terminal is likely to render ANSI escape codes correctly.
        /// Checks: output not redirected, and either a non-Windows OS or Windows 10+ with
        /// virtual terminal processing enabled (or the TERM / WT_SESSION env vars are set).
        /// </summary>
        public static bool IsSupported => _supported;

        private static bool DetectSupport()
        {
            if (Console.IsOutputRedirected) return false;

            // Non-Windows terminals generally support ANSI
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return true;

            // Windows Terminal sets WT_SESSION; VS Code terminal sets TERM_PROGRAM
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION")))    return true;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TERM_PROGRAM"))) return true;

            // Try enabling VTP via Windows API
            return TryEnableWindowsVirtualTerminal();
        }

        private static bool TryEnableWindowsVirtualTerminal()
        {
            try
            {
                // kernel32 interop — only called on Windows
                var handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
                if (!GetConsoleMode(handle, out uint mode)) return false;
                const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
                return SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
            catch { return false; }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        // ── Foreground / background (256-color palette) ───────────────────────

        /// <summary>Sets the foreground to a 256-color palette index (0–255).</summary>
        public static void SetForeground(int colorIndex)
        {
            if (!_supported) return;
            Console.Write($"{ESC}38;5;{Math.Max(0, Math.Min(255, colorIndex))}m");
        }

        /// <summary>Sets the background to a 256-color palette index (0–255).</summary>
        public static void SetBackground(int colorIndex)
        {
            if (!_supported) return;
            Console.Write($"{ESC}48;5;{Math.Max(0, Math.Min(255, colorIndex))}m");
        }

        /// <summary>Sets the foreground to a true-color RGB value.</summary>
        public static void SetForegroundRgb(byte r, byte g, byte b)
        {
            if (!_supported) return;
            Console.Write($"{ESC}38;2;{r};{g};{b}m");
        }

        /// <summary>Sets the background to a true-color RGB value.</summary>
        public static void SetBackgroundRgb(byte r, byte g, byte b)
        {
            if (!_supported) return;
            Console.Write($"{ESC}48;2;{r};{g};{b}m");
        }

        // ── Text decorations ──────────────────────────────────────────────────

        /// <summary>Enables bold text.</summary>
        public static void Bold()       { if (_supported) Console.Write($"{ESC}1m"); }

        /// <summary>Enables dim/faint text.</summary>
        public static void Dim()        { if (_supported) Console.Write($"{ESC}2m"); }

        /// <summary>Enables italic text.</summary>
        public static void Italic()     { if (_supported) Console.Write($"{ESC}3m"); }

        /// <summary>Enables underlined text.</summary>
        public static void Underline()  { if (_supported) Console.Write($"{ESC}4m"); }

        /// <summary>Enables blinking text (supported on some terminals).</summary>
        public static void Blink()      { if (_supported) Console.Write($"{ESC}5m"); }

        /// <summary>Swaps foreground and background colors.</summary>
        public static void Invert()     { if (_supported) Console.Write($"{ESC}7m"); }

        /// <summary>Enables strikethrough text.</summary>
        public static void Strikethrough() { if (_supported) Console.Write($"{ESC}9m"); }

        /// <summary>Resets all ANSI attributes (color, bold, underline, etc.) to defaults.</summary>
        public static void Reset()      { if (_supported) Console.Write($"{ESC}0m"); }

        // ── Convenience write helpers ─────────────────────────────────────────

        /// <summary>
        /// Writes text in a 256-color foreground, then resets attributes.
        /// Falls back to normal <see cref="Console.Write"/> if ANSI is not supported.
        /// </summary>
        public static void Write(string text, int fg256)
        {
            SetForeground(fg256);
            Console.Write(text);
            Reset();
        }

        /// <summary>
        /// Writes text in a true-color foreground, then resets attributes.
        /// Falls back to normal <see cref="Console.Write"/> if ANSI is not supported.
        /// </summary>
        public static void Write(string text, byte r, byte g, byte b)
        {
            SetForegroundRgb(r, g, b);
            Console.Write(text);
            Reset();
        }

        /// <summary>
        /// Writes a decorated line with optional bold and a 256-color foreground.
        /// </summary>
        public static void WriteLine(string text, int fg256, bool bold = false, bool underline = false)
        {
            if (bold)      Bold();
            if (underline) Underline();
            SetForeground(fg256);
            Console.WriteLine(text);
            Reset();
        }

        // ── Gradient helper ───────────────────────────────────────────────────

        /// <summary>
        /// Writes text with a horizontal RGB gradient from <paramref name="from"/> to <paramref name="to"/>.
        /// Each character gets an interpolated color. Requires true-color support.
        /// </summary>
        public static void WriteGradient(string text,
            (byte R, byte G, byte B) from,
            (byte R, byte G, byte B) to)
        {
            if (!_supported || string.IsNullOrEmpty(text))
            {
                Console.Write(text);
                return;
            }

            for (int i = 0; i < text.Length; i++)
            {
                double t = text.Length == 1 ? 0 : (double)i / (text.Length - 1);
                byte   r = (byte)(from.R + (to.R - from.R) * t);
                byte   g = (byte)(from.G + (to.G - from.G) * t);
                byte   b = (byte)(from.B + (to.B - from.B) * t);
                SetForegroundRgb(r, g, b);
                Console.Write(text[i]);
            }
            Reset();
        }
    }
}
