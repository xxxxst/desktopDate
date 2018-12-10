using csharpHelp.ui;
using desktopDate.model;
using desktopDate.services;
using desktopDate.util;
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
	/// TimerBox.xaml 的交互逻辑
	/// </summary>
	public partial class TimerBox : UserControl {
		ObservableCollection<TimerVM> lstTimerVM = new ObservableCollection<TimerVM>();

		List<TimerModel> arrTimer = null;
		int nowSecond = 0;
		//bool isStart = false;

		private TimerVM editVM = null;
		private EditTimeType editType = EditTimeType.Hour;
		private int oldValue = 0;
		private TimerVM selectVM = null;

		public TimerBox() {
			InitializeComponent();

			if(DesignerProperties.GetIsInDesignMode(this)) {
				return;
			}

			grdSetting.Visibility = Visibility.Collapsed;
			txtMusic.Text = MainModel.ins.cfgMd.timerMusicPath;

			arrTimer = MainModel.ins.cfgMd.lstTimer;
			for(int i = 0; i < arrTimer.Count; ++i) {
				TimerVM vm = new TimerVM();
				vm.Index = i;
				vm.md = arrTimer[i];
				//int totalSecond = TimeFormat.getTotalSecond(arrTimer[i].hour, arrTimer[i].minute, arrTimer[i].second);
				//vm.Time = formatTime(totalSecond);
				vm.Hour = arrTimer[i].hour;
				vm.Minute = arrTimer[i].minute;
				vm.Second = arrTimer[i].second;
				lstTimerVM.Add(vm);
			}

			lstTimer.ItemsSource = lstTimerVM;

			TimerServer.ins.onTimerUpdated = (int _nowSecond, int _totalSecond) => {
				nowSecond = _nowSecond;
				Dispatcher.Invoke(updateNowTime);
			};

			TimerServer.ins.onTimerFinished = () => {
				nowSecond = 0;
				Dispatcher.Invoke(() => {
					var win = MainModel.ins.detailWin;
					win.show(DetailWinViewBox.Timer);

					updateNowTime();
					updateTimerStatus();
				});
			};

			TimerServer.ins.onPlayMusicFinished = () => {
				Dispatcher.Invoke(() => {
					if(selectVM != null) {
						selectVM.IsSelect = false;
						selectVM = null;
					}
					updateTimerStatus();
				});
			};

			updateTimerStatus();
			updateNowTime();
		}

		private void updateNowTime() {
			lblNowTime.Content = TimeFormat.formatTime(nowSecond);
		}

		private void updateTimerStatus() {
			TimerServer ins = TimerServer.ins;
			btnStartNow.Visibility = ins.isStart() && ins.isPause ? Visibility.Visible : Visibility.Collapsed;
			btnPause.Visibility = ins.isStart() && !ins.isPause ? Visibility.Visible : Visibility.Collapsed;
			btnStop.Visibility = ins.isStart() || ins.isPlay() ? Visibility.Visible : Visibility.Collapsed;
			btnSetting.Visibility = !ins.isStart() && !ins.isPlay() ? Visibility.Visible : Visibility.Collapsed;

			imgIcon.Visibility = !ins.isPlay() ? Visibility.Visible : Visibility.Collapsed;
			imgIconRotate.Visibility = ins.isPlay() ? Visibility.Visible : Visibility.Collapsed;
		}

		private void btnStartNow_Click(object sender, RoutedEventArgs e) {
			TimerServer.ins.start();
			updateTimerStatus();
		}

		private void btnPause_Click(object sender, RoutedEventArgs e) {
			TimerServer.ins.pause();
			updateTimerStatus();
		}

		private void btnStop_Click(object sender, RoutedEventArgs e) {
			if(selectVM != null) {
				selectVM.IsSelect = false;
				selectVM = null;
			}

			TimerServer.ins.stop();
			nowSecond = 0;
			updateTimerStatus();
			updateNowTime();
		}

		private void btnAdd_Click(object sender, RoutedEventArgs e) {
			TimerModel md = new TimerModel();
			TimerVM vm = new TimerVM() {
				Index = lstTimerVM.Count,
				md = md,
				IsNew = true,
			};

			foreach(var item in lstTimerVM) {
				item.IsNew = false;
			}

			arrTimer.Add(md);
			lstTimerVM.Add(vm);
			lstTimer.ScrollIntoView(lstTimerVM.Last());

			AutoSaveServer.ins.hasChanged = true;
		}

		private void btnSetting_Click(object sender, RoutedEventArgs e) {
			btnSetting.IsSelect = !btnSetting.IsSelect;
			grdTimer.Visibility = btnSetting.IsSelect ? Visibility.Collapsed : Visibility.Visible;
			grdSetting.Visibility = !btnSetting.IsSelect ? Visibility.Collapsed : Visibility.Visible;
		}

		private void btnStart_Click(object sender, RoutedEventArgs e) {
			TimerVM vm = (sender as MiniButton).Tag as TimerVM;
			nowSecond = TimeFormat.getTotalSecond(vm.md.hour, vm.md.minute, vm.md.second);
			//nowSecond = vm.md.totalSecond;

			if(selectVM != null) {
				selectVM.IsSelect = false;
			}
			selectVM = vm;
			selectVM.IsSelect = true;

			TimerServer.ins.restart(vm.md);

			updateNowTime();
			updateTimerStatus();
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e) {
			TimerVM vm = (sender as MiniButton).Tag as TimerVM;

			arrTimer.RemoveAt(vm.Index);
			lstTimerVM.RemoveAt(vm.Index);
			for(int i = vm.Index; i < lstTimerVM.Count; ++i) {
				lstTimerVM[i].Index = i;
			}

			AutoSaveServer.ins.hasChanged = true;
		}

		private void txtMusic_TextChanged(object sender, TextChangedEventArgs e) {
			MainModel.ins.cfgMd.timerMusicPath = txtMusic.Text;
			//Debug.WriteLine(txtMusic.Text);

			AutoSaveServer.ins.hasChanged = true;
		}

		private void lblHour_Click(object sender, RoutedEventArgs e) {
			TimerVM vm = (sender as MiniButton).Tag as TimerVM;
			initEditMode(vm, EditTimeType.Hour);
		}

		private void lblMinute_Click(object sender, RoutedEventArgs e) {
			TimerVM vm = (sender as MiniButton).Tag as TimerVM;
			initEditMode(vm, EditTimeType.Minute);
		}

		private void lblSecond_Click(object sender, RoutedEventArgs e) {
			TimerVM vm = (sender as MiniButton).Tag as TimerVM;
			initEditMode(vm, EditTimeType.Second);
		}

		private void initEditMode(TimerVM vm, EditTimeType type) {
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
			val = (int)((x - gapL) / (w - gap) * (maxVal+1));
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
					case EditTimeType.Second: editVM.Second = oldValue; break;
				}
			} else if(e.ChangedButton == MouseButton.Left) {
				switch(editType) {
					case EditTimeType.Hour: editVM.md.hour = editVM.Hour; break;
					case EditTimeType.Minute: editVM.md.minute = editVM.Minute; break;
					case EditTimeType.Second: editVM.md.second = editVM.Second; break;
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

	public class TimerVM : INotifyPropertyChanged {
		int _Index = 0;
		public int Index {
			get { return _Index; }
			set { _Index = value; updatePro("Index"); }
		}

		public TimerModel md = null;

		//string _Time = "00:00:00";
		//public string Time {
		//	get { return _Time; }
		//	set { _Time = value; updatePro("Time"); }
		//}

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

		//Is Select
		bool _IsSelect = false;
		public bool IsSelect {
			get { return _IsSelect; }
			set { _IsSelect = value; updatePro("IsSelect"); }
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
