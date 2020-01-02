using csharpHelp.services;
using csharpHelp.util;
using desktopDate.control;
using desktopDate.model;
using desktopDate.services;
using desktopDate.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
		public static MainWindow ins = null;

		static Dictionary<string, Key> mapKey = new Dictionary<string, Key>();
		//public Dictionary<Key, string> mapCtlKey = new Dictionary<Key, string>();
		//public Dictionary<Key, string> mapIgnoreKey = new Dictionary<Key, string>();
		uint keyCode = 0;
		KeyboardCtl keyCtl = new KeyboardCtl();
		//ChineseLunisolarCalendar ChineseCalendar = new ChineseLunisolarCalendar();
		bool isShowChineseDate = false;

		//XmlCtl xmlCfg = null;
		DetailWin detailWin = null;
		XmlModelServer xmlCfgServer = null;
		ConfigCtl cfgCtl = new ConfigCtl();

		object saveLock = new object();

		List<FestivalInfo> arrFestival = new List<FestivalInfo>();
		Dictionary<string, FestivalInfo> mapTimeToFestival = new Dictionary<string, FestivalInfo>();

		public MainWindow() {
			InitializeComponent();

			ins = this;
			MainModel.ins.mainWin = this;
			
			//xmlCfg = new XmlCtl();
			//xmlCfg.load("config.xml");
			string configPath = SysConst.configPath();
			MainModel.ins.cfgMd = new ConfigModel();
			xmlCfgServer = new XmlModelServer(MainModel.ins.cfgMd, configPath);
			try{
				xmlCfgServer.loadFromXml();
				//ConfigModel md = MainModel.ins.cfgMd;
				
				//for(int i = 0; i < md.lstBox.Count; ++i) {
				//	Debug.WriteLine("11:" + md.lstBox[i].timer);
				//}
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
			cfgCtl.load();
			if(!File.Exists(configPath)) {
				saveConfig();
			}

			EventServer.ins.onConfigLoaded();

			//ignoreMouseEvent();
			setPos();
		}

		public static IntPtr getHandle() {
			return new WindowInteropHelper(ins).Handle;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			appendToWindow();
			
			NowTimeServer.ins.init();
			FestivalServer.ins.init();
			ClockServer.ins.init();
			TimerServer.ins.init();
			AutoSaveServer.ins.init();
			
			initDetailWin();

			updateDate();

			//initKeys();
			//keyCode = stringToKeyCode("RightCtrl");
			//keyCtl.checkKey = checkKey;
			//keyCtl.init();

			//timer = new DispatcherTimer();
			//timer.Tick += new EventHandler(timerProc);
			//timer.Interval = new TimeSpan(0, 0, 0, 1);
			//timer.Start();
			NowTimeServer.ins.timer.Tick += new EventHandler(timerProc);
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
			int oldstyle = ComUtil.GetWindowLong(Handle, ComUtil.GWL_STYLE);
			ComUtil.SetWindowLong(Handle, ComUtil.GWL_STYLE, oldstyle & (~(ComUtil.WS_CAPTION | ComUtil.WS_CAPTION_2)) | ComUtil.WS_EX_LAYERED);

			//不在Alt+Tab中显示
			int oldExStyle = ComUtil.GetWindowLong(Handle, ComUtil.GWL_EXSTYLE);
			ComUtil.SetWindowLong(Handle, ComUtil.GWL_EXSTYLE, oldExStyle & (~ComUtil.WS_EX_APPWINDOW) | ComUtil.WS_EX_TOOLWINDOW);

			//win7
			if(Environment.OSVersion.Version.Major <= 6.1) {
				return;
			}

			IntPtr pWnd = ComUtil.FindWindow("Progman", null);
			if (pWnd != IntPtr.Zero) {
				IntPtr pWnd2 = ComUtil.FindWindowEx(pWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
				if (pWnd2 != IntPtr.Zero) {
					ComUtil.SendMessage(pWnd, 0x052c, IntPtr.Zero, IntPtr.Zero);
				} else {
					//SendMessage(pWnd, 0x052c, (IntPtr)1, IntPtr.Zero);
				}
			}

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

		private void setPos() {
			ConfigModel md = MainModel.ins.cfgMd;
			if(md.dx <= 0 || md.dx >= 99999) {
				md.dx = 0;
			}
			if(md.dy <= 0 || md.dy >= 99999) {
				md.dy = 0;
			}
			if(md.width <= 0 || md.width >= 99999) {
				md.width = (int)Width;
			}
			if(md.height <= 0 || md.height >= 99999) {
				md.height = (int)Height;
			}

			Width = md.width;
			Height = md.height;

			int w = 0;
			int h = Screen.PrimaryScreen.Bounds.Height;
			for (int i = 0; i < Screen.AllScreens.Length; ++i) {
				w += Screen.AllScreens[i].Bounds.Width;
			}
			switch(md.winAlign) {
				case WinAlign.LeftTop: {
					Left = md.dx;
					Top = md.dy;
					break;
				}
				case WinAlign.RightTop: {
					Left = w - Width - md.dx;
					Top = md.dy;
					break;
				}
				case WinAlign.LeftBottom: {
					Left = md.dx;
					Top = h - Height - md.dy;
					break;
				}
				case WinAlign.RightBottom:
				default: {
					Left = w - Width - md.dx;
					Top = h - Height - md.dy;
					break;
				}
			}
			//Left = w - Width;
			//Top = h - Height;

			if(Height < 40) {
				rowLeft1.Height = new GridLength(0);
				rowRight2.Height = new GridLength(0);
			}
		}

		private void initDetailWin() {
			detailWin = new DetailWin();
			MainModel.ins.detailWin = detailWin;

			refreshDate();

			detailWin.onClose = () => {
				//detailWin = null;
			};

			ConfigModel md = MainModel.ins.cfgMd;
			switch(md.winAlign) {
				case WinAlign.LeftTop: {
					detailWin.Left = Left;
					detailWin.Top = Top + Height+ 10;
					break;
				}
				case WinAlign.RightTop: {
					detailWin.Left = Left + Width - detailWin.Width;
					detailWin.Top = Top + Height + 10;
					break;
				}
				case WinAlign.LeftBottom: {
					detailWin.Left = Left;
					detailWin.Top = Top - detailWin.Height - 10;
					break;
				}
				case WinAlign.RightBottom:
				default: {
					detailWin.Left = Left + Width - detailWin.Width;
					detailWin.Top = Top - detailWin.Height - 10;
					break;
				}
			}
			//detailWin.Left = Left + Width - detailWin.Width;
			//detailWin.Top = Top - detailWin.Height - 10;
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

			refreshDate();
		}

		private void refreshDate() {
			DateTime date = DateTime.Now;

			arrFestival = FestivalServer.ins.getNextYearFestival();
			mapTimeToFestival = new Dictionary<string, FestivalInfo>();
			for (int i = 0; i < arrFestival.Count; ++i) {
				mapTimeToFestival[arrFestival[i].showTime] = arrFestival[i];
			}

			string strNowTime = date.ToString("MM/dd");
			string festivalDesc = "";
			if (mapTimeToFestival.ContainsKey(strNowTime)) {
				festivalDesc = mapTimeToFestival[strNowTime].desc;
			}
			if (festivalDesc == "") {
				var md = arrFestival.First();
				festivalDesc = md.desc + "(" + md.dayOfRange + "天)";
			}
			lblFestival.Content = festivalDesc;

			//农历
			lblChineseDate.Content = ChineseDateServer.ins.GetChineseDateTime(date, out int mounth, out int day);
			
			detailWin?.updateDate(arrFestival);

			////节日
			//string festival = FestivalServer.ins.getFestival(date);
			//lblFestival.Content = festival;

			////农历
			//lblChineseDate.Content = ChineseDateServer.ins.GetChineseDateTime(date, out int mounth, out int day);

			////农历节日
			//string chineseFestival = FestivalServer.ins.getChineseFestival(date.Year, mounth, day);
			//if(chineseFestival != "") {
			//	lblFestival.Content = chineseFestival;
			//}

			//if((string)lblFestival.Content == "") {
			//	FestivalModel md = FestivalServer.ins.getNextFestival();
			//	lblFestival.Content = md.name + "(" + md.dayOfRange + "天)";
			//}
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
				//if(detailWin == null) {
				//	detailWin = new DetailWin();
				//	MainModel.ins.detailWin = detailWin;

				//	detailWin.onClose = () => {
				//		//detailWin = null;
				//	};

				//	detailWin.Left = Left + Width - detailWin.Width;
				//	detailWin.Top = Top - detailWin.Height - 10;

				//	detailWin.Show();
				//} else {
				//	detailWin.Show();
				//}
				detailWin.Show();
			}
		}

		private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
			grdMain.Opacity = 0.6;
		}

		private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
			grdMain.Opacity = 0.5;
		}

		private void Window_Closed(object sender, EventArgs e) {
			try {
				bool hasChanged = AutoSaveServer.ins.hasChanged;

				AutoSaveServer.ins.clear();
				NowTimeServer.ins.clear();
				TimerServer.ins.clear();
				ClockServer.ins.clear();

				if(detailWin != null) {
					detailWin.Close();
					detailWin = null;
				}

				if(hasChanged) {
					saveConfig();
				}
				//xmlCfg.save();
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void saveConfig() {
			lock(saveLock) {
				try {
					cfgCtl.save();
					xmlCfgServer.save();
				} catch(Exception) {

				}
			}
		}
	}
	
}
