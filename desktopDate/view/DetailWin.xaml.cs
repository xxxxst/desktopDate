using desktopDate.services;
using desktopDate.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	/// DetailWin.xaml 的交互逻辑
	/// </summary>
	public partial class DetailWin : Window {
		public Action onClose = null;

		List<FestivalModel> arrFestival = new List<FestivalModel>();
		List<FestivalVM> arrFestivalVM = new List<FestivalVM>();

		public DetailWin() {
			InitializeComponent();

			arrFestival = FestivalServer.ins.getListFestival();
			for(int i = 0; i < arrFestival.Count; ++i) {
				FestivalModel md = arrFestival[i];

				FestivalVM vm = new FestivalVM();
				vm.Name = md.name;
				vm.Time = md.time;
				int day = md.dayOfRange;
				//if(day < 0) {
				//	//vm.DayOfRange = "结束";
				//	vm.DayOfRange = "距离1年" + (0 - day) + "天";
				//	vm.dayOfRangeType = DayOfRangeType.Finish;
				//} else 
				if(day == 0) {
					vm.DayOfRange = "今天";
					vm.dayOfRangeType = DayOfRangeType.Now;
				} else {
					if(md.isFinished) {
						//vm.DayOfRange = "距离1年" + (day - 365) + "天";
						vm.dayOfRangeType = DayOfRangeType.Finish;
					} else {
						vm.dayOfRangeType = DayOfRangeType.Wait;
					}

					//if(day > 365) {
					//	vm.DayOfRange = "距离" + (day/365) + "年" + (day % 365) + "天";
					//} else {
					//	vm.DayOfRange = "距离" + day + "天";
					//}
					vm.DayOfRange = "距离" + day + "天";
				}
				arrFestivalVM.Add(vm);
			}

			lstFestival.ItemsSource = arrFestivalVM;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			IntPtr Handle = new WindowInteropHelper(this).Handle;

			//隐藏边框
			long oldstyle = ComUtil.GetWindowLong(Handle, ComUtil.GWL_STYLE);
			ComUtil.SetWindowLong(Handle, ComUtil.GWL_STYLE, oldstyle & (~(ComUtil.WS_CAPTION | ComUtil.WS_CAPTION_2)) | ComUtil.WS_EX_LAYERED);

			//不在Alt+Tab中显示
			long oldExStyle = ComUtil.GetWindowLong(Handle, ComUtil.GWL_EXSTYLE);
			ComUtil.SetWindowLong(Handle, ComUtil.GWL_EXSTYLE, oldExStyle & (~ComUtil.WS_EX_APPWINDOW) | ComUtil.WS_EX_TOOLWINDOW);
		}

		private void Window_Deactivated(object sender, EventArgs e) {
			onClose?.Invoke();

			Close();
		}
	}

	public class FestivalVM : INotifyPropertyChanged {
		public static Brush finishColor = createBrush("#bababa");
		public static Brush nowColor = createBrush("#00ff00");
		public static Brush waitColor = createBrush("#ffffff");

		string _Name = "";
		public string Name {
			get { return _Name; }
			set { _Name = value; updatePro("Name"); }
		}

		string _Time = "";
		public string Time {
			get { return _Time; }
			set { _Time = value; updatePro("Time"); }
		}
		
		string _DayOfRange = "";
		public string DayOfRange {
			get { return _DayOfRange; }
			set { _DayOfRange = value; updatePro("DayOfRange"); }
		}

		public DayOfRangeType dayOfRangeType = DayOfRangeType.Finish;
		public Brush DayOfRangeColor {
			get {
				switch(dayOfRangeType) {
					case DayOfRangeType.Finish: return finishColor;
					case DayOfRangeType.Now: return nowColor;
					case DayOfRangeType.Wait: return waitColor;
					default: return finishColor;
				}
			}
			set { updatePro("DayOfRangeColor"); }
		}

		public virtual event PropertyChangedEventHandler PropertyChanged;
		public virtual void updatePro(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public static SolidColorBrush createBrush(string color) {
			return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
		}
	}

	public enum DayOfRangeType {
		Finish, Now, Wait
	}
}
