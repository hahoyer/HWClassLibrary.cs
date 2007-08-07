using System;
using System.Diagnostics;
using HWClassLibrary.Helper;

namespace HWClassLibrary.Debug
{
	/// <summary>
	/// Summary description for Dumpable.
	/// </summary>
    [dump("Dump")]
    public class Dumpable
	{
        /// <summary>
        /// generate dump string to be shown in debug windows
        /// </summary>
        /// <returns></returns>
        public virtual string DebuggerDump()
        {
            return Tracer.Dump(this);
        }

        /// <summary>
        /// dump string to be shown in debug windows
        /// </summary>
        [DumpData(false)]
        public string DebuggerDumpString { get { return DebuggerDump(); } }

        /// <summary>
        /// Method dump with break,  
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static void NotImplementedFunction(params object[] p)
        {
            string os = Tracer.DumpMethodWithData("not implemented", 1, null, p);
            Tracer.Line(os);
            Debugger.Break();
        }
        /// <summary>
        /// Method start dump, 
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        [DebuggerHidden]
        protected void StartMethodDump(bool trace, params object[] p)
        {
            if (!trace)
                return;
            string os = Tracer.DumpMethodWithData("", 1, this, p);
            Tracer.Line(os);
            Tracer.IndentStart();
        }
        /// <summary>
        /// Method start dump, 
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        [DebuggerHidden]
        protected void StartMethodDumpWithBreak(bool trace, params object[] p)
        {
            if (!trace)
                return;
            string os = Tracer.DumpMethodWithData("", 1, this, p);
            Tracer.Line(os);
            Tracer.IndentStart();
            Debugger.Break();
        }
        /// <summary>
		/// Method dump, 
		/// </summary>
		/// <param name="trace"></param>
		/// <param name="rv"></param>
		/// <returns></returns>
		[DebuggerHidden]
		protected static T ReturnMethodDump<T>(bool trace, T rv)
		{
			if(!trace)
				return rv;
			Tracer.Line(Tracer.MethodHeader(1) + "[returns] "+Tracer.Dump(rv));
			Tracer.IndentEnd();
			return rv;
		}
		/// <summary>
		/// Method dump, 
		/// </summary>
		/// <param name="trace"></param>
		/// <param name="rv"></param>
		/// <returns></returns>
		[DebuggerHidden]
        protected static T ReturnMethodDumpWithBreak<T>(bool trace, T rv)
		{
			if(!trace)
				return rv;
			Tracer.Line("returns "+Tracer.Dump(rv));
			Tracer.IndentEnd();
			Debugger.Break();
			return rv;
		}

		/// <summary>
		/// Method dump with break,  
		/// </summary>
		/// <param name="text"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		[DebuggerHidden]
		protected void DumpMethodWithBreak(string text, params object[] p)
		{
			string os = Tracer.DumpMethodWithData(text, 1, this, p);
			Tracer.Line(os);
			Debugger.Break();
		}
        /// <summary>
        /// Method dump with break,  
        /// </summary>
        /// <param name="text"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        [DebuggerHidden]
        protected static void DumpDataWithBreak(string text, params object[] p)
        {
            string os = Tracer.DumpData(text, 1, p);
            Tracer.Line(os);
            Debugger.Break();
        }
        /// <summary>
        /// Method dump with break,  
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [DebuggerHidden]
        protected void NotImplementedMethod(params object[] p)
        {
            if(IsInDump)
                throw new NotImplementedException();

            string os = Tracer.DumpMethodWithData("not implemented", 1, this, p);
            Tracer.Line(os);
            Debugger.Break();
        }

		/// <summary>
        /// Default dump behaviour
        /// </summary>
        /// <returns></returns>
        public virtual string Dump()
        {
            return GetType().FullName + HWString.Surround("{", DumpData(), "}");
        }

        /// <summary>
        /// Gets a value indicating whether this instance is in dump.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is in dump; otherwise, <c>false</c>.
        /// </value>
        /// created 23.09.2006 17:39
        [DumpData(false)]
        public bool IsInDump { get { return _isInDump; } }
	    
	    bool _isInDump = false;
        /// <summary>
        /// Default dump of data
        /// </summary>
        /// <returns></returns>
        public virtual string DumpData()
        {
            bool oldIsInDump = _isInDump;
            string result;
            _isInDump = true;
            try
            {
                result = Tracer.DumpData(this);
            }
            catch (Exception )
            {
                result = "<not implemented>";
            }
            _isInDump = oldIsInDump;
            return result;
        }

    }
}
