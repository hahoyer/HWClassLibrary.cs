# Overview

Provides some support for log4net logging.
It installs a text writer that uses log4net.
The writer is registered using:

    Log4NetTextWriter.Register();
This will register log4net as the only text-writer. 
To keep other writers like console or debug-textwriter, use 

    Log4NetTextWriter.Register(exclusive:false);
Those calls should be made as early as possible.

# Issues and Co.
**known issue:**  The built-in debug-textwriter will probably (self-)register later. 
If you don't need it just disable it via: 

    DebugTextWriter.Enabled = false;


- Report to project website on github (https://github.com/hahoyer/HWClassLibrary.cs)
- issues and feedback: https://github.com/hahoyer/HWClassLibrary.cs/issues
- [![Support me on ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/G2G4BH6WX)

