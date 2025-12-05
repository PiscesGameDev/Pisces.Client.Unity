using System;
using UnityWebSocket;
using T2FGame.Client.Utils;

namespace T2FGame.Client.Network.Channel
{
    /// <summary>
    /// WebSocket 通道实现
    /// 使用第三方 UnityWebSocket 插件实现
    /// 适用于 WebGL 平台或需要穿透防火墙的场景
    /// </summary>
    public class WebSocketChannel : IProtocolChannel
    {
        /// <summary>
        /// WebSocket 客户端
        /// </summary>
        private WebSocket _socket;

        /// <summary>
        /// 服务器地址
        /// </summary>
        private string _serverUrl;

        /// <summary>
        /// 服务器端口
        /// </summary>
        private int _serverPort;

        /// <summary>
        /// 是否正在连接
        /// </summary>
        private volatile bool _isConnecting;

        /// <summary>
        /// 是否已释放
        /// </summary>
        private volatile bool _isDisposed;

        public ChannelType ChannelType => ChannelType.WebSocket;

        public bool IsConnected { get; private set; }

        public event Action<IProtocolChannel> SendMessageEvent;
        public event Action<IProtocolChannel, byte[]> ReceiveMessageEvent;
        public event Action<IProtocolChannel> DisconnectServerEvent;

        public void OnInit()
        {
            _isDisposed = false;
            GameLogger.Log("[WebSocketChannel] Initialized");
        }

        public void Connect(string host, int port)
        {
            if (_isDisposed)
            {
                GameLogger.LogError("[WebSocketChannel] Channel is disposed");
                return;
            }

            if (_socket != null)
            {
                if (IsConnected || _isConnecting)
                {
                    GameLogger.LogWarning("[WebSocketChannel] Already connected or connecting");
                    return;
                }

                // 清理旧连接
                CleanupSocket();
            }

            _serverUrl = host;
            _serverPort = port;
            _isConnecting = true;

            try
            {
                // 构建 WebSocket URL
                // 如果 host 已经是完整的 ws:// 或 wss:// URL，直接使用
                // 否则构建 URL
                string wsUrl;
                if (host.StartsWith("ws://", StringComparison.OrdinalIgnoreCase) ||
                    host.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
                {
                    wsUrl = host;
                }
                else
                {
                    wsUrl = $"ws://{host}:{port}";
                }

                _socket = new WebSocket(wsUrl);
                _socket.OnOpen += OnOpen;
                _socket.OnClose += OnClose;
                _socket.OnMessage += OnMessage;
                _socket.OnError += OnError;
                _socket.ConnectAsync();

                GameLogger.Log($"[WebSocketChannel] Connecting to {wsUrl}");
            }
            catch (Exception ex)
            {
                _isConnecting = false;
                GameLogger.LogError($"[WebSocketChannel] Connect failed: {ex.Message}");
                throw;
            }
        }

        public void Disconnect()
        {
            if (_socket == null) return;

            try
            {
                IsConnected = false;
                _isConnecting = false;

                if (_socket.ReadyState == WebSocketState.Open ||
                    _socket.ReadyState == WebSocketState.Connecting)
                {
                    _socket.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogWarning($"[WebSocketChannel] Disconnect error: {ex.Message}");
            }
            finally
            {
                CleanupSocket();
                DisconnectServerEvent?.Invoke(this);
                GameLogger.Log("[WebSocketChannel] Disconnected");
            }
        }

        public void Send(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            if (!IsConnected || _socket == null)
            {
                GameLogger.LogWarning("[WebSocketChannel] Cannot send: not connected");
                return;
            }

            try
            {
                _socket.SendAsync(data);
                SendMessageEvent?.Invoke(this);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[WebSocketChannel] Send failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        public void SendText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (!IsConnected || _socket == null)
            {
                GameLogger.LogWarning("[WebSocketChannel] Cannot send: not connected");
                return;
            }

            try
            {
                _socket.SendAsync(text);
                SendMessageEvent?.Invoke(this);
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"[WebSocketChannel] SendText failed: {ex.Message}");
            }
        }

        private void OnOpen(object sender, OpenEventArgs e)
        {
            _isConnecting = false;
            IsConnected = true;
            GameLogger.Log("[WebSocketChannel] Connected");
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            var wasConnected = IsConnected;
            IsConnected = false;
            _isConnecting = false;

            GameLogger.Log($"[WebSocketChannel] Closed: Code={e.Code}, Reason={e.Reason}");

            if (wasConnected)
            {
                DisconnectServerEvent?.Invoke(this);
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            _isConnecting = false;
            GameLogger.LogError($"[WebSocketChannel] Error: {e.Message}");

            // 如果连接时出错，触发断开事件
            if (IsConnected)
            {
                IsConnected = false;
                DisconnectServerEvent?.Invoke(this);
            }
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsBinary)
            {
                // 处理二进制消息
                ReceiveMessageEvent?.Invoke(this, e.RawData);
            }
            else
            {
                // 当前框架保持仅接受二进制数据
                // 如果需要处理文本消息，可以转换为字节数组
                GameLogger.LogWarning($"[WebSocketChannel] Received text message (ignored): {e.Data?.Length ?? 0} chars");
            }
        }

        /// <summary>
        /// 清理 WebSocket 资源
        /// </summary>
        private void CleanupSocket()
        {
            if (_socket == null) return;

            _socket.OnOpen -= OnOpen;
            _socket.OnClose -= OnClose;
            _socket.OnMessage -= OnMessage;
            _socket.OnError -= OnError;
            _socket = null;
        }

        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        public WebSocketState? State => _socket?.ReadyState;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            Disconnect();
            GameLogger.Log("[WebSocketChannel] Disposed");
        }
    }
}
