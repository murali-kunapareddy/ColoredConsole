using System;
using System.Threading;

namespace bcd
{
    /// <summary>
    /// An animated indeterminate progress spinner that renders inside a
    /// <see cref="ColoredConsole"/> bordered box. Use when task duration is unknown.
    /// <para>Always wrap in a <c>using</c> block, or call <see cref="Complete"/> explicitly.</para>
    /// </summary>
    public class Spinner : IDisposable
    {
        // Braille dot animation — smooth and compact
        private const string Frames  = "⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏";
        private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(80);

        private readonly ColoredConsole _cc;
        private readonly WriteOptions   _options;
        private readonly Timer          _timer;
        private readonly object         _lock = new object();

        private string  _message;
        private int     _frameIndex;
        private bool    _disposed;

        /// <summary>
        /// Creates an animated spinner that renders using the given <see cref="ColoredConsole"/>.
        /// </summary>
        /// <param name="cc">Console instance. Creates a default instance if <c>null</c>.</param>
        /// <param name="message">Initial status message displayed beside the spinner.</param>
        /// <param name="options">Render options. Defaults to <c>foreColor = Yellow, tabStop = 1</c>.</param>
        public Spinner(ColoredConsole cc = null, string message = "Working...", WriteOptions options = null)
        {
            _cc      = cc ?? new ColoredConsole();
            _message = message ?? string.Empty;
            _options = options ?? new WriteOptions
            {
                ForeColor = ConsoleColor.Yellow,
                TabStop   = 1
            };

            if (!Console.IsOutputRedirected)
            {
                Console.CursorVisible = false;
                _timer = new Timer(OnTick);
                ResetTimer();
            }
        }

        /// <summary>Updates the status message displayed beside the spinner (thread-safe).</summary>
        public void UpdateMessage(string message)
        {
            lock (_lock) { _message = message ?? string.Empty; }
        }

        /// <summary>
        /// Stops the spinner and renders a final completion line.
        /// </summary>
        /// <param name="message">Completion message. Defaults to "Done" or "Failed".</param>
        /// <param name="success">
        /// <c>true</c> renders a green ✓; <c>false</c> renders a red ✗.
        /// </param>
        public void Complete(string message = null, bool success = true)
        {
            if (_timer == null) { _disposed = true; return; }

            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;

                _timer.Change(Timeout.Infinite, Timeout.Infinite);

                var symbol = success ? "✓" : "✗";
                var color  = success ? ConsoleColor.Green : ConsoleColor.Red;
                var text   = message ?? (success ? "Done" : "Failed");

                _cc.WriteLine($"{symbol} {text}", new WriteOptions
                {
                    ForeColor = color,
                    TabStop   = _options.TabStop
                });

                if (!Console.IsOutputRedirected)
                    Console.CursorVisible = true;

                _timer.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose() => Complete();

        // ── Private ──────────────────────────────────────────────────────────

        private void OnTick(object _)
        {
            lock (_lock)
            {
                if (_disposed) return;

                string msg   = _message;
                char   frame = Frames[_frameIndex++ % Frames.Length];

                // Write() (via RenderWrite) repositions the cursor to the start of the
                // line it just wrote — no manual SetCursorPosition needed.
                _cc.Write($"{frame} {msg}", _options);

                ResetTimer();
            }
        }

        private void ResetTimer()
            => _timer?.Change(Interval, TimeSpan.FromMilliseconds(-1));
    }
}
