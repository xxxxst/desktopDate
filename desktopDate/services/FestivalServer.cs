using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopDate.services {
	public class FestivalServer {
		public static FestivalServer ins = new FestivalServer();

		public Dictionary<string, string> mapFestival = new Dictionary<string, string>();
		public Dictionary<string, string> mapChineseFestival = new Dictionary<string, string>();

		private string lastDay = "";

		public void init() {
			initFestival();
		}

		private void initFestival() {
			mapFestival["01/01"] = "元旦";
			mapFestival["02/14"] = "情人节";
			mapFestival["03/08"] = "妇女节";
			mapFestival["03/12"] = "植树节";
			mapFestival["04/01"] = "愚人节";
			mapFestival["05/01"] = "劳动节";
			mapFestival["06/01"] = "儿童节";
			mapFestival["08/01"] = "建军节";
			mapFestival["08/12"] = "青年节";
			mapFestival["09/10"] = "教师节";
			mapFestival["10/01"] = "国庆节";
			mapFestival["11/01"] = "万圣节";
			mapFestival["11/26"] = "感恩节";
			mapFestival["12/24"] = "平安夜";
			mapFestival["12/25"] = "圣诞节";

			mapChineseFestival["12/30"] = "除夕";
			mapChineseFestival["01/01"] = "春节";
			mapChineseFestival["01/15"] = "元宵节";
			mapChineseFestival["05/05"] = "端午节";
			mapChineseFestival["07/07"] = "七夕节";
			mapChineseFestival["08/15"] = "中秋节";
			mapChineseFestival["09/09"] = "重阳节";

			//calcWeekFestival(mapFestival, "感恩节", 4, 2014);
		}

		private void calcWeekFestival(int year, int month, int weekCount, int festivalWeek, out int day) {
			//DateTime date = DateTime.Now;
			DateTime date = new DateTime(year, month, 1);

			int week = (int)date.DayOfWeek;
			day = 7 * (weekCount - 1);
			if(week <= festivalWeek) {
				day += festivalWeek - week + 1;
			} else {
				day += 7 - (week - festivalWeek) + 1;
			}
			//Debug.WriteLine("aa:" + year + "," + day);
		}

		private bool isWeekFestival(int year, int month, int weekCount, int festivalWeek) {
			DateTime date = DateTime.Now;
			if(date.Year != year) {
				return false;
			}
			if(date.Month != month) {
				return false;
			}

			calcWeekFestival(year, month, weekCount, festivalWeek, out int day);

			return (date.Day == day);
		}

		private bool isFinishWeekFestival(int month, int weekCount, int festivalWeek) {
			DateTime date = DateTime.Now;
			if(date.Month > month) {
				return true;
			}

			calcWeekFestival(date.Year, month, weekCount, festivalWeek, out int day);

			return (date.Day > day);
		}

		public string getFestival(DateTime date) {
			string temp = date.ToString("MM/dd");
			if(temp == lastDay) {
				return "";
			}
			lastDay = temp;

			//节日
			if(mapFestival.ContainsKey(lastDay)) {
				return mapFestival[lastDay];
			} else {
				//感恩节
				if(isWeekFestival(date.Year, 11, 4, 4)){
					return "感恩节";
				}
				if(isWeekFestival(date.Year, 5, 2, 7)) {
					return "母亲节";
				}
				if(isWeekFestival(date.Year, 6, 3, 7)) {
					return "父亲节";
				}
				return "";
			}
		}

		public string getChineseFestival(int year, int month, int day) {
			string lastDayChinese = month.ToString().PadLeft(2, '0') + "/" + day.ToString().PadLeft(2, '0');

			if(mapChineseFestival.ContainsKey(lastDayChinese)) {
				return mapChineseFestival[lastDayChinese];
			}

			return "";
		}

		private string getNextWeekFestivalTime(DateTime date, int month, int weekCount, int festivalWeek) {
			int day = 0;
			calcWeekFestival(date.Year, month, weekCount, festivalWeek, out day);
			if(date.Month > month || (date.Month == month && date.Day > day)) {
				calcWeekFestival(date.Year + 1, month, weekCount, festivalWeek, out day);
			}
			return month.ToString().PadLeft(2, '0') + "/" + day.ToString().PadLeft(2, '0');
		}

		List<FestivalModel> lstFestival = new List<FestivalModel>();
		public List<FestivalModel> getListFestival(){
			if(lstFestival.Count > 0) {
				return lstFestival;
			}

			DateTime date = DateTime.Now;
			date = new DateTime(date.Year, date.Month, date.Day);
			foreach (string key in mapFestival.Keys) {
				lstFestival.Add(new FestivalModel(mapFestival[key], key, -1));
			}
			foreach(string key in mapChineseFestival.Keys) {
				lstFestival.Add(new FestivalModel(mapChineseFestival[key], key, -1));
			}

			string time = "";
			time = getNextWeekFestivalTime(date, 11, 4, 4);
			lstFestival.Add(new FestivalModel("感恩节", time, -1));

			time = getNextWeekFestivalTime(date, 5, 2, 7);
			lstFestival.Add(new FestivalModel("母亲节", time, -1));

			time = getNextWeekFestivalTime(date, 6, 3, 7);
			lstFestival.Add(new FestivalModel("父亲节", time, -1));

			lstFestival.Sort((a,b) =>  a.time.CompareTo(b.time));

			for(int i = 0; i < lstFestival.Count; ++i) {
				try {

					string[] arr = lstFestival[i].time.Split('/');
					int month = Convert.ToInt32(arr[0]);
					int day = Convert.ToInt32(arr[1]);

					DateTime temp = new DateTime(date.Year, month, day);
					TimeSpan ts = temp - date;
					lstFestival[i].dayOfRange = ts.Days;
					lstFestival[i].isFinished = ts.Days < 0;

					if(ts.Days < 0) {
						temp = new DateTime(date.Year + 1, month, day);
						ts = temp - date;
						lstFestival[i].dayOfRange = ts.Days;
					}
				} catch(Exception) { }
			}
			lstFestival.Sort((a, b) => {
				if(a.isFinished && !b.isFinished) {
					return 1;
				}
				if(!a.isFinished && b.isFinished) {
					return -1;
				}
				//if(a.dayOfRange < 0 && b.dayOfRange >= 0) {
				//	return 1;
				//}
				//if(a.dayOfRange >= 0 && b.dayOfRange < 0) {
				//	return -1;
				//}
				return a.time.CompareTo(b.time);
			});

			return lstFestival;
		}

		public FestivalModel getNextFestival() {
			return getListFestival().FirstOrDefault();
		}
	}

	public class FestivalModel {
		public string name = "";
		public string time = "";
		public int dayOfRange = -1;
		public bool isFinished = false;

		public FestivalModel() { }

		public FestivalModel(string _name, string _time, int _dayOfRange) {
			name = _name;
			time = _time;
			dayOfRange = _dayOfRange;
		}
	}
}
