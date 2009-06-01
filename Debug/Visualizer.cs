using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace HWClassLibrary.Debug
{
    public class Visualizer : DialogDebuggerVisualizer
    {
        private readonly TreeForm _treeForm = new TreeForm();

        /// <summary>
        /// </summary>
        /// <param name="windowService">An object of type <see cref="T:Microsoft.VisualStudio.DebuggerVisualizers.IDialogVisualizerService" />, which provides methods your visualizer can use to display Windows forms, controls, and dialogs.</param>
        /// <param name="objectProvider">An object of type <see cref="T:Microsoft.VisualStudio.DebuggerVisualizers.IVisualizerObjectProvider" />. This object provides communication from the debugger side of the visualizer to the object source (<see cref="T:Microsoft.VisualStudio.DebuggerVisualizers.VisualizerObjectSource" />) on the debuggee side.</param>
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            _treeForm.Target = objectProvider.GetObject();
            windowService.ShowDialog(_treeForm);
        }

        public static void TestShowVisualizer(object objectToVisualize)
        {
            var myHost = new VisualizerDevelopmentHost(objectToVisualize, typeof (Visualizer));
            myHost.ShowVisualizer();
        }
    }
}