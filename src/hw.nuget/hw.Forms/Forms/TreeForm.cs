using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace hw.Forms
{
    public sealed partial class TreeForm : Form
    {
        readonly PositionConfig _positionConfig;
        object _target;
        public TreeForm()
        {
            InitializeComponent();
            _positionConfig = new PositionConfig {Target = this};
        }

        public object Target
        {
            get { return _target; }
            set
            {
                _target = value;
                treeView1.Connect(_target);
                Text = _target.GetAdditionalInfo();
            }
        }
    }
}