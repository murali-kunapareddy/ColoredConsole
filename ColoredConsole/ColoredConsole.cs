using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace bcd
{
    public class ColoredConsole
    {
        static readonly char DBL_TL = '╔';
        static readonly char DBL_TR = '╗';
        static readonly char DBL_LR = '║';
        static readonly char DBL_LJ = '╠';
        static readonly char DBL_RJ = '╣';
        static readonly char DBL_TB = '═';
        static readonly char DBL_TJ = '╦';
        static readonly char DBL_BJ = '╩';
        static readonly char DBL_CJ = '╬';
        static readonly char DBL_BL = '╚';
        static readonly char DBL_BR = '╝';
        //
        static readonly char SGL_TL = '┌';   //218
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
        static readonly char MIX_DTSJ = '╤'; //209
        static readonly char MIX_DBSJ = '╧'; //207
        static readonly char MIX_DLSJ = '╟'; //199
        static readonly char MIX_DRSJ = '╢'; //182
        static readonly char MIX_STDJ = '╥'; //210
        static readonly char MIX_SBDJ = '╨'; //208
        static readonly char MIX_SLDJ = '╞'; //198
        static readonly char MIX_SRDJ = '╡'; //181

        public int ConsoleWidth { get; set; }
        public LineStyle ConsoleLineStyle { get; set; }
        public TextPosition ConsoleTextPosition { get; set; }
        public TextStyle ConsoleTextStyle { get; set; }
        public ConsoleColor ConsoleBackColor { get; set; }
        public ConsoleColor ConsoleForeColor { get; set; }
        public ConsoleColor ConsoleLineColor { get; set; }
        public string LogFile { get; set; }
        public bool LogEmail { get; set; }
        private int AvailableWidth { get; set; }
        private StringBuilder sbMailBody = new StringBuilder();

        #region ***** CONSTRUCTORS *****

        public ColoredConsole(int consoleWidth = 79,
            LineStyle lineStyle = LineStyle.Double,
            TextPosition textPosition = TextPosition.Left,
            TextStyle textStyle = TextStyle.None,
            ConsoleColor backColor = ConsoleColor.Black,
            ConsoleColor foreColor = ConsoleColor.White,
            ConsoleColor lineColor = ConsoleColor.Yellow)
        {
            this.ConsoleWidth = consoleWidth;
            this.ConsoleLineStyle = lineStyle;
            this.ConsoleTextPosition = textPosition;
            this.ConsoleTextStyle = textStyle;
            this.ConsoleBackColor = ConsoleColor.Black;
            this.ConsoleForeColor = ConsoleColor.White;
            this.ConsoleLineColor = ConsoleColor.Yellow;
            //
            this.AvailableWidth = this.ConsoleWidth - 4;
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

        public void DrawTopLine()
        {
            drawTopLine(ConsoleLineStyle, ConsoleBackColor, ConsoleLineColor);
        }

        public void DrawTopLine(LineStyle lineStyle)
        {
            drawTopLine(lineStyle, ConsoleBackColor, ConsoleLineColor);
        }

        public void DrawTopLine(LineStyle lineStyle, ConsoleColor backColor, ConsoleColor lineColor)
        {
            drawTopLine(lineStyle, backColor, lineColor);
        }

        public void DrawBottomLine()
        {
            drawBottomLine(ConsoleLineStyle, ConsoleBackColor, ConsoleLineColor);
        }

        public void DrawBottomLine(LineStyle lineStyle)
        {
            drawBottomLine(lineStyle, ConsoleBackColor, ConsoleLineColor);
        }

        public void DrawBottomLine(LineStyle lineStyle, ConsoleColor backColor, ConsoleColor lineColor)
        {
            drawBottomLine(lineStyle, backColor, lineColor);
        }

        public void DrawSeparator(LineStyle verticalLineStyle, LineStyle horizongalLineStyle)
        {
            if (verticalLineStyle == LineStyle.Single && horizongalLineStyle == LineStyle.Single)
                drawVSHSLine();
            else if (verticalLineStyle == LineStyle.Double && horizongalLineStyle == LineStyle.Double)
                drawVDHDLine();
            else if (verticalLineStyle == LineStyle.Single && horizongalLineStyle == LineStyle.Double)
                drawVSHDLine();
            else
                drawVDHSLine();
        }

        public void DrawBox(string message, LineStyle lineStyle, TextPosition textPosition, int tabStop, TextStyle textStyle, ConsoleColor backColor, ConsoleColor foreColor, ConsoleColor lineColor)
        {
            DrawTopLine(lineStyle, backColor, lineColor);
            Write(message, lineStyle, textPosition, tabStop, textStyle, backColor, foreColor, lineColor);
            DrawBottomLine(lineStyle, backColor, lineColor);
        }

        public void Write(string message,
            LineStyle lineStyle = LineStyle.Double,
            TextPosition textPosition = TextPosition.Left,
            int tabStop = 0,
            TextStyle textStyle = TextStyle.None,
            ConsoleColor backColor = ConsoleColor.Black,
            ConsoleColor foreColor = ConsoleColor.White,
            ConsoleColor lineColor = ConsoleColor.Yellow)
        {
            writeLine(message, lineStyle, textPosition, tabStop, textStyle, backColor, foreColor, lineColor);
            WriteLogMessage(message, tabStop);
        }

        public void WriteLogMessage(string msg, int tab = 0)
        {
            if (LogFile != null && LogFile != string.Empty)
            {
                // This text is always added, making the file longer over time
                // if it is not deleted.
                using (StreamWriter sw = File.AppendText(LogFile))
                {
                    if (msg == "=")
                    {
                        msg = $"========== {DateTime.Now.ToString("dd MMMM yyyy HH:mm")} =========={Environment.NewLine}";
                    }
                    else
                    {
                        msg = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] - {msg}{Environment.NewLine}";
                    }

                    sw.Write(msg.PadLeft(tab * 4));
                }
            }

            //if (LogFile != null && LogFile != string.Empty)
            //{
            //    FileInfo fi = new FileInfo(LogFile);

            //    using (FileStream fs = File.Open(LogFile, FileMode.OpenOrCreate,FileAccess.ReadWrite, FileShare.ReadWrite))
            //    {

            //        if (msg == "=")
            //        {
            //            msg = $"========== {DateTime.Now.ToString("dd MMMM yyyy HH:mm")} =========={Environment.NewLine}";
            //        }
            //        else
            //        {
            //            msg = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] - {msg}{Environment.NewLine}";
            //        }
            //        //sw.WriteLine(msg.PadLeft(tab * 4));
            //        byte[] info = new UTF8Encoding(true).GetBytes(msg);
            //        fs.Write(info, 0, info.Length);

            //    }
            //}
        }

        #endregion

        #region ***** PRIVATE METHODS *****

        private void drawTopLine(LineStyle ls, ConsoleColor backColor, ConsoleColor lineColor)
        {
            Console.BackgroundColor = backColor;
            Console.ForegroundColor = lineColor;
            switch (ls)
            {
                case LineStyle.Single:
                    Console.WriteLine($"{SGL_TL}{new string(SGL_TB, ConsoleWidth - 2)}{SGL_TR}");
                    break;
                case LineStyle.Double:
                default:
                    Console.WriteLine($"{DBL_TL}{new string(DBL_TB, ConsoleWidth - 2)}{DBL_TR}");
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
                    Console.WriteLine($"{SGL_BL}{new string(SGL_TB, ConsoleWidth - 2)}{SGL_BR}");
                    break;
                case LineStyle.Double:
                default:
                    Console.WriteLine($"{DBL_BL}{new string(DBL_TB, ConsoleWidth - 2)}{DBL_BR}");
                    break;
            }
            Console.ResetColor();
        }

        private void drawVDHSLine()
        {
            Console.BackgroundColor = ConsoleBackColor;
            Console.ForegroundColor = ConsoleLineColor;
            Console.WriteLine($"{MIX_DLSJ}{new string(SGL_TB, ConsoleWidth - 2)}{MIX_DRSJ}");
            Console.ResetColor();
        }

        private void drawVSHDLine()
        {
            Console.BackgroundColor = ConsoleBackColor;
            Console.ForegroundColor = ConsoleLineColor;
            Console.WriteLine($"{MIX_SLDJ}{new string(DBL_TB, ConsoleWidth - 2)}{MIX_SRDJ}");
            Console.ResetColor();
        }

        private void drawVDHDLine()
        {
            Console.BackgroundColor = ConsoleBackColor;
            Console.ForegroundColor = ConsoleLineColor;
            Console.WriteLine($"{DBL_LJ}{new string(DBL_TB, ConsoleWidth - 2)}{DBL_RJ}");
            Console.ResetColor();
        }

        private void drawVSHSLine()
        {
            Console.BackgroundColor = ConsoleBackColor;
            Console.ForegroundColor = ConsoleLineColor;
            Console.WriteLine($"{SGL_LJ}{new string(SGL_TB, ConsoleWidth - 2)}{SGL_RJ}");
            Console.ResetColor();
        }

        private void writeLine(string msg,
            LineStyle ls = LineStyle.Double,
            TextPosition tp = TextPosition.Left,
            int tab = 0,
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
                Console.ResetColor();
            }
            else
            {
                // split the big message into small messages that fit inside the console width
                var splitMsg = SplitByLength(msg, AvailableWidth);
                foreach (string bitMsg in splitMsg)
                {
                    writeLine(bitMsg, ls, tp, tab, ts, bc, fc, lc);
                }
            }
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
            Single = 1,
            Double = 2
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
