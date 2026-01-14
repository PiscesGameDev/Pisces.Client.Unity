using System.Collections.Concurrent;
using Pisces.Client.Utils;

namespace Pisces.Client.Network
{
    /// <summary>
    /// GameClient - 请求去重部分
    /// </summary>
    public partial class GameClient
    {
        /// <summary>
        /// 路由锁定表
        /// Key: CmdMerge
        /// </summary>
        private readonly ConcurrentDictionary<int, byte> _lockedRoutes = new();

        /// <summary>
        /// 尝试锁定路由
        /// </summary>
        /// <param name="cmdMerge">路由标识</param>
        /// <returns>是否锁定成功</returns>
        private bool TryLockRoute(int cmdMerge)
        {
            if (!_options.EnableRequestDedup)
                return true;

            if (_options.DedupExcludeList.Contains(cmdMerge))
                return true;

            return _lockedRoutes.TryAdd(cmdMerge, 0);
        }

        /// <summary>
        /// 解锁路由
        /// </summary>
        /// <param name="cmdMerge">路由标识</param>
        private void UnlockRoute(int cmdMerge)
        {
            _lockedRoutes.TryRemove(cmdMerge, out _);
        }

        /// <summary>
        /// 清理所有路由锁定
        /// </summary>
        private void ClearLockedRoutes()
        {
            _lockedRoutes.Clear();
            GameLogger.LogDebug("[GameClient] 已清理所有路由锁定");
        }
    }
}
