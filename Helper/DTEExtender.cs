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
using EnvDTE;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    static class DTEExtender
    {
        public static string[] ItemFileNames(this ProjectItem item)
        {
            var result = new List<string>();
            for(short i = 0; i < item.FileCount; i++)
                result.Add(item.FileNames[i]);
            return result.ToArray();
        }

        public static string FileName(this ProjectItem item) { return ItemFileNames(item).Single(); }
    }
}