using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using hw.Helper;
using JetBrains.Annotations;
using log4net;
using log4net.Config;
using log4net.Core;

namespace hw.DebugFormatter
{
    [PublicAPI]
    public sealed class Log4NetTextWriter : TextWriter
    {
        public sealed class Information
        {
            [PublicAPI]
            public string Caller;

            [PublicAPI]
            public Level Level;

            public void Write(string value)
            {
                var log = LogManager.GetLogger(Caller);
                log.Logger.Log(typeof(Log4NetTextWriter), Level, value, null);
            }

            public static Information AutoCreate()
                => new Information
                {
                    Caller = FindStackFrame().GetMethod().ReflectedType.CompleteName(),
                    Level = Level.Info
                };

            static StackFrame FindStackFrame()
            {
                var stackTrace = new StackTrace(true);

                var isLoggingLevel = false;
                for(var frameDepth = 0; frameDepth < stackTrace.FrameCount - 1; frameDepth++)
                {
                    var stackFrame = stackTrace.GetFrame(frameDepth);
                    var methodBase = stackFrame.GetMethod();
                    if(methodBase.GetAttribute<IsLoggingFunction>(true) != null)
                        isLoggingLevel = true;
                    else if(isLoggingLevel)
                        return stackTrace.GetFrame(frameDepth);
                }

                return stackTrace.GetFrame(1);
            }

            static MethodInfo Method(Action<string> action) => action.Method;
        }

        sealed class MissingLog4NetXmlFileException : Exception { }

        static Log4NetTextWriter InstanceCache;

        string LineStartForLog = "";


        Log4NetTextWriter() => XmlConfigurator.Configure((FileInfo)"log4net.xml".ToSmbFile().FileSystemInfo);

        static Log4NetTextWriter Instance => InstanceCache ??= GetInstance();

        public override Encoding Encoding => Encoding.UTF8;

        static Log4NetTextWriter GetInstance()
        {
            var result = "log4net.xml".ToSmbFile().Exists? new Log4NetTextWriter() : null;
            return result;
        }

        [PublicAPI]
        public static void Register(bool exclusive = true)
        {
            if(Instance == null)
                throw new MissingLog4NetXmlFileException();

            Console.SetOut(exclusive? (TextWriter)Instance : new TextWriters(Instance, Console.Out));
        }

        public static void WriteToLog(string value, bool isLine, Information logData, string indentedValue)
            => Instance?.Write(value, isLine, logData, indentedValue);

        void Write(string value, bool isLine, Information logData, string indentedValue)
        {
            if(isLine)
            {
                logData?.Write(LineStartForLog + indentedValue + value);
                LineStartForLog = "";
            }
            else
                LineStartForLog += value;
        }

        public override void Write(char value) => Write(value + "", false, Information.AutoCreate(), "");
        public override void Write(string value) => Write(value, false, Information.AutoCreate(), "");
        public override void WriteLine(string value) => Write(value, true, Information.AutoCreate(), "");
    }
}