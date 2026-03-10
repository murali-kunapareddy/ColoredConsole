using System;
using System.IO;
using bcd;
using Xunit;

namespace Tests
{
    /// <summary>
    /// Unit tests for ColoredConsole. Console.Out is redirected to a StringWriter so the
    /// text content (without color) can be asserted against. Color is applied via
    /// Console.ForegroundColor which does not inject any bytes into the output stream.
    /// </summary>
    public class ColoredConsoleTests : IDisposable
    {
        private readonly StringWriter _out;
        private readonly TextWriter   _originalOut;

        public ColoredConsoleTests()
        {
            _originalOut = Console.Out;
            _out         = new StringWriter();
            Console.SetOut(_out);
        }

        public void Dispose()
        {
            Console.SetOut(_originalOut);
            _out.Dispose();
        }

        private string Output => _out.ToString();

        // ── WriteLine ─────────────────────────────────────────────────────────

        [Fact]
        public void WriteLine_BlankLine_ProducesBorderedEmptyLine()
        {
            var cc = new ColoredConsole(width: 40);
            cc.WriteLine();

            // Bordered blank line: ║ <spaces> ║ + newline
            Assert.Contains("║", Output);
        }

        [Fact]
        public void WriteLine_ShortMessage_AppearsInOutput()
        {
            var cc = new ColoredConsole(width: 40);
            cc.WriteLine("Hello");

            Assert.Contains("Hello", Output);
        }

        [Fact]
        public void WriteLine_LongMessage_WrapsAcrossMultipleLines()
        {
            var cc = new ColoredConsole(width: 30);
            // "one two three four five six" will word-wrap at the 26-char available width
            cc.WriteLine("one two three four five six seven eight nine");

            var lines = Output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.True(lines.Length >= 2, "Long message should produce at least 2 bordered lines");
        }

        [Fact]
        public void WriteLine_WordWrap_DoesNotSplitWordsMidCharacter()
        {
            var cc = new ColoredConsole(width: 30);
            cc.WriteLine("Hello World FooBar Baz");

            // "Hello" and "World" should not be split mid-word
            Assert.DoesNotContain("Hel\nlo", Output);
            Assert.DoesNotContain("Wor\nld", Output);
        }

        // ── AutoNumber ────────────────────────────────────────────────────────

        [Fact]
        public void WriteLine_AutoNumber_IncrementsCounter()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("Item A", new WriteOptions { AutoNumber = true });
            cc.WriteLine("Item B", new WriteOptions { AutoNumber = true });
            cc.WriteLine("Item C", new WriteOptions { AutoNumber = true });

            Assert.Contains("1.", Output);
            Assert.Contains("2.", Output);
            Assert.Contains("3.", Output);
        }

        [Fact]
        public void ResetAutoNumber_ResetsCounterToZero()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("First",  new WriteOptions { AutoNumber = true });
            cc.WriteLine("Second", new WriteOptions { AutoNumber = true });
            cc.ResetAutoNumber();
            cc.WriteLine("Again",  new WriteOptions { AutoNumber = true });

