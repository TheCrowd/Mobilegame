using System;


namespace SGF.Time
{

    public class TimeUtils
    {

        readonly static DateTime DateTime_1970_01_01_08_00_00 = new DateTime(1970, 1, 1, 8, 0, 0);

        // ms from 1/1/1970 0:00 am
        public static double GetTotalMilliseconds()
        {
            DateTime nowtime = DateTime.Now.ToLocalTime();
            return nowtime.Subtract(DateTime_1970_01_01_08_00_00).TotalMilliseconds;
        }

        public static double GetTotalSeconds()
        {
            DateTime nowtime = DateTime.Now.ToLocalTime();
            return nowtime.Subtract(DateTime_1970_01_01_08_00_00).TotalSeconds;
        }


        public static TimeSpan GetTimeSpanSince1970()
        {
            return DateTime.Now.Subtract(DateTime_1970_01_01_08_00_00);
        }

        public static string FormatShowTime(ulong timeInSec)
        {
            string _text = "";
            ulong showTime;
            if ((timeInSec / (ulong)86400) > 0)
            {
                showTime = timeInSec / (ulong)86400;
                _text = showTime.ToString() + "days";
            }
            else if ((timeInSec / (ulong)3600) > 0)
            {
                showTime = timeInSec / (ulong)3600;
                _text = showTime.ToString() + "h(s)";
            }
            else if ((timeInSec / (ulong)60) > 0)
            {
                showTime = timeInSec / (ulong)60;
                _text = showTime.ToString() + "min(s)";
            }
            else
            {
                // 30 secs and 1 min 30 secs will be regarded as 1 min
                _text = "1 min";
            }
            return _text;
        }


        public static ulong getDay(ulong _time)
        {
            return (_time / (ulong)86400);
        }

        public static ulong getHour(ulong _time)
        {
            return ((_time % (ulong)86400) / 3600);
        }

        public static ulong getMinute(ulong _time)
        {
            return ((_time % (ulong)3600) / (ulong)60);
        }

        public static ulong getSecond(ulong _time)
        {
            return (_time % (ulong)60);
        }

        //--------------------------------------------------------------------------------
        public const uint OnDaySecond = 24 * 60 * 60;
        public const uint OnHourSecond = 60 * 60;


        public static string GetTimeString(string format, long seconds)
        {
            string label = format;
            int ms = (int)(seconds * 1000);
            int s = (int)seconds;
            int m = s / 60;
            int h = m / 60;
            int d = h / 24;

            string t = "";
            //day
            if (label.IndexOf("%dd") >= 0)
            {
                t = d >= 10 ? d.ToString() : ("0" + d.ToString());
                label = label.Replace("%dd", t);
                h = h % 24;
            }
            else if (label.IndexOf("%d") >= 0)
            {
                label = label.Replace("%d", d.ToString());
                h = h % 24;
            }

            //hour
            if (label.IndexOf("%hh") >= 0)
            {
                t = h >= 10 ? h.ToString() : ("0" + h.ToString());
                label = label.Replace("%hh", t);
                m = m % 60;
            }
            else if (label.IndexOf("%h") >= 0)
            {
                label = label.Replace("%h", h.ToString());
                m = m % 60;
            }

            //minute
            if (label.IndexOf("%mm") >= 0)
            {
                t = m >= 10 ? m.ToString() : ("0" + m.ToString());
                label = label.Replace("%mm", t);
                s = s % 60;
            }
            else if (label.IndexOf("%m") >= 0)
            {
                label = label.Replace("%m", m.ToString());
                s = s % 60;
            }

            //second
            if (label.IndexOf("%ss") >= 0)
            {
                t = s >= 10 ? s.ToString() : ("0" + s.ToString());
                label = label.Replace("%ss", t);
                ms = ms % 1000;
            }
            else if (label.IndexOf("%s") >= 0)
            {
                label = label.Replace("%s", s.ToString());
                ms = ms % 1000;
            }

            //ms
            if (label.IndexOf("ms") >= 0)
            {
                t = ms.ToString();
                label = label.Replace("%ms", t);
            }

            return label;
        }

