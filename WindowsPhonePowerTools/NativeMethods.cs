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
        [DllImport("dwmapi.dll", PreserveSig = true)]
        internal static extern Int32 DwmSetWindowAttribute(
            IntPtr hwnd,
            Int32 attr,
            ref Int32 attrValue,
            Int32 attrSize);

        [DllImport("dwmapi.dll")]
        internal static extern Int32 DwmExtendFrameIntoClientArea(
            IntPtr hWnd,
            ref MARGINS pMarInset);

        [DllImport("user32")]
        internal static extern Boolean GetMonitorInfo(
            IntPtr hMonitor,
            MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(
            IntPtr handle,
            Int32 flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(
            IntPtr hWnd,
            UInt32 msg,
            IntPtr wParam,
            IntPtr lParam);

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
            GETMINMAXINFO     = 0x0024,
            WINDOWPOSCHANGING = 0x0046,
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
                case WM.GETMINMAXINFO:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;

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

                    if (!forceHeightWidth)
                        return IntPtr.Zero;

                    Marshal.StructureToPtr(pos, lParam, true);
                    handled = true;
                    
                    break;
            }

            return IntPtr.Zero;
        }

        internal static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area 
            // of the correct monitor.
            Int32 MONITOR_DEFAULTTONEAREST = 0x00000002;

            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);

                RECT rcWorkArea = monitorInfo.m_rcWork;
                RECT rcMonitorArea = monitorInfo.m_rcMonitor;

                mmi.m_ptMaxPosition.m_x = Math.Abs(rcWorkArea.m_left - rcMonitorArea.m_left);
                mmi.m_ptMaxPosition.m_y = Math.Abs(rcWorkArea.m_top - rcMonitorArea.m_top);

                mmi.m_ptMaxSize.m_x = Math.Abs(rcWorkArea.m_right - rcWorkArea.m_left);
                mmi.m_ptMaxSize.m_y = Math.Abs(rcWorkArea.m_bottom - rcWorkArea.m_top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        internal static void ShowShadowUnderWindow(IntPtr intPtr)
        {
            MARGINS marInset = new MARGINS();
            marInset.m_bottomHeight = -1;
            marInset.m_leftWidth = -1;
            marInset.m_rightWidth = -1;
            marInset.m_topHeight = -1;

            DwmExtendFrameIntoClientArea(intPtr, ref marInset);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal sealed class MONITORINFO
        {
            public Int32 m_cbSize;
            public RECT m_rcMonitor;
            public RECT m_rcWork;
            public Int32 m_dwFlags;

            public MONITORINFO()
            {
                m_cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                m_rcMonitor = new RECT();
                m_rcWork = new RECT();
                m_dwFlags = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct RECT
        {
            public static readonly RECT Empty = new RECT();

            public Int32 m_left;
            public Int32 m_top;
            public Int32 m_right;
            public Int32 m_bottom;

            public RECT(Int32 left, Int32 top, Int32 right, Int32 bottom)
            {
                m_left = left;
                m_top = top;
                m_right = right;
                m_bottom = bottom;
            }

            public RECT(RECT rcSrc)
            {
                m_left = rcSrc.m_left;
                m_top = rcSrc.m_top;
                m_right = rcSrc.m_right;
                m_bottom = rcSrc.m_bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MARGINS
        {
            public Int32 m_leftWidth;
            public Int32 m_rightWidth;
            public Int32 m_topHeight;
            public Int32 m_bottomHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public Int32 m_x;
            public Int32 m_y;

            public POINT(Int32 x, Int32 y)
            {
                m_x = x;
                m_y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT m_ptReserved;
            public POINT m_ptMaxSize;
            public POINT m_ptMaxPosition;
            public POINT m_ptMinTrackSize;
            public POINT m_ptMaxTrackSize;
        };
    }
}
