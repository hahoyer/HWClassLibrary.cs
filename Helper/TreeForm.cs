using System;
using System.Windows.Forms;
using HWClassLibrary.Debug;

namespace HWClassLibrary.Helper
{
    [Obsolete("Use version from HWClassLibrary.TreeStructure", true)]
    public partial class TreeForm : Form
    {
        private object _target;
        
        public TreeForm() { InitializeComponent(); }
        
        public object Target
        {
            get { return _target; }
            set
            {
                _target = value;
                Service.Connect(treeView1, _target);
                Text = Service.GetAdditionalInfo(value);
            }
        }
    }
}