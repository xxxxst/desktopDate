using desktopDate.control;
using desktopDate.services;
using desktopDate.util;
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

namespace desktopDate.view {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {
		

		static Dictionary<string, Key> mapKey = new Dictionary<string, Key>();
		//public Dictionary<Key, string> mapCtlKey = new Dictionary<Key, string>();
		//public Dictionary<Key, string> mapIgnoreKey = new Dictionary<Key, string>();
		uint keyCode = 0;
		KeyboardCtl keyCtl = new KeyboardCtl();
		ChineseLunisolarCalendar ChineseCalendar = new ChineseLunisolarCalendar();
		bool isShowChineseDate = false;

		//Dictionary<string, string> mapFestival = new Dictionary<string, string>();
		//Dictionary<string, string> mapChineseFestival = new Dictionary<string, string>();

		DetailWin detailWin = null;

		public MainWindow() {
			InitializeComponent();

			FestivalServer.ins.init();

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

		//bool isFindWindow = false;
		ComUtil.CallBack enumWinCallBack = null;
		private void appendToWindow() {
			IntPtr Handle = new WindowInteropHelper(this).Handle;

			//隐藏边框
			long oldstyle = ComUtil.GetWindowLong(Handle, ComUtil.GWL_STYLE);
			ComUtil.SetWindowLong(Handle, ComUtil.GWL_STYLE, oldstyle & (~(ComUtil.WS_CAPTION | ComUtil.WS_CAPTION_2)) | ComUtil.WS_EX_LAYERED);

			//不在Alt+Tab中显示
			long oldExStyle = ComUtil.GetWindowLong(Handle, ComUtil.GWL_EXSTYLE);
			ComUtil.SetWindowLong(Handle, ComUtil.GWL_EXSTYLE, oldExStyle & (~ComUtil.WS_EX_APPWINDOW) | ComUtil.WS_EX_TOOLWINDOW);

			//SetLayeredWindowAttributes(Handle, 0, 128, LWA_COLORKEY );

			//IntPtr pWnd = FindWindow("Progman", null);
			//if(pWnd != IntPtr.Zero) {
			//	IntPtr pWnd2 = FindWindowEx(pWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
			//	if(pWnd2 != IntPtr.Zero) {
			//		//SendMessage(pWnd, 0x052c, IntPtr.Zero, IntPtr.Zero);
			//	} else {
			//		SendMessage(pWnd, 0x052c, (IntPtr)1, IntPtr.Zero);
			//	}
			//}

			enumWinCallBack = new ComUtil.CallBack(enumWindowsProc);
			ComUtil.EnumWindows(enumWinCallBack, IntPtr.Zero);
		}

		//private int count = 0;
		public bool enumWindowsProc(IntPtr hwnd, IntPtr lParam) {
			int size = 255;
			StringBuilder lpClassName = new StringBuilder(size);
			ComUtil.GetClassName(hwnd, lpClassName, lpClassName.Capacity);

			string text = lpClassName.ToString();

			if(!text.Contains("WorkerW")) {
				return true;
			}

			//if (!text.Contains("WorkerW") && !text.Contains("Progman")) {
			//	return true;
			//}
			//++count;
			//if(count < 12) {
			//	Debug.WriteLine("aa:" + count);
			//	return true;
			//}

			IntPtr pWnd = hwnd;

			pWnd = ComUtil.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
			if(pWnd == IntPtr.Zero) return true;

			pWnd = ComUtil.FindWindowEx(pWnd, IntPtr.Zero, "SysListView32", null);
			if(pWnd == IntPtr.Zero) return true;

			//isFindWindow = true;
			IntPtr Handle = new WindowInteropHelper(GetWindow(this)).Handle;
			ComUtil.SetParent(Handle, pWnd);

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

			if (ComUtil.DwmIsCompositionEnabled()) {
				ComUtil.MARGINS margin = new ComUtil.MARGINS();
				margin.Right = margin.Left = margin.Bottom = margin.Top = -1;
				ComUtil.DwmExtendFrameIntoClientArea(Handle, ref margin);
			}

			//SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_TRANSPARENT | WS_EX_LAYERED);
			//SetLayeredWindowAttributes(Handle, 0, 128, LWA_ALPHA);
		}

		//private void initFestival() {
		//	mapFestival["01/01"] = "元旦";
		//	mapFestival["02/14"] = "情人节";
		//	mapFestival["03/08"] = "妇女节";
		//	mapFestival["03/12"] = "植树节";
		//	mapFestival["04/01"] = "愚人节";
		//	mapFestival["05/01"] = "劳动节";
		//	mapFestival["06/01"] = "儿童节";
		//	mapFestival["08/01"] = "建军节";
		//	mapFestival["08/12"] = "青年节";
		//	mapFestival["09/10"] = "教师节";
		//	mapFestival["10/01"] = "国庆节";
		//	mapFestival["11/01"] = "万圣节";
		//	mapFestival["11/26"] = "感恩节";
		//	mapFestival["12/24"] = "平安夜";
		//	mapFestival["12/25"] = "圣诞节";

		//	mapChineseFestival["12/30"] = "除夕";
		//	mapChineseFestival["01/01"] = "春节";
		//	mapChineseFestival["01/15"] = "元宵节";
		//	mapChineseFestival["05/05"] = "端午节";
		//	mapChineseFestival["07/07"] = "七夕节";
		//	mapChineseFestival["08/15"] = "中秋节";
		//	mapChineseFestival["09/09"] = "重阳节";
		//}

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
			string temp = date.ToString("MM/dd");
			if (temp == lastDay) {
				return;
			}
			lastDay = temp;

			//节日
			string festival = FestivalServer.ins.getFestival(date);
			lblFestival.Content = festival;

			//节日
			//if (mapFestival.ContainsKey(lastDay)) {
			//	lblFestival.Content = mapFestival[lastDay];
			//} else {
			//	//lblFestival.Content = lblWeek.Content;
			//	lblFestival.Content = "";
			//}
			
			//农历
			lblChineseDate.Content = GetChineseDateTime(date, out int mounth, out int day);

			//农历节日
			string chineseFestival = FestivalServer.ins.getChineseFestival(date.Year, mounth, day);
			if(chineseFestival != "") {
				lblFestival.Content = chineseFestival;
			}

			if((string)lblFestival.Content == "") {
				FestivalModel md = FestivalServer.ins.getNextFestival();
				lblFestival.Content = md.name + "(" + md.dayOfRange + "天)";
			}

			//string lastDayChinese = mounth.ToString().PadLeft(2, '0') + "/" + day.ToString().PadLeft(2, '0');
			//if (FestivalServer.ins.mapChineseFestival.ContainsKey(lastDayChinese)) {
			//	lblFestival.Content = FestivalServer.ins.mapChineseFestival[lastDayChinese];
			//}
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

			if(e.ChangedButton == MouseButton.Left) {
				if(detailWin == null) {
					detailWin = new DetailWin();

					detailWin.onClose = () => {
						detailWin = null;
					};

					detailWin.Left = Left + Width - detailWin.Width;
					detailWin.Top = Top - detailWin.Height - 10;

					detailWin.Show();
				}
			}
		}

		private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
			grdMain.Opacity = 0.6;
		}

		private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
			grdMain.Opacity = 0.5;
		}
	}
	
}
