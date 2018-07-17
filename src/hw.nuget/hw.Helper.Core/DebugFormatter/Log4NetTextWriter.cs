using System;
using System.IO;
using System.Text;
using hw.Helper;
using log4net;
using log4net.Config;
using log4net.Core;

namespace hw.DebugFormatter
{
    public sealed class Log4NetTextWriter : TextWriter
    {
        public sealed class Information
        {
            public string Caller;
            public Level Level;

            public void Write(string value)
            {
                var log = LogManager.GetLogger(Caller);
                log.Logger.Log(typeof(Log4NetTextWriter), Level, value, null);
            }
        }

        sealed class MissingLog4NetXmlFileException : Exception {}

        static Log4NetTextWriter InstanceCache;
        static Log4NetTextWriter Instance => InstanceCache ?? (InstanceCache = GetInstance());

        static Log4NetTextWriter GetInstance()
            => "log4net.xml".ToSmbFile().Exists ? new Log4NetTextWriter() : null;

        public static void Register(bool exclusive = true)
        {
            if(!exclusive)
                return;

            if(Instance == null)
                throw new MissingLog4NetXmlFileException();

            Console.SetOut(Instance);
        }

        public static void WriteToLog(string value, bool isLine, Information logData, string indentedValue)
            => Instance?.Write(value, isLine, logData, indentedValue);

        string LineStartForLog = "";

        Log4NetTextWriter() {XmlConfigurator.Configure();}

        public override Encoding Encoding => Encoding.UTF8;

        void Write(string value, bool isLine, Information logData, string indentedValue)
        {
            if(isLine)
            {
                logData?.Write(LineStartForLog + indentedValue);
                LineStartForLog = "";
            }
            else
                LineStartForLog += value;
        }

        public override void Write(char value) {}
        public override void Write(string value) {}

        public override void WriteLine(string value) {}
    }
}