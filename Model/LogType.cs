
using System;
using System.Windows.Forms;

namespace VirtualClassroom.NET.Model
{
    public enum LogType
    {
        Info,
        Trace,
        Warning,
        Error,
        Exception
    }

    public class LogItem
    {
        public DateTime Time { get; set; }
        public LogType Type { get; set; }
        public string Message { get; set; }
    }
}
