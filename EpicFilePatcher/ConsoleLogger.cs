using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicFilePatcher
{
    public enum LogType
    {
        Info,
        Warning,
        Error
    }
    static class ConsoleLogger
    {
        public static void Log(LogType type, string? s)
        {
            Console.WriteLine($"[{DateTime.Now}] [{Enum.GetName(typeof(LogType), type)}] {s ?? "null"}");
        }
    }
}
