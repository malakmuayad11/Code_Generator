using System.Diagnostics;
using System.Threading.Tasks;

namespace Logger
{
    public static class clsLogger
    {
        private static string _Source;
        private static string _LogName;
        static clsLogger()
        {
            _Source = "CodeGenerator";
            _LogName = "Application";
        }
        public static async Task Log(string Message, EventLogEntryType Type)
        {
            if (!EventLog.SourceExists(_Source))
            {
                EventLog.CreateEventSource(_Source, _LogName);
                await Task.Delay(500); // Allow time for the source to be registered
            }

            EventLog.WriteEntry(_Source, Message, Type);
        }
    }
}