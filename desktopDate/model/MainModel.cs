using csharpHelp.services;
using desktopDate.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopDate.model {
	public class MainModel {
		public static MainModel ins = new MainModel();

		public DetailWin detailWin = null;
		public MainWindow mainWIn = null;

		public ConfigModel cfgMd = null;
	}
	
	[XmlRoot("desktopDate")]
	public class ConfigModel {
		[XmlAttr("dataVersion")] public string dataVersion = "1.0.0";

		[XmlValue("config.alarmTimeSecond")] public int alarmTimeSecond = 30;

		[XmlAttr("clockBox.music")] public string clockMusicPath = "";
		[XmlAttr("timerBox.music")] public string timerMusicPath = "";

		[XmlListChild("clockBox.clock")] public List<ClockModel> lstClock = null;
		[XmlListChild("timerBox.timer")] public List<TimerModel> lstTimer = null;
	}

	public class TimerModel {
		[XmlAttr("totalSecond")] public int totalSecond = 0;
		//[XmlAttr("desc")] public string desc = "";
	}

	public class ClockModel {
		[XmlAttr("time")] public string time = "00:00:00";

		public int hour = 0;
		public int minute = 0;
		public int second = 0;
	}
}
