using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopDate.util {
	public class TimeFormat {
		public static string formatTime(int totalSecond) {
			int hour = totalSecond / 3600;
			int minute = (totalSecond - hour * 3600) / 60;
			int second = totalSecond % 60;

			return formatTime(hour, minute, second);
		}
		
		public static string formatTime(int hour, int minute) {
			string strHour = hour.ToString().PadLeft(2, '0');
			string strMinute = minute.ToString().PadLeft(2, '0');

			return $"{strHour}:{strMinute}";
		}

		public static string formatTime(int hour, int minute, int second) {
			string strHour = hour.ToString().PadLeft(2, '0');
			string strMinute = minute.ToString().PadLeft(2, '0');
			string strSecond = second.ToString().PadLeft(2, '0');

			return $"{strHour}:{strMinute}:{strSecond}";
		}

		public static void parseTime(String time, out int hour, out int minute, out int second) {
			hour = 0;
			minute = 0;
			second = 0;

			try {
				string[] arr = time.Split(':');
				if(arr.Length >= 1) {
					second = Convert.ToInt32(arr[arr.Length - 1]);
				}
				if(arr.Length >= 2) {
					minute = Convert.ToInt32(arr[arr.Length - 2]);
				}
				if(arr.Length >= 3) {
					hour = Convert.ToInt32(arr[arr.Length - 3]);
				}
			} catch(Exception) {

			}
		}

		public static void parseTime(String time, out int hour, out int minute) {
			hour = 0;
			minute = 0;

			try {
				string[] arr = time.Split(':');
				if(arr.Length >= 1) {
					minute = Convert.ToInt32(arr[arr.Length - 1]);
				}
				if(arr.Length >= 2) {
					hour = Convert.ToInt32(arr[arr.Length - 2]);
				}
			} catch(Exception) {

			}
		}

		public static void calcTime(int totalSecond, out int hour, out int minute, out int second) {
			hour = totalSecond / 3600;
			minute = (totalSecond - hour * 3600) / 60;
			second = totalSecond % 60;
		}

		public static int getTotalSecond(int hour, int minute, int second) {
			return hour * 3600 + minute * 60 + second;
		}
	}
}
