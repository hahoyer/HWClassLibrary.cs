using System;
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
    [PublicAPI]
    public sealed class PositionConfig : IDisposable
    {
        readonly Func<string> GetFileName;
        Form InternalTarget;
        bool LoadPositionCalled;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="getFileName">
        ///     function to obtain filename of configuration file.
        ///     <para>It will be called each time the name is required. </para>
        ///     <para>Default: Target.Name</para>
        /// </param>
        public PositionConfig(Func<string> getFileName = null)
            => GetFileName = getFileName ?? (() => InternalTarget == null? null : InternalTarget.Name);

        /// <summary>
        ///     Form that will be controlled by this instance
        /// </summary>
        public Form Target
        {
            [UsedImplicitly]
            get => InternalTarget;
            set
            {
                Disconnect();
                InternalTarget = value;
                Connect();
            }
        }

        /// <summary>
        ///     Name that will be used as filename
        /// </summary>
        public string FileName => GetFileName();

        Rectangle? Position
        {
            get => Convert
                (0, null, s => (Rectangle?)new RectangleConverter().ConvertFromString(s));
            set => Save(value, WindowState);
        }

        string[] ParameterStrings
        {
            get
            {
                if(InternalTarget == null)
                    return null;

                var content = FileHandle.String;
                return content == null? null : content.Split('\n');
            }
        }

        SmbFile FileHandle
        {
            get
            {
                var fileName = FileName;
                return fileName == null? null : fileName.ToSmbFile();
            }
        }

        FormWindowState WindowState
        {
            get => Convert(1, FormWindowState.Normal, s => s.Parse<FormWindowState>());
            set => Save(Position, value);
        }

        void IDisposable.Dispose() { Disconnect(); }

        void Disconnect()
        {
            if(InternalTarget == null)
                return;

            InternalTarget.SuspendLayout();
            LoadPositionCalled = false;
            InternalTarget.Load -= OnLoad;
            InternalTarget.LocationChanged -= OnLocationChanged;
            InternalTarget.SizeChanged -= OnLocationChanged;
            InternalTarget.ResumeLayout();
            InternalTarget = null;
        }

        void Connect()
        {
            if(InternalTarget == null)
                return;
            InternalTarget.SuspendLayout();
            LoadPositionCalled = false;
            InternalTarget.Load += OnLoad;
            InternalTarget.LocationChanged += OnLocationChanged;
            InternalTarget.SizeChanged += OnLocationChanged;
            InternalTarget.ResumeLayout();
        }

        void OnLocationChanged(object target, EventArgs e)
        {
            if(target != InternalTarget)
                return;
            SavePosition();
        }

        void OnLoad(object target, EventArgs e)
        {
            if(target != InternalTarget)
                return;
            LoadPosition();
        }

        void Save(Rectangle? position, FormWindowState state)
        {
            var fileHandle = FileHandle;
            (fileHandle != null).Assert();
            fileHandle.String = "{0}\n{1}"
                .ReplaceArgs
                (
                    position == null? "" : new RectangleConverter().ConvertToString(position.Value),
                    state
                );
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
            (fileHandle != null).Assert();
            if(fileHandle.String != null)
            {
                var position = Position;
                (position != null).Assert();
                InternalTarget.SuspendLayout();
                InternalTarget.StartPosition = FormStartPosition.Manual;
                InternalTarget.Bounds = EnsureVisible(position.Value);
                InternalTarget.WindowState = WindowState;
                InternalTarget.ResumeLayout(true);
            }

            LoadPositionCalled = true;
        }

        void SavePosition()
        {
            if(!LoadPositionCalled)
                return;

            if(InternalTarget.WindowState == FormWindowState.Normal)
                Position = InternalTarget.Bounds;

            WindowState = InternalTarget.WindowState;
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
                result.X += leftDistance < rightDistance? -(leftDistance + 10) : rightDistance + 10;

            var topDistance = value.Top - closestScreen.Bounds.Bottom;
            var bottomDistance = value.Bottom - closestScreen.Bounds.Top;

            if(topDistance > 0 && bottomDistance > 0)
                result.Y += topDistance < bottomDistance? -(topDistance + 10) : bottomDistance + 10;

            closestScreen.Bounds.IntersectsWith(result).Assert();
            return result;
        }
    }
}