        /// <summary>
        /// do not show days
        /// </summary>
        /// <param name="format"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string GetTimeStringV2(string format, long seconds)
        {
            string label = format;
            int ms = (int)(seconds * 1000);
            int s = (int)seconds;
            int m = s / 60;
            int h = m / 60;
            int d = h / 24;

            string t = "";
            //days
            if (label.IndexOf("%dd") >= 0)
            {
                t = d >= 10 ? d.ToString() : ("0" + d.ToString());
                label = label.Replace("%dd", t);
            }
            else if (label.IndexOf("%d") >= 0)
            {
                label = label.Replace("%d", d.ToString());
            }
            h = h % 24;

            //hours
            if (label.IndexOf("%hh") >= 0)
            {
                t = h >= 10 ? h.ToString() : ("0" + h.ToString());
                label = label.Replace("%hh", t);
            }
            else if (label.IndexOf("%h") >= 0)
            {
                label = label.Replace("%h", h.ToString());
            }
            m = m % 60;

            //mins
            if (label.IndexOf("%mm") >= 0)
            {
                t = m >= 10 ? m.ToString() : ("0" + m.ToString());
                label = label.Replace("%mm", t);
            }
            else if (label.IndexOf("%m") >= 0)
            {
                label = label.Replace("%m", m.ToString());
            }
            s = s % 60;

            //secs
            if (label.IndexOf("%ss") >= 0)
            {
                t = s >= 10 ? s.ToString() : ("0" + s.ToString());
                label = label.Replace("%ss", t);
            }
            else if (label.IndexOf("%s") >= 0)
            {
                label = label.Replace("%s", s.ToString());
            }
            ms = ms % 1000;

            //ms
            if (label.IndexOf("ms") >= 0)
            {
                t = ms.ToString();
                label = label.Replace("%ms", t);
            }

            return label;
        }


        public static uint GetUnixTime()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));

            DateTime nowTime = DateTime.Now;

            uint unixTime = (uint)(Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero) / 1000);
            return unixTime;
        }

        // 
        public static uint GetUnixTime(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));

            uint unixTime = (uint)(Math.Round((time - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero) / 1000);
            return unixTime;
        }

        public static DateTime GetLocalTime(uint timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dtResult = dtStart.Add(toNow);
            return dtResult;
        }


        private static uint DAY_PER_YEAR = 365;
        private static uint DAY_PER_MONTH = 30;
        private static uint DAY_PER_WEEK = 7;
        public static string DateStringFromNow(DateTime dt)
        {
            TimeSpan span = DateTime.Now - dt;

            double year = (span.TotalDays / DAY_PER_YEAR);
            double month = (span.TotalDays / DAY_PER_MONTH);
            double week = (span.TotalDays / DAY_PER_WEEK);

            if (year > 1)
            {
                return string.Format("{0}years ago", (int)Math.Floor(year));
            }
            else if (month > 1)
            {
                return string.Format("{0}months ago", (int)Math.Floor(month));
            }
            else if (week > 1)
            {
                return string.Format("{0}weeks ago", (int)Math.Floor(week));
            }
            else if (span.TotalDays > 1)
            {
                return string.Format("{0}days ago", (int)Math.Floor(span.TotalDays));
            }
            else if (span.TotalHours > 1)
            {
                return string.Format("{0}hours ago", (int)Math.Floor(span.TotalHours));
            }
            else if (span.TotalMinutes > 1)
            {
                return string.Format("{0}mins ago", (int)Math.Floor(span.TotalMinutes));
            }
            else
            {
                return string.Format("{0}secs ago", (int)Math.Floor(span.TotalSeconds));
            }
        }


    }
}

