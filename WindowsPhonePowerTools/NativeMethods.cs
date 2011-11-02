using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WindowsPhonePowerTools
{
    // from: https://github.com/moodmosaic/BonusBits.CodeSamples/blob/master/BonusBits.CodeSamples.MetroUI/Retro1/MainWindow.xaml.cs
    internal static class NativeMethods
    {

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [Flags]
        internal enum SWP
        {
            ASYNCWINDOWPOS = 0x4000,
            DEFERERASE = 0x2000,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            HIDEWINDOW = 0x0080,
            NOACTIVATE = 0x0010,
            NOCOPYBITS = 0x0100,
            NOMOVE = 0x0002,
            NOOWNERZORDER = 0x0200,
            NOREDRAW = 0x0008,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            NOSIZE = 0x0001,
            NOZORDER = 0x0004,
            SHOWWINDOW = 0x0040,
        }

        // see more at: http://blogs.microsoft.co.il/blogs/arik/SingleInstance.cs.txt
        internal enum WM
        {
            GETMINMAXINFO         = 0x0024,
            WINDOWPOSCHANGING     = 0x0046,
            DWMNCRENDERINGCHANGED = 0x031F,
        }

        internal static IntPtr WindowProc(
            IntPtr hwnd,
            Int32 msg,
            IntPtr wParam,
            IntPtr lParam,
            ref Boolean handled)
        {
            switch ((WM)msg)
            {
                case WM.WINDOWPOSCHANGING:
                    
                    WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                    if ((pos.flags & (int)SWP.NOMOVE) != 0)
                    {
                        return IntPtr.Zero;
                    }
                    
                    Window window = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                    
                    if (window == null)
                    {
                        return IntPtr.Zero;
                    }

                    bool forceHeightWidth = false;

                    if (pos.cx < window.MinWidth) { 
                        pos.cx = (int)window.MinWidth;

                        forceHeightWidth = true;
                    }

                    if (pos.cy < window.MinHeight)
                    {
                        pos.cy = (int)window.MinHeight;

                        forceHeightWidth = true;
                    }

                    if (forceHeightWidth)
                    {
                        Marshal.StructureToPtr(pos, lParam, true);
                        handled = true;
                    }
                    
                    break;
            }

            return IntPtr.Zero;
        }
    }
}
