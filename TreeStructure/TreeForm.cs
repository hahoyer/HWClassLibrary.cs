#region Copyright (C) 2012

//     Project HWClassLibrary
//     Copyright (C) 2011 - 2012 Harald Hoyer
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
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWClassLibrary.TreeStructure
{
    public sealed partial class TreeForm : Form
    {
        readonly PositionConfig _positionConfig = new PositionConfig("TreeForm.Position");
        object _target;
        bool _initialLocationSet;
        public TreeForm() { InitializeComponent(); }

        public object Target
        {
            get { return _target; }
            set
            {
                _target = value;
                treeView1.Connect(_target);
                Text = value.GetAdditionalInfo();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if(_positionConfig.Position != null)
            {
                SuspendLayout();
                StartPosition = FormStartPosition.Manual;
                Bounds = EnsureVisible(_positionConfig.Position.Value);
                WindowState = _positionConfig.WindowState;
                ResumeLayout(true);
            }
            _initialLocationSet = true;
        }
        static Rectangle EnsureVisible(Rectangle value)
        {
            if (Screen.AllScreens.Any(s => s.Bounds.IntersectsWith(value)))
                return value;
            var closestScreen = Screen.FromRectangle(value);
            throw new NotImplementedException();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            if(_initialLocationSet)
                _positionConfig.Position = Bounds;
            base.OnLocationChanged(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            _positionConfig.Position = Bounds;
            _positionConfig.WindowState = WindowState;
            base.OnSizeChanged(e);
        }
    }

    sealed class PositionConfig
    {
        readonly string _name;
        public PositionConfig(string name) { _name = name; }
        internal Rectangle? Position
        {
            get
            {
                var content = _name.FileHandle().String;
                if(content == null)
                    return null;

                return (Rectangle?) new RectangleConverter().ConvertFromString(content.Split(' ')[0]);
            }
            set { Save(value, WindowState); }
        }

        void Save(Rectangle? position, FormWindowState state)
        {
            _name.FileHandle().String
                = new RectangleConverter().ConvertToString(position) + " " + state;
        }

        internal FormWindowState WindowState
        {
            get
            {
                var content = _name.FileHandle().String;
                if(content == null)
                    return FormWindowState.Normal;
                return content.Split(' ')[1].Parse<FormWindowState>();
            }
            set { Save(Position, value); }
        }

    }
}