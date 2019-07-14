using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PoePriceChecker
{
    class GetPoEWinInfo
    {
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]

        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public int[] Size { get; set; }
        public int[] Location { get; set; }



        public GetPoEWinInfo()
        {
            Size = new int[2];
            Location = new int[2];
        }

        private void getWinSizeLocation()
        {
            string WinTitle = "";
            IntPtr PoE = FindWindow(null, WinTitle);
            if (PoE != IntPtr.Zero)
            {
                RECT rc = new RECT();

                GetWindowRect(PoE, ref rc);

                int width = rc.Right - rc.Left;
                int height = rc.Bottom - rc.Top;
                int x = rc.Left;
                int y = rc.Top;

                Console.WriteLine(string.Format("width:{0} height:{1} left:{2} top:{3}", width, height, x, y));
            }
            else
            {
                Console.WriteLine("can not find the window");
            }
        }
    }
}
