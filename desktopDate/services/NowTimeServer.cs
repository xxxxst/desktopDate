using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace desktopDate.services {
	public class NowTimeServer {
		public static NowTimeServer ins = new NowTimeServer();

		public DispatcherTimer timer = new DispatcherTimer();

		public void init(){
			timer = new DispatcherTimer();
			timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
			timer.Start();
		}

		public void clear() {
			timer.Stop();
		}

	}
}
