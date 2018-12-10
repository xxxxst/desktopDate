using desktopDate.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace desktopDate.services {
	public class AutoSaveServer {
		public static AutoSaveServer ins = new AutoSaveServer();

		public bool hasChanged = false;

		private const int saveWaitTime = 2 * 1000;
		private Timer timer = null;

		public void init() {
			timer = new Timer();
			timer.Enabled = true;
			timer.Interval = saveWaitTime;
			timer.Elapsed += timerProc;
			timer.Start();
		}

		private void timerProc(object sender, ElapsedEventArgs e) {
			if(!hasChanged) {
				return;
			}

			try {
				hasChanged = false;
				MainWindow.ins.saveConfig();
			} catch(Exception) {

			}
		}

		public void clear() {
			if(timer != null) {
				timer.Stop();
				timer = null;
			}
		}

	}
}
