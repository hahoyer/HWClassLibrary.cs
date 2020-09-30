using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using hw.Helper;

namespace hw.DebugFormatter
{
    sealed class Writer
    {
        sealed class WriteInitiator
        {
            string _name = "";
            string _lastName = "";

            public bool ThreadChanged { get { return _name != _lastName; } }

            public string ThreadFlagString { get { return "[" + _lastName + "->" + _name + "]\n"; } }

            public void NewThread()
            {
                _lastName = _name;
                _name = Thread.CurrentThread.ManagedThreadId.ToString();
            }
        }

        int _indentCount;
        bool _isLineStart = true;
        readonly WriteInitiator _writeInitiator = new WriteInitiator();

        public Writer() { DebugTextWriter.Register(); }

        internal void IndentStart() { _indentCount++; }
        internal void IndentEnd() { _indentCount--; }

        internal void ThreadSafeWrite(string s, bool isLine)
        {
            lock(_writeInitiator)
            {
                _writeInitiator.NewThread();

                s = s.Indent(isLineStart: _isLineStart, count: _indentCount);

                if(_writeInitiator.ThreadChanged && Debugger.IsAttached)
                {
                    var threadFlagString = _writeInitiator.ThreadFlagString;
                    if(!_isLineStart)
                        if(s.Length > 0 && s[0] == '\n')
                            threadFlagString = "\n" + threadFlagString;
                        else
                            throw new NotImplementedException();
                    System.Diagnostics.Debug.Write(threadFlagString);
                }

                Write(s, isLine);

                _isLineStart = isLine;
            }
        }

        static void Write(string s, bool isLine)
        {
            if(isLine)
                Console.WriteLine(s);
            else
                Console.Write(s);
        }
    }
}