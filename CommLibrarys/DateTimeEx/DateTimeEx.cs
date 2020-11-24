using System;
using System.Collections.Generic;
using System.Text;

namespace CommLibrarys
{
    /// <summary>
    /// 注:
    /// 1.DateTime值类型代表了一个从公元0001年1月1日0点0分0秒到公元9999年12月31日23点59分59秒之间的具体日期时刻。因此，你可以用DateTime值类型来描述任何在想象范围之内的时间。一个DateTime值代表了一个具体的时刻
    /// 2.TimeSpan值包含了许多属性与方法，用于访问或处理一个TimeSpan值下面的列表涵盖了其中的一部分：
    ///  Add：与另一个TimeSpan值相加。 
    ///  Days:返回用天数计算的TimeSpan值。 
    ///  Duration:获取TimeSpan的绝对值。 
    ///  Hours:返回用小时计算的TimeSpan值 
    ///  Milliseconds:返回用毫秒计算的TimeSpan值。 
    ///  Minutes:返回用分钟计算的TimeSpan值。 
    ///  Negate:返回当前实例的相反数。 
    ///  Seconds:返回用秒计算的TimeSpan值。 
    ///  Subtract:从中减去另一个TimeSpan值。 
    ///  Ticks:返回TimeSpan值的tick数。 
    ///  TotalDays:返回TimeSpan值表示的天数。 
    ///  TotalHours:返回TimeSpan值表示的小时数。 
    ///  TotalMilliseconds:返回TimeSpan值表示的毫秒数。 
    ///  TotalMinutes:返回TimeSpan值表示的分钟数。 
    ///  TotalSeconds:返回TimeSpan值表示的秒数。
    /// </summary>
    public class DateTimeEx
    {
        /// <summary>
        /// 获取当前时间的时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetMillisTicks()
        {
            DateTime timeStamp = new DateTime(1970, 1, 1, 0, 0, 0);  //得到1970年的时间戳
            long a = (DateTime.UtcNow.Ticks - timeStamp.Ticks) / 10000;  //注意这里有时区问题，用now就要减掉8个小时
            return a;
        }

        /// <summary>
        /// 获取指定时间的时间戳 毫秒
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetMillisTicks(DateTime time)
        {
            DateTime timeStamp = new DateTime(1970, 1, 1, 0, 0, 0);  //得到1970年的时间戳
            //long a = (time.Ticks - timeStamp.Ticks) / 10000 - 8 * 60 * 60;  //注意这里有时区问题，用now就要减掉8个小时
            long a = (time.Ticks - timeStamp.Ticks) / TimeSpan.TicksPerMillisecond - 8 * 60 * 60 * 1000;
            return a;
        }

        /// <summary>
        /// 获取当前时间的时间戳 秒
        /// </summary>
        /// <returns></returns>
        public static long GetTicks()
        {
            DateTime timeStamp = new DateTime(1970, 1, 1, 0, 0, 0);  //得到1970年的时间戳
            long a = (DateTime.UtcNow.Ticks - timeStamp.Ticks) / TimeSpan.TicksPerSecond;  //注意这里有时区问题，用now就要减掉8个小时
            return a;
        }

        /// <summary>
        /// 获取指定时间的时间戳 秒
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTicks(DateTime time)
        {
            DateTime timeStamp = new DateTime(1970, 1, 1, 0, 0, 0);  //得到1970年的时间戳
            //long a = (time.Ticks - timeStamp.Ticks) / TimeSpan.TicksPerSecond - 8 * 60 * 60;  //注意这里有时区问题，用now就要减掉8个小时
            long time_t = (time.Ticks - timeStamp.Ticks) / TimeSpan.TicksPerSecond - 8 * 60 * 60;  //注意这里有时区问题，用now就要减掉8个小时
            return time_t;
        }

        /// <summary>
        /// 转换毫秒
        /// </summary>
        /// <param name="millis"></param>
        /// <returns></returns>
        public static DateTime ConvertoMillisToDate(long millis)
        {
            System.DateTime time = System.DateTime.MinValue;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0));
            time = startTime.AddMilliseconds(millis);
            return time;

        }

        /// <summary>
        /// 将Unix时间戳转换为DateTime类型时间
        /// </summary>
        /// <param name="d">double 型数字</param>
        /// <returns>DateTime</returns>
        public static DateTime ConvertSecondDateTime(long second)
        {
            System.DateTime time = System.DateTime.MinValue;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0));
            time = startTime.AddSeconds(second);
            return time;
        }

        /// <summary>
        /// 已重载.计算两个日期的时间间隔,返回的是时间间隔的日期差的绝对值.
        /// </summary>
        /// <param name="DateTime1">第一个日期和时间</param>
        /// <param name="DateTime2">第二个日期和时间</param>
        /// <returns></returns>
        private static string DateDiffStr(DateTime DateTime1, DateTime DateTime2)
        {
            string dateDiff = null;
            try
            {
                TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
                TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
                TimeSpan ts = ts1.Subtract(ts2).Duration();
                dateDiff = ts.Days.ToString() + "天"
                        + ts.Hours.ToString() + "小时"
                        + ts.Minutes.ToString() + "分钟"
                        + ts.Seconds.ToString() + "秒";
            }
            catch
            {

            }
            return dateDiff;
        }
        /// <summary>
        /// 已重载.计算一个时间与当前本地日期和时间的时间间隔,返回的是时间间隔的日期差的绝对值.
        /// </summary>
        /// <param name="DateTime1">一个日期和时间</param>
        /// <returns></returns>
        private static string DateDiff(DateTime DateTime1)
        {
            return DateDiffStr(DateTime1, DateTime.Now);
        }
        
        public static long DateDiffInt64(DateTime startTime, DateTime endTime)
        {
            long time_t = endTime.Ticks - startTime.Ticks;
            return time_t;
        }

        public static long DateDiffInt64(DateTime startTime)
        {
            return DateDiffInt64(startTime, DateTime.Now);
        }
    }
}
