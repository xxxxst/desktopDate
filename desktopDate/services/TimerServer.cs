using desktopDate.control;
using desktopDate.model;
using desktopDate.util;
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
	public class TimerServer {
		public static TimerServer ins = new TimerServer();

		//public bool isStart = false;
		private TimerModel startTimer = null;
		private int totalSecond = 0;
		private int nowSecond = 0;
		public bool isPause = false;
		//public bool isPlay = false;

		public Action<int, int> onTimerUpdated = null;
		public Action onTimerFinished = null;
		public Action onPlayMusicFinished = null;

		private MusicPlayer musicPlayer = null;

		//private long nowNS = 0;
		private long parseNS = 0;
		private long startTime = 0;
		private long parseTime = 0;
		private Timer timer = null;
		private SetTimeout waitAlarm = null;
		private bool _isAlarm = false;

		public void init() {
			musicPlayer = new MusicPlayer();
			musicPlayer.handle = MainWindow.getHandle();
		}

		public bool isStart() {
			return startTimer != null;
		}

		public void restart(TimerModel _startTimer) {
			stop();

			totalSecond = TimeFormat.getTotalSecond(_startTimer.hour, _startTimer.minute, _startTimer.second);

			if(totalSecond <= 0) {
				onTimerFinished?.Invoke();
				return;
			}

			startTimer = _startTimer;
			isPause = false;
			//isPlay = false;

			parseNS = 0;
			nowSecond = totalSecond;
			startTime = DateTime.Now.ToFileTimeUtc();

			timer = new Timer();
			timer.Enabled = true;
			timer.Interval = 200;
			timer.Elapsed += timerProc;
			timer.Start();
		}

		public void start() {
			if(!isPause) {
				return;
			}
			long nowTime = DateTime.Now.ToFileTimeUtc();
			parseNS += nowTime - parseTime;
			isPause = false;
		}

		public void pause() {
			if(isPause) {
				return;
			}
			parseTime = DateTime.Now.ToFileTimeUtc();
			isPause = true;
		}

		public void stop() {
			try {
				if(timer != null) {
					timer.Stop();
					timer = null;
				}

				bool isPlay = musicPlayer.isPlay() || _isAlarm;
				try {
					musicPlayer.stop();
				} catch(Exception) { }

				_isAlarm = false;

				if(isPlay) {
					onPlayMusicFinished?.Invoke();
				}

				if(waitAlarm != null) {
					waitAlarm.stop();
					waitAlarm = null;
				}
			} catch(Exception) {

			}

			startTimer = null;
		}

		private void timerProc(object sender, ElapsedEventArgs e) {
			if(isPause) {
				return;
			}
			long nowTime = DateTime.Now.ToFileTimeUtc();
			long nowNS = nowTime - startTime - parseNS;
			int second = totalSecond - (int)(nowNS / 10000000);
			if(second >= nowSecond) {
				return;
			}
			nowSecond = second;

			onTimerUpdated?.Invoke(nowSecond, totalSecond);
			if(nowSecond <= 0) {
				stop();
				playMusic();
				onTimerFinished?.Invoke();
			}
		}

		public bool isAlarm() {
			return musicPlayer.isPlay() || _isAlarm;
		}
		
		private void playMusic() {
			_isAlarm = true;

			string path = MainModel.ins.cfgMd.timerMusicPath;
			if(File.Exists(path)) {
				try {
					musicPlayer.path = path;
					musicPlayer.volume = (float)MainModel.ins.cfgMd.timerVolume / 100;
					musicPlayer.play();
				} catch(Exception ex) {
					Debug.WriteLine(ex.ToString());
				}
			}

			//isPlay = true;
			try {
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
		}
	}
}
