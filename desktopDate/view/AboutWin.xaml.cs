using desktopDate.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace desktopDate.view {
	/// <summary>
	/// AboutWin.xaml 的交互逻辑
	/// </summary>
	public partial class AboutWin : Window {
		public AboutWin() {
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			IntPtr Handle = new WindowInteropHelper(this).Handle;

			//隐藏边框
			int oldstyle = ComUtil.GetWindowLong(Handle, ComUtil.GWL_STYLE);
			ComUtil.SetWindowLong(Handle, ComUtil.GWL_STYLE, oldstyle & (~(ComUtil.WS_CAPTION | ComUtil.WS_CAPTION_2)) | ComUtil.WS_EX_LAYERED);

			//不在Alt+Tab中显示
			int oldExStyle = ComUtil.GetWindowLong(Handle, ComUtil.GWL_EXSTYLE);
			ComUtil.SetWindowLong(Handle, ComUtil.GWL_EXSTYLE, oldExStyle & (~ComUtil.WS_EX_APPWINDOW) | ComUtil.WS_EX_TOOLWINDOW);
		}

		private void Window_MouseUp(object sender, MouseButtonEventArgs e) {
			try {
				Close();
			} catch (Exception) { }
		}

		private void Window_Deactivated(object sender, EventArgs e) {
			try {
				Close();
			} catch (Exception) { }
		}

		private void lblUrlJump_MouseUp(object sender, MouseButtonEventArgs e) {
			Label lbl = sender as Label;
			if(lbl == null) {
				return;
			}
			
			Process.Start(lbl.Content.ToString());
		}
	}
}
