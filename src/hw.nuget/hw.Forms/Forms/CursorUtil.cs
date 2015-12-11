using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using hw.DebugFormatter;

namespace hw.Forms
{
    public class CursorUtil
    {
        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr handle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        // Based on the article and comments here:
        // http://www.switchonthecode.com/tutorials/csharp-tutorial-how-to-use-custom-cursors
        // Note that the returned Cursor must be disposed of after use, or you'll leak memory!

        public struct IconInfo
        {
            [DisableDump]
            public bool fIcon;
            [DisableDump]
            public int xHotspot;
            [DisableDump]
            public int yHotspot;
            [DisableDump]
            public IntPtr hbmMask;
            [DisableDump]
            public IntPtr hbmColor;

            internal Point HotSpot
            {
                get { return new Point(xHotspot, yHotspot); }
                set
                {
                    xHotspot = value.X;
                    yHotspot = value.Y;
                }
            }

            internal IconInfo(Bitmap bitmap)
                : this()
            {
                var hicon = bitmap.GetHicon();
                GetIconInfo(hicon, ref this);
                if (hicon != IntPtr.Zero)
                    DestroyIcon(hicon);

            }

            [DisableDump]
            internal Cursor Cursor
            {
                get
                {
                    var cursorPtr = CreateIconIndirect(ref this);

                    if (hbmColor != IntPtr.Zero)
                        DeleteObject(hbmColor);
                    if (hbmMask != IntPtr.Zero)
                        DeleteObject(hbmMask);

                    return new Cursor(cursorPtr);
                }
            }
        }
    }
}