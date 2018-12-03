using csharpHelp.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopDate.model {
	public class MainModel {
		public static MainModel ins = new MainModel();
		
		public ConfigModel cfgMd = null;
	}
	
	[XmlRoot("desktopDate")]
	public class ConfigModel {
		[XmlAttr("timerBox.timer.totalSecond")] public List<int> lstTimer = null;
	}
}
