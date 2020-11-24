using System;
using System.Globalization;

namespace System
{
    public class ChinaDateTime
    {
        private int year;
        private int month;
        private int dayofmonth;
        private bool isleap;
        public DateTime time;
        private ChineseLunisolarCalendar cc;
        public int Year
        {
            get
            {
                return this.year;
            }
        }
        public int Month
        {
            get
            {
                return this.month;
            }
        }
        public int DayOfMonth
        {
            get
            {
                return this.dayofmonth;
            }
        }
        public bool IsLeap
        {
            get
            {
                return this.isleap;
            }
        }
        public ChinaDateTime(DateTime time)
        {
            this.cc = new ChineseLunisolarCalendar();
            if (time > this.cc.MaxSupportedDateTime || time < this.cc.MinSupportedDateTime)
            {
                string arg_7E_0 = "参数日期时间不在支持范围内，支持范围：";
                DateTime dateTime = this.cc.MinSupportedDateTime;
                string arg_7E_1 = dateTime.ToString();
                string arg_7E_2 = "到";
                dateTime = this.cc.MaxSupportedDateTime;
                throw new Exception(arg_7E_0 + arg_7E_1 + arg_7E_2 + dateTime.ToString());
            }
            this.year = this.cc.GetYear(time);
            this.month = this.cc.GetMonth(time);
            this.dayofmonth = this.cc.GetDayOfMonth(time);
            this.isleap = this.cc.IsLeapMonth(this.year, this.month);
            this.time = time;
        }
    }
}
