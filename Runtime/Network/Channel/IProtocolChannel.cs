using System;

namespace T2FGame.Client.Network.Channel
{
    /// <summary>
    ///  通信协议通道的接口
    /// </summary>
    public interface IProtocolChannel
    {
        /// <summary>
        /// 传输协议类型
        /// </summary>
        ChannelType ChannelType { get; }

        /// <summary>
        /// 是否已连接
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// 初始化
        /// </summary>
        void OnInit();
        
        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="host">服务器地址</param>
        /// <param name="port">服务器端口</param>
        void Connect(string host, int port);

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">要发送的数据（已编码的完整数据包）</param>
        void Send(byte[] data);
        
        /// <summary>
        /// 发送消息成功事件
        /// </summary>
        public event Action<IProtocolChannel> SendMessageEvent;
        /// <summary>
        /// 接收消息成功事件
        /// </summary>
        public event Action<IProtocolChannel, byte[]> ReceiveMessageEvent;
        /// <summary>
        /// 与服务器断开连接事件
        /// </summary>
        public event Action<IProtocolChannel> DisconnectServerEvent;
    }
}