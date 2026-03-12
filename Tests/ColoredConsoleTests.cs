using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using bcd;
using Xunit;

// Disable parallel execution: all test classes share Console.Out (global state)
// and must not race against each other.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

// ────────────────────────────────────────────────────────────────────────────
//  ColoredConsole.NET — Full Feature Test Suite
//
//  Strategy: Console.Out is redirected to a StringWriter for every test so
//  the rendered text (without color) can be asserted against.
//  Console.IsOutputRedirected == true during tests; code paths that depend on
//  a real terminal (cursor positioning, spinner timer, progress-bar timer) are
//  therefore skipped automatically — which is correct behaviour.
// ────────────────────────────────────────────────────────────────────────────

namespace Tests
{
    // ── Shared base: redirect Console.Out ────────────────────────────────────

    public abstract class TestBase : IDisposable
    {
        private readonly TextWriter   _originalOut;
        protected readonly StringWriter Out;

        protected TestBase()
        {
            _originalOut = Console.Out;
            Out          = new StringWriter();
            Console.SetOut(Out);
        }

        protected string Output => Out.ToString();

        protected string[] Lines(bool removeEmpty = true) =>
            Output.Split('\n',
                removeEmpty ? StringSplitOptions.RemoveEmptyEntries
                            : StringSplitOptions.None);

        public void Dispose()
        {
            Console.SetOut(_originalOut);
            Out.Dispose();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  1. CONSTRUCTOR & PROPERTIES
    // ════════════════════════════════════════════════════════════════════════

    public class ConstructorAndPropertyTests : TestBase
    {
        [Fact]
        public void DefaultConstructor_SetsReasonableWidth()
        {
            // When output is redirected ResolveWidth falls back to 79
            var cc = new ColoredConsole();
            Assert.True(cc.Width >= 40);
        }

        [Fact]
        public void ExplicitWidth_SetsExactWidth()
        {
            var cc = new ColoredConsole(width: 80);
            Assert.Equal(80, cc.Width);
        }

        [Fact]
        public void DefaultTheme_IsNotNull()
        {
            var cc = new ColoredConsole();
            Assert.NotNull(cc.Theme);
        }

        [Fact]
        public void AsciiModeConstructor_True_SetsProperty()
        {
            var cc = new ColoredConsole(asciiMode: true);
            Assert.True(cc.AsciiMode);
        }

        [Fact]
        public void AsciiModeConstructor_False_SetsProperty()
        {
            var cc = new ColoredConsole(asciiMode: false);
            Assert.False(cc.AsciiMode);
        }

        [Fact]
        public void LogEnabled_FalseByDefault()
        {
            var cc = new ColoredConsole(width: 40);
            Assert.False(cc.LogEnabled);
        }

        [Fact]
        public void AutoNumberCounter_ZeroByDefault()
        {
            var cc = new ColoredConsole(width: 40);
            Assert.Equal(0, cc.AutoNumberCounter);
        }

        [Fact]
        public void Theme_DefaultPreset_HasCorrectBorderColor()
        {
            var cc = new ColoredConsole(Theme.Default, width: 40);
            Assert.Equal(ConsoleColor.Yellow, cc.Theme.LineColor);
        }

        [Fact]
        public void Theme_LightPreset_HasWhiteBackground()
        {
            var cc = new ColoredConsole(Theme.Light, width: 40);
            Assert.Equal(ConsoleColor.White, cc.Theme.BackColor);
        }

        [Fact]
        public void Theme_HackerPreset_HasGreenForeground()
        {
            var cc = new ColoredConsole(Theme.Hacker, width: 40);
            Assert.Equal(ConsoleColor.Green, cc.Theme.ForeColor);
        }

        [Fact]
        public void Theme_CustomMutation_IsReflected()
        {
            var theme = new Theme { LineColor = ConsoleColor.Magenta };
            var cc    = new ColoredConsole(theme, width: 40);
            Assert.Equal(ConsoleColor.Magenta, cc.Theme.LineColor);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  2. RESIZE & ASCII MODE TOGGLE
    // ════════════════════════════════════════════════════════════════════════

    public class ResizeAndAsciiModeTests : TestBase
    {
        [Fact]
        public void Resize_ExplicitWidth_UpdatesWidth()
        {
            var cc = new ColoredConsole(width: 40);
            cc.Resize(80);
            Assert.Equal(80, cc.Width);
        }

        [Fact]
        public void Resize_Auto_KeepsWidthReasonable()
        {
            var cc = new ColoredConsole(width: 40);
            cc.Resize(0);
            Assert.True(cc.Width >= 40);
        }

        [Fact]
        public void SetAsciiMode_True_EnablesAscii()
        {
            var cc = new ColoredConsole(width: 40);
            cc.SetAsciiMode(true);
            Assert.True(cc.AsciiMode);
        }

        [Fact]
        public void SetAsciiMode_False_DisablesAscii()
        {
            var cc = new ColoredConsole(width: 40, asciiMode: true);
            cc.SetAsciiMode(false);
            Assert.False(cc.AsciiMode);
        }

        [Fact]
        public void SetAsciiMode_TogglesAtRuntime_DrawTopLine_ChangesChars()
        {
            var cc = new ColoredConsole(width: 40);

            cc.SetAsciiMode(false);
            cc.DrawTopLine();
            string unicode = Lines()[0].TrimEnd('\r');

            Out.GetStringBuilder().Clear();
            cc.SetAsciiMode(true);
            cc.DrawTopLine();
            string ascii = Lines()[0].TrimEnd('\r');

            Assert.StartsWith("╔", unicode);
            Assert.StartsWith("+", ascii);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  3. DRAW: TOP / BOTTOM LINE
    // ════════════════════════════════════════════════════════════════════════

    public class BoxBorderLineTests : TestBase
    {
        [Fact]
        public void DrawTopLine_Double_HasCorrectWidth()
        {
            const int w = 40;
            var cc = new ColoredConsole(width: w);
            cc.DrawTopLine();
            Assert.Equal(w, Lines()[0].TrimEnd('\r').Length);
        }

        [Fact]
        public void DrawTopLine_Double_UsesDoubleCorners()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawTopLine(LineStyle.Double);
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("╔", line);
            Assert.EndsWith("╗",   line);
            Assert.Contains("═",   line);
        }

        [Fact]
        public void DrawTopLine_Single_UsesCorrectCorners()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawTopLine(LineStyle.Single);
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("┌", line);
            Assert.EndsWith("┐",   line);
            Assert.Contains("─",   line);
        }

        [Fact]
        public void DrawBottomLine_Double_UsesDoubleCorners()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawBottomLine(LineStyle.Double);
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("╚", line);
            Assert.EndsWith("╝",   line);
        }

        [Fact]
        public void DrawBottomLine_Single_UsesCorrectCorners()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawBottomLine(LineStyle.Single);
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("└", line);
            Assert.EndsWith("┘",   line);
        }

        [Fact]
        public void DrawTopLine_Ascii_UsesPlainChars()
        {
            var cc   = new ColoredConsole(width: 40, asciiMode: true);
            cc.DrawTopLine();
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("+", line);
            Assert.EndsWith("+",   line);
            Assert.Contains("-",   line);
        }

        [Fact]
        public void DrawBottomLine_Ascii_UsesPlainChars()
        {
            var cc   = new ColoredConsole(width: 40, asciiMode: true);
            cc.DrawBottomLine();
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("+", line);
            Assert.EndsWith("+",   line);
        }

        [Fact]
        public void DrawTopLine_HasExactlyOneLine()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawTopLine();
            Assert.Single(Lines());
        }

        [Fact]
        public void DrawBottomLine_HasExactlyOneLine()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawBottomLine();
            Assert.Single(Lines());
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  4. DRAW: SEPARATOR
    // ════════════════════════════════════════════════════════════════════════

    public class SeparatorTests : TestBase
    {
        [Fact]
        public void DrawSeparator_Default_ProducesOneLine()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawSeparator();
            Assert.Single(Lines());
        }

        [Fact]
        public void DrawSeparator_Default_HasCorrectWidth()
        {
            const int w = 40;
            var cc = new ColoredConsole(width: w);
            cc.DrawSeparator();
            Assert.Equal(w, Lines()[0].TrimEnd('\r').Length);
        }

        [Fact]
        public void DrawSeparator_DoubleSingle_UsesCorrectJoiners()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawSeparator(LineStyle.Double, LineStyle.Single);
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("╟", line);
            Assert.EndsWith("╢",   line);
            Assert.Contains("─",   line);
        }

