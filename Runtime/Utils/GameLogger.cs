using UnityEngine;

namespace T2FGame.Client.Utils
{
    /// <summary>
    /// 游戏日志工具
    /// </summary>
    public static class GameLogger
    {
        /// <summary>
        /// 是否启用日志
        /// </summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>
        /// 日志标签
        /// </summary>
        public static string Tag { get; set; } = "T2FGame";

        /// <summary>
        /// 输出普通日志
        /// </summary>
        public static void Log(string message)
        {
            if (Enabled)
            {
                Debug.Log(FormatMessage(message));
            }
        }

        /// <summary>
        /// 输出普通日志（带格式化）
        /// </summary>
        public static void Log(string format, params object[] args)
        {
            if (Enabled)
            {
                Debug.Log(FormatMessage(string.Format(format, args)));
            }
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        public static void LogWarning(string message)
        {
            if (Enabled)
            {
                Debug.LogWarning(FormatMessage(message));
            }
        }

        /// <summary>
        /// 输出警告日志（带格式化）
        /// </summary>
        public static void LogWarning(string format, params object[] args)
        {
            if (Enabled)
            {
                Debug.LogWarning(FormatMessage(string.Format(format, args)));
            }
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        public static void LogError(string message)
        {
            // 错误日志始终输出
            Debug.LogError(FormatMessage(message));
        }

        /// <summary>
        /// 输出错误日志（带格式化）
        /// </summary>
        public static void LogError(string format, params object[] args)
        {
            Debug.LogError(FormatMessage(string.Format(format, args)));
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        public static void LogException(System.Exception ex)
        {
            Debug.LogException(ex);
        }

        private static string FormatMessage(string message)
        {
            return string.IsNullOrEmpty(Tag) ? message : $"[{Tag}] {message}";
        }
    }
}
