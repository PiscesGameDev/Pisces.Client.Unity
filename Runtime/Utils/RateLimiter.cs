using System;
using System.Diagnostics;
using System.Threading;

namespace Pisces.Client.Utils
{
    /// <summary>
    /// 令牌桶限流器
    /// 用于控制消息发送速率，防止网络拥塞和服务器过载
    ///
    /// 工作原理：
    /// - 桶中有固定数量的令牌（maxTokens）
    /// - 每次发送消息消耗一个令牌
    /// - 令牌以固定速率补充（refillRate/秒）
    /// - 令牌不足时，消息被限流
    /// </summary>
    public class RateLimiter
    {
        private static readonly double TickFrequency = Stopwatch.Frequency;

        private readonly int _maxTokens;
        private readonly int _refillRate;
        private double _tokens;
        private long _lastRefillTicks;
        private readonly object _lock = new();

        // 统计信息
        private long _totalAcquired;
        private long _totalRejected;

        /// <summary>
        /// 创建令牌桶限流器
        /// </summary>
        /// <param name="maxTokens">桶容量（最大突发量），默认 100</param>
        /// <param name="refillRate">每秒补充令牌数（持续速率），默认 50</param>
        public RateLimiter(int maxTokens = 100, int refillRate = 50)
        {
            if (maxTokens <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxTokens), "桶容量必须大于 0");
            if (refillRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(refillRate), "补充速率必须大于 0");

            _maxTokens = maxTokens;
            _refillRate = refillRate;
            _tokens = maxTokens;
            _lastRefillTicks = Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// 桶容量（最大突发量）
        /// </summary>
        public int MaxTokens => _maxTokens;

        /// <summary>
        /// 每秒补充令牌数
        /// </summary>
        public int RefillRate => _refillRate;

        /// <summary>
        /// 当前可用令牌数
        /// </summary>
        public int AvailableTokens
        {
            get
            {
                lock (_lock)
                {
                    Refill();
                    return (int)_tokens;
                }
            }
        }

        /// <summary>
        /// 总成功获取次数
        /// </summary>
        public long TotalAcquired => Interlocked.Read(ref _totalAcquired);

        /// <summary>
        /// 总被拒绝次数
        /// </summary>
        public long TotalRejected => Interlocked.Read(ref _totalRejected);

        /// <summary>
        /// 尝试获取一个令牌（非阻塞）
        /// </summary>
        /// <returns>是否成功获取</returns>
        public bool TryAcquire()
        {
            return TryAcquire(1);
        }

        /// <summary>
        /// 尝试获取指定数量的令牌（非阻塞）
        /// </summary>
        /// <param name="tokens">需要的令牌数</param>
        /// <returns>是否成功获取</returns>
        public bool TryAcquire(int tokens)
        {
            if (tokens <= 0)
                return true;

            lock (_lock)
            {
                Refill();

                if (_tokens >= tokens)
                {
                    _tokens -= tokens;
                    Interlocked.Increment(ref _totalAcquired);
                    return true;
                }

                Interlocked.Increment(ref _totalRejected);
                return false;
            }
        }

        /// <summary>
        /// 获取一个令牌，如果没有则阻塞等待
        /// </summary>
        /// <param name="timeoutMs">超时时间（毫秒），0 表示无限等待</param>
        /// <returns>是否成功获取（超时返回 false）</returns>
        public bool Acquire(int timeoutMs = 0)
        {
            var startTicks = Stopwatch.GetTimestamp();

            while (!TryAcquire())
            {
                if (timeoutMs > 0)
                {
                    var elapsedMs = (Stopwatch.GetTimestamp() - startTicks) * 1000.0 / TickFrequency;
                    if (elapsedMs >= timeoutMs)
                    {
                        return false;
                    }
                }

                // 计算等待时间：获取一个令牌需要的毫秒数
                var waitMs = Math.Max(1, (int)(1000.0 / _refillRate));
                Thread.Sleep(Math.Min(waitMs, 10));
            }

            return true;
        }

        /// <summary>
        /// 重置限流器状态
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _tokens = _maxTokens;
                _lastRefillTicks = Stopwatch.GetTimestamp();
                Interlocked.Exchange(ref _totalAcquired, 0);
                Interlocked.Exchange(ref _totalRejected, 0);
            }
        }

        /// <summary>
        /// 补充令牌
        /// </summary>
        private void Refill()
        {
            var now = Stopwatch.GetTimestamp();
            var elapsed = (now - _lastRefillTicks) / TickFrequency; // 秒

            if (elapsed > 0)
            {
                _tokens = Math.Min(_maxTokens, _tokens + elapsed * _refillRate);
                _lastRefillTicks = now;
            }
        }
    }
}
