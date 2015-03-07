using System;
using System.Diagnostics;
using System.Reflection;
using SquidReports.DataCollector.Interface;
using NLog;

namespace SquidReports.DataCollector
{
    public class NLogManager : ILogManager
    {
        public ILogger GetCurrentClassLogger()
        {
            return new NLogger(LogManager.GetLogger(GetClassFullName()));
        }

        private static string GetClassFullName()
        {
            // I stole this from the NLog source :)
            // https://github.com/NLog/NLog/blob/master/src/NLog/LogManager.cs#L347

            string className;
            Type declaringType;
            int framesToSkip = 2;

            do
            {
                StackFrame frame = new StackFrame(framesToSkip, false);
                MethodBase method = frame.GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    className = method.Name;
                    break;
                }

                framesToSkip++;
                className = declaringType.FullName;
            } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return className;
        }
    }
}