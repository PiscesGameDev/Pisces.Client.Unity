using System;
using System.Collections.Generic;
using Pisces.Client.Sdk;
using Pisces.Protocol;
using UnityEngine;

namespace Pisces.Client.Network.Core
{
    /// <summary>
    /// 网络消息日志条目
    /// </summary>
    internal class NetworkMessageLog
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 是否为发送消息 (true=发送, false=接收)
        /// </summary>
        public bool IsOutgoing { get; set; }

        /// <summary>
        /// 消息类型 (Heartbeat, Business, TimeSync, Disconnect)
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// 命令合并ID (CmdMerge)
        /// </summary>
        public int CmdMerge { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        public int MsgId { get; set; }

        /// <summary>
        /// 数据大小 (字节)
        /// </summary>
        public int DataSize { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// 响应状态码
        /// </summary>
        public int ResponseStatus { get; set; }

        /// <summary>
        /// 服务器返回的错误描述
        /// </summary>
        public string ValidMsg { get; set; }

        /// <summary>
        /// 错误信息 (本地生成)
        /// </summary>
        public string ErrorInfo { get; set; }

        /// <summary>
        /// 请求到响应的耗时（毫秒），仅对响应消息有效
        /// </summary>
        public float? ElapsedMs { get; set; }

        /// <summary>
        /// 是否为广播消息 (接收消息且无MsgId)
        /// </summary>
        public bool IsBroadcast => !IsOutgoing && MsgId == 0 && MessageType == MessageType.Business;

        /// <summary>
        /// 获取格式化的 Cmd 显示
        /// </summary>
        public string CmdDisplay => CmdKit.ToString(CmdMerge);
    }

    /// <summary>
    /// 网络统计数据
    /// 用于收集和展示网络运行时状态
    /// </summary>
    internal class NetworkStatistics
    {
        private const int MaxLogCount = 200;
        private const float RateUpdateInterval = 1f;
        private const int MaxPendingRequests = 1000;

        // 基础统计
        private long _totalSendCount;
        private long _totalRecvCount;
        private long _totalSendBytes;
        private long _totalRecvBytes;

        // 速率计算
        private long _lastSendBytes;
        private long _lastRecvBytes;
        private long _lastSendCount;
        private long _lastRecvCount;
        private float _lastRateUpdateTime;

        private float _sendBytesPerSec;
        private float _recvBytesPerSec;
        private float _sendCountPerSec;
        private float _recvCountPerSec;

        // 心跳统计
        private DateTime _lastHeartbeatSendTime;
        private DateTime _lastHeartbeatRecvTime;
        private int _currentHeartbeatTimeoutCount;

        // 重连统计
        private int _reconnectCount;
        private DateTime? _connectedTime;

        // 限流统计
        private long _rateLimitedCount;

        // 发送失败统计
        private long _sendFailedCount;

        // 消息日志
        private readonly LinkedList<NetworkMessageLog> _messageLogs = new();
        private readonly object _logLock = new();

        // 请求时间追踪（用于计算响应耗时）
        private readonly Dictionary<int, DateTime> _pendingRequestTimes = new();


        // 是否启用日志记录
        private bool _enableLogging = true;

        #region Properties

        /// <summary>
        /// 总发送消息数
        /// </summary>
        public long TotalSendCount => _totalSendCount;

        /// <summary>
        /// 总接收消息数
        /// </summary>
        public long TotalRecvCount => _totalRecvCount;

        /// <summary>
        /// 总发送字节数
        /// </summary>
        public long TotalSendBytes => _totalSendBytes;

        /// <summary>
        /// 总接收字节数
        /// </summary>
        public long TotalRecvBytes => _totalRecvBytes;

        /// <summary>
        /// 每秒发送字节数
        /// </summary>
        public float SendBytesPerSec => _sendBytesPerSec;

        /// <summary>
        /// 每秒接收字节数
        /// </summary>
        public float RecvBytesPerSec => _recvBytesPerSec;

        /// <summary>
        /// 每秒发送消息数
        /// </summary>
        public float SendCountPerSec => _sendCountPerSec;

        /// <summary>
        /// 每秒接收消息数
        /// </summary>
        public float RecvCountPerSec => _recvCountPerSec;

        /// <summary>
        /// 上次发送心跳时间
        /// </summary>
        public DateTime LastHeartbeatSendTime => _lastHeartbeatSendTime;

        /// <summary>
        /// 上次接收心跳响应时间
        /// </summary>
        public DateTime LastHeartbeatRecvTime => _lastHeartbeatRecvTime;

        /// <summary>
        /// 当前心跳超时次数
        /// </summary>
        public int CurrentHeartbeatTimeoutCount => _currentHeartbeatTimeoutCount;

        /// <summary>
        /// 重连次数
        /// </summary>
        public int ReconnectCount => _reconnectCount;

        /// <summary>
        /// 被限流的消息数
        /// </summary>
        public long RateLimitedCount => _rateLimitedCount;

        /// <summary>
        /// 发送失败的消息数
        /// </summary>
        public long SendFailedCount => _sendFailedCount;

        /// <summary>
        /// 连接建立时间
        /// </summary>
        public DateTime? ConnectedTime => _connectedTime;

        /// <summary>
        /// 连接时长
        /// </summary>
        public TimeSpan ConnectionDuration =>
            _connectedTime.HasValue ? DateTime.Now - _connectedTime.Value : TimeSpan.Zero;

        /// <summary>
        /// 消息日志数量
        /// </summary>
        public int LogCount
        {
            get
            {
                lock (_logLock)
                {
                    return _messageLogs.Count;
                }
            }
        }

        /// <summary>
        /// 是否启用消息日志记录
        /// </summary>
        public bool EnableLogging
        {
            get => _enableLogging;
            set => _enableLogging = value;
        }

        #endregion

        #region Recording Methods

        /// <summary>
        /// 记录发送消息
        /// </summary>
        public void RecordSend(int bytes,RequestCommand command)
        {
            _totalSendCount++;
            _totalSendBytes += bytes;

            // 记录请求发送时间（用于计算响应耗时）
            if (command.MsgId != 0)
            {
                lock (_logLock)
                {
                    // 防止内存泄漏，限制追踪数量
                    if (_pendingRequestTimes.Count >= MaxPendingRequests)
                    {
                        _pendingRequestTimes.Clear();
                    }
                    _pendingRequestTimes[command.MsgId] = DateTime.Now;
                }
            }

            if (_enableLogging)
            {
                AddLog(new NetworkMessageLog
                {
                    Timestamp = DateTime.Now,
                    IsOutgoing = true,
                    MessageType = command.MessageType,
                    CmdMerge = command.CmdMerge,
                    MsgId = command.MsgId,
                    DataSize = bytes,
                    IsSuccess = true
                });
            }
        }


        public void RecordReceive(ExternalMessage message)
        {
            var dataSize = message.Data?.Length ?? 0;
            var isSuccess = message.ResponseStatus == 0;
            var errorInfo = isSuccess ? null : $"Status: {message.ResponseStatus}";

            // 计算响应耗时
            float? elapsedMs = null;
            if (message.MsgId != 0)
            {
                lock (_logLock)
                {
                    if (_pendingRequestTimes.TryGetValue(message.MsgId, out var sendTime))
                    {
                        elapsedMs = (float)(DateTime.Now - sendTime).TotalMilliseconds;
                        _pendingRequestTimes.Remove(message.MsgId);
                    }
                }
            }

            _totalRecvCount++;
            _totalRecvBytes += dataSize;
            if (_enableLogging && message.CmdMerge != 0)
            {
                AddLog(new NetworkMessageLog
                {
                    Timestamp = DateTime.Now,
                    IsOutgoing = false,
                    CmdMerge = message.CmdMerge,
                    MsgId = message.MsgId,
                    DataSize = dataSize,
                    IsSuccess = isSuccess,
                    ErrorInfo = errorInfo,
                    ElapsedMs = elapsedMs
                });
            }
        }

        /// <summary>
        /// 记录心跳发送
        /// </summary>
        public void RecordHeartbeatSend()
        {
            _lastHeartbeatSendTime = DateTime.Now;
            _currentHeartbeatTimeoutCount++;
        }

        /// <summary>
        /// 记录心跳响应
        /// </summary>
        public void RecordHeartbeatReceive()
        {
            _lastHeartbeatRecvTime = DateTime.Now;
            _currentHeartbeatTimeoutCount = 0;
        }

        /// <summary>
        /// 记录连接建立
        /// </summary>
        public void RecordConnected()
        {
            _connectedTime = DateTime.Now;
        }

        /// <summary>
        /// 记录连接断开
        /// </summary>
        public void RecordDisconnected()
        {
            _connectedTime = null;
        }

        /// <summary>
        /// 记录重连
        /// </summary>
        public void RecordReconnect()
        {
            _reconnectCount++;
        }

        /// <summary>
        /// 记录被限流的消息
        /// </summary>
        public void RecordRateLimited()
        {
            _rateLimitedCount++;
        }

        /// <summary>
        /// 记录发送失败的消息
        /// </summary>
        public void RecordSendFailed()
        {
            _sendFailedCount++;
        }

        /// <summary>
        /// 重置重连计数
        /// </summary>
        public void ResetReconnectCount()
        {
            _reconnectCount = 0;
        }

        #endregion

        #region Rate Calculation

        /// <summary>
        /// 更新速率统计 (应在 Update 中调用)
        /// </summary>
        public void UpdateRates()
        {
            float currentTime = Time.realtimeSinceStartup;
            float deltaTime = currentTime - _lastRateUpdateTime;

            if (deltaTime >= RateUpdateInterval)
            {
                // 计算速率
                long sendBytesDelta = _totalSendBytes - _lastSendBytes;
                long recvBytesDelta = _totalRecvBytes - _lastRecvBytes;
                long sendCountDelta = _totalSendCount - _lastSendCount;
                long recvCountDelta = _totalRecvCount - _lastRecvCount;

                _sendBytesPerSec = sendBytesDelta / deltaTime;
                _recvBytesPerSec = recvBytesDelta / deltaTime;
                _sendCountPerSec = sendCountDelta / deltaTime;
                _recvCountPerSec = recvCountDelta / deltaTime;

                // 更新上次值
                _lastSendBytes = _totalSendBytes;
                _lastRecvBytes = _totalRecvBytes;
                _lastSendCount = _totalSendCount;
                _lastRecvCount = _totalRecvCount;
                _lastRateUpdateTime = currentTime;
            }
        }

        #endregion

        #region Log Management

        private void AddLog(NetworkMessageLog log)
        {
            lock (_logLock)
            {
                _messageLogs.AddFirst(log);

                // 限制日志数量
                while (_messageLogs.Count > MaxLogCount)
                {
                    _messageLogs.RemoveLast();
                }
            }
        }

        /// <summary>
        /// 获取消息日志副本
        /// </summary>
        public List<NetworkMessageLog> GetLogs(int maxCount = 100)
        {
            lock (_logLock)
            {
                var result = new List<NetworkMessageLog>();
                int count = 0;
                foreach (var log in _messageLogs)
                {
                    if (count >= maxCount) break;
                    result.Add(log);
                    count++;
                }
                return result;
            }
        }

        /// <summary>
        /// 清除消息日志
        /// </summary>
        public void ClearLogs()
        {
            lock (_logLock)
            {
                _messageLogs.Clear();
            }
        }

        #endregion

        #region Reset

        /// <summary>
        /// 重置所有统计数据
        /// </summary>
        public void Reset()
        {
            _totalSendCount = 0;
            _totalRecvCount = 0;
            _totalSendBytes = 0;
            _totalRecvBytes = 0;

            _lastSendBytes = 0;
            _lastRecvBytes = 0;
            _lastSendCount = 0;
            _lastRecvCount = 0;

            _sendBytesPerSec = 0;
            _recvBytesPerSec = 0;
            _sendCountPerSec = 0;
            _recvCountPerSec = 0;

            _currentHeartbeatTimeoutCount = 0;
            _reconnectCount = 0;
            _rateLimitedCount = 0;
            _sendFailedCount = 0;
            _connectedTime = null;

            lock (_logLock)
            {
                _pendingRequestTimes.Clear();
            }

            ClearLogs();
        }

        #endregion

        #region Formatting Helpers

        /// <summary>
        /// 格式化字节数
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{bytes / (1024f * 1024f):F2} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024f:F2} KB";
            return $"{bytes} B";
        }

        /// <summary>
        /// 格式化速率
        /// </summary>
        public static string FormatBytesPerSec(float bytesPerSec)
        {
            if (bytesPerSec >= 1024 * 1024)
                return $"{bytesPerSec / (1024f * 1024f):F2} MB/s";
            if (bytesPerSec >= 1024)
                return $"{bytesPerSec / 1024f:F2} KB/s";
            return $"{bytesPerSec:F0} B/s";
        }

        /// <summary>
        /// 格式化时间间隔
        /// </summary>
        public static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        /// <summary>
        /// 格式化时间距今
        /// </summary>
        public static string FormatTimeAgo(DateTime time)
        {
            var diff = DateTime.Now - time;
            if (diff.TotalSeconds < 1)
                return "刚刚";
            if (diff.TotalSeconds < 60)
                return $"{(int)diff.TotalSeconds}秒前";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}分钟前";
            return $"{(int)diff.TotalHours}小时前";
        }

        #endregion
    }
}