        [Fact]
        public void DrawSeparator_SingleSingle_UsesCorrectJoiners()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawSeparator(LineStyle.Single, LineStyle.Single);
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("├", line);
            Assert.EndsWith("┤",   line);  // Verifies the fixed bug (was ├ on right before fix)
        }

        [Fact]
        public void DrawSeparator_DoubleDotted_UsesDots()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("║", line);
            Assert.Contains("·",   line);
        }

        [Fact]
        public void DrawSeparator_DoubleDashed_UsesDashes()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dashed);
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("║", line);
            Assert.Contains("-",   line);
        }

        [Fact]
        public void DrawSeparator_Ascii_UsesPlainChars()
        {
            var cc   = new ColoredConsole(width: 40, asciiMode: true);
            cc.DrawSeparator();
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("+", line);
            Assert.EndsWith("+",   line);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  5. DRAW: BOX
    // ════════════════════════════════════════════════════════════════════════

    public class DrawBoxTests : TestBase
    {
        [Fact]
        public void DrawBox_ProducesExactlyThreeLines()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawBox("Title");
            Assert.Equal(3, Lines().Length);
        }

        [Fact]
        public void DrawBox_TopLine_StartsWithDoubleCorner()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawBox("Title");
            var first = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("╔", first);
            Assert.EndsWith("╗",   first);
        }

        [Fact]
        public void DrawBox_BottomLine_EndsWithDoubleCorner()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawBox("Title");
            var last = Lines().Last().TrimEnd('\r');
            Assert.StartsWith("╚", last);
            Assert.EndsWith("╝",   last);
        }

        [Fact]
        public void DrawBox_ContentLine_ContainsBordersAndText()
        {
            var cc      = new ColoredConsole(width: 40);
            cc.DrawBox("Hello");
            var content = Lines()[1];
            Assert.Contains("║",       content);
            // DrawBox default is TextStyle.SpacedCaps: "Hello" → "H E L L O"
            Assert.Contains("H E L L O", content);
        }

        [Fact]
        public void DrawBox_SingleStyle_UsesCorrectCorners()
        {
            var cc   = new ColoredConsole(width: 40);
            cc.DrawBox("Title", lineStyle: LineStyle.Single);
            var first = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("┌", first);
            Assert.EndsWith("┐",   first);
        }

        [Fact]
        public void DrawBox_AllLinesHaveCorrectWidth()
        {
            const int w = 50;
            var cc = new ColoredConsole(width: w);
            cc.DrawBox("My Title");
            foreach (var line in Lines())
                Assert.Equal(w, line.TrimEnd('\r').Length);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  6. DRAW: SECTION HEADER
    // ════════════════════════════════════════════════════════════════════════

    public class SectionHeaderTests : TestBase
    {
        [Fact]
        public void DrawSectionHeader_ProducesThreeLines_BlankHeaderBlank()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawSectionHeader("My Section");
            // blank + header + blank = 3 bordered lines
            Assert.Equal(3, Lines().Length);
        }

        [Fact]
        public void DrawSectionHeader_MiddleLine_ContainsTextInCaps()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawSectionHeader("Deployments");
            // Default TextStyle.Caps → uppercased; embedded in separator fill chars
            Assert.Contains("DEPLOYMENTS", Lines()[1]);
        }

        [Fact]
        public void DrawSectionHeader_AllLines_HaveBorderChars()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawSectionHeader("Stats");
            foreach (var line in Lines())
                Assert.Contains("║", line);
        }

        [Fact]
        public void DrawSectionHeader_BlankLines_ContainNoBorderContent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawSectionHeader("Info");
            var all = Lines();
            // First and last lines are blank — they have borders but no embedded text fill chars
            Assert.DoesNotContain("─", all[0]);
            Assert.DoesNotContain("─", all[2]);
        }

        [Fact]
        public void DrawSectionHeader_WithSingleLineStyle_UsesSingleFill()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawSectionHeader("Test", lineStyle: LineStyle.Single);
            Assert.Contains("─", Lines()[1]);
        }

        [Fact]
        public void DrawSectionHeader_CenteredText_TextIsPresent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawSectionHeader("Centered", textPosition: TextPosition.Center);
            Assert.Contains("CENTERED", Lines()[1]);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  7. WRITELINE
    // ════════════════════════════════════════════════════════════════════════

    public class WriteLineTests : TestBase
    {
        [Fact]
        public void WriteLine_NoArgs_ProducesBorderedBlankLine()
        {
            var cc = new ColoredConsole(width: 40);
            cc.WriteLine();
            var line = Lines()[0];
            Assert.Contains("║", line);
        }

        [Fact]
        public void WriteLine_ShortMessage_AppearsInOutput()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("Hello World");
            Assert.Contains("Hello World", Output);
        }

        [Fact]
        public void WriteLine_ShortMessage_ProducesOneLine()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("Short");
            Assert.Single(Lines());
        }

        [Fact]
        public void WriteLine_LongMessage_WordWrapsAcrossMultipleLines()
        {
            var cc = new ColoredConsole(width: 30);
            // Available width = 26; this sentence is longer
            cc.WriteLine("one two three four five six seven eight nine ten");
            Assert.True(Lines().Length >= 2, "Long message must produce at least 2 bordered lines");
        }

        [Fact]
        public void WriteLine_WordWrap_DoesNotSplitWordsMidCharacter()
        {
            var cc = new ColoredConsole(width: 30);
            cc.WriteLine("Hello World FooBar Baz");
            Assert.DoesNotContain("Hel\nlo", Output);
            Assert.DoesNotContain("Wor\nld", Output);
        }

        [Fact]
        public void WriteLine_BlankLine_HasCorrectWidth()
        {
            const int w = 50;
            var cc = new ColoredConsole(width: w);
            cc.WriteLine();
            Assert.Equal(w, Lines()[0].TrimEnd('\r').Length);
        }

        [Fact]
        public void WriteLine_Message_LineHasCorrectWidth()
        {
            const int w = 50;
            var cc = new ColoredConsole(width: w);
            cc.WriteLine("test");
            Assert.Equal(w, Lines()[0].TrimEnd('\r').Length);
        }

        [Fact]
        public void WriteLine_WithTabStop_IndentsText()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("Indented",    new WriteOptions { TabStop = 1 });
            cc.WriteLine("NotIndented", new WriteOptions { TabStop = 0 });

            var lines = Lines();
            // Both lines should have the bordered structure; indented one has extra leading spaces
            Assert.Contains("Indented",    lines[0]);
            Assert.Contains("NotIndented", lines[1]);
        }

        [Fact]
        public void Write_DoesNotAdvanceLine_SameLineInOutput()
        {
            var cc = new ColoredConsole(width: 60);
            cc.Write("Animate me");
            // Output should contain the text (no line was fully lost)
            Assert.Contains("Animate me", Output);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  8. AUTO-NUMBERING
    // ════════════════════════════════════════════════════════════════════════

    public class AutoNumberTests : TestBase
    {
        [Fact]
        public void AutoNumber_ThreeLines_IncrementsCounter()
        {
            var cc  = new ColoredConsole(width: 60);
            var opt = new WriteOptions { AutoNumber = true };
            cc.WriteLine("Alpha", opt);
            cc.WriteLine("Beta",  opt);
            cc.WriteLine("Gamma", opt);

            Assert.Contains("1.", Output);
            Assert.Contains("2.", Output);
            Assert.Contains("3.", Output);
        }

        [Fact]
        public void AutoNumber_Counter_Property_Reflects_Count()
        {
            var cc  = new ColoredConsole(width: 60);
            var opt = new WriteOptions { AutoNumber = true };
            cc.WriteLine("A", opt);
            cc.WriteLine("B", opt);
            Assert.Equal(2, cc.AutoNumberCounter);
        }

        [Fact]
        public void ResetAutoNumber_ResetsToZero()
        {
            var cc  = new ColoredConsole(width: 60);
            var opt = new WriteOptions { AutoNumber = true };
            cc.WriteLine("First",  opt);
            cc.WriteLine("Second", opt);
            cc.ResetAutoNumber();
            Assert.Equal(0, cc.AutoNumberCounter);
        }

        [Fact]
        public void ResetAutoNumber_RestartsCounting()
        {
            var cc  = new ColoredConsole(width: 60);
            var opt = new WriteOptions { AutoNumber = true };

            cc.WriteLine("First",  opt);
            cc.WriteLine("Second", opt);
            cc.ResetAutoNumber();
            cc.WriteLine("Again",  opt);

            // "1." should appear at least twice (before and after reset)
            int count = 0, idx = 0;
            while ((idx = Output.IndexOf("1.", idx, StringComparison.Ordinal)) >= 0) { count++; idx++; }
            Assert.True(count >= 2, "Counter should restart at 1 after reset");
        }

        [Fact]
        public void AutoNumber_FluentNumbered_SameAsPropertySet()
        {
            // WriteOptions { AutoNumber = true } and .Numbered() must produce identical output
            var cc1 = new ColoredConsole(width: 60);
            var cc2 = new ColoredConsole(width: 60);

            cc1.WriteLine("Item", new WriteOptions { AutoNumber = true });
            cc2.WriteLine("Item", new WriteOptions().Numbered());

            var lines = Lines();
            Assert.Equal(2, lines.Length);
            Assert.Equal(lines[0].TrimEnd('\r'), lines[1].TrimEnd('\r'));
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  9. SEMANTIC WRITE METHODS
    // ════════════════════════════════════════════════════════════════════════

    public class SemanticWriteTests : TestBase
    {
        [Fact]
        public void WriteSuccess_ContainsCheckmarkAndMessage()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteSuccess("Build passed");
            Assert.Contains("✓",           Output);
            Assert.Contains("Build passed", Output);
        }

        [Fact]
        public void WriteError_ContainsCrossAndMessage()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteError("Connection failed");
            Assert.Contains("✗",                Output);
            Assert.Contains("Connection failed", Output);
        }

        [Fact]
        public void WriteWarning_ContainsWarningSymbolAndMessage()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteWarning("Low memory");
            Assert.Contains("⚠",          Output);
            Assert.Contains("Low memory",  Output);
        }

        [Fact]
        public void WriteInfo_ContainsInfoSymbolAndMessage()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteInfo("Server started");
            Assert.Contains("ℹ",             Output);
            Assert.Contains("Server started", Output);
        }

        [Fact]
        public void WriteSuccess_WithTabStop_IndentsCorrectly()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteSuccess("OK", tabStop: 1);
            Assert.Contains("✓", Output);
            Assert.Contains("OK", Output);
        }

        [Fact]
        public void SemanticMethods_EachProducesOneLine()
        {
            var cc = new ColoredConsole(width: 60);

            cc.WriteSuccess("s"); Assert.Single(Lines());
            Out.GetStringBuilder().Clear();

            cc.WriteError("e");   Assert.Single(Lines());
            Out.GetStringBuilder().Clear();

            cc.WriteWarning("w"); Assert.Single(Lines());
            Out.GetStringBuilder().Clear();

            cc.WriteInfo("i");    Assert.Single(Lines());
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  10. WRITE OPTIONS — FLUENT API
    // ════════════════════════════════════════════════════════════════════════

    public class WriteOptionsFluentTests : TestBase
    {
        // ── Color shorthands ─────────────────────────────────────────────────

        [Fact] public void Green_SetsGreenForeColor()
            => Assert.Equal(ConsoleColor.Green,    new WriteOptions().Green().ForeColor);

        [Fact] public void Red_SetsRedForeColor()
            => Assert.Equal(ConsoleColor.Red,      new WriteOptions().Red().ForeColor);

        [Fact] public void Yellow_SetsYellowForeColor()
            => Assert.Equal(ConsoleColor.Yellow,   new WriteOptions().Yellow().ForeColor);

        [Fact] public void Cyan_SetsCyanForeColor()
            => Assert.Equal(ConsoleColor.Cyan,     new WriteOptions().Cyan().ForeColor);

        [Fact] public void White_SetsWhiteForeColor()
            => Assert.Equal(ConsoleColor.White,    new WriteOptions().White().ForeColor);

        [Fact] public void Dimmed_SetsDarkGrayForeColor()
            => Assert.Equal(ConsoleColor.DarkGray, new WriteOptions().Dimmed().ForeColor);

        // ── Alignment shorthands ──────────────────────────────────────────────

        [Fact] public void Centered_SetsCenterPosition()
            => Assert.Equal(TextPosition.Center, new WriteOptions().Centered().TextPosition);

        [Fact] public void RightAligned_SetsRightPosition()
            => Assert.Equal(TextPosition.Right, new WriteOptions().RightAligned().TextPosition);

        [Fact] public void Aligned_SetsPosition()
            => Assert.Equal(TextPosition.Left, new WriteOptions().Aligned(TextPosition.Left).TextPosition);

        // ── Tab stop ─────────────────────────────────────────────────────────

        [Fact] public void Tabbed_SetsTabStop()
            => Assert.Equal(3, new WriteOptions().Tabbed(3).TabStop);

        // ── Numbered ─────────────────────────────────────────────────────────

        [Fact] public void Numbered_SetsAutoNumberTrue()
            => Assert.True(new WriteOptions().Numbered().AutoNumber);

        // ── Styling ──────────────────────────────────────────────────────────

        [Fact] public void Styled_SetsCapsStyle()
            => Assert.Equal(TextStyle.Caps, new WriteOptions().Styled(TextStyle.Caps).TextStyle);

        [Fact] public void Styled_SetsSpacedStyle()
            => Assert.Equal(TextStyle.Spaced, new WriteOptions().Styled(TextStyle.Spaced).TextStyle);

        // ── Direct setters ────────────────────────────────────────────────────

        [Fact] public void WithColor_SetsForeColor()
            => Assert.Equal(ConsoleColor.Magenta, new WriteOptions().WithColor(ConsoleColor.Magenta).ForeColor);

        [Fact] public void WithBack_SetsBackColor()
            => Assert.Equal(ConsoleColor.DarkBlue, new WriteOptions().WithBack(ConsoleColor.DarkBlue).BackColor);

        [Fact] public void WithBorder_SetsLineColor()
            => Assert.Equal(ConsoleColor.Gray, new WriteOptions().WithBorder(ConsoleColor.Gray).LineColor);

        // ── Chaining ─────────────────────────────────────────────────────────

        [Fact]
        public void FluentChain_AllOptionsSetCorrectly()
        {
            var opt = new WriteOptions().Cyan().Tabbed(2).Numbered().Centered();

            Assert.Equal(ConsoleColor.Cyan, opt.ForeColor);
            Assert.Equal(2,                 opt.TabStop);
            Assert.True(opt.AutoNumber);
            Assert.Equal(TextPosition.Center, opt.TextPosition);
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        [Fact]
        public void WriteOptions_CenteredText_AppearsInOutput()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("Centered", new WriteOptions().Centered());
            Assert.Contains("Centered", Output);
        }

        [Fact]
        public void WriteOptions_RightAligned_AppearsInOutput()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("Right", new WriteOptions().RightAligned());
            Assert.Contains("Right", Output);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  11. TEXT STYLE
    // ════════════════════════════════════════════════════════════════════════

    public class TextStyleTests : TestBase
    {
        [Fact]
        public void TextStyle_Caps_OutputIsUpperCase()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("hello world", new WriteOptions().Styled(TextStyle.Caps));
            Assert.Contains("HELLO WORLD", Output);
        }

        [Fact]
        public void TextStyle_None_OutputIsUnchanged()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("Hello World", new WriteOptions().Styled(TextStyle.None));
            Assert.Contains("Hello World", Output);
        }

        [Fact]
        public void TextStyle_Spaced_InsertsSpacesBetweenChars()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("AB", new WriteOptions().Styled(TextStyle.Spaced));
            Assert.Contains("A B", Output);
        }

        [Fact]
        public void TextStyle_SpacedCaps_IsUpperCaseWithSpaces()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("ab", new WriteOptions().Styled(TextStyle.SpacedCaps));
            Assert.Contains("A B", Output);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  12. LINE STYLE VARIANTS
    // ════════════════════════════════════════════════════════════════════════

    public class LineStyleTests : TestBase
    {
        [Fact]
        public void DrawTopLine_Double_UsesDoubleHorizontalFill()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawTopLine(LineStyle.Double);
            Assert.Contains("═", Lines()[0]);
        }

        [Fact]
        public void DrawTopLine_Single_UsesSingleHorizontalFill()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawTopLine(LineStyle.Single);
            Assert.Contains("─", Lines()[0]);
        }

        [Fact]
        public void DrawSeparator_Dotted_UsesDotFill()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dotted);
            Assert.Contains("·", Lines()[0]);
        }

        [Fact]
        public void DrawSeparator_Dashed_UsesDashFill()
        {
            var cc = new ColoredConsole(width: 40);
            cc.DrawSeparator(LineStyle.Double, LineStyle.Dashed);
            Assert.Contains("-", Lines()[0]);
        }

        [Fact]
        public void WriteLine_SingleLineStyle_UsesCorrectBorderChar()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("test", new WriteOptions { LineStyle = LineStyle.Single });
            Assert.Contains("│", Lines()[0]);
        }

        [Fact]
        public void WriteLine_DoubleLineStyle_UsesCorrectBorderChar()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteLine("test", new WriteOptions { LineStyle = LineStyle.Double });
            Assert.Contains("║", Lines()[0]);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  13. ASCII MODE (full rendering)
    // ════════════════════════════════════════════════════════════════════════

    public class AsciiModeRenderingTests : TestBase
    {
        [Fact]
        public void AsciiMode_DrawTopLine_UsesPlus()
        {
            var cc = new ColoredConsole(width: 40, asciiMode: true);
            cc.DrawTopLine();
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("+", line);
            Assert.EndsWith("+",   line);
        }

        [Fact]
        public void AsciiMode_DrawBottomLine_UsesPlus()
        {
            var cc = new ColoredConsole(width: 40, asciiMode: true);
            cc.DrawBottomLine();
            var line = Lines()[0].TrimEnd('\r');
            Assert.StartsWith("+", line);
            Assert.EndsWith("+",   line);
        }

        [Fact]
        public void AsciiMode_DrawSeparator_UsesDash()
        {
            var cc = new ColoredConsole(width: 40, asciiMode: true);
            cc.DrawSeparator();
            Assert.Contains("-", Lines()[0]);
        }

        [Fact]
        public void AsciiMode_WriteLine_UsesPipeForBorder()
        {
            var cc = new ColoredConsole(width: 40, asciiMode: true);
            cc.WriteLine("test");
            Assert.Contains("|", Lines()[0]);
        }

        [Fact]
        public void AsciiMode_DrawList_UsesDashBullet()
        {
            var cc = new ColoredConsole(width: 60, asciiMode: true);
            cc.DrawList(null, new[] { "Item" });
            Assert.Contains("- Item", Output);
            Assert.DoesNotContain("•",  Output);
        }

        [Fact]
        public void AsciiMode_DoesNotContainUnicodeBoxChars()
        {
            var cc = new ColoredConsole(width: 40, asciiMode: true);
            cc.DrawTopLine();
            cc.WriteLine("hello");
            cc.DrawBottomLine();
            Assert.DoesNotContain("║", Output);
            Assert.DoesNotContain("╔", Output);
            Assert.DoesNotContain("═", Output);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  14. DRAW: KEY-VALUE
    // ════════════════════════════════════════════════════════════════════════

    public class KeyValueTests : TestBase
    {
        [Fact]
        public void DrawKeyValue_ContainsBothKeyAndValue()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawKeyValue("Host", "localhost");
            Assert.Contains("Host",      Output);
            Assert.Contains("localhost", Output);
        }

        [Fact]
        public void DrawKeyValue_ContainsColonSeparator()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawKeyValue("Port", "5432");
            Assert.Contains("Port: 5432", Output);
        }

        [Fact]
        public void DrawKeyValue_ProducesOneLine()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawKeyValue("Key", "Value");
            Assert.Single(Lines());
        }

        [Fact]
        public void DrawKeyValue_HasCorrectWidth()
        {
            const int w = 60;
            var cc = new ColoredConsole(width: w);
            cc.DrawKeyValue("A", "B");
            Assert.Equal(w, Lines()[0].TrimEnd('\r').Length);
        }

        [Fact]
        public void DrawKeyValue_WithTabStop_IndentsCorrectly()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawKeyValue("Env", "production", tabStop: 1);
            Assert.Contains("Env",        Output);
            Assert.Contains("production", Output);
        }

        [Fact]
        public void DrawKeyValue_MultipleEntries_AllAppear()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawKeyValue("CPU",  "72%");
            cc.DrawKeyValue("RAM",  "4 GB");
            cc.DrawKeyValue("Disk", "88 GB");
            Assert.Contains("CPU",  Output);
            Assert.Contains("RAM",  Output);
            Assert.Contains("Disk", Output);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  15. DRAW: LIST
    // ════════════════════════════════════════════════════════════════════════

    public class DrawListTests : TestBase
    {
        [Fact]
        public void DrawList_NoHeader_RendersAllItems()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawList(null, new[] { "Alpha", "Beta", "Gamma" });
            Assert.Contains("Alpha", Output);
            Assert.Contains("Beta",  Output);
            Assert.Contains("Gamma", Output);
        }

        [Fact]
        public void DrawList_NoHeader_HasCorrectLineCount()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawList(null, new[] { "A", "B", "C" });
            Assert.Equal(3, Lines().Length);
        }

        [Fact]
        public void DrawList_NoHeader_UsesDefaultBullet()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawList(null, new[] { "Item" });
            Assert.Contains("•", Output);
        }

        [Fact]
        public void DrawList_WithHeader_HeaderAppearsInOutput()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawList("My List", new[] { "Item" });
            // DrawSectionHeader uses TextStyle.Caps → "MY LIST"
            Assert.Contains("MY LIST", Output);
        }

        [Fact]
        public void DrawList_WithHeader_ItemStillAppears()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawList("Fruits", new[] { "Apple" });
            Assert.Contains("Apple", Output);
        }

        [Fact]
        public void DrawList_WithHeader_ProducesMoreLinesThanItems()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawList("List", new[] { "One", "Two" });
            // Header = 3 lines (blank + header + blank) + 2 items = 5
            Assert.Equal(5, Lines().Length);
        }

        [Fact]
        public void DrawList_CustomBullet_UsesCustomChar()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawList(null, new[] { "Item" }, bullet: '★');
            Assert.Contains("★", Output);
        }

        [Fact]
        public void DrawList_NullItems_DoesNotThrow()
        {
            var cc = new ColoredConsole(width: 60);
            var ex = Record.Exception(() => cc.DrawList(null, null));
            Assert.Null(ex);
        }

        [Fact]
        public void DrawList_AsciiMode_UsesDashBullet()
        {
            var cc = new ColoredConsole(width: 60, asciiMode: true);
            cc.DrawList(null, new[] { "Item" });
            Assert.Contains("- Item", Output);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  16. WRITE: COLUMNS
    // ════════════════════════════════════════════════════════════════════════

    public class WriteColumnsTests : TestBase
    {
        [Fact]
        public void WriteColumns_StringArray_TwoValues_ProducesSingleLine()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteColumns(new[] { "Alpha", "Beta" });
            Assert.Single(Lines());
        }

        [Fact]
        public void WriteColumns_StringArray_BothValuesPresent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteColumns(new[] { "Alpha", "Beta" });
            Assert.Contains("Alpha", Lines()[0]);
            Assert.Contains("Beta",  Lines()[0]);
        }

        [Fact]
        public void WriteColumns_ThreeValues_AllPresent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteColumns(new[] { "CPU", "RAM", "Disk" });
            Assert.Contains("CPU",  Output);
            Assert.Contains("RAM",  Output);
            Assert.Contains("Disk", Output);
        }

        [Fact]
        public void WriteColumns_TupleOverload_FormatsAsLabelColon()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteColumns(("CPU", "72%"), ("RAM", "4 GB"));
            Assert.Contains("CPU: 72%",  Output);
            Assert.Contains("RAM: 4 GB", Output);
        }

        [Fact]
        public void WriteColumns_TupleOverload_ProducesSingleLine()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteColumns(("A", "1"), ("B", "2"), ("C", "3"));
            Assert.Single(Lines());
        }

        [Fact]
        public void WriteColumns_WithAlignments_ValuesStillPresent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteColumns(
                new[] { "Left", "Center", "Right" },
                new[] { TextPosition.Left, TextPosition.Center, TextPosition.Right });
            Assert.Contains("Left",   Output);
            Assert.Contains("Center", Output);
            Assert.Contains("Right",  Output);
        }

        [Fact]
        public void WriteColumns_WithForeColors_ValuesStillPresent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.WriteColumns(
                new[] { "Name", "Status" },
                null,
                new[] { ConsoleColor.White, ConsoleColor.Green });
            Assert.Contains("Name",   Output);
            Assert.Contains("Status", Output);
        }

        [Fact]
        public void WriteColumns_LineLength_IsExactlyBoxWidth()
        {
            // The remainder from integer column-width division is added to the last column,
            // so the rendered line is always exactly _width characters wide.
            const int w = 60;
            var cc = new ColoredConsole(width: w);
            cc.WriteColumns(new[] { "A", "B" });
            Assert.Equal(w, Lines()[0].TrimEnd('\r').Length);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  17. DRAW: TABLE
    // ════════════════════════════════════════════════════════════════════════

    public class DrawTableTests : TestBase
    {
        // ── Line counts ────────────────────────────────────────────────────

        [Fact]
        public void DrawTable_DefaultStyle_HeaderPlusTwoRows_SixLines()
        {
            // top + header + hdr-sep + row1 + row2 + bottom = 6
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "Name", "Score" },
                new[] { new[] { "Alice", "98" }, new[] { "Bob", "42" } });
            Assert.Equal(6, Lines().Length);
        }

        [Fact]
        public void DrawTable_WithRowSeparators_CorrectLineCount()
        {
            // top + header + hdr-sep + row1 + row-sep + row2 + bottom = 7
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "A", "B" },
                new[] { new[] { "1", "2" }, new[] { "3", "4" } },
                new TableOptions { ShowRowSeparators = true });
            Assert.Equal(7, Lines().Length);
        }

        [Fact]
        public void DrawTable_NoHeader_ThreeLines()
        {
            // top + row + bottom = 3
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "Name", "Score" },
                new[] { new[] { "Alice", "98" } },
                new TableOptions { ShowHeader = false });
            Assert.Equal(3, Lines().Length);
        }

        // ── Data content ───────────────────────────────────────────────────

        [Fact]
        public void DrawTable_DataCells_ContainExpectedValues()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "Name", "City" },
                new[] { new[] { "Alice", "New York" }, new[] { "Bob", "London" } });
            Assert.Contains("Alice",    Output);
            Assert.Contains("New York", Output);
            Assert.Contains("London",   Output);
        }

        [Fact]
        public void DrawTable_Headers_AppearInOutput()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "Name", "Role", "Status" },
                new[] { new[] { "Alice", "Dev", "Active" } });
            Assert.Contains("Name",   Output);
            Assert.Contains("Role",   Output);
            Assert.Contains("Status", Output);
        }

        // ── Outer border wrapping ──────────────────────────────────────────

        [Fact]
        public void DrawTable_EveryLine_StartsWithOuterBorderChar()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "Col1", "Col2" },
                new[] { new[] { "A", "B" } });
            foreach (var line in Lines())
                Assert.StartsWith("║", line.TrimEnd('\r'));
        }

        [Fact]
        public void DrawTable_EveryLine_EndsWithOuterBorderChar()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "Col1", "Col2" },
                new[] { new[] { "A", "B" } });
            foreach (var line in Lines())
                Assert.EndsWith("║", line.TrimEnd('\r'));
        }

        [Fact]
        public void DrawTable_EveryLine_HasCorrectTotalWidth()
        {
            const int w = 60;
            var cc = new ColoredConsole(width: w);
            cc.DrawTable(
                new[] { "Name", "Score" },
                new[] { new[] { "Alice", "98" } });
            foreach (var line in Lines())
                Assert.Equal(w, line.TrimEnd('\r').Length);
        }

        // ── Table styles ───────────────────────────────────────────────────

        [Fact]
        public void DrawTable_AllSingle_TopLineUsesLightCorner()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "A", "B" },
                new[] { new[] { "1", "2" } },
                new TableOptions { Style = TableStyle.AllSingle });
            // After the outer ║ and 2-space indent, the table top-left should be ┌
            Assert.Contains("┌", Lines()[0]);
        }

        [Fact]
        public void DrawTable_AllDouble_TopLineUsesDoubleCorner()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "A", "B" },
                new[] { new[] { "1", "2" } },
                new TableOptions { Style = TableStyle.AllDouble });
            Assert.Contains("╔", Lines()[0]);
        }

        [Fact]
        public void DrawTable_DefaultStyle_TopLineUsesDoubleCorner()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "A", "B" },
                new[] { new[] { "1", "2" } });
            Assert.Contains("╔", Lines()[0]);
        }

        // ── Column alignment ───────────────────────────────────────────────

        [Fact]
        public void DrawTable_ColumnAlignments_DataStillPresent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "Name", "Score" },
                new[] { new[] { "Alice", "98" } },
                new TableOptions
                {
                    ColumnAlignments = new[] { TextPosition.Left, TextPosition.Right }
                });
            Assert.Contains("Alice", Output);
            Assert.Contains("98",    Output);
        }

        [Fact]
        public void DrawTable_ExplicitColumnWidths_DataStillPresent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "Name", "Role" },
                new[] { new[] { "Bob", "Designer" } },
                new TableOptions { ColumnWidths = new[] { 10, 12 } });
            Assert.Contains("Bob",      Output);
            Assert.Contains("Designer", Output);
        }

        // ── Table alignment ────────────────────────────────────────────────

        [Fact]
        public void DrawTable_LeftAlignment_Default_HasTwoSpaceIndent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "X" },
                new[] { new[] { "1" } });
            // First line: ║ + 2 spaces + table top-left char
            var top = Lines()[0].TrimEnd('\r');
            Assert.Equal("║  ", top.Substring(0, 3));
        }

        [Fact]
        public void DrawTable_RightAlignment_DataPresent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "X" },
                new[] { new[] { "1" } },
                new TableOptions { Alignment = TableAlignment.Right });
            Assert.Contains("1", Output);
        }

        [Fact]
        public void DrawTable_CenterAlignment_DataPresent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "Name" },
                new[] { new[] { "Alice" } },
                new TableOptions { Alignment = TableAlignment.Center });
            Assert.Contains("Alice", Output);
        }

        [Fact]
        public void DrawTable_JustifiedAlignment_DataPresent()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTable(
                new[] { "Name", "Role" },
                new[] { new[] { "Alice", "Dev" } },
                new TableOptions { Alignment = TableAlignment.Justified });
            Assert.Contains("Alice", Output);
            Assert.Contains("Dev",   Output);
        }

        [Fact]
        public void DrawTable_CenterAlignment_EveryLine_HasCorrectWidth()
        {
            const int w = 60;
            var cc = new ColoredConsole(width: w);
            cc.DrawTable(
                new[] { "A", "B" },
                new[] { new[] { "1", "2" } },
                new TableOptions { Alignment = TableAlignment.Center });
            foreach (var line in Lines())
                Assert.Equal(w, line.TrimEnd('\r').Length);
        }

        [Fact]
        public void DrawTable_JustifiedAlignment_EveryLine_HasCorrectWidth()
        {
            const int w = 60;
            var cc = new ColoredConsole(width: w);
            cc.DrawTable(
                new[] { "Name", "Role" },
                new[] { new[] { "Alice", "Dev" } },
                new TableOptions { Alignment = TableAlignment.Justified });
            foreach (var line in Lines())
                Assert.Equal(w, line.TrimEnd('\r').Length);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  18. THEME INTEGRATION
    // ════════════════════════════════════════════════════════════════════════

    public class ThemeIntegrationTests : TestBase
    {
        [Fact]
        public void DefaultTheme_DrawTopLine_ProducesDoubleCorner()
        {
            var cc = new ColoredConsole(Theme.Default, width: 40);
            cc.DrawTopLine();
            Assert.StartsWith("╔", Lines()[0].TrimEnd('\r'));
        }

        [Fact]
        public void HackerTheme_DrawTopLine_ProducesDoubleCorner()
        {
            // Hacker theme uses same LineStyle.Double default
            var cc = new ColoredConsole(Theme.Hacker, width: 40);
            cc.DrawTopLine();
            Assert.StartsWith("╔", Lines()[0].TrimEnd('\r'));
        }

        [Fact]
        public void LightTheme_WriteLine_ProducesOutput()
        {
            var cc = new ColoredConsole(Theme.Light, width: 60);
            cc.WriteLine("Light theme text");
            Assert.Contains("Light theme text", Output);
        }

        [Fact]
        public void HackerTheme_WriteSuccess_ContainsCheckmark()
        {
            var cc = new ColoredConsole(Theme.Hacker, width: 60);
            cc.WriteSuccess("All systems go");
            Assert.Contains("✓",              Output);
            Assert.Contains("All systems go", Output);
        }

        [Fact]
        public void DefaultTheme_SuccessColor_IsGreen()
        {
            Assert.Equal(ConsoleColor.Green, Theme.Default.SuccessColor);
        }

        [Fact]
        public void DefaultTheme_ErrorColor_IsRed()
        {
            Assert.Equal(ConsoleColor.Red, Theme.Default.ErrorColor);
        }

        [Fact]
        public void DefaultTheme_WarningColor_IsDarkYellow()
        {
            Assert.Equal(ConsoleColor.DarkYellow, Theme.Default.WarningColor);
        }

        [Fact]
        public void DefaultTheme_InfoColor_IsCyan()
        {
            Assert.Equal(ConsoleColor.Cyan, Theme.Default.InfoColor);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  19. ASYNC WRITE METHODS
    // ════════════════════════════════════════════════════════════════════════

    public class AsyncWriteTests : TestBase
    {
        [Fact]
        public async Task WriteLineAsync_ProducesOutput()
        {
            var cc = new ColoredConsole(width: 60);
            await cc.WriteLineAsync("Async line");
            Assert.Contains("Async line", Output);
        }

        [Fact]
        public async Task WriteLineAsync_WithOptions_ProducesOutput()
        {
            var cc = new ColoredConsole(width: 60);
            await cc.WriteLineAsync("Async colored", new WriteOptions().Cyan());
            Assert.Contains("Async colored", Output);
        }

        [Fact]
        public async Task WriteAsync_ProducesOutput()
        {
            var cc = new ColoredConsole(width: 60);
            await cc.WriteAsync("Async write");
            Assert.Contains("Async write", Output);
        }

        [Fact]
        public async Task WriteLineAsync_MultipleAwaited_AllLinesPresent()
        {
            var cc = new ColoredConsole(width: 60);
            await cc.WriteLineAsync("Line one");
            await cc.WriteLineAsync("Line two");
            await cc.WriteLineAsync("Line three");
            Assert.Contains("Line one",   Output);
            Assert.Contains("Line two",   Output);
            Assert.Contains("Line three", Output);
        }

        [Fact]
        public async Task WriteLineAsync_ProducesCorrectLineCount()
        {
            var cc = new ColoredConsole(width: 60);
            await cc.WriteLineAsync("A");
            await cc.WriteLineAsync("B");
            Assert.Equal(2, Lines().Length);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  20. LOGGING
    // ════════════════════════════════════════════════════════════════════════

    public class LoggingTests : TestBase
    {
        [Fact]
        public void EnableLogging_LogEnabled_BecomesTrue()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                using var cc = new ColoredConsole(width: 60);
                cc.EnableLogging(tempDir);
                Assert.True(cc.LogEnabled);
            }
            finally { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); }
        }

        [Fact]
        public void EnableLogging_CreatesLogFile()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                using (var cc = new ColoredConsole(width: 60))
                    cc.EnableLogging(tempDir);
                Assert.True(Directory.GetFiles(tempDir, "*.log").Length > 0);
            }
            finally { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); }
        }

        [Fact]
        public void WriteLog_DefaultFormatter_ContainsTimestampBracket()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                using (var cc = new ColoredConsole(width: 60))
                {
                    cc.EnableLogging(tempDir);
                    cc.WriteLog("ping");
                }
                var content = File.ReadAllText(Directory.GetFiles(tempDir, "*.log")[0]);
                Assert.Contains("[",    content);
                Assert.Contains("ping", content);
            }
            finally { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); }
        }

        [Fact]
        public void WriteLog_CustomFormatter_AppliesFormatter()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                using (var cc = new ColoredConsole(width: 60))
                {
                    cc.EnableLogging(tempDir, msg => $"CUSTOM|{msg}");
                    cc.WriteLog("test");
                }
                var content = File.ReadAllText(Directory.GetFiles(tempDir, "*.log")[0]);
                Assert.Contains("CUSTOM|test", content);
            }
            finally { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); }
        }

        [Fact]
        public void WriteLog_WithoutEnableLogging_DoesNotThrow()
        {
            var cc = new ColoredConsole(width: 60);
            var ex = Record.Exception(() => cc.WriteLog("orphan"));
            Assert.Null(ex);
        }

        [Fact]
        public void LogFile_AppendMode_PreservesExistingContent()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                using (var cc = new ColoredConsole(width: 60))
                {
                    cc.EnableLogging(tempDir);
                    cc.WriteLog("first");
                }
                using (var cc = new ColoredConsole(width: 60))
                {
                    cc.EnableLogging(tempDir);
                    cc.WriteLog("second");
                }
                var content = File.ReadAllText(Directory.GetFiles(tempDir, "*.log")[0]);
                Assert.Contains("first",  content);
                Assert.Contains("second", content);
            }
            finally { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); }
        }

        [Fact]
        public void Dispose_ClosesLogFile()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                var cc = new ColoredConsole(width: 60);
                cc.EnableLogging(tempDir);
                cc.Dispose();
                Assert.False(cc.LogEnabled);
            }
            finally { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  21. IDISPOSABLE
    // ════════════════════════════════════════════════════════════════════════

    public class DisposableTests : TestBase
    {
        [Fact]
        public void Using_Block_DisposesCleanly()
        {
            var ex = Record.Exception(() =>
            {
                using var cc = new ColoredConsole(width: 40);
                cc.WriteLine("Inside using block");
            });
            Assert.Null(ex);
        }

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var cc = new ColoredConsole(width: 40);
            var ex = Record.Exception(() =>
            {
                cc.Dispose();
                cc.Dispose();
            });
            Assert.Null(ex);
        }

        [Fact]
        public void Dispose_LogEnabled_ReturnsFalse()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                var cc = new ColoredConsole(width: 60);
                cc.EnableLogging(tempDir);
                Assert.True(cc.LogEnabled);
                cc.Dispose();
                Assert.False(cc.LogEnabled);
            }
            finally { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  22. INTEGRATION — FULL WORKFLOW
    // ════════════════════════════════════════════════════════════════════════

    public class IntegrationTests : TestBase
    {
        [Fact]
        public void FullBox_TopContentSeparatorContentBottom_ProducesFiveLines()
        {
            var cc = new ColoredConsole(width: 60);
            cc.DrawTopLine();
            cc.WriteLine("Header text");
            cc.DrawSeparator();
            cc.WriteLine("Body text");
            cc.DrawBottomLine();
            Assert.Equal(5, Lines().Length);
        }

        [Fact]
        public void FullWorkflow_AllOutputContainsExpectedText()
        {
            var cc = new ColoredConsole(width: 70);
            cc.DrawBox("Dashboard");
            cc.DrawSectionHeader("Status");
            cc.WriteSuccess("Service running");
            cc.WriteError("DB timeout");
            cc.WriteWarning("High CPU");
            cc.WriteInfo("Version 2.1");
            cc.DrawKeyValue("Uptime", "99.9%");
            cc.DrawList(null, new[] { "Task A", "Task B" });
            cc.DrawBottomLine();

            Assert.Contains("✓",              Output);
            Assert.Contains("Service running", Output);
            Assert.Contains("✗",              Output);
            Assert.Contains("DB timeout",      Output);
            Assert.Contains("⚠",              Output);
            Assert.Contains("High CPU",        Output);
            Assert.Contains("ℹ",              Output);
            Assert.Contains("Version 2.1",     Output);
            Assert.Contains("Uptime",          Output);
            Assert.Contains("99.9%",           Output);
            Assert.Contains("Task A",          Output);
            Assert.Contains("Task B",          Output);
        }

        [Fact]
        public void AllLines_InFullWorkflow_HaveConsistentWidth()
        {
            const int w = 60;
            var cc = new ColoredConsole(width: w);
            cc.DrawTopLine();
            cc.WriteLine("Line one");
            cc.DrawSeparator();
            cc.WriteLine("Line two");
            cc.DrawBottomLine();

            foreach (var line in Lines())
                Assert.Equal(w, line.TrimEnd('\r').Length);
        }

        [Fact]
        public void AutoNumber_ResetAndRestart_InContextOfFullOutput()
        {
            var cc  = new ColoredConsole(width: 60);
            var opt = new WriteOptions { AutoNumber = true };

            cc.DrawSectionHeader("Steps");
            cc.WriteLine("Install",   opt);
            cc.WriteLine("Configure", opt);
            cc.WriteLine("Deploy",    opt);
            cc.ResetAutoNumber();

            cc.DrawSectionHeader("Cleanup");
            cc.WriteLine("Rollback",  opt);

            Assert.Contains("1.", Output);
            Assert.Contains("2.", Output);
            Assert.Contains("3.", Output);
            Assert.Contains("Install",   Output);
            Assert.Contains("Rollback",  Output);
        }

        [Fact]
        public void NestedThemes_MultipleInstances_RenderIndependently()
        {
            var ccDefault = new ColoredConsole(Theme.Default, width: 60);
            var ccHacker  = new ColoredConsole(Theme.Hacker,  width: 60);

            ccDefault.DrawTopLine();
            ccHacker.DrawTopLine();

            // Both produce ╔ because both use LineStyle.Double by default
            var lines = Lines();
            Assert.All(lines, l => Assert.StartsWith("╔", l.TrimEnd('\r')));
        }
    }
}
