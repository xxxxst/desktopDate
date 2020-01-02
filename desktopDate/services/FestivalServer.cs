using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopDate.services {
	public enum FestivalType {
		YuanDan,		//元旦
		QinRenJie,		//情人节
		FuNvJie,		//妇女节
		ZhiShuJie,		//植树节
		YuRenJie,		//愚人节
		LaoDongJie,		//劳动节
		ErTongJie,		//儿童节
		JianJunJie,		//建军节
		QingNianJie,	//青年节
		JiaoShiJie,		//教师节
		GuoQingJie,		//国庆节
		WanShengJie,	//万圣节
		PingAnYe,		//平安夜
		ShengDanJie,	//圣诞节
		GanEnJie,		//感恩节
		MuQingJie,		//母亲节
		FuQingJie,      //父亲节

		ChuXi,          //除夕
		ChunJie,        //春节
		YuanXiaoJie,    //元宵节
		DuanWuJie,      //端午节
		QiXiJie,        //七夕节
		ZhongQiuJie,    //中秋节
		ChongYangJie,   //重阳节
	}

	//public enum ChineseFestivalType {
	//	ChuXi,			//除夕
	//	ChunJie,		//春节
	//	YuanXiaoJie,	//元宵节
	//	DuanWuJie,		//端午节
	//	QiXiJie,		//七夕节
	//	ZhongQiuJie,	//中秋节
	//	ChongYangJie,	//重阳节
	//}

	public enum FestivalCalcType {
		Default,		//指定日期
		Week,			//某月第几个星期几
		MonthLastDay,	//某月最后一天
	}

	public class FestivalInfo {
		public FestivalType type = FestivalType.YuanDan;
		public bool isChineseFestival = false;
		public string desc = "";
		public FestivalCalcType calcType = FestivalCalcType.Default;
		public string timeInfo = "";

		public DateTime date = DateTime.Now;

		public bool isFinished = false;
		public int dayOfRange = -1;
		public string time = "";
		public string showTime = "";

		public FestivalInfo() { }
		public FestivalInfo(FestivalType _type, bool _isChineseFestival, string _desc, FestivalCalcType _calcType, string _timeInfo) { type = _type; isChineseFestival = _isChineseFestival; desc = _desc; calcType = _calcType; timeInfo = _timeInfo; }

		public FestivalInfo Clone() {
			FestivalInfo md = new FestivalInfo(type, isChineseFestival, desc, calcType, timeInfo);
			md.date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond);
			return md;
		}
	}

	public class FestivalServer {
		public static FestivalServer ins = new FestivalServer();

		private Dictionary<string, string> mapFestival = new Dictionary<string, string>();
		private Dictionary<string, string> mapChineseFestival = new Dictionary<string, string>();

		private Dictionary<FestivalType, FestivalInfo> mapFestivalCalcInfo = new Dictionary<FestivalType, FestivalInfo>();

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
			//mapFestival["11/26"] = "感恩节";
			mapFestival["12/24"] = "平安夜";
			mapFestival["12/25"] = "圣诞节";

			//mapChineseFestival["12/29"] = "除夕";
			mapChineseFestival["01/01"] = "春节";
			mapChineseFestival["01/15"] = "元宵节";
			mapChineseFestival["05/05"] = "端午节";
			mapChineseFestival["07/07"] = "七夕节";
			mapChineseFestival["08/15"] = "中秋节";
			mapChineseFestival["09/09"] = "重阳节";

			//calcWeekFestival(mapFestival, "感恩节", 4, 2014);

			setCalcInfo(FestivalType.YuanDan		, false	, "元旦"	, FestivalCalcType.Default	, "01/01");
			setCalcInfo(FestivalType.QinRenJie		, false	, "情人节"	, FestivalCalcType.Default	, "02/14");
			setCalcInfo(FestivalType.FuNvJie		, false	, "妇女节"	, FestivalCalcType.Default	, "03/08");
			setCalcInfo(FestivalType.ZhiShuJie		, false	, "植树节"	, FestivalCalcType.Default	, "03/12");
			setCalcInfo(FestivalType.YuRenJie		, false	, "愚人节"	, FestivalCalcType.Default	, "04/01");

			setCalcInfo(FestivalType.LaoDongJie		, false	, "劳动节"	, FestivalCalcType.Default	, "05/01");
			setCalcInfo(FestivalType.ErTongJie		, false	, "儿童节"	, FestivalCalcType.Default	, "06/01");
			setCalcInfo(FestivalType.JianJunJie		, false	, "建军节"	, FestivalCalcType.Default	, "08/01");
			setCalcInfo(FestivalType.QingNianJie	, false	, "青年节"	, FestivalCalcType.Default	, "08/12");
			setCalcInfo(FestivalType.JiaoShiJie		, false	, "教师节"	, FestivalCalcType.Default	, "09/10");
			setCalcInfo(FestivalType.GuoQingJie		, false	, "国庆节"	, FestivalCalcType.Default	, "10/01");
			setCalcInfo(FestivalType.WanShengJie	, false	, "万圣节"	, FestivalCalcType.Default	, "11/01");

			setCalcInfo(FestivalType.PingAnYe		, false	, "平安夜"	, FestivalCalcType.Default	, "12/24");
			setCalcInfo(FestivalType.ShengDanJie	, false	, "圣诞节"	, FestivalCalcType.Default	, "12/25");
			setCalcInfo(FestivalType.GanEnJie		, false	, "感恩节"	, FestivalCalcType.Week		, "11/04/04");
			setCalcInfo(FestivalType.MuQingJie		, false	, "母亲节"	, FestivalCalcType.Week		, "05/02/07");
			setCalcInfo(FestivalType.FuQingJie 		, false	, "父亲节"	, FestivalCalcType.Week		, "06/03/07");

			setCalcInfo(FestivalType.ChuXi      	, true	, "除夕"	, FestivalCalcType.MonthLastDay, "12");
			setCalcInfo(FestivalType.ChunJie    	, true	, "春节"	, FestivalCalcType.Default, "01/01");
			setCalcInfo(FestivalType.YuanXiaoJie	, true	, "元宵节"	, FestivalCalcType.Default, "01/15");
			setCalcInfo(FestivalType.DuanWuJie  	, true	, "端午节"	, FestivalCalcType.Default, "05/05");
			setCalcInfo(FestivalType.QiXiJie    	, true	, "七夕节"	, FestivalCalcType.Default, "07/07");
			setCalcInfo(FestivalType.ZhongQiuJie	, true	, "中秋节"	, FestivalCalcType.Default, "08/15");
			setCalcInfo(FestivalType.ChongYangJie	, true	, "重阳节"	, FestivalCalcType.Default, "09/09");
		}
		private void setCalcInfo(FestivalType type, bool isChineseFestival, string desc, FestivalCalcType calcType, string timeInfo) {
			mapFestivalCalcInfo[type] = new FestivalInfo(type, isChineseFestival, desc, calcType, timeInfo);
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

		private string getFestival(DateTime date) {
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

		private string getChineseFestival(int year, int month, int day) {
			string lastDayChinese = month.ToString().PadLeft(2, '0') + "/" + day.ToString().PadLeft(2, '0');

			if(mapChineseFestival.ContainsKey(lastDayChinese)) {
				return mapChineseFestival[lastDayChinese];
			}

			return "";
		}

		private string getNextWeekFestivalTime(DateTime date, int month, int weekCount, int festivalWeek, out int year) {
			int day = 0;
			year = date.Year;
			calcWeekFestival(date.Year, month, weekCount, festivalWeek, out day);
			if(date.Month > month || (date.Month == month && date.Day > day)) {
				++year;
				calcWeekFestival(date.Year + 1, month, weekCount, festivalWeek, out day);
			}
			return month.ToString().PadLeft(2, '0') + "/" + day.ToString().PadLeft(2, '0');
		}

		public DateTime getFestival(int year, FestivalType type) {
			DateTime date = DateTime.Now;

			if(!mapFestivalCalcInfo.ContainsKey(type)){
				return date;
			}

			FestivalInfo clacInfo = mapFestivalCalcInfo[type];

			if (clacInfo.isChineseFestival) {
				//农历节日
				switch (clacInfo.calcType) {
					case FestivalCalcType.MonthLastDay: {
						//某月最后一天
						int.TryParse(clacInfo.timeInfo, out int month);
						if (month == 12) {
							year += 1;
							month = 1;
						}
						DateTime dtTemp = ChineseDateServer.ins.toDateTime(year, month, 1);
						dtTemp = dtTemp.AddDays(-1);
						return dtTemp;
					}
					default: {
						//指定日期
						string[] arr = clacInfo.timeInfo.Split('/');
						if (arr.Length < 2) {
							return date;
						}
						int.TryParse(arr[0], out int month);
						int.TryParse(arr[1], out int day);
						DateTime dtTemp = ChineseDateServer.ins.toDateTime(year, month, day);
						return dtTemp;
					}
				}
			} else {
				//阳历节日
				switch (clacInfo.calcType) {
					case FestivalCalcType.Week: {
						//某月第几个星期几
						string[] arr = clacInfo.timeInfo.Split('/');
						if (arr.Length < 3) {
							return date;
						}
						int.TryParse(arr[0], out int month);
						int.TryParse(arr[1], out int weekCount);
						int.TryParse(arr[2], out int week);
						calcWeekFestival(year, month, weekCount, week, out int day);
						DateTime dtTemp = new DateTime(year, month, day);
						return dtTemp;
					}
					default: {
						//指定日期
						string[] arr = clacInfo.timeInfo.Split('/');
						if (arr.Length < 2) {
							return date;
						}
						int.TryParse(arr[0], out int month);
						int.TryParse(arr[1], out int day);
						DateTime dtTemp = new DateTime(year, month, day);
						return dtTemp;
					}
				}
			}
		}

		//public List<FestivalInfo> getListFestivalByYear(int year) {
		//	List<FestivalInfo> rst = new List<FestivalInfo>();
		//	foreach (FestivalType type in mapFestivalCalcInfo.Keys) {
		//		FestivalInfo md = mapFestivalCalcInfo[type].Clone();
		//		md.date = getFestival(year, type);
		//		rst.Add(md);
		//	}

		//	return rst;
		//}

		/// <summary>
		/// 获取从当前日期开始，接下来一年的节日
		/// </summary>
		/// <returns></returns>
		public List<FestivalInfo> getNextYearFestival() {
			return getNextYearFestival(DateTime.Now);
		}

		/// <summary>
		/// 获取从阳历指定日期开始，接下来一年的节日
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public List<FestivalInfo> getNextYearFestival(DateTime date) {
			List<FestivalInfo> rst = new List<FestivalInfo>();
			foreach (FestivalType type in mapFestivalCalcInfo.Keys) {
				FestivalInfo md = mapFestivalCalcInfo[type].Clone();
				TimeSpan ts = new TimeSpan();
				if (!md.isChineseFestival) {
					DateTime dtTemp = getFestival(date.Year, type);

					ts = dtTemp - date;
					if (ts.TotalDays + 1 < 0) {
						dtTemp = getFestival(date.Year + 1, type);
						//md.isFinished = true;

						ts = dtTemp - date;
					}

					md.date = dtTemp;
				} else {
					//农历日期和用阳历日期有错位
					//为了判断哪些节日已经过去了，需要比较前后三年的日期。
					DateTime dtTemp = getFestival(date.Year - 1, type);

					ts = dtTemp - date;
					if (ts.TotalDays + 1 < 0) {
						dtTemp = getFestival(date.Year, type);

						ts = dtTemp - date;
						if (ts.TotalDays + 1 < 0) {
							dtTemp = getFestival(date.Year + 1, type);
							//md.isFinished = true;
							ts = dtTemp - date;
						}
					}

					md.date = dtTemp;
				}
				//DateTime dtTemp = getFestival(date.Year, type);
				md.time = md.date.ToString("yyyy/MM/dd");
				md.showTime = md.date.ToString("MM/dd");
				md.dayOfRange = (int)(ts.TotalDays + 1);
				md.isFinished = md.date.Year > date.Year;
				rst.Add(md);
			}

			rst.Sort((a, b) => {
				if (a.time == b.time && a.isChineseFestival != b.isChineseFestival) {
					return a.isChineseFestival ? 1 : -1;
				}
				return a.time.CompareTo(b.time);
			});

			return rst;
		}

		List<FestivalModel> lstFestival = new List<FestivalModel>();
		private List<FestivalModel> getListFestival(){
			if(lstFestival.Count > 0) {
				return lstFestival;
			}

			DateTime date = DateTime.Now;
			date = new DateTime(date.Year, date.Month, date.Day);
			foreach (string key in mapFestival.Keys) {
				lstFestival.Add(new FestivalModel(mapFestival[key], date.Year + "/" + key, key, -1));
			}
			foreach(string key in mapChineseFestival.Keys) {
				string[] arr = key.Split('/');
				if(arr.Length < 2) {
					continue;
				}
				int.TryParse(arr[0], out int month);
				int.TryParse(arr[1], out int day);
				int chineseYearTemp = date.Year;
				DateTime dtTemp = ChineseDateServer.ins.toDateTime(date.Year, month, day);
				if (dtTemp.Year > date.Year) {
					chineseYearTemp = date.Year - 1;
					dtTemp = ChineseDateServer.ins.toDateTime(chineseYearTemp, month, day);
				}
				string strTime1 = dtTemp.ToString("yyyy/MM/dd");
				string strTime2 = dtTemp.ToString("MM/dd");
				lstFestival.Add(new FestivalModel(mapChineseFestival[key], strTime1, strTime2, -1) { originTime = key, isChinseeTime = true, chineseYearTemp = chineseYearTemp });
			}

			string time = "";
			int yearWeek = 0;
			time = getNextWeekFestivalTime(date, 11, 4, 4, out yearWeek);
			lstFestival.Add(new FestivalModel("感恩节", yearWeek + "/" + time, time, -1));

			time = getNextWeekFestivalTime(date, 5, 2, 7, out yearWeek);
			lstFestival.Add(new FestivalModel("母亲节", yearWeek + "/" + time, time, -1));

			time = getNextWeekFestivalTime(date, 6, 3, 7, out yearWeek);
			lstFestival.Add(new FestivalModel("父亲节", yearWeek + "/" + time, time, -1));

			lstFestival.Sort((a,b) =>  a.time.CompareTo(b.time));

			for(int i = 0; i < lstFestival.Count; ++i) {
				try {
					string[] arr = lstFestival[i].time.Split('/');
					int year = Convert.ToInt32(arr[0]);
					int month = Convert.ToInt32(arr[1]);
					int day = Convert.ToInt32(arr[2]);

					DateTime temp = new DateTime(year, month, day);
					TimeSpan ts = temp - date;
					lstFestival[i].dayOfRange = ts.Days;
					lstFestival[i].isFinished = year > date.Year || ts.Days < 0;

					if(ts.Days < 0) {
						if(lstFestival[i].isChinseeTime) {
							//chinese festival
							string[] arr1 = lstFestival[i].originTime.Split('/');
							int.TryParse(arr1[0], out int month1);
							int.TryParse(arr1[1], out int day1);

							temp = ChineseDateServer.ins.toDateTime(lstFestival[i].chineseYearTemp + 1, month1, day1);
							lstFestival[i].time = temp.ToString("yyyy/MM/dd");
							lstFestival[i].showTime = temp.ToString("MM/dd");
						} else {
							//normal festival
							temp = new DateTime(date.Year + 1, month, day);
							lstFestival[i].time = temp.ToString("yyyy/MM/dd");
						}
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
		public string originTime = "";
		public string showTime = "";
		public int dayOfRange = -1;
		public bool isFinished = false;
		public bool isChinseeTime = false;
		public int chineseYearTemp = -1;

		public FestivalModel() { }

		public FestivalModel(string _name, string _time, string _showTime, int _dayOfRange) {
			name = _name;
			time = _time;
			showTime = _showTime;
			dayOfRange = _dayOfRange;
		}
	}
}
