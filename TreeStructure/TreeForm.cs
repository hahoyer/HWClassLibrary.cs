using System;
using System.Windows.Forms;
using HWClassLibrary.Debug;

namespace HWClassLibrary.TreeStructure
{
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
                Extender.Connect(treeView1, _target);
                Text = Extender.GetAdditionalInfo(value);
            }
        }
    }
}