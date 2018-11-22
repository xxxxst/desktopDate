using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
		//对外部软件窗口发送一些消息(如 窗口最大化、最小化等)
		[DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
		public delegate bool CallBack(IntPtr hwnd, IntPtr lParam);
		[DllImport("user32.dll")]
		public static extern int EnumWindows(CallBack enumProc, IntPtr lParam);
		[DllImport("user32.dll")]
		static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
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
		const int GWL_EXSTYLE = -20;
		const int WS_EX_TRANSPARENT = 0x20;
		const int WS_EX_LAYERED = 0x80000;
		const int LWA_ALPHA = 2;

		[DllImport("dwmapi.dll")]
		public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		public static extern bool DwmIsCompositionEnabled();
		[DllImport("kernel32.dll")]
		static extern uint GetLastError();

		static Dictionary<string, Key> mapKey = new Dictionary<string, Key>();
		//public Dictionary<Key, string> mapCtlKey = new Dictionary<Key, string>();
		//public Dictionary<Key, string> mapIgnoreKey = new Dictionary<Key, string>();
		uint keyCode = 0;
		KeyboardCtl keyCtl = new KeyboardCtl();
		ChineseLunisolarCalendar ChineseCalendar = new ChineseLunisolarCalendar();
		bool isShowChineseDate = false;

		Dictionary<string, string> mapFestival = new Dictionary<string, string>();
		Dictionary<string, string> mapChineseFestival = new Dictionary<string, string>();

		public MainWindow() {
			InitializeComponent();

			initFestival();

			//ignoreMouseEvent();
			setPos();

			updateDate();

			//initKeys();
			//keyCode = stringToKeyCode("RightCtrl");
			//keyCtl.checkKey = checkKey;
			//keyCtl.init();

			DispatcherTimer readDataTimer = new DispatcherTimer();
			readDataTimer.Tick += new EventHandler(timerProc);
			readDataTimer.Interval = new TimeSpan(0, 0, 0, 1);
			readDataTimer.Start();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			appendToWindow();
		}

		private void initKeys() {
			foreach (Key key in Enum.GetValues(typeof(Key))) {
				mapKey[key.ToString()] = key;
			}
			mapKey["Ctl"] = Key.LeftCtrl;
			mapKey["Ctrl"] = Key.LeftCtrl;
			mapKey["RCtl"] = Key.RightCtrl;
			mapKey["RCtrl"] = Key.RightCtrl;
			mapKey["RightCtl"] = Key.RightCtrl;
			mapKey["RightCtrl"] = Key.RightCtrl;
			mapKey["Shift"] = Key.LeftShift;
			mapKey["Alt"] = Key.LeftAlt;

			//mapCtlKey[Key.LeftCtrl] = "Ctl";
			//mapCtlKey[Key.RightCtrl] = "Ctl";
			//mapCtlKey[Key.LeftShift] = "Shift";
			//mapCtlKey[Key.RightShift] = "Shift";
			//mapCtlKey[Key.LeftAlt] = "Alt";
			//mapCtlKey[Key.RightAlt] = "Alt";

			//mapIgnoreKey[Key.Capital] = "";
			//mapIgnoreKey[Key.NumLock] = "";
			//mapIgnoreKey[Key.LWin] = "";
			//mapIgnoreKey[Key.RWin] = "";
			//mapIgnoreKey[Key.Escape] = "";
		}

		CallBack myCallBack = null;
		private void appendToWindow() {
			IntPtr Handle = new WindowInteropHelper(this).Handle;

			myCallBack = new CallBack(enumWindowsProc);
			EnumWindows(myCallBack, IntPtr.Zero);

			//IntPtr pWnd = FindWindow("Progman", null);
			//IntPtr pWnd = FindWindow("WorkerW", null);
			//if (pWnd == IntPtr.Zero) return;
			//Debug.WriteLine("aaa");

			//pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
			//if (pWnd == IntPtr.Zero) return;
			//Debug.WriteLine("bbb");

			//pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SysListView32", null);
			//if (pWnd == IntPtr.Zero) return;
			//Debug.WriteLine("ccc");

			//SetParent(Handle, pWnd);
		}

		public bool enumWindowsProc(IntPtr hwnd, IntPtr lParam) {
			int size = 255;
			StringBuilder lpClassName = new StringBuilder(size);
			GetClassName(hwnd, lpClassName, lpClassName.Capacity);

			string text = lpClassName.ToString();

			if (!text.Contains("WorkerW")) {
				return true;
			}

			IntPtr pWnd = FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
			if (pWnd == IntPtr.Zero) return true;
			//Debug.WriteLine("bbb");

			pWnd = FindWindowEx(pWnd, IntPtr.Zero, "SysListView32", null);
			if (pWnd == IntPtr.Zero) return true;
			//Debug.WriteLine("ccc");

			//Debug.WriteLine(text + "," + Convert.ToString((int)hwnd.ToInt32(), 16));

			IntPtr Handle = new WindowInteropHelper(GetWindow(this)).Handle;
			//Debug.WriteLine(text + "," + Convert.ToString(Handle.ToInt32(), 16) + "," + Convert.ToString(hwnd.ToInt32(), 16));
			//Debug.WriteLine("ddd:" + GetLastError());
			SetParent(Handle, pWnd);
			//Debug.WriteLine("ddd:" + GetLastError());

			return false;
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

		private void initFestival() {
			mapFestival["1/1"] = "元旦";
			mapFestival["2/14"] = "情人节";
			mapFestival["3/8"] = "妇女节";
			mapFestival["3/12"] = "植树节";
			mapFestival["4/1"] = "愚人节";
			mapFestival["5/1"] = "劳动节";
			mapFestival["6/1"] = "儿童节";
			mapFestival["8/1"] = "建军节";
			mapFestival["8/12"] = "青年节";
			mapFestival["9/10"] = "教师节";
			mapFestival["10/1"] = "国庆节";
			mapFestival["11/1"] = "万圣节";
			mapFestival["11/26"] = "感恩节";
			mapFestival["12/24"] = "平安夜";
			mapFestival["12/25"] = "圣诞节";

			mapChineseFestival["12/30"] = "除夕";
			mapChineseFestival["1/1"] = "春节";
			mapChineseFestival["1/15"] = "元宵节";
			mapChineseFestival["5/5"] = "端午节";
			mapChineseFestival["7/7"] = "七夕节";
			mapChineseFestival["8/15"] = "中秋节";
			mapChineseFestival["9/9"] = "重阳节";
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
			updateDate();
		}

		string lastDay = "";
		private void updateDate() {
			DateTime date = DateTime.Now;
			lblTime.Content = date.ToString("HH;mm:ss");
			lblWeek.Content = date.ToString("dddd");
			lblDate.Content = date.ToString("yyyy/MM/dd");

			//每天刷新一次
			string temp = date.Month + "/" + date.Day;
			if (temp == lastDay) {
				return;
			}
			lastDay = temp;

			//节日
			if (mapFestival.ContainsKey(lastDay)) {
				lblFestival.Content = mapFestival[lastDay];
			} else {
				//lblFestival.Content = lblWeek.Content;
				lblFestival.Content = "";
			}
			
			//农历
			lblChineseDate.Content = GetChineseDateTime(date, out int mounth, out int day);
			
			//农历节日
			string lastDayChinese = mounth + "/" + day;
			if (mapChineseFestival.ContainsKey(lastDayChinese)) {
				lblFestival.Content = mapChineseFestival[lastDayChinese];
			}
		}

		string[] months = { "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二" };
		///<summary>返回农历月</summary>
		public string GetLunisolarMonth(int month) {
			if (month < 13 && month > 0) {
				return months[month - 1];
			}

			return "零";
		}

		string[] days = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
		string[] days1 = { "初", "十", "廿", "三" };
		///<summary>返回农历日</summary>
		public string GetLunisolarDay(int day) {
			if (day > 0 && day < 32) {
				if (day != 20 && day != 30) {
					return string.Concat(days1[(day - 1) / 10], days[(day - 1) % 10]);
				} else {
					return string.Concat(days[(day - 1) / 10], days1[1]);
				}
			}

			return "零零";
		}

		///<summary>根据公历获取农历日期</summary>
		public string GetChineseDateTime(DateTime datetime, out int month, out int day) {
			int year = ChineseCalendar.GetYear(datetime);
			month = ChineseCalendar.GetMonth(datetime);
			day = ChineseCalendar.GetDayOfMonth(datetime);
			//获取闰月， 0 则表示没有闰月
			int leapMonth = ChineseCalendar.GetLeapMonth(year);

			bool isleap = false;

			if (leapMonth > 0) {
				if (leapMonth == month) {
					//闰月
					isleap = true;
					month--;
				} else if (month > leapMonth) {
					month--;
				}
			}

			return (isleap ? "闰" : "") + GetLunisolarMonth(month) + "月" + GetLunisolarDay(day);
		}

		public uint stringToKeyCode(string strKey) {
			//Dictionary<string, Key> mapKey = new Dictionary<string, Key>();

			uint keyData = 0;
			string[] arrData = strKey.Split('+');

			int count = Math.Min(arrData.Length, 4);
			for (int i = 0; i < count; ++i) {
				if (mapKey.ContainsKey(arrData[i])) {
					keyData = keyData | ((uint)(byte)mapKey[arrData[i]]) << 24;

					//switch (mapKey[arrData[i]]) {
					//case Key.LeftCtrl: keyData = keyData | (Byte)Key.LeftCtrl << 24; break;
					//case Key.LeftShift: keyData = keyData | (Byte)Key.LeftShift << 16; break;
					//case Key.LeftAlt: keyData = keyData | (Byte)Key.LeftAlt << 8; break;
					//default: keyData = keyData & 0xffffff00 | (Byte)mapKey[arrData[i]]; break;
					//}
					//keyData = keyData | (UInt32)((Byte)KeyCvt.ins.mapKey[arrData[i]] << ((3 - i) * 8));
				}
			}
			//keyData = keyData | (Byte)KeyCvt.ins.mapKey[arrData[arrData.Length - 1]];
			return keyData;
		}

		private void switchDateInfo() {
			isShowChineseDate = !isShowChineseDate;

			lblWeek.Visibility = !isShowChineseDate ? Visibility.Visible : Visibility.Collapsed;
			lblDate.Visibility = !isShowChineseDate ? Visibility.Visible : Visibility.Collapsed;
			lblFestival.Visibility = isShowChineseDate ? Visibility.Visible : Visibility.Collapsed;
			lblChineseDate.Visibility = isShowChineseDate ? Visibility.Visible : Visibility.Collapsed;
		}

		private void checkKey(uint key) {
			if (key == keyCode) {
				//Debug.WriteLine($"aa:{keyData}");
				//Debug.WriteLine("aaa");
				switchDateInfo();
			}
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
