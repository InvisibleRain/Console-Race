using System.Runtime.InteropServices;
using System;

namespace Console_Races
{
    class ConsoleFont
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 SetCurrentConsoleFontEx(
            IntPtr ConsoleOutput,
            bool MaximumWindow,
            ref CONSOLE_FONT_INFO_EX ConsoleCurrentFontEx);

        public enum StdHandle
        {
            OutputHandle = -11
        }

        [DllImport("kernel32")]
        public static extern IntPtr GetStdHandle(StdHandle index);

        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CONSOLE_FONT_INFO_EX
        {
            public uint cbSize;
            public uint nFont;
            public COORD dwFontSize;
            public int FontFamily;
            public int FontWeight;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] // Edit sizeconst if the font name is too big
            public string FaceName;
        }

        public static void ChangeFont(string FontName, short width, short height)
        {
            // Instantiating CONSOLE_FONT_INFO_EX and setting its size (the function will fail otherwise)
            ConsoleFont.CONSOLE_FONT_INFO_EX ConsoleFontInfo = new ConsoleFont.CONSOLE_FONT_INFO_EX();
            ConsoleFontInfo.cbSize = (uint)Marshal.SizeOf(ConsoleFontInfo);

            // Optional, implementing this will keep the fontweight and fontsize from changing
            // See notes
            // GetCurrentConsoleFontEx(GetStdHandle(StdHandle.OutputHandle), false, ref ConsoleFontInfo);

            ConsoleFontInfo.FaceName = FontName;
            ConsoleFontInfo.dwFontSize.X = width;
            ConsoleFontInfo.dwFontSize.Y = height;

            SetCurrentConsoleFontEx(GetStdHandle(StdHandle.OutputHandle), false, ref ConsoleFontInfo);
        }
    }

    //Call
    //ConsoleFont.ChangeFont("Lucida Console", 6, 8);

}
