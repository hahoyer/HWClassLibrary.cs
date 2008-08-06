using System;
using System.Diagnostics;
using HWClassLibrary.Debug;

[assembly: CLSCompliant(true)]
[assembly: DebuggerVisualizer(typeof (Visualizer), Target = typeof (Dumpable), Description = "Dumpable")]
[assembly: DebuggerVisualizer(typeof (Visualizer), Target = typeof (String), Description = "String")]
[assembly: DebuggerVisualizer(typeof(Visualizer), Target = typeof(MarshalByRefObject), Description = "Dumpable")]