using System;
using System.Threading;

// Adapted from Daniel Wolf's progress bar
// https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54

namespace bcd
{
    /// <summary>
    /// A thread-safe, animated ASCII progress bar that renders inside a
    /// <see cref="ColoredConsole"/> bordered box.
    /// <para>Always use in a <c>using</c> block to ensure the final 100% state is rendered.</para>
    /// </summary>
    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int    BlockCount = 50;
        private const string Animation  = @"|/-\";
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1.0 / 8);

        private readonly ColoredConsole _cc;
        private readonly WriteOptions   _options;
        private readonly Timer          _timer;
        private readonly object         _lock = new object();

        private long _progressBits;   // double stored as long for Interlocked support
        private int  _animIndex;
        private bool _disposed;

        /// <summary>
        /// Creates an animated progress bar that renders using the given <see cref="ColoredConsole"/>.
        /// </summary>
        /// <param name="cc">
        /// The console instance to render into. Creates a default instance if <c>null</c>,
        /// but passing your existing instance ensures consistent width and theme.
        /// </param>
        /// <param name="options">
        /// Render options for the progress text. Defaults to <c>tabStop = 1, foreColor = Blue</c>.
        /// </param>
        public ProgressBar(ColoredConsole cc = null, WriteOptions options = null)
        {
            _cc = cc ?? new ColoredConsole();
            _options = options ?? new WriteOptions
            {
                TabStop   = 1,
                ForeColor = ConsoleColor.Blue
            };

            if (!Console.IsOutputRedirected)
            {
                Console.CursorVisible = false;
                _timer = new Timer(OnTick);
                ResetTimer();
            }
        }

        /// <summary>
        /// Reports progress in the range [0.0, 1.0]. Thread-safe; safe to call from any thread.
        /// </summary>
        public void Report(double value)
        {
            value = Math.Max(0.0, Math.Min(1.0, value));
            Interlocked.Exchange(ref _progressBits, BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Stops the animation, renders the final 100% bar, and restores the cursor.
        /// </summary>
        public void Dispose()
        {
            if (_timer == null)
            {
                // Output was redirected at construction — no timer was started
                _disposed = true;
                return;
            }

            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;

                // Stop future callbacks before rendering the final state
                _timer.Change(Timeout.Infinite, Timeout.Infinite);

                _cc.WriteLine(
                    $"[{new string('#', BlockCount)}] 100%  ",
                    new WriteOptions
                    {
                        TabStop   = _options.TabStop,
                        ForeColor = ConsoleColor.Green
                    });

                if (!Console.IsOutputRedirected)
                    Console.CursorVisible = true;

                _timer.Dispose();
            }
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void OnTick(object _)
        {
            lock (_lock)
            {
                if (_disposed) return;

                double progress = BitConverter.Int64BitsToDouble(
                    Interlocked.Read(ref _progressBits));

                int filled  = (int)(progress * BlockCount);
                int percent = (int)(progress * 100);
                string text = $"[{new string('#', filled)}{new string('-', BlockCount - filled)}]"
                            + $" {percent,3}% {Animation[_animIndex++ % Animation.Length]}";

                // Write() (via RenderWrite) already repositions the cursor to the
                // start of the line it just wrote — no manual SetCursorPosition needed.
                _cc.Write(text, _options);

                ResetTimer();
            }
        }

        private void ResetTimer()
            => _timer?.Change(Interval, TimeSpan.FromMilliseconds(-1));
    }
}
