using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace desktopDate.util {
	public class ComUtil {
		//对外部软件窗口发送一些消息(如 窗口最大化、最小化等)
		[DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
		public delegate bool CallBack(IntPtr hwnd, IntPtr lParam);
		[DllImport("user32.dll")]
		public static extern int EnumWindows(CallBack enumProc, IntPtr lParam);
		[DllImport("user32.dll")]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);
		[DllImport("user32.dll")]
		public static extern IntPtr FindWindowEx(IntPtr hWnd1, IntPtr hWnd2, string lpsz1, string lpsz2);
		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hwnd, int nIndex);
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);
		[DllImport("user32.dll")]
		public static extern int SetLayeredWindowAttributes(IntPtr Handle, int crKey, byte bAlpha, int dwFlags);
		public const int GWL_EXSTYLE = -20;
		public const int WS_EX_TRANSPARENT = 0x20;
		public const int WS_EX_LAYERED = 0x80000;
		public const int LWA_ALPHA = 2;
		public const int WS_EX_APPWINDOW = 0x40000;
		public const int WS_EX_TOOLWINDOW = 0x80;

		[DllImport("dwmapi.dll")]
		public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern bool DwmIsCompositionEnabled();
		[DllImport("kernel32.dll")]
		public static extern uint GetLastError();

		public const int GWL_STYLE = -16;
		public const int WS_CAPTION = 0x00C00000;
		public const int WS_CAPTION_2 = 0X00C0000;

		[DllImport("user32.dll")]
		public extern static bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
		public static uint LWA_COLORKEY = 0x00000001;

		public struct WindowInfo {
			public IntPtr hWnd;
			public string szWindowName;
			public string szClassName;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MARGINS {
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;
		}
	}
}
