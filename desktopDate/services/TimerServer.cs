using desktopDate.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private long parseNS = 0;
		private int nowSecond = 0;
		public bool isPause = false;
		public bool isPlay = false;

		public Action<int, int> onTimerUpdated = null;
		public Action onTimerFinished = null;

		private long startTime = 0;
		private long parseTime = 0;
		private Timer timer = null;

		public bool isStart() {
			return startTimer != null;
		}

		public void restart(TimerModel _startTimer) {
			if(isStart()) {
				stop();
			}

			startTimer = _startTimer;
			isPause = false;
			isPlay = false;

			parseNS = 0;
			totalSecond = startTimer.totalSecond;
			nowSecond = totalSecond;
			startTime = DateTime.Now.ToFileTimeUtc();

			timer = new Timer();
			timer.Enabled = true;
			timer.Interval = 1000;
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
			if(timer != null) {
				timer.Stop();
				timer = null;
			}

			startTimer = null;
		}

		private void timerProc(object sender, ElapsedEventArgs e) {
			if(isPause) {
				return;
			}
			long nowTime = DateTime.Now.ToFileTimeUtc();
			nowSecond = totalSecond - (int)((nowTime - startTime - parseNS) /10000000);

			onTimerUpdated?.Invoke(nowSecond, totalSecond);
			if(nowSecond <= 0) {
				stop();
				onTimerFinished?.Invoke();
			}
		}
	}
}
