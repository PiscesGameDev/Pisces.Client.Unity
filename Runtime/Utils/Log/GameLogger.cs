namespace T2FGame.Client.Utils
{
    /// <summary>
    /// 游戏日志工具
    /// </summary>
    public static class GameLogger
    {
        private static ILog _logger = new DefaultLog();

        /// <summary>
        /// 是否启用日志
        /// </summary>
        public static bool Enabled { get; set; } = true;

        public static void SetLog(ILog logger)
        {
            if (logger != null)
            {
                _logger = logger;
            }
        }

        /// <summary>
        /// 输出普通日志
        /// </summary>
        public static void Log(string message)
        {
            if (Enabled)
            {
                _logger.Log(message);
            }
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        public static void LogWarning(string message)
        {
            if (Enabled)
            {
                _logger.LogWarning(message);
            }
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        public static void LogError(string message)
        {
            // 错误日志始终输出
            _logger.LogError(message);
        }
    }
}
