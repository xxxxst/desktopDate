using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopDate.services {

	public class EventServer {
		public static EventServer ins = new EventServer();

		public event Action configLoadedEvent;
		public void onConfigLoaded() {
			configLoadedEvent?.Invoke();
		}

		public event Action detailWinHideEvent;
		public void onDetailWinHide() {
			detailWinHideEvent?.Invoke();
		}
	}
}
