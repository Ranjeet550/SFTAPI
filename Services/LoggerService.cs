using SFT.Model;

using SFT.Services;

using System.Diagnostics;

namespace SFT.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly AppDbContext _context;

        public LoggerService(AppDbContext context)
        {
            _context = context;
        }

        public void LogEvent(string message, string category, int triggeredBy)
        {
            var log = new Model.EventLog
            {
                Event = message,
                EventTriggeredBy = triggeredBy,
                Category = category,
            };

            _context.EventLogs.Add(log);
            _context.SaveChanges();
        }

        public void LogError(string error, string errormessage, string Controller)
        {
            var log = new ErrorLog
            {
                Error = error,
                Message = errormessage,
                OccuranceSpace = Controller
            };

            _context.ErrorLogs.Add(log);
            _context.SaveChanges();
        }
    }
}

