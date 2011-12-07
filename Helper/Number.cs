// 
//     Project HWClassLibrary
//     Copyright (C) 2011 - 2011 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    /// <summary>
    ///     Helper functions for numbers
    /// </summary>
    public static class Number
    {
        /// <summary>
        ///     Gets the giga.
        /// </summary>
        /// <value>The giga.</value>
        /// [created 30.08.2006 22:24]
        public static int Giga { get { return 1000 * 1000 * 1000; } }

        /// <summary>
        ///     Gets the mega.
        /// </summary>
        /// <value>The mega.</value>
        /// [created 30.08.2006 22:24]
        public static int Mega { get { return 1000 * 1000; } }

        /// <summary>
        ///     Gets the kilo.
        /// </summary>
        /// <value>The kilo.</value>
        /// [created 30.08.2006 22:24]
        public static int Kilo { get { return 1000; } }

        /// <summary>
        ///     Formats a number by use of Giga, Mega anf kilo symbols.
        /// </summary>
        /// <param name="i"> The i. </param>
        /// <returns> </returns>
        /// [created 30.08.2006 22:24]
        public static string SmartDump(int i)
        {
            Single x = i;
            if(i < Kilo)
                return i.ToString();
            if(i < 10 * Kilo)
                return (x / Kilo).ToString("N2") + "k";
            if(i < 100 * Kilo)
                return (x / Kilo).ToString("N1") + "k";
            if(i < Mega)
                return (x / Kilo).ToString("N0") + "k";
            if(i < 10 * Mega)
                return (x / Mega).ToString("N2") + "M";
            if(i < 100 * Mega)
                return (x / Mega).ToString("N1") + "M";
            if(i < Giga)
                return (x / Mega).ToString("N0") + "M";
            if(i < 10 * Giga)
                return (x / Giga).ToString("N2") + "G";
            if(i < 100 * Giga)
                return (x / Giga).ToString("N1") + "G";

            throw new NotImplementedException();
        }
    }
}