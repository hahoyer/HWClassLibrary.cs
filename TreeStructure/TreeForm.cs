using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HWClassLibrary.TreeStructure
{
    public partial class TreeForm : Form
    {
        private object _target;

        public TreeForm()
        {
            InitializeComponent();
        }

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
    }
}