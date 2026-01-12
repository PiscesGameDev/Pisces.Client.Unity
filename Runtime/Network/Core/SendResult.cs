namespace Pisces.Client.Network.Core
{
    /// <summary>
    /// 消息发送结果
    /// </summary>
    public enum SendResult
    {
        /// <summary>
        /// 发送成功（已加入发送队列）
        /// </summary>
        Success,

        /// <summary>
        /// 未连接到服务器
        /// </summary>
        NotConnected,

        /// <summary>
        /// 被限流丢弃
        /// </summary>
        RateLimited,

        /// <summary>
        /// 客户端已关闭
        /// </summary>
        ClientClosed,

        /// <summary>
        /// 无效的消息
        /// </summary>
        InvalidMessage,

        /// <summary>
        /// 通道发送失败
        /// </summary>
        ChannelError
    }
}
