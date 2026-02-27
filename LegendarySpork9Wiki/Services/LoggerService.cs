using log4net;

namespace LegendarySpork9Wiki.Services
{
    public class LoggerService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LoggerService));

        public void LogInfo(string message)
        {
            _logger.Info(message);
        }

        public void LogWarning(string message)
        {
            _logger.Warn(message);
        }

        public void LogError(string message)
        {
            _logger.Error(message);
        }

        public void LogError(string message, Exception ex)
        {
            _logger.Error(message, ex);
        }

        public void LogDebug(string message)
        {
            _logger.Debug(message);
        }
    }
}
