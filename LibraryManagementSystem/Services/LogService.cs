using Serilog;
using Serilog.Events;

namespace LibraryManagementSystem.Services
{
    public class LogService
    {
        private readonly ILogger logger;

        public LogService()
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(a => a.File("Logs/log-.txt", rollingInterval: RollingInterval.Day))
                .CreateLogger();


            logger = Serilog.Log.Logger;
        }

        public void Log(string message, LogEventLevel level)
        {
            logger.Write(level, message);
        }
    }
}