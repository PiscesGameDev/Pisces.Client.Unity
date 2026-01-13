using System;
using System.Threading;
using UnityEngine;

namespace Pisces.Client.Utils
{
    /// <summary>
    /// 主线程调度器
    /// 提供将回调调度到 Unity 主线程执行的功能
    /// </summary>
    public static class MainThreadDispatcher
    {
        private static SynchronizationContext _context;
        private static int _mainThreadId;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized => _context != null;

        /// <summary>
        /// 检查当前是否在主线程
        /// </summary>
        public static bool IsOnMainThread => IsInitialized && Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        /// <summary>
        /// 在场景加载前自动初始化
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _context = SynchronizationContext.Current;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            
            if (_context == null)
            {
                GameLogger.LogWarning("[MainThreadDispatcher] 无法获取 SynchronizationContext，" + "确保在Unity主线程中初始化");
            }
        }

        /// <summary>
        /// 在主线程上执行回调（如果已经在主线程则直接执行）
        /// </summary>
        /// <param name="action">要执行的操作</param>
        public static void InvokeOnMainThread(Action action)
        {
            if (action == null)
                return;

            // 如果已经在主线程，直接执行
            if (IsOnMainThread)
            {
                action();
                return;
            }

            // 不在主线程，调度执行
            if (_context != null)
            {
                _context.Post(_ => action(), null);
            }
            else
            {
                // 如果没有同步上下文，直接执行（可能在编辑器模式或测试中）
                action();
            }
        }

        /// <summary>
        /// 在主线程上执行回调（带参数）
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="action">要执行的操作</param>
        /// <param name="state">传递的参数</param>
        public static void InvokeOnMainThread<T>(Action<T> action, T state)
        {
            if (action == null)
                return;

            // 如果已经在主线程，直接执行
            if (IsOnMainThread)
            {
                action(state);
                return;
            }

            if (_context != null)
            {
                _context.Post(_ => action(state), null);
            }
            else
            {
                action(state);
            }
        }

        /// <summary>
        /// 同步在主线程执行（会阻塞当前线程直到主线程执行完成）
        /// </summary>
        /// <param name="action">要执行的操作</param>
        public static void InvokeOnMainThreadSync(Action action)
        {
            if (action == null)
                return;

            if (IsOnMainThread)
            {
                action();
                return;
            }

            if (_context != null)
            {
                var resetEvent = new ManualResetEventSlim(false);
                Exception exception = null;

                _context.Post(_ =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                }, null);

                resetEvent.Wait();
                resetEvent.Dispose();

                if (exception != null)
                {
                    GameLogger.LogError($"[MainThreadDispatcher] 执行回调时发生异常: {exception}");
                }
            }
            else
            {
                throw new InvalidOperationException("MainThreadDispatcher 未初始化，无法同步调用");
            }
        }
    }
}