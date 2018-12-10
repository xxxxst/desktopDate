using csharpHelp.ui;
using desktopDate.model;
using desktopDate.services;
using desktopDate.util;
using desktopDate.view.util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace desktopDate.view {
	/// <summary>
	/// ClockBox.xaml 的交互逻辑
	/// </summary>
	public partial class ClockBox : UserControl {
		ObservableCollection<ClockVM> lstClockVM = new ObservableCollection<ClockVM>();

		List<ClockModel> arrClock = null;
		//int nowSecond = 0;
		//bool isStart = false;

		private ClockVM editVM = null;
		private EditTimeType editType = EditTimeType.Hour;
		private int oldValue = 0;

		public ClockBox() {
			InitializeComponent();

			if(DesignerProperties.GetIsInDesignMode(this)) {
				return;
			}

			grdSetting.Visibility = Visibility.Collapsed;
			txtMusic.Text = MainModel.ins.cfgMd.timerMusicPath;

			arrClock = MainModel.ins.cfgMd.lstClock;
			for(int i = 0; i < arrClock.Count; ++i) {
				ClockVM vm = new ClockVM();
				vm.Index = i;
				vm.md = arrClock[i];
				vm.Desc = arrClock[i].desc;
				vm.IsEnable = arrClock[i].isEnable;
				//vm.Time = formatTime(totalSecond);
				vm.Hour = arrClock[i].hour;
				vm.Minute = arrClock[i].minute;
				lstClockVM.Add(vm);
			}

			lstTimer.ItemsSource = lstClockVM;
			
			ClockServer.ins.onPlayMusicStart = () => {
				Dispatcher.Invoke(() => {
					var win = MainModel.ins.detailWin;
					win.show(DetailWinViewBox.Clock);

					updateTimerStatus();
				});
			};

			ClockServer.ins.onPlayMusicFinished = () => {
				Dispatcher.Invoke(updateTimerStatus);
			};

			updateTimerStatus();

			NowTimeServer.ins.timer.Tick += new EventHandler(timerProc);
		}

		private void timerProc(object sender, EventArgs e) {
			DateTime date = DateTime.Now;
			lblNowTime.Content = date.ToString("HH;mm:ss");
		}

		private void updateTimerStatus() {
			ClockServer ins = ClockServer.ins;

			btnStop.Visibility = ins.isPlay() ? Visibility.Visible : Visibility.Collapsed;
			btnSetting.Visibility = !ins.isPlay() ? Visibility.Visible : Visibility.Collapsed;

			imgIcon.Visibility = !ins.isPlay() ? Visibility.Visible : Visibility.Collapsed;
			imgIconRotate.Visibility = ins.isPlay() ? Visibility.Visible : Visibility.Collapsed;
		}

		private void btnStop_Click(object sender, RoutedEventArgs e) {
			ClockServer.ins.stop();
			updateTimerStatus();
		}

		private void btnAdd_Click(object sender, RoutedEventArgs e) {
			ClockModel md = new ClockModel();
			ClockVM vm = new ClockVM() {
				Index = lstClockVM.Count,
				md = md,
				IsNew = true,
			};

			foreach(var item in lstClockVM) {
				item.IsNew = false;
			}

			arrClock.Add(md);
			lstClockVM.Add(vm);
			lstTimer.ScrollIntoView(lstClockVM.Last());

			AutoSaveServer.ins.hasChanged = true;
		}

		private void btnSetting_Click(object sender, RoutedEventArgs e) {
			btnSetting.IsSelect = !btnSetting.IsSelect;
			grdTimer.Visibility = btnSetting.IsSelect ? Visibility.Collapsed : Visibility.Visible;
			grdSetting.Visibility = !btnSetting.IsSelect ? Visibility.Collapsed : Visibility.Visible;
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e) {
			ClockVM vm = (sender as MiniButton).Tag as ClockVM;

			arrClock.RemoveAt(vm.Index);
			lstClockVM.RemoveAt(vm.Index);
			for(int i = vm.Index; i < lstClockVM.Count; ++i) {
				lstClockVM[i].Index = i;
			}

			AutoSaveServer.ins.hasChanged = true;
		}

		private void txtMusic_TextChanged(object sender, TextChangedEventArgs e) {
			MainModel.ins.cfgMd.clockMusicPath = txtMusic.Text;
			//Debug.WriteLine(txtMusic.Text);

			AutoSaveServer.ins.hasChanged = true;
		}

		private void lblHour_Click(object sender, RoutedEventArgs e) {
			ClockVM vm = (sender as MiniButton).Tag as ClockVM;
			initEditMode(vm, EditTimeType.Hour);
		}

		private void lblMinute_Click(object sender, RoutedEventArgs e) {
			ClockVM vm = (sender as MiniButton).Tag as ClockVM;
			initEditMode(vm, EditTimeType.Minute);
		}

		private void txtDesc_TextChanged(object sender, TextChangedEventArgs e) {
			ClockVM vm = (sender as TextBox).Tag as ClockVM;
			vm.md.desc = vm.Desc;
			vm.IsNew = false;

			AutoSaveServer.ins.hasChanged = true;
		}

		private void chekEnable_Checked(object sender, RoutedEventArgs e) {
			ClockVM vm = (sender as RoundCheckBox).Tag as ClockVM;
			vm.md.isEnable = (vm.IsEnable == true);

			AutoSaveServer.ins.hasChanged = true;
		}

		private void chekEnable_Unchecked(object sender, RoutedEventArgs e) {
			ClockVM vm = (sender as RoundCheckBox).Tag as ClockVM;
			vm.md.isEnable = (vm.IsEnable == true);

			AutoSaveServer.ins.hasChanged = true;
		}

		private void initEditMode(ClockVM vm, EditTimeType type) {
			editVM = vm;
			editVM.IsEdit = true;
			MainModel.ins.isEditTime = true;
			editType = type;
			switch(type) {
				case EditTimeType.Hour: oldValue = editVM.Hour; break;
				case EditTimeType.Minute: oldValue = editVM.Minute; break;
				case EditTimeType.Second: oldValue = editVM.Second; break;
			}
		}

		private void grdTimeEditMask_MouseMove(object sender, MouseEventArgs e) {
			Grid grd = sender as Grid;
			if(grd == null) {
				return;
			}

			double x = e.GetPosition(grd).X;

			double w = grd.ActualWidth;
			switch(editType) {
				case EditTimeType.Hour: editVM.Hour = calcTimeVal(x, w, 23); break;
				case EditTimeType.Minute: editVM.Minute = calcTimeVal(x, w, 59); break;
				case EditTimeType.Second: editVM.Second = calcTimeVal(x, w, 59); break;
			}
		}

		private int calcTimeVal(double x, double w, int maxVal) {
			const int gapL = 20;
			const int gapR = 60;
			const int gap = gapL + gapR;

			int val = 0;
			val = (int)((x - gapL) / (w - gap) * (maxVal + 1));
			val = Math.Max(val, 0);
			val = Math.Min(val, maxVal);

			return val;
		}

		private void grdTimeEditMask_MouseUp(object sender, MouseButtonEventArgs e) {
			if(editVM == null) {
				return;
			}

			if(e.ChangedButton != MouseButton.Right && e.ChangedButton != MouseButton.Left) {
				return;
			}

			if(e.ChangedButton == MouseButton.Right) {
				switch(editType) {
					case EditTimeType.Hour: editVM.Hour = oldValue; break;
					case EditTimeType.Minute: editVM.Minute = oldValue; break;
				}
			} else if(e.ChangedButton == MouseButton.Left) {
				switch(editType) {
					case EditTimeType.Hour: editVM.md.hour = editVM.Hour; break;
					case EditTimeType.Minute: editVM.md.minute = editVM.Minute; break;
				}

				AutoSaveServer.ins.hasChanged = true;
			}

			editVM.IsEdit = false;
			editVM = null;
			MainModel.ins.isEditTime = false;

			return;
		}

		private void btnClearMusic_Click(object sender, RoutedEventArgs e) {
			txtMusic.Text = "";
		}

	}

	public class ClockVM : INotifyPropertyChanged {
		int _Index = 0;
		public int Index {
			get { return _Index; }
			set { _Index = value; updatePro("Index"); }
		}

		public ClockModel md = null;

		//desc
		string _Desc = "";
		public string Desc {
			get { return _Desc; }
			set { _Desc = value; updatePro("Desc"); }
		}

		//Is Enable
		bool? _IsEnable = true;
		public bool? IsEnable {
			get { return _IsEnable; }
			set { _IsEnable = value; updatePro("IsEnable"); }
		}

		//hour
		int _Hour = 0;
		public int Hour {
			get { return _Hour; }
			set { _Hour = value; updatePro("StrHour"); }
		}

		public string StrHour {
			get { return _Hour.ToString().PadLeft(2, '0'); }
		}

		//minute
		int _Minute = 0;
		public int Minute {
			get { return _Minute; }
			set { _Minute = value; updatePro("StrMinute"); }
		}

		public string StrMinute {
			get { return _Minute.ToString().PadLeft(2, '0'); }
		}

		//second
		int _Second = 0;
		public int Second {
			get { return _Second; }
			set { _Second = value; updatePro("StrSecond"); }
		}

		public string StrSecond {
			get { return _Second.ToString().PadLeft(2, '0'); }
		}

		//is new
		bool _IsNew = false;
		public bool IsNew {
			get { return _IsNew; }
			set { _IsNew = value; updatePro("IsNewVisibility"); }
		}

		public object IsNewVisibility {
			get { return _IsNew ? Visibility.Visible : Visibility.Collapsed; }
		}

		//edit
		bool _IsEdit = false;
		public bool IsEdit {
			get { return _IsEdit; }
			set { _IsEdit = value; updatePro("IsEditVisibility"); updatePro("IsNotEditVisibility"); }
		}

		public Visibility IsEditVisibility {
			get { return _IsEdit ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility IsNotEditVisibility {
			get { return !_IsEdit ? Visibility.Visible : Visibility.Collapsed; }
		}

		//
		public virtual event PropertyChangedEventHandler PropertyChanged;
		public virtual void updatePro(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public static SolidColorBrush createBrush(string color) {
			return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
		}
	}
}
