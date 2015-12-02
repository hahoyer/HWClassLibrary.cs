using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

// ReSharper disable ConvertMethodToExpressionBody
// ReSharper disable MergeConditionalExpression

namespace hw.Forms
{
    /// <summary>
    ///     Used to persist location of window of parent
    /// </summary>
    public sealed class PositionConfig : IDisposable
    {
        Form _target;
        bool _loadPositionCalled;
        readonly Func<string> _getFileName;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="getFileName">
        ///     function to obtain filename of configuration file.
        ///     <para>It will be called each time the name is required. </para>
        ///     <para>Default: Target.Name</para>
        /// </param>
        public PositionConfig(Func<string> getFileName = null)
        {
            _getFileName = getFileName ?? (() => _target == null ? null : _target.Name);
        }

        /// <summary>
        ///     Form that will be controlled by this instance
        /// </summary>
        public Form Target
        {
            [UsedImplicitly]
            get { return _target; }
            set
            {
                Disconnect();
                _target = value;
                Connect();
            }
        }

        /// <summary>
        ///     Name that will be used as filename
        /// </summary>
        public string FileName { get { return _getFileName(); } }

        void IDisposable.Dispose() { Disconnect(); }

        void Disconnect()
        {
            if(_target == null)
                return;

            _target.SuspendLayout();
            _loadPositionCalled = false;
            _target.Load -= OnLoad;
            _target.LocationChanged -= OnLocationChanged;
            _target.SizeChanged -= OnLocationChanged;
            _target.ResumeLayout();
            _target = null;
        }

        void Connect()
        {
            if(_target == null)
                return;
            _target.SuspendLayout();
            _loadPositionCalled = false;
            _target.Load += OnLoad;
            _target.LocationChanged += OnLocationChanged;
            _target.SizeChanged += OnLocationChanged;
            _target.ResumeLayout();
        }

        void OnLocationChanged(object target, EventArgs e)
        {
            if(target != _target)
                return;
            SavePosition();
        }

        void OnLoad(object target, EventArgs e)
        {
            if(target != _target)
                return;
            LoadPosition();
        }

        Rectangle? Position
        {
            get
            {
                return Convert
                    (0, null, s => (Rectangle?) new RectangleConverter().ConvertFromString(s));
            }
            set { Save(value, WindowState); }
        }

        string[] ParameterStrings
        {
            get
            {
                if(_target == null)
                    return null;

                var content = FileHandle.String;
                return content == null ? null : content.Split('\n');
            }
        }

        File FileHandle
        {
            get
            {
                var fileName = FileName;
                return fileName == null ? null : fileName.FileHandle();
            }
        }

        void Save(Rectangle? position, FormWindowState state)
        {
            var fileHandle = FileHandle;
            Tracer.Assert(fileHandle != null);
            fileHandle.String = "{0}\n{1}"
                .ReplaceArgs
                (
                    position == null ? "" : new RectangleConverter().ConvertToString(position.Value),
                    state
                );
        }

        FormWindowState WindowState
        {
            get { return Convert(1, FormWindowState.Normal, s => s.Parse<FormWindowState>()); }
            set { Save(Position, value); }
        }

        T Convert<T>(int position, T defaultValue, Func<string, T> converter)
        {
            return ParameterStrings == null || ParameterStrings.Length <= position
                ? defaultValue
                : converter(ParameterStrings[position]);
        }

        void LoadPosition()
        {
            var fileHandle = FileHandle;
            Tracer.Assert(fileHandle != null);
            if(fileHandle.String != null)
            {
                var position = Position;
                Tracer.Assert(position != null);
                _target.SuspendLayout();
                _target.StartPosition = FormStartPosition.Manual;
                _target.Bounds = EnsureVisible(position.Value);
                _target.WindowState = WindowState;
                _target.ResumeLayout(true);
            }
            _loadPositionCalled = true;
        }

        void SavePosition()
        {
            if(!_loadPositionCalled)
                return;

            if(_target.WindowState == FormWindowState.Normal)
                Position = _target.Bounds;

            WindowState = _target.WindowState;
        }

        static Rectangle EnsureVisible(Rectangle value)
        {
            var allScreens = Screen.AllScreens;
            if(allScreens.Any(s => s.Bounds.IntersectsWith(value)))
                return value;
            var closestScreen = Screen.FromRectangle(value);
            var result = value;

            var leftDistance = value.Left - closestScreen.Bounds.Right;
            var rightDistance = value.Right - closestScreen.Bounds.Left;

            if(leftDistance > 0 && rightDistance > 0)
                result.X += leftDistance < rightDistance ? -(leftDistance + 10) : rightDistance + 10;

            var topDistance = value.Top - closestScreen.Bounds.Bottom;
            var bottomDistance = value.Bottom - closestScreen.Bounds.Top;

            if(topDistance > 0 && bottomDistance > 0)
                result.Y += topDistance < bottomDistance ? -(topDistance + 10) : bottomDistance + 10;

            Tracer.Assert(closestScreen.Bounds.IntersectsWith(result));
            return result;
        }
    }
}