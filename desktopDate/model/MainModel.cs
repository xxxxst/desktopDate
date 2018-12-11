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
		public MainWindow mainWin = null;

		public ConfigModel cfgMd = null;

		public bool isEditTime = false;
	}
	
	[XmlRoot("desktopDate")]
	public class ConfigModel {
		[XmlAttr("dataVersion")] public string dataVersion = "1.0.0";
		
		[XmlAttr("win.dx")] public int dx = 0;
		[XmlAttr("win.dy")] public int dy = 0;
		[XmlAttr("win.width")] public int width = 180;
		[XmlAttr("win.height")] public int height = 74;
		[XmlAttr("win.align")] public WinAlign winAlign = WinAlign.RightBottom;

		[XmlValue("config.alarmTimeSecond")] public int alarmTimeSecond = 30;

		[XmlAttr("clockBox.music")] public string clockMusicPath = "";
		[XmlAttr("timerBox.music")] public string timerMusicPath = "";

		[XmlAttr("clockBox.volume")] public int clockVolume = 50;
		[XmlAttr("timerBox.volume")] public int timerVolume = 50;

		[XmlListChild("clockBox.clock")] public List<ClockModel> lstClock = null;
		[XmlListChild("timerBox.timer")] public List<TimerModel> lstTimer = null;
	}

	public class TimerModel {
		//[XmlAttr("totalSecond")] public int totalSecond = 0;
		[XmlAttr("time")] public string time = "00:00:00";
		//public int totalSecond = 0;

		public int hour = 0;
		public int minute = 0;
		public int second = 0;
		//[XmlAttr("desc")] public string desc = "";
	}

	public class ClockModel {
		[XmlAttr("time")] public string time = "00:00";
		[XmlAttr("isEnable")] public bool isEnable = true;
		[XmlAttr("desc")] public string desc = "";

		public int hour = 0;
		public int minute = 0;
		//public int second = 0;
	}

	enum EditTimeType { Hour, Minute, Second }

	public enum WinAlign { LeftTop, RightTop, RightBottom, LeftBottom };
}
