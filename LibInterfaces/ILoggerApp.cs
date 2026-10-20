using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LibInterfaces
{
    public interface ILoggerApp
    {
       
        public void Exception(Exception ex, [CallerMemberName] String member = "", [CallerFilePath] String file = "", [CallerLineNumber] int line = 0);
        public void Info(String message, [CallerMemberName] String member = "", [CallerFilePath] String file = "", [CallerLineNumber] int line = 0);
        public void Error(String message, [CallerMemberName] String member = "", [CallerFilePath] String file = "", [CallerLineNumber] int line = 0);
        public void Error(String message, Exception? ex, [CallerMemberName] String member = "", [CallerFilePath] String file = "", [CallerLineNumber] int line = 0);

        public void Warn(String message, [CallerMemberName] String member = "", [CallerFilePath] String file = "", [CallerLineNumber] int line = 0);

        public void Debug(String message, [CallerMemberName] String member = "", [CallerFilePath] String file = "", [CallerLineNumber] int line = 0);

        public void Verbose(String message, [CallerMemberName] String member = "", [CallerFilePath] String file = "", [CallerLineNumber] int line = 0);
        public void Log(String message, LogLevel level, [CallerMemberName] String member = "", [CallerFilePath] String file = "", [CallerLineNumber] int line = 0);
    
    }
}
