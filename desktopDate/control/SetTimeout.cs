using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace desktopDate.control {
	public class SetTimeout {
		Task task = null;
		CancellationTokenSource cts = null;

		public SetTimeout(Action fun, int ms) {
			if(fun == null) {
				return;
			}

			cts = new CancellationTokenSource();

			task = new Task(() => {
				Thread.Sleep(ms);
				fun();
			}, cts.Token);
		}

		public void stop() {
			try {
				cts?.Cancel();
			} catch(Exception) {

			}
		}
	}
}
