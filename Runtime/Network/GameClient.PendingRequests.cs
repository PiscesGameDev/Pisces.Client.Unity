using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Pisces.Client.Sdk;
using Pisces.Client.Utils;
using UnityEngine;

namespace Pisces.Client.Network
{
    /// <summary>
    /// 待处理请求信息
    /// </summary>
    internal sealed class PendingRequestInfo
    {
        public UniTaskCompletionSource<ResponseMessage> Tcs { get; set; }
        public long CreatedTicks { get; set; }
        public int CmdMerge { get; set; }

        public void Reset()
        {
            Tcs = null;
            CreatedTicks = 0;
            CmdMerge = 0;
        }
    }

    /// <summary>
    /// GameClient - 待处理请求管理部分
    /// </summary>
    public partial class GameClient
    {
        /// <summary>
        /// 清理循环间隔（毫秒）
        /// </summary>
        private const int CleanupIntervalMs = 5000;

        /// <summary>
        /// 请求超时阈值倍数（相对于 RequestTimeoutMs）
        /// 超过此倍数的请求将被强制清理
        /// </summary>
        private const float CleanupTimeoutMultiplier = 2.0f;

        private CancellationTokenSource _cleanupCts;

        /// <summary>
        /// 启动待处理请求清理任务
        /// </summary>
        private void StartPendingRequestsCleanup()
        {
            StopPendingRequestsCleanup();

            _cleanupCts = new CancellationTokenSource();
            PendingRequestsCleanupLoop(_cleanupCts.Token).Forget();

            GameLogger.Log("[GameClient] 待处理请求清理任务已启动");
        }

        /// <summary>
        /// 停止待处理请求清理任务
        /// </summary>
        private void StopPendingRequestsCleanup()
        {
            _cleanupCts?.Cancel();
            _cleanupCts?.Dispose();
            _cleanupCts = null;
        }

        /// <summary>
        /// 待处理请求清理循环
        /// </summary>
        private async UniTaskVoid PendingRequestsCleanupLoop(CancellationToken cancellationToken)
        {
            var timeoutTicks = (long)(_options.RequestTimeoutMs * CleanupTimeoutMultiplier * Stopwatch.Frequency / 1000);

            while (!cancellationToken.IsCancellationRequested && Application.isPlaying)
            {
                try
                {
                    await UniTask.Delay(CleanupIntervalMs, cancellationToken: cancellationToken);

                    if (!Application.isPlaying)
                        break;

                    CleanupStalePendingRequests(timeoutTicks);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    GameLogger.LogError($"[GameClient] 清理待处理请求时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 清理过期的待处理请求
        /// </summary>
        private void CleanupStalePendingRequests(long timeoutTicks)
        {
            var now = Stopwatch.GetTimestamp();
            var cleanedCount = 0;

            foreach (var kvp in _pendingRequests)
            {
                // 由于我们现在存储的是简单的 TCS，无法获取创建时间
                // 这里我们依赖外部的超时机制，只清理那些 TCS 已完成但未移除的项
                if (kvp.Value.Task.Status != UniTaskStatus.Pending)
                {
                    if (_pendingRequests.TryRemove(kvp.Key, out _))
                    {
                        cleanedCount++;
                    }
                }
            }

            if (cleanedCount > 0)
            {
                GameLogger.Log($"[GameClient] 清理了 {cleanedCount} 个已完成的待处理请求");
            }

            // 如果待处理请求数量过多，记录警告
            var pendingCount = _pendingRequests.Count;
            if (pendingCount > 100)
            {
                GameLogger.LogWarning($"[GameClient] 待处理请求数量过多: {pendingCount}");
            }
        }

        /// <summary>
        /// 获取当前待处理请求数量
        /// </summary>
        public int PendingRequestCount => _pendingRequests.Count;
    }
}
