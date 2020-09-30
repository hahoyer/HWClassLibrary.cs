using System;
using System.IO;
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
        static Log4NetTextWriter InstanceCache;

        string LineStartForLog = "";

        Log4NetTextWriter() => XmlConfigurator.Configure();
        static Log4NetTextWriter Instance => InstanceCache ?? (InstanceCache = GetInstance());

        public override Encoding Encoding => Encoding.UTF8;

        static Log4NetTextWriter GetInstance()
            => "log4net.xml".ToSmbFile().Exists ? new Log4NetTextWriter() : null;

        [PublicAPI]
        public static void Register(bool exclusive = true)
        {
            if (!exclusive)
                return;

            if (Instance == null)
                throw new MissingLog4NetXmlFileException();

            Console.SetOut(Instance);
        }

        public static void WriteToLog(string value, bool isLine, Information logData, string indentedValue)
            => Instance?.Write(value, isLine, logData, indentedValue);

        void Write(string value, bool isLine, Information logData, string indentedValue)
        {
            if (isLine)
            {
                logData?.Write(LineStartForLog + indentedValue);
                LineStartForLog = "";
            }
            else
                LineStartForLog += value;
        }

        public override void Write(char value) { }
        public override void Write(string value) { }
        public override void WriteLine(string value) { }

        public sealed class Information
        {
            [PublicAPI] public string Caller;

            [PublicAPI] public Level Level;

            public void Write(string value)
            {
                var log = LogManager.GetLogger(Caller);
                log.Logger.Log(typeof(Log4NetTextWriter), Level, value, null);
            }
        }

        sealed class MissingLog4NetXmlFileException : Exception { }
    }
}