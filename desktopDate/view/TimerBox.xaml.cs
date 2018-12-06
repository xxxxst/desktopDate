using csharpHelp.ui;
using desktopDate.model;
using desktopDate.services;
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
				vm.Time = formatTime(arrTimer[i].totalSecond);
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
					if(!win.IsVisible) {
						win.Show();
					}

					updateNowTime();
					updateTimerStatus();
				});
			};

			TimerServer.ins.onPlayMusicFinished = () => {
				Dispatcher.Invoke(updateTimerStatus);
			};

			updateTimerStatus();
			updateNowTime();
		}

		private void updateNowTime() {
			lblNowTime.Content = formatTime(nowSecond);
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

		private string formatTime(int totalSecond) {
			int hour = totalSecond / 3600;
			int minute = (totalSecond - hour * 3600) / 60;
			int second = totalSecond % 60;

			string strHour = hour.ToString().PadLeft(2, '0');
			string strMinute = minute.ToString().PadLeft(2, '0');
			string strSecond = second.ToString().PadLeft(2, '0');

			return $"{strHour}:{strMinute}:{strSecond}";
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
		}

		private void btnSetting_Click(object sender, RoutedEventArgs e) {
			btnSetting.IsSelect = !btnSetting.IsSelect;
			grdTimer.Visibility = btnSetting.IsSelect ? Visibility.Collapsed : Visibility.Visible;
			grdSetting.Visibility = !btnSetting.IsSelect ? Visibility.Collapsed : Visibility.Visible;
		}

		private void btnStart_Click(object sender, RoutedEventArgs e) {
			TimerVM vm = (sender as MiniButton).Tag as TimerVM;
			nowSecond = vm.md.totalSecond;

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
		}

		private void txtMusic_TextChanged(object sender, TextChangedEventArgs e) {
			MainModel.ins.cfgMd.timerMusicPath = txtMusic.Text;
			//Debug.WriteLine(txtMusic.Text);
		}
	}

	public class TimerVM : INotifyPropertyChanged {
		int _Index = 0;
		public int Index {
			get { return _Index; }
			set { _Index = value; updatePro("Index"); }
		}

		public TimerModel md = null;

		string _Time = "00:00:00";
		public string Time {
			get { return _Time; }
			set { _Time = value; updatePro("Time"); }
		}

		bool _IsNew = false;
		public bool IsNew {
			get { return _IsNew; }
			set { _IsNew = value; updatePro("IsNew"); updatePro("IsNewVisibility"); }
		}

		public Visibility IsNewVisibility {
			get { return _IsNew ? Visibility.Visible : Visibility.Collapsed; }
		}

		public virtual event PropertyChangedEventHandler PropertyChanged;
		public virtual void updatePro(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public static SolidColorBrush createBrush(string color) {
			return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
		}
	}

}
