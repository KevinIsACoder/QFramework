using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark
{
	[XLua.LuaCallCSharp]
	public static class TimeHelper
	{
		private static readonly DateTime m_StandardTime;

		static TimeHelper()
		{
			m_StandardTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		}

		private static bool m_Synced;
		private static DateTime m_LocalTimeAtSync;
		private static DateTime m_RemoteTimeAtSync;

		public static DateTime now
		{
			get
			{
				DateTime now = DateTime.UtcNow;
				if (m_Synced) {
					var deltaTime = now - m_LocalTimeAtSync;
					now = m_RemoteTimeAtSync + deltaTime;
				}
				return now;
			}
		}

		public static int timestamp => Convert.ToInt32(Math.Floor(totalSeconds));

		public static int hours => (now - m_StandardTime).Hours;

		public static double totalHours => (now - m_StandardTime).TotalHours;

		public static int minutes => (now - m_StandardTime).Minutes;

		public static double totalMinutes => (now - m_StandardTime).TotalMinutes;

		public static int seconds => (now - m_StandardTime).Seconds;

		public static double totalSeconds => (now - m_StandardTime).TotalSeconds;

		public static int milliseconds => (now - m_StandardTime).Milliseconds;

		public static double totalMilliseconds => (now - m_StandardTime).TotalMilliseconds;

		public static void Sync(double timestamp)
		{
			m_Synced = true;
			m_LocalTimeAtSync = DateTime.UtcNow;
			m_RemoteTimeAtSync = m_StandardTime.AddSeconds(timestamp);
		}
	}
}