            // "1." appears twice (before and after reset)
            int count = 0;
            int idx   = 0;
            while ((idx = Output.IndexOf("1.", idx)) >= 0) { count++; idx++; }
            Assert.True(count >= 2);
        }

        // ── Semantic write ────────────────────────────────────────────────────

        [Fact]
        public void WriteSuccess_ContainsCheckmark()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteSuccess("Operation complete");

            Assert.Contains("✓", Output);
            Assert.Contains("Operation complete", Output);
        }

        [Fact]
        public void WriteError_ContainsCross()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteError("Something failed");

            Assert.Contains("✗", Output);
            Assert.Contains("Something failed", Output);
        }

        [Fact]
        public void WriteWarning_ContainsWarningSymbol()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteWarning("Low disk space");

            Assert.Contains("⚠", Output);
            Assert.Contains("Low disk space", Output);
        }

        [Fact]
        public void WriteInfo_ContainsInfoSymbol()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteInfo("Watching for changes");

            Assert.Contains("ℹ", Output);
            Assert.Contains("Watching for changes", Output);
        }

        // ── DrawBox ───────────────────────────────────────────────────────────

        [Fact]
        public void DrawBox_ProducesThreeLines()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawBox("Title");

            var lines = Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(3, lines.Length);
        }

        [Fact]
        public void DrawBox_TopLineStartsWithCornerChar()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawBox("Title");

            var first = Output.Split('\n')[0].TrimEnd('\r');
            Assert.StartsWith("╔", first);
            Assert.EndsWith("╗", first);
        }

        // ── DrawTopLine / DrawBottomLine ──────────────────────────────────────

        [Fact]
        public void DrawTopLine_HasCorrectWidth()
        {
            const int width = 40;
            var cc = new ColoredConsole(width: width);
            cc.DrawTopLine();

            var line = Output.Split('\n')[0].TrimEnd('\r');
            Assert.Equal(width, line.Length);
        }

        [Fact]
        public void DrawTopLine_Single_UsesCorrectCorners()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawTopLine(LineStyle.Single);

            var line = Output.Split('\n')[0].TrimEnd('\r');
            Assert.StartsWith("┌", line);
            Assert.EndsWith("┐", line);
        }

        // ── DrawSeparator ─────────────────────────────────────────────────────

        [Fact]
        public void DrawSeparator_DoubleSingle_UsesCorrectJoiners()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);

            var line = Output.Split('\n')[0].TrimEnd('\r');
            Assert.StartsWith("╟", line);
            Assert.EndsWith("╢", line);
        }

        [Fact]
        public void DrawSeparator_SingleSingle_UsesCorrectJoiners()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawSeparator(LineStyle.Single, LineStyle.Single);

            var line = Output.Split('\n')[0].TrimEnd('\r');
            Assert.StartsWith("├", line);
            Assert.EndsWith("┤", line);   // BUG was ├ on right — this verifies the fix
        }

        // ── DrawTable ─────────────────────────────────────────────────────────

        [Fact]
        public void DrawTable_WithHeaderAndTwoRows_ProducesCorrectLineCount()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                headers: new[] { "Name", "Score" },
                rows:    new[] { new[] { "Alice", "98" }, new[] { "Bob", "42" } });

            // Expected lines: top + header + header-sep + row1 + row2 + bottom = 6
            var lines = Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(6, lines.Length);
        }

        [Fact]
        public void DrawTable_WithRowSeparators_ProducesCorrectLineCount()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                headers: new[] { "A", "B" },
                rows:    new[] { new[] { "1", "2" }, new[] { "3", "4" } },
                options: new TableOptions { ShowRowSeparators = true });

            // top + header + hdr-sep + row1 + row-sep + row2 + bottom = 7
            var lines = Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(7, lines.Length);
        }

        [Fact]
        public void DrawTable_DataCells_ContainExpectedValues()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                headers: new[] { "Name", "City" },
                rows:    new[] { new[] { "Alice", "New York" }, new[] { "Bob", "London" } });

            Assert.Contains("Alice",    Output);
            Assert.Contains("New York", Output);
            Assert.Contains("London",   Output);
        }

        [Fact]
        public void DrawTable_NoHeader_OmitsHeaderRow()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                headers: new[] { "Name", "Score" },
                rows:    new[] { new[] { "Alice", "98" } },
                options: new TableOptions { ShowHeader = false });

            // top + row + bottom = 3 lines
            var lines = Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(3, lines.Length);
        }

        // ── WriteColumns ──────────────────────────────────────────────────────

        [Fact]
        public void WriteColumns_TwoValues_ProducesSingleBorderedLine()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteColumns(new[] { "Alpha", "Beta" });

            var lines = Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.Single(lines);
            Assert.Contains("Alpha", lines[0]);
            Assert.Contains("Beta",  lines[0]);
        }

        [Fact]
        public void WriteColumns_TupleOverload_FormatsAsLabelValue()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteColumns(("CPU", "72%"), ("RAM", "4 GB"));

            Assert.Contains("CPU: 72%",  Output);
            Assert.Contains("RAM: 4 GB", Output);
        }

        // ── DrawKeyValue ──────────────────────────────────────────────────────

        [Fact]
        public void DrawKeyValue_ContainsBothKeyAndValue()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawKeyValue("Host", "localhost");

            Assert.Contains("Host",      Output);
            Assert.Contains("localhost", Output);
        }

        // ── DrawList ─────────────────────────────────────────────────────────

        [Fact]
        public void DrawList_ThreeItems_ProducesThreeItemLines()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawList(null, new[] { "One", "Two", "Three" });

            Assert.Contains("One",   Output);
            Assert.Contains("Two",   Output);
            Assert.Contains("Three", Output);
        }

        [Fact]
        public void DrawList_WithHeader_RendersHeaderToo()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawList("My List", new[] { "Item" });

            Assert.Contains("My List", Output);
            Assert.Contains("Item",    Output);
        }

        // ── ASCII mode ────────────────────────────────────────────────────────

        [Fact]
        public void AsciiMode_DrawTopLine_UsesPlainChars()
        {
            var cc = new ColoredConsole(width: 40, asciiMode: true);
            cc.DrawTopLine();

            var line = Output.Split('\n')[0].TrimEnd('\r');
            Assert.StartsWith("+", line);
            Assert.EndsWith("+",   line);
            Assert.Contains("-",   line);
        }

        [Fact]
        public void SetAsciiMode_ToggleAtRuntime_Works()
        {
            var cc = new ColoredConsole(width: 40);
            cc.SetAsciiMode(true);
            cc.DrawTopLine();

            var line = Output.Split('\n')[0].TrimEnd('\r');
            Assert.StartsWith("+", line);
        }

        // ── WriteOptions fluent API ───────────────────────────────────────────

        [Fact]
        public void WriteOptions_FluentChain_BuildsCorrectly()
        {
            var opt = new WriteOptions().Cyan().Tabbed(2).Numbered();

            Assert.Equal(ConsoleColor.Cyan, opt.ForeColor);
            Assert.Equal(2,    opt.TabStop);
            Assert.True(opt.AutoNumber);
        }

        [Fact]
        public void WriteOptions_FluentChain_GreenCentered()
        {
            var opt = new WriteOptions().Green().Centered();

            Assert.Equal(ConsoleColor.Green,    opt.ForeColor);
            Assert.Equal(TextPosition.Center,   opt.TextPosition);
        }

        // ── Resize ────────────────────────────────────────────────────────────

        [Fact]
        public void Resize_ExplicitWidth_UpdatesWidth()
        {
            var cc = new ColoredConsole(width: 40);
            cc.Resize(60);

            Assert.Equal(60, cc.Width);
        }

        // ── Logging ───────────────────────────────────────────────────────────

        [Fact]
        public void EnableLogging_CustomFormatter_UsesFormatter()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                using (var cc = new ColoredConsole(width: 60))
                {
                    cc.EnableLogging(tempDir, msg => $"CUSTOM|{msg}");
                    cc.WriteLog("test message");
                }

                var logFile = Directory.GetFiles(tempDir, "*.log")[0];
                var content = File.ReadAllText(logFile);
                Assert.Contains("CUSTOM|test message", content);
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void EnableLogging_DefaultFormatter_ContainsTimestamp()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                using (var cc = new ColoredConsole(width: 60))
                {
                    cc.EnableLogging(tempDir);
                    cc.WriteLog("hello");
                }

                var logFile = Directory.GetFiles(tempDir, "*.log")[0];
                var content = File.ReadAllText(logFile);
                Assert.Contains("[", content);   // timestamp bracket
                Assert.Contains("hello", content);
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
