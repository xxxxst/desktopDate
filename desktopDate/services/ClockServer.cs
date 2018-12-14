using desktopDate.control;
using desktopDate.model;
using desktopDate.view;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace desktopDate.services {
	public class ClockServer {
		public static ClockServer ins = new ClockServer();

		public ClockModel playClockModel = null;

		public Action onPlayMusicStart = null;
		public Action onPlayMusicFinished = null;

		private MusicPlayer musicPlayer = null;
		
		private Timer timer = null;
		private SetTimeout waitAlarm = null;
		private long lastStopTime = 0;
		private bool _isAlarm = false;

		public void init() {
			musicPlayer = new MusicPlayer();
			musicPlayer.handle = MainWindow.getHandle();

			timer = new Timer();
			timer.Enabled = true;
			timer.Interval = 400;
			timer.Elapsed += timerProc;
			timer.Start();
		}

		public void stop() {
			try {
				bool isPlay = musicPlayer.isPlay() || _isAlarm;
				try {
					musicPlayer.stop();
				} catch(Exception) { }

				_isAlarm = false;

				if(isPlay) {
					lastStopTime = DateTime.Now.ToFileTimeUtc();
					onPlayMusicFinished?.Invoke();
				}

				if(waitAlarm != null) {
					waitAlarm.stop();
					waitAlarm = null;
				}
			} catch(Exception) {

			}
		}

		private void timerProc(object sender, ElapsedEventArgs e) {
			DateTime date = DateTime.Now;

			if(isAlarm()) {
				return;
			}

			//停止响铃后5s内不重复响铃
			long nowTime = date.ToFileTimeUtc();
			if(nowTime - lastStopTime <= 5 * 1000 * 1000 * 10) {
				return;
			}

			List<ClockModel> lst = MainModel.ins.cfgMd.lstClock;
			for(int i = 0; i < lst.Count; ++i) {
				if(!lst[i].isEnable) {
					continue;
				}

				if(lst[i].hour != date.Hour) {
					continue;
				}
				if(lst[i].minute != date.Minute) {
					continue;
				}

				if(date.Second <= 1) {
					playClockModel = lst[i];
					playMusic();
					break;
				}
			}
		}

		public bool isAlarm() {
			return musicPlayer.isPlay() || _isAlarm;
		}

		private void playMusic() {
			_isAlarm = true;

			string path = MainModel.ins.cfgMd.clockMusicPath;
			if(File.Exists(path)) {
				try {
					musicPlayer.path = path;
					musicPlayer.volume = (float)MainModel.ins.cfgMd.clockVolume / 100;
					musicPlayer.play();
				} catch(Exception ex) {
					Debug.WriteLine(ex.ToString());
				}
			}

			//isPlay = true;
			try {
				onPlayMusicStart?.Invoke();

				int waitTime = MainModel.ins.cfgMd.alarmTimeSecond;
				if(waitTime <= 0 || waitTime > 3600) {
					waitTime = 30;
				}

				waitAlarm = new SetTimeout(() => {
					stop();
				}, waitTime * 1000);
			} catch(Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void clear() {
			stop();

			if(timer != null) {
				timer.Stop();
				timer = null;
			}
		}

	}
}
