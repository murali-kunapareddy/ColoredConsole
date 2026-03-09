using System;
using System.Threading;
using Bcd;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ColoredConsole is IDisposable — using block flushes and closes the log file
            using (var cc = new ColoredConsole())
            {
                cc.EnableLogging("Log");

                // ── Title box ─────────────────────────────────────────────────
                cc.DrawBox("ColoredConsole.NET — Demo v2.0");

                // ── Basic box structure ────────────────────────────────────────
                cc.DrawTopLine();
                cc.WriteLine("Header");
                cc.DrawSeparator();
                cc.WriteLine("This is body text.");

                // ── Section header with auto-numbering ─────────────────────────
                cc.DrawSectionHeader("Numbered List");
                cc.WriteLine("First item.", new WriteOptions { AutoNumber = true });
                cc.WriteLine("Second item is intentionally longer to demonstrate the word-aware text wrapping over multiple lines.", new WriteOptions { AutoNumber = true });
                cc.WriteLine("Third item.", new WriteOptions { AutoNumber = true });

                // ── Prompt ────────────────────────────────────────────────────
                cc.DrawSeparator();
                cc.WriteLine();
                var name = cc.Prompt("What is your name?");
                cc.WriteLine($"Hello, {name}!", new WriteOptions { ForeColor = ConsoleColor.Green });

                // ── Progress bar (uses the same cc instance for consistent width/theme) ──
                cc.DrawSeparator(LineStyle.Double, LineStyle.Single);
                cc.WriteLine("Running a task...");

                using (var pb = new ProgressBar(cc))
                {
                    for (int i = 0; i <= 10; i++)
                    {
                        pb.Report((double)i / 10);
                        Thread.Sleep(400);
                    }
                }

                // ── Separator styles ──────────────────────────────────────────
                cc.DrawSeparator(LineStyle.Double, LineStyle.Double);
                var answer = cc.Prompt("Did the progress bar display correctly?",
                    new WriteOptions { ForeColor = ConsoleColor.Cyan });
                cc.WriteLine(answer);

                // ── Hacker theme demo ─────────────────────────────────────────
                cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
                cc.DrawSectionHeader("Themed Output", foreColor: ConsoleColor.DarkGreen);

                using (var hacker = new ColoredConsole(Theme.Hacker))
                {
                    hacker.DrawTopLine();
                    hacker.WriteLine("Hacker Theme", new WriteOptions { TextStyle = TextStyle.SpacedCaps, TextPosition = TextPosition.Center });
                    hacker.DrawSeparator();
                    hacker.WriteLine("Green text · cyan borders · black background.");
                    hacker.DrawBottomLine();
                }

                // ── Footer ────────────────────────────────────────────────────
                cc.DrawSeparator(LineStyle.Double, LineStyle.Dashed);
                cc.WriteLine("Footer", new WriteOptions { ForeColor = ConsoleColor.DarkGray });
                cc.DrawBottomLine();
            }
        }
    }
}
