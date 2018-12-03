using System;
using System.Collections.Generic;
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

		public Action<int, int> onTimerUpdated = null;
		public Action onTimerTriggered = null;

		private long startTime = 0;
		private Timer timer = null;

		public bool isStart() {
			return startTimer != null;
		}

		public void start(TimerModel _startTimer) {
			if(isStart()) {
				stop();
			}

			startTimer = _startTimer;

			totalSecond = startTimer.totalSecond;
			nowSecond = totalSecond;
			startTime = DateTime.Now.ToFileTimeUtc();

			timer = new Timer();
			timer.Interval = 1000;
			timer.Elapsed += timerProc;
			timer.Start();
		}

		public void stop() {
			if(timer != null) {
				timer.Stop();
				timer = null;
			}

			startTimer = null;
		}

		private void timerProc(object sender, ElapsedEventArgs e) {
			long nowTime = DateTime.Now.ToFileTimeUtc();
			nowSecond = totalSecond - (int)(nowTime - startTime);

			onTimerUpdated?.Invoke(nowSecond, totalSecond);
			if(nowSecond <= 0) {
				stop();
				onTimerTriggered?.Invoke();
			}
		}
	}
	
	public class TimerModel {
		public int totalSecond = 0;
	}
}
