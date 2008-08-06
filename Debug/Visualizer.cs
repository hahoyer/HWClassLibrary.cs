using System;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Microsoft.VisualStudio.DebuggerVisualizers;

[assembly : DebuggerVisualizer(
    typeof(Visualizer),
    Target = typeof(Dumpable),
    Description = "Dumpable")]

namespace HWClassLibrary.Debug
{
    public class Visualizer : DialogDebuggerVisualizer
    {
        /// <summary>
        /// </summary>
        /// <param name="windowService">An object of type <see cref="T:Microsoft.VisualStudio.DebuggerVisualizers.IDialogVisualizerService" />, which provides methods your visualizer can use to display Windows forms, controls, and dialogs.</param>
        /// <param name="objectProvider">An object of type <see cref="T:Microsoft.VisualStudio.DebuggerVisualizers.IVisualizerObjectProvider" />. This object provides communication from the debugger side of the visualizer to the object source (<see cref="T:Microsoft.VisualStudio.DebuggerVisualizers.VisualizerObjectSource" />) on the debuggee side.</param>
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            windowService.ShowDialog(new TreeForm
            {
                Target = objectProvider.GetObject(),
                Text = objectProvider.GetObject().GetType().FullName
            });
        }
        public static void TestShowVisualizer(object objectToVisualize)
        {
            var myHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(Visualizer));
            myHost.ShowVisualizer();
        }
    }


}