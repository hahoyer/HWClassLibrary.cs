using System.Windows.Forms;
using JetBrains.Annotations;
// ReSharper disable CheckNamespace

namespace hw.Forms
{
    [PublicAPI]
    public sealed partial class TreeForm : Form
    {
        readonly PositionConfig PositionConfig;
        object TargetValue;

        public TreeForm()
        {
            InitializeComponent();
            PositionConfig = new PositionConfig {Target = this};
        }

        public object Target
        {
            get => TargetValue;
            set
            {
                TargetValue = value;
                treeView1.Connect(TargetValue);
                Text = TargetValue.GetAdditionalInfo();
            }
        }
    }
}