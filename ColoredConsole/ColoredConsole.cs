using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace bcd
{
    public class ColoredConsole
    {
        static readonly char DOT_TB = '·';  // 250 - top & bottom interpunct
        static readonly char DSH_TB = '-';  //     - top & bottom interpunct
        //
        static readonly char DBL_TL = '╔';  // double top left
        static readonly char DBL_TR = '╗';  // double top right
        static readonly char DBL_LR = '║';  // 186 - double left & right
        static readonly char DBL_LJ = '╠';  // double left joiner
        static readonly char DBL_RJ = '╣';  // double right joiner
        static readonly char DBL_TB = '═';  // double top & bottom
        static readonly char DBL_TJ = '╦';  // double top joiner
        static readonly char DBL_BJ = '╩';  // double bottom joiner
        static readonly char DBL_CJ = '╬';  // double cross joiner
        static readonly char DBL_BL = '╚';  // double bottom left
        static readonly char DBL_BR = '╝';  // double bottom right
        //
        static readonly char SGL_TL = '┌';  // 218
        static readonly char SGL_TR = '┐';
        static readonly char SGL_LR = '│';
        static readonly char SGL_LJ = '├';
        static readonly char SGL_RJ = '┤';
        static readonly char SGL_TB = '─';   //196
        static readonly char SGL_TJ = '┬';
        static readonly char SGL_BJ = '┴';
        static readonly char SGL_CJ = '┼';   //197
        static readonly char SGL_BL = '└';
        static readonly char SGL_BR = '┘';   //217
        //
        static readonly char MIX_DTSJ = '╤'; //209  double top single joiner
        static readonly char MIX_DBSJ = '╧'; //207  double bottom single joiner
        static readonly char MIX_DLSJ = '╟'; //199  double left single joiner
        static readonly char MIX_DRSJ = '╢'; //182  double right single joiner
        static readonly char MIX_STDJ = '╥'; //210  single top double joiner
        static readonly char MIX_SBDJ = '╨'; //208  single bottom double joiner
        static readonly char MIX_SLDJ = '╞'; //198  single left double joiner
        static readonly char MIX_SRDJ = '╡'; //181  single right double joiner

        public int DefaultWidth { get; set; }
        public LineStyle DefaultLineStyle { get; set; }
        public LineStyle DefaultVerticalLineStyle { get; set; }
        public LineStyle DefaultHorizontalLineStyle { get; set; }
        public TextPosition DefaultTextPosition { get; set; }
        public TextStyle DefaultTextStyle { get; set; }
        public ConsoleColor DefaultBackColor { get; set; }
        public ConsoleColor DefaultForeColor { get; set; }
        public ConsoleColor DefaultLineColor { get; set; }
        public int AutoNumberCounter { get; set; } = 0;
        public bool LogEnable { get; set; }
        public string LogFolder { get; set; } = "Log";
        public bool LogEmail { get; set; }
        private int AvailableWidth { get; set; }
        private StringBuilder sbMailBody = new StringBuilder();

        #region ***** CONSTRUCTORS *****

        public ColoredConsole(int consoleWidth = 79,
            LineStyle lineStyle = LineStyle.Double,
            LineStyle verticalLineStyle = LineStyle.Double,
            LineStyle horizontalLineStyle = LineStyle.Single,
            TextPosition textPosition = TextPosition.Left,
            TextStyle textStyle = TextStyle.None,
            ConsoleColor backColor = ConsoleColor.Black,
            ConsoleColor foreColor = ConsoleColor.White,
            ConsoleColor lineColor = ConsoleColor.Yellow)
        {
            this.DefaultWidth = consoleWidth;
            this.DefaultLineStyle = lineStyle;
            this.DefaultVerticalLineStyle = verticalLineStyle;
            this.DefaultHorizontalLineStyle = horizontalLineStyle;
            this.DefaultTextPosition = textPosition;
            this.DefaultTextStyle = textStyle;
            this.DefaultBackColor = backColor;
            this.DefaultForeColor = foreColor;
            this.DefaultLineColor = lineColor;
            //
            this.AvailableWidth = this.DefaultWidth - 4;
            //
            /*
            if (ConfigurationManager.AppSettings["LogEnabled"] == "YES")
            {
                string folder = ConfigurationManager.AppSettings["LogFileFolder"];
                folder = AppDomain.CurrentDomain.BaseDirectory + folder;
                if (Directory.Exists(folder))
                {
                    var logFile = $"{DateTime.Now.ToString("yyMMdd")}.log";
                    logFile = Path.Combine(folder, logFile);
                    if (File.Exists(logFile))
                    {
                        this.LogFile = logFile;
                    }
                    else
                    {
                        //File.Create(logFile);
                        // Create a file to write to.
                        using (StreamWriter sw = File.CreateText(logFile))
                        {
                            sw.WriteLine("========== LOG FILE STARTED ==========");
                        }
                        this.LogFile = logFile;
                    }
                }
                else
                {
                    Directory.CreateDirectory(folder);
                    var logFile = $"{DateTime.Now.ToString("yyMMdd")}.log";
                    logFile = Path.Combine(folder, logFile);
                    File.Create(logFile);
                    this.LogFile = LogFile;
                }
            }
            */
        }

        #endregion

        #region ***** PUBLIC METHODS *****

        #region ***** TOP LINE ******
        public void DrawTopLine()
        {
            drawTopLine(DefaultLineStyle, DefaultBackColor, DefaultLineColor);
        }

        public void DrawTopLine(LineStyle lineStyle)
        {
            drawTopLine(lineStyle, DefaultBackColor, DefaultLineColor);
        }

        public void DrawTopLine(LineStyle lineStyle, ConsoleColor backColor, ConsoleColor lineColor)
        {
            drawTopLine(lineStyle, backColor, lineColor);
        }

        #endregion

        #region ***** BOTTOM LINE ******

        public void DrawBottomLine()
        {
            drawBottomLine(DefaultLineStyle, DefaultBackColor, DefaultLineColor);
        }

        public void DrawBottomLine(LineStyle lineStyle)
        {
            drawBottomLine(lineStyle, DefaultBackColor, DefaultLineColor);
        }

        public void DrawBottomLine(LineStyle lineStyle, ConsoleColor backColor, ConsoleColor lineColor)
        {
            drawBottomLine(lineStyle, backColor, lineColor);
        }

        #endregion

        #region ***** SEPARATOR ******
        public void DrawSeparator() => DrawSeparator(DefaultVerticalLineStyle, DefaultHorizontalLineStyle);

        public void DrawSeparator(LineStyle verticalLineStyle, LineStyle horizongalLineStyle)
        {
            if (verticalLineStyle == LineStyle.Single && horizongalLineStyle == LineStyle.Dotted)
                draw_V_SGL_H_DOT_Line();
            else if (verticalLineStyle == LineStyle.Single && horizongalLineStyle == LineStyle.Dashed)
                draw_V_SGL_H_DSH_Line();
            else if (verticalLineStyle == LineStyle.Single && horizongalLineStyle == LineStyle.Single)
                draw_V_SGL_H_SGL_Line();
            else if (verticalLineStyle == LineStyle.Single && horizongalLineStyle == LineStyle.Double)
                draw_V_SGL_H_DBL_Line();
            else if (verticalLineStyle == LineStyle.Double && horizongalLineStyle == LineStyle.Dotted)
                draw_V_DBL_H_DOT_Line();
            else if (verticalLineStyle == LineStyle.Double && horizongalLineStyle == LineStyle.Dashed)
                draw_V_DBL_H_DSH_Line();
            else if (verticalLineStyle == LineStyle.Double && horizongalLineStyle == LineStyle.Single)
                draw_V_DBL_H_SGL_Line();
            else if (verticalLineStyle == LineStyle.Double && horizongalLineStyle == LineStyle.Double)
                draw_V_DBL_H_DBL_Line();
        }

        #endregion

        #region ***** SECTION HEAD *****
        public void DrawSeparator(string message,
            LineStyle sectionLineStyle = LineStyle.Single,
            TextPosition textPosition = TextPosition.Left,
            int tabStop = 1,
            bool autoNumber = false,
            TextStyle textStyle = TextStyle.Caps,
            ConsoleColor backColor = ConsoleColor.Black,
            ConsoleColor foreColor = ConsoleColor.DarkRed,
            ConsoleColor lineColor = ConsoleColor.Yellow)
        {
            switch (sectionLineStyle)
            {
                case LineStyle.Single:
                    message = $"{new string(SGL_TB, tabStop * 4)} {message} {new string(SGL_TB, AvailableWidth - (message.Length + tabStop * 4) - 2)}";
                    break;
                case LineStyle.Double:
                    message = message.PadLeft(tabStop * 4, DBL_TB);
                    message = message.PadRight(AvailableWidth - message.Length, DBL_TB);
                    break;
                case LineStyle.Dotted:
                    message = message.PadLeft(tabStop * 4, DOT_TB);
                    message = message.PadRight(AvailableWidth - message.Length, DOT_TB);
                    break;
                case LineStyle.Dashed:
                    message = message.PadLeft(tabStop * 4, DSH_TB);
                    message = message.PadRight(AvailableWidth - message.Length, DSH_TB);
                    break;
                default: break;
            }
            WriteLine();
            WriteLine(message, DefaultLineStyle, textPosition, 0, autoNumber, textStyle, backColor, foreColor, lineColor);
            WriteLine();
        }

        #endregion

        #region ***** BOX ******
        public void DrawBox(string message,
            LineStyle lineStyle = LineStyle.Double,
            TextPosition textPosition = TextPosition.Center,
            int tabStop = 0,
            bool autoNumber = false,
            TextStyle textStyle = TextStyle.SpacedCaps,
            ConsoleColor backColor = ConsoleColor.Black,
            ConsoleColor foreColor = ConsoleColor.DarkRed,
            ConsoleColor lineColor = ConsoleColor.Yellow)
        {
            DrawTopLine(lineStyle, backColor, lineColor);
            WriteLine(message, lineStyle, textPosition, tabStop, autoNumber, textStyle, backColor, foreColor, lineColor);
            DrawBottomLine(lineStyle, backColor, lineColor);
        }

        #endregion

        #region ***** PROMPT ******
        public string Prompt(string message,
            LineStyle lineStyle = LineStyle.Double,
            TextPosition textPosition = TextPosition.Left,
            int tabStop = 0,
            bool autoNumber = false,
            TextStyle textStyle = TextStyle.None,
            ConsoleColor backColor = ConsoleColor.Black,
            ConsoleColor foreColor = ConsoleColor.White,
            ConsoleColor lineColor = ConsoleColor.Yellow)
        {
            writeLine(message, lineStyle, textPosition, tabStop, autoNumber, textStyle, backColor, foreColor, lineColor);
            string retval = ReadLine();
            WriteLogMessage(message, tabStop);
            return retval;
        }
        #endregion

        #region ***** WRITE MESSAGE + LOG ******
        public void Write(string message,
            LineStyle lineStyle = LineStyle.Double,
            TextPosition textPosition = TextPosition.Left,
            int tabStop = 0,
            bool autoNumber = false,
            TextStyle textStyle = TextStyle.None,
            ConsoleColor backColor = ConsoleColor.Black,
            ConsoleColor foreColor = ConsoleColor.White,
            ConsoleColor lineColor = ConsoleColor.Yellow)
        {
            write(message, lineStyle, textPosition, tabStop, autoNumber, textStyle, backColor, foreColor, lineColor);
            WriteLogMessage(message, tabStop);
        }

        public void WriteLine(string message = "",
            LineStyle lineStyle = LineStyle.Double,
            TextPosition textPosition = TextPosition.Left,
            int tabStop = 0,
            bool autoNumber = false,
            TextStyle textStyle = TextStyle.None,
            ConsoleColor backColor = ConsoleColor.Black,
            ConsoleColor foreColor = ConsoleColor.White,
            ConsoleColor lineColor = ConsoleColor.Yellow)
        {
            writeLine(message, lineStyle, textPosition, tabStop, autoNumber, textStyle, backColor, foreColor, lineColor);
            WriteLogMessage(message, tabStop);
        }

        public void WriteLogMessage(string msg, int tab = 0)
        {
            if (LogEnable)
            {
                // generate file name
                string logFile = $"{DateTime.Now.ToString("yy-MM-dd")}.log";
                string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFolder);
                string logPath = string.Empty;
                // create log folder
                Directory.CreateDirectory(logFolder);
                // generate log path
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFolder, logFile);
                //
                FileInfo fi = new FileInfo(logPath);
                // This text is always added, making the file longer over time
                // if it is not deleted.
                using (FileStream fs = File.Open(logPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    if (msg == "=")
                    {
                        msg = $"========== {DateTime.Now.ToString("dd MMMM yyyy HH:mm")} =========={Environment.NewLine}";
                    }
                    else
                    {
                        msg = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] - {msg}{Environment.NewLine}";
                    }
                    //sw.WriteLine(msg.PadLeft(tab * 4));
                    byte[] info = new UTF8Encoding(true).GetBytes(msg);
                    fs.Write(info, 0, info.Length);
                }
            }
        }
        #endregion

        #endregion

        #region ***** PRIVATE METHODS *****

        private void drawTopLine(LineStyle ls, ConsoleColor backColor, ConsoleColor lineColor)
        {
            Console.BackgroundColor = backColor;
            Console.ForegroundColor = lineColor;
            switch (ls)
            {
                case LineStyle.Single:
                    Console.WriteLine($"{SGL_TL}{new string(SGL_TB, DefaultWidth - 2)}{SGL_TR}");
                    break;
                case LineStyle.Double:
                default:
                    Console.WriteLine($"{DBL_TL}{new string(DBL_TB, DefaultWidth - 2)}{DBL_TR}");
                    break;
            }
            Console.ResetColor();
        }

        private void drawBottomLine(LineStyle ls, ConsoleColor backColor, ConsoleColor lineColor)
        {
            Console.BackgroundColor = backColor;
            Console.ForegroundColor = lineColor;
            switch (ls)
            {
                case LineStyle.Single:
                    Console.WriteLine($"{SGL_BL}{new string(SGL_TB, DefaultWidth - 2)}{SGL_BR}");
                    break;
                case LineStyle.Double:
                default:
                    Console.WriteLine($"{DBL_BL}{new string(DBL_TB, DefaultWidth - 2)}{DBL_BR}");
                    break;
            }
            Console.ResetColor();
        }

        private void draw_V_SGL_H_DOT_Line()
        {
            Console.BackgroundColor = DefaultBackColor;
            Console.ForegroundColor = DefaultLineColor;
            Console.WriteLine($"{SGL_LR}{new string(DOT_TB, DefaultWidth - 2)}{SGL_LR}");
            Console.ResetColor();
        }
        private void draw_V_SGL_H_DSH_Line()
        {
            Console.BackgroundColor = DefaultBackColor;
            Console.ForegroundColor = DefaultLineColor;
            Console.WriteLine($"{SGL_LR}{new string(DSH_TB, DefaultWidth - 2)}{SGL_LR}");
            Console.ResetColor();
        }
        private void draw_V_SGL_H_SGL_Line()
        {
            Console.BackgroundColor = DefaultBackColor;
            Console.ForegroundColor = DefaultLineColor;
            Console.WriteLine($"{SGL_LJ}{new string(SGL_TB, DefaultWidth - 2)}{SGL_LJ}");
            Console.ResetColor();
        }
        private void draw_V_SGL_H_DBL_Line()
        {
            Console.BackgroundColor = DefaultBackColor;
            Console.ForegroundColor = DefaultLineColor;
            Console.WriteLine($"{MIX_SLDJ}{new string(DBL_TB, DefaultWidth - 2)}{MIX_SRDJ}");
            Console.ResetColor();
        }
        private void draw_V_DBL_H_DOT_Line()
        {
            Console.BackgroundColor = DefaultBackColor;
            Console.ForegroundColor = DefaultLineColor;
            Console.WriteLine($"{DBL_LR}{new string(DOT_TB, DefaultWidth - 2)}{DBL_LR}");
            Console.ResetColor();
        }
        private void draw_V_DBL_H_DSH_Line()
        {
            Console.BackgroundColor = DefaultBackColor;
            Console.ForegroundColor = DefaultLineColor;
            Console.WriteLine($"{DBL_LR}{new string(DSH_TB, DefaultWidth - 2)}{DBL_LR}");
            Console.ResetColor();
        }
        private void draw_V_DBL_H_SGL_Line()
        {
            Console.BackgroundColor = DefaultBackColor;
            Console.ForegroundColor = DefaultLineColor;
            Console.WriteLine($"{MIX_DLSJ}{new string(SGL_TB, DefaultWidth - 2)}{MIX_DRSJ}");
            Console.ResetColor();
        }
        private void draw_V_DBL_H_DBL_Line()
        {
            Console.BackgroundColor = DefaultBackColor;
            Console.ForegroundColor = DefaultLineColor;
            Console.WriteLine($"{DBL_LJ}{new string(DBL_TB, DefaultWidth - 2)}{DBL_RJ}");
            Console.ResetColor();
        }

        private void write(string msg,
            LineStyle ls = LineStyle.Double,
            TextPosition tp = TextPosition.Left,
            int tab = 0,
            bool an = false,
            TextStyle ts = TextStyle.None,
            ConsoleColor bc = ConsoleColor.Black,
            ConsoleColor fc = ConsoleColor.White,
            ConsoleColor lc = ConsoleColor.Yellow)
        {
            msg = msg.Trim();
            char lr;
            if (msg.Length <= AvailableWidth)
            {
                Console.BackgroundColor = bc;
                Console.ForegroundColor = lc;
                if (ls == LineStyle.Double) lr = DBL_LR; else lr = SGL_LR;
                Console.Write($"{lr} ");
                Console.ForegroundColor = fc;
                Console.Write(formatMessage(msg, tp, tab, ts));
                Console.ForegroundColor = lc;
                Console.WriteLine($" {lr}");
                // below two lines added 
                var top = Console.CursorTop;
                Console.SetCursorPosition(0, top - 1);
                //
                Console.ResetColor();
            }
            else
            {
                // split the big message into small messages that fit inside the console width
                var splitMsg = SplitByLength(msg, AvailableWidth);
                foreach (string bitMsg in splitMsg)
                {
                    writeLine(bitMsg, ls, tp, tab, an, ts, bc, fc, lc);
                }
            }
            //
        }
        private void writeLine(string msg,
            LineStyle ls = LineStyle.Double,
            TextPosition tp = TextPosition.Left,
            int tab = 0,
            bool an = false,
            TextStyle ts = TextStyle.None,
            ConsoleColor bc = ConsoleColor.Black,
            ConsoleColor fc = ConsoleColor.White,
            ConsoleColor lc = ConsoleColor.Yellow)
        {
            msg = msg.Trim();
            // update string with auto number if tp = left
            if (an && tp == TextPosition.Left)
            {
                AutoNumberCounter++;
                var anText = $"{AutoNumberCounter}.";
                anText = anText.PadRight(4);
                msg = $"{anText}{msg}";
            }
            char lr;
            if (msg.Length <= AvailableWidth)
            {
                Console.BackgroundColor = bc;
                Console.ForegroundColor = lc;
                if (ls == LineStyle.Double) lr = DBL_LR; else lr = SGL_LR;
                Console.Write($"{lr} ");
                Console.ForegroundColor = fc;
                Console.Write(formatMessage(msg, tp, tab, ts));
                Console.ForegroundColor = lc;
                Console.WriteLine($" {lr}");
                Console.ResetColor();
            }
            else
            {
                // split the big message into small messages that fit inside the console width
                var splitMsg = SplitByLength(msg, AvailableWidth);
                bool isFirst = true;
                foreach (string bitMsg in splitMsg)
                {
                    bool anTemp = false;
                    if (an)
                    {
                        if (isFirst)
                            writeLine(bitMsg, ls, tp, tab, anTemp, ts, bc, fc, lc);
                        else
                            writeLine(bitMsg, ls, tp, tab + 1, anTemp, ts, bc, fc, lc);
                    }
                    else
                        writeLine(bitMsg, ls, tp, tab, anTemp, ts, bc, fc, lc);
                    isFirst = false;

                }
            }
            //
        }

        private string ReadLine(
            LineStyle ls = LineStyle.Double,
            TextPosition tp = TextPosition.Left,
            int tab = 0,
            TextStyle ts = TextStyle.None,
            ConsoleColor bc = ConsoleColor.Black,
            ConsoleColor fc = ConsoleColor.Green,
            ConsoleColor lc = ConsoleColor.Yellow)
        {
            char lr;
            //
            Console.BackgroundColor = bc;
            Console.ForegroundColor = lc;
            if (ls == LineStyle.Double) lr = DBL_LR; else lr = SGL_LR;
            Console.Write($"{lr} ");
            Console.ForegroundColor = fc;
            Console.Write(formatMessage("? ", tp, tab + 1, ts));
            Console.ForegroundColor = lc;
            Console.WriteLine($" {lr}");
            // put the cursor back
            Console.SetCursorPosition((tab + 1) * 4 + 4, Console.CursorTop - 1);
            Console.ForegroundColor = fc;
            var retval = Console.ReadLine();
            Console.ResetColor();
            return retval;
            //
        }

        private string formatMessage(string msg, TextPosition tp, int tab, TextStyle ts)
        {
            switch (ts)
            {
                case TextStyle.Spaced:
                    return padString(msg.Aggregate(string.Empty, (c, i) => c + i + ' '), tp, tab);
                case TextStyle.Caps:
                    return padString(msg.ToUpper(), tp, tab);
                case TextStyle.SpacedCaps:
                    return padString(msg.Aggregate(string.Empty, (c, i) => c + i + ' ').ToUpper(), tp, tab);
                case TextStyle.None:
                default:
                    return padString(msg, tp, tab);
            }
        }

        private string padString(string str, TextPosition tp, int tab)
        {
            // pad the string 
            var padding = AvailableWidth - str.Length;
            switch (tp)
            {
                case TextPosition.Center:
                    return str.PadLeft((padding) / 2 + str.Length).PadRight(AvailableWidth);
                case TextPosition.Right:
                    return str.PadLeft(AvailableWidth);
                case TextPosition.Left:
                default:
                    str = str.PadLeft(str.Length + tab * 4);
                    return str.PadRight(AvailableWidth);
            }
        }

        private IEnumerable<string> SplitByLength(string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

        protected virtual bool IsFileinUse(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

        #endregion

        #region ***** PUBLIC ENUMS *****

        public enum LineStyle
        {
            Dotted = 1, // DOT
            Dashed = 2, // DSH
            Single = 3, // SGL
            Double = 4  // DBL
        }

        public enum TextPosition
        {
            Left = 1,
            Center = 2,
            Right = 3
        }

        public enum TextStyle
        {
            Spaced = 1,
            Caps = 2,
            SpacedCaps = 3,
            None = 0
        }

        #endregion
    }
}
