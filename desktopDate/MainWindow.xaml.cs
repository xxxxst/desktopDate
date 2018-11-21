using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace desktopDate {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hwnd, int nIndex);
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);
		[DllImport("user32.dll")]
		public static extern int SetLayeredWindowAttributes(IntPtr Handle, int crKey, byte bAlpha, int dwFlags);
		const int GWL_EXSTYLE = -20;
		const int WS_EX_TRANSPARENT = 0x20;
		const int WS_EX_LAYERED = 0x80000;
		const int LWA_ALPHA = 2;

		[DllImport("dwmapi.dll")]
		public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern bool DwmIsCompositionEnabled();

		public MainWindow() {
			InitializeComponent();

			//ignoreMouseEvent();
			setPos();

			DispatcherTimer readDataTimer = new DispatcherTimer();
			readDataTimer.Tick += new EventHandler(timerProc);
			readDataTimer.Interval = new TimeSpan(0, 0, 0, 1);
			readDataTimer.Start();
		}

		private void ignoreMouseEvent() {
			IntPtr Handle = new WindowInteropHelper(this).Handle;

			//var mainWindowSrc = HwndSource.FromHwnd(Handle);
			//if (mainWindowSrc != null) {
			//	if (mainWindowSrc.CompositionTarget != null) {
			//		mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
			//	}
			//}

			if (DwmIsCompositionEnabled()) {
				MARGINS margin = new MARGINS();
				margin.Right = margin.Left = margin.Bottom = margin.Top = -1;
				DwmExtendFrameIntoClientArea(Handle, ref margin);
			}

			//SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_TRANSPARENT | WS_EX_LAYERED);
			//SetLayeredWindowAttributes(Handle, 0, 128, LWA_ALPHA);
		}

		private void setPos() {
			int w = 0;
			int h = Screen.PrimaryScreen.Bounds.Height;
			for (int i = 0; i < Screen.AllScreens.Length; ++i) {
				w += Screen.AllScreens[i].Bounds.Width;
			}
			Left = w - Width;
			Top = h - Height;
		}

		private void timerProc(object sender, EventArgs e) {
			DateTime date = DateTime.Now;
			lblTime.Content = date.ToString("HH;mm:ss");
			lblWeek.Content = date.ToString("dddd");
			lblDate.Content = date.ToString("yyyy/MM/dd");
		}

		private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			//Debug.WriteLine("aaa:" + e.ChangedButton);
			//if (e.ChangedButton == MouseButton.Right) {
			//	Task.Run(() => {
			//		Thread.Sleep(500);
			//		Dispatcher.Invoke(() => {
			//			Close();
			//		});
			//	});
			//}
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
			//双击中键退出
			if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2) {
				Task.Run(() => {
					Thread.Sleep(500);
					Dispatcher.Invoke(() => {
						Close();
					});
				});
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MARGINS {
		public int Left;
		public int Right;
		public int Top;
		public int Bottom;
	}
}
