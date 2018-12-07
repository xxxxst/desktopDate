using desktopDate.model;
using desktopDate.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopDate.control {
	public class ConfigCtl {
		ConfigModel md {
			get { return MainModel.ins.cfgMd; }
		}

		public void load() {
			List<TimerModel> lstTimer = md.lstTimer;
			for(int i = 0; i < lstTimer.Count; ++i) {
				TimerModel tm = lstTimer[i];
				TimeFormat.parseTime(lstTimer[i].time, out tm.hour, out tm.minute, out tm.second);
				//lstTimer[i].totalSecond = hour * 3600 + minute * 60 + second;
			}

			List<ClockModel> lstClock = md.lstClock;
			for(int i = 0; i < lstClock.Count; ++i) {
				ClockModel cm = lstClock[i];
				TimeFormat.parseTime(lstClock[i].time, out cm.hour, out cm.minute);
			}
		}

		public void save() {
			List<TimerModel> lstTimer = md.lstTimer;
			for(int i = 0; i < lstTimer.Count; ++i) {
				TimerModel tm = lstTimer[i];
				lstTimer[i].time = TimeFormat.formatTime(tm.hour, tm.minute, tm.second);
			}

			List<ClockModel> lstClock = md.lstClock;
			for(int i = 0; i < lstClock.Count; ++i) {
				ClockModel cm = lstClock[i];
				lstClock[i].time = TimeFormat.formatTime(cm.hour, cm.minute);
			}
		}
	}
}
