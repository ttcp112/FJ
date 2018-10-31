using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Utilities
{
    public class DateTimeHelper
    {
        public static string GetAMPM(TimeSpan ts)
        {
            //var res = "";
            DateTime dt = new DateTime().Add(ts);
            //if (dt.ToString("tt").Equals("PM"))
            //{
            //    if (dt.Hour == 12)
            //    {
            //        return res = "00"
            //                + ":" + ((dt.Minute.ToString().Length == 1) ? "0" + (dt.Minute) : (dt.Minute).ToString())
            //                + ":" + ((dt.Second.ToString().Length == 1) ? "0" + (dt.Second) : (dt.Second).ToString())
            //                + " " + dt.ToString("tt");
            //    }
            //}
            return dt.ToString("hh:mm:ss tt");
        }

        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        public static void GetDateTime(ref BaseReportModel model)
        {
            model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
            model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 23, 59, 59);
        }

        public static DateTime SetFromDate(DateTime FromDate)
        {
            return new DateTime(FromDate.Year, FromDate.Month, FromDate.Day, 0, 0, 0);
        }

        public static DateTime SetToDate(DateTime ToDate)
        {
            return new DateTime(ToDate.Year, ToDate.Month, ToDate.Day, 23, 59, 59);
        }

        public static DateTime SetFromDate(DateTime FromDate, TimeSpan TimeSpan)
        {
            return new DateTime(FromDate.Year, FromDate.Month, FromDate.Day, TimeSpan.Hours, TimeSpan.Minutes, TimeSpan.Seconds);
        }

        public static DateTime SetToDate(DateTime ToDate, TimeSpan TimeSpan)
        {
            return new DateTime(ToDate.Year, ToDate.Month, ToDate.Day, TimeSpan.Hours, TimeSpan.Minutes, TimeSpan.Seconds);
        }

        /*editor by trongntn 01/0182017 */

        public static DateTime GetDateImport(string sDateTime, ref string msg)
        {
            DateTime date = DateTime.Now;
            try
            {
                string[] arrDate = sDateTime.Split(' ')[0].Split('/');
                int day = int.Parse(arrDate[0]);
                int month = int.Parse(arrDate[1]);
                int year = int.Parse(arrDate[2]);
                date = new DateTime(year, month, day,12,0,0, DateTimeKind.Unspecified);
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
            }
            return date;
        }

        public static DateTime GetTimeImport(string sTime, ref string msg)
        {
            DateTime date = Commons._MinDate;
            try
            {
                string[] arrTime = sTime.Split(':');
                int hh = int.Parse(arrTime[0]);
                int mm = int.Parse(arrTime[1]);

                date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hh, mm, 59);
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
            }
            return date;
        }
    }
}
