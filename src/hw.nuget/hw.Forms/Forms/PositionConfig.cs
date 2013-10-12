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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;

namespace hw.Forms
{
    /// <summary>
    /// Used to persist location of window of parent
    /// </summary>
    public sealed class PositionConfig : IDisposable
    {
        readonly Form _target;
        readonly string _name;
        bool _initialLocationSet;
    
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="target"></param>
        public PositionConfig(Form target)
        {
            _target = target;
            _name = target.Name;
            _target.Load += OnLoad;
            _target.LocationChanged += OnLocationChanged;
            _target.SizeChanged += OnLocationChanged;
        }

        void IDisposable.Dispose()
        {
            _target.Load -= OnLoad;
            _target.LocationChanged -= OnLocationChanged;
            _target.SizeChanged -= OnLocationChanged;
        }

        void OnLocationChanged(object s, EventArgs e) { SavePosition((Form) s); }
        void OnLoad(object s, EventArgs e) { LoadPosition((Form) s); }

        Rectangle? Position { get { return Convert(0, null, s => (Rectangle?) new RectangleConverter().ConvertFromString(s)); } set { Save(value, WindowState); } }

        string[] ParameterStrings
        {
            get
            {
                var content = _name.FileHandle().String;
                return content == null ? null : content.Split('\n');
            }
        }

        void Save(Rectangle? position, FormWindowState state)
        {
            _name.FileHandle().String = "{0}\n{1}"
                .ReplaceArgs(
                    position == null ? "" : new RectangleConverter().ConvertToString(position.Value),
                    state
                );
        }

        FormWindowState WindowState { get { return Convert(1, FormWindowState.Normal, s => s.Parse<FormWindowState>()); } set { Save(Position, value); } }

        T Convert<T>(int position, T defaultValue, Func<string, T> converter)
        {
            if(ParameterStrings == null)
                return defaultValue;
            return converter(ParameterStrings[position]);
        }
        void LoadPosition(Form treeForm)
        {
            if(Position != null)
            {
                treeForm.SuspendLayout();
                treeForm.StartPosition = FormStartPosition.Manual;
                treeForm.Bounds = EnsureVisible(Position.Value);
                treeForm.WindowState = WindowState;
                treeForm.ResumeLayout(true);
            }
            _initialLocationSet = true;
        }

        void SavePosition(Form treeForm)
        {
            if(!_initialLocationSet)
                return;

            if(treeForm.WindowState == FormWindowState.Normal)
                Position = treeForm.Bounds;

            WindowState = treeForm.WindowState;
        }

        static Rectangle EnsureVisible(Rectangle value)
        {
            if(Screen.AllScreens.Any(s => s.Bounds.IntersectsWith(value)))
                return value;
            var closestScreen = Screen.FromRectangle(value);
            throw new NotImplementedException();
        }
    }
}