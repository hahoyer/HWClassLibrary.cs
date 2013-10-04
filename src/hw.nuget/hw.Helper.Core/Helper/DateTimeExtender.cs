#region Copyright (C) 2013

//     Project hw.nuget
//     Copyright (C) 2013 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Helper
{
    public static class DateTimeExtender
    {
        public static string Format(this DateTime dateTime)
        {
            var result = "";
            result += dateTime.Hour.ToString("00");
            result += ":";
            result += dateTime.Minute.ToString("00");
            result += ":";
            result += dateTime.Second.ToString("00");
            result += ".";
            result += dateTime.Millisecond.ToString("000");
            result += " ";
            result += dateTime.Day.ToString("00");
            result += ".";
            result += dateTime.Month.ToString("00");
            result += ".";
            result += dateTime.Year.ToString("00");
            return result;
        }

        public static string DynamicShortFormat(this DateTime dateTime, bool showMiliseconds)
        {
            var result = "";
            result += dateTime.Hour.ToString("00");
            result += ":";
            result += dateTime.Minute.ToString("00");
            result += ":";
            result += dateTime.Second.ToString("00");
            if(showMiliseconds)
            {
                result += ".";
                result += dateTime.Millisecond.ToString("000");
            }

            var nowDate = DateTime.Now.Date;
            var sameYear = nowDate.Year == dateTime.Year;
            var sameMonth = sameYear && nowDate.Month == dateTime.Month;
            var sameDay = sameMonth && nowDate.Day == dateTime.Day;

            if(!sameDay)
            {
                result += " ";
                result += dateTime.Day.ToString("00");
                result += ".";
            }
            if(!sameMonth)
            {
                result += dateTime.Month.ToString("00");
                result += ".";
            }
            if(!sameYear)
                result += dateTime.Year.ToString("00");
            return result;
        }

        public static string Format3Digits(this TimeSpan timeSpan)
        {
            if(timeSpan.TotalMinutes >= 1)
                return timeSpan.ToString();
            var nanoSeconds = ((long) (timeSpan.TotalMilliseconds * 1000 * 1000)).Format3Digits() + "ns";
            return nanoSeconds.Replace("kns", "µs").Replace("Mns", "ms").Replace("Gns", "s");
        }
    }
}