using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopDate.services {
	public class ChineseDateServer {
		public static ChineseDateServer ins = new ChineseDateServer();

		ChineseLunisolarCalendar ChineseCalendar = new ChineseLunisolarCalendar();

		string[] months = { "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "腊" };
		///<summary>返回农历月</summary>
		public string GetLunisolarMonth(int month) {
			if(month < 13 && month > 0) {
				return months[month - 1];
			}

			return "零";
		}

		string[] days = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
		string[] days1 = { "初", "十", "廿", "三" };
		///<summary>返回农历日</summary>
		public string GetLunisolarDay(int day) {
			if(day > 0 && day < 32) {
				if(day != 20 && day != 30) {
					return string.Concat(days1[(day - 1) / 10], days[(day - 1) % 10]);
				} else {
					return string.Concat(days[(day - 1) / 10], days1[1]);
				}
			}

			return "零零";
		}

		///<summary>根据公历获取农历日期</summary>
		public string GetChineseDateTime(DateTime datetime, out int month, out int day) {
			int year = ChineseCalendar.GetYear(datetime);
			month = ChineseCalendar.GetMonth(datetime);
			day = ChineseCalendar.GetDayOfMonth(datetime);
			//获取闰月， 0 则表示没有闰月
			int leapMonth = ChineseCalendar.GetLeapMonth(year);

			bool isleap = false;

			if(leapMonth > 0) {
				if(leapMonth == month) {
					//闰月
					isleap = true;
					month--;
				} else if(month > leapMonth) {
					month--;
				}
			}

			return (isleap ? "闰" : "") + GetLunisolarMonth(month) + "月" + GetLunisolarDay(day);
		}

		public DateTime toDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0) {
			// 闰月
			// 闰月的节日在润月
			int leapMonth = ChineseCalendar.GetLeapMonth(year);
			if(leapMonth > 0) {
				if(month > leapMonth - 1) {
					++month;
				}
			}
			DateTime date = ChineseCalendar.ToDateTime(year, month, day, hour, minute, second, millisecond);
			if(year == 2020 && month == 13 && day == 5) {
				Console.WriteLine("aaa: " + date.ToString("yyyy/MM/dd"));
			}
			return date;
		}

	}
}
