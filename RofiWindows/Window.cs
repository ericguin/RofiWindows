using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using HWND = System.IntPtr;

namespace RofiWindows
{
    public class Window
    {
        public HWND Handle { get; }
        public string Title { get; }

        protected Window(HWND handle, string title)
        {
            Handle = handle;
            Title = title;
        }

        public void Activate() => SetForegroundWindow(Handle);

        public static IEnumerable<Window> ListWindows() =>
            GetOpenWindows().Select(_ => new Window(_.Key, _.Value));

        // https://stackoverflow.com/a/43640787
        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        private static IDictionary<HWND, string> GetOpenWindows()
        {
            HWND shellWindow = GetShellWindow();
            Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

            EnumWindows(delegate (HWND hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.Cloaked, out bool cloaked, Marshal.SizeOf(typeof(bool)));
                if (cloaked) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                string title = builder.ToString();
                windows[hWnd] = title;
                return true;

            }, 0);

            return windows;
        }

        private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(HWND hwnd, DWMWINDOWATTRIBUTE dwAttribute, out bool pvAttribute, int cbAttribute);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(HWND hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HWND hWnd, out RECT lpRect);

        [DllImport("User32", CharSet = CharSet.Unicode)]
        static extern bool GetWindowInfo(HWND hwnd, out WINDOWINFO pwi);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WINDOWINFO
        {
            public int cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public int dwStyle;
            public int dwExStyle;
            public int dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public IntPtr atomWindowType;
            public int wCreatorVersion;
        }
        enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }
    }

    public static class WindowExtensions
    {
        public static IEnumerable<Window> FilterWindowsByTitleFuzzy(this IEnumerable<Window> windows, string filter) =>
            Process.ExtractTop(filter, windows.Select(_ => _.Title)).Join(windows, res => res.Value, win => win.Title, (res, win) => new { w = win, r = res }).OrderByDescending(_ => _.r.Score).Select(_ => _.w);
    }
}
