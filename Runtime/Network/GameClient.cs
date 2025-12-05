// using System;
// using System.Threading;
// using Cysharp.Threading.Tasks;
// using T2FGame.Protocol;
//
// namespace T2FGame.Client.Network
// {
//     public class GameClient : IGameClient
//     {
//         private readonly GameClientOptions _options;
//         
//         public ConnectionState State { get; }
//         public bool IsConnected { get; }
//         public GameClientOptions Options { get; }
//         
//         private bool _disposed;
//         /// <summary>
//         /// 当前连接状态
//         /// </summary>
//         public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
//         
//         public GameClient(GameClientOptions options = null)
//         {
//             _options = options?.Clone() ?? new GameClientOptions();
//             _receiveBuffer = new PacketBuffer(_options.ReceiveBufferSize);
//         }
//         
//         
//         public UniTask ConnectAsync()
//         {
//             
//         }
//
//         public UniTask DisconnectAsync()
//         {
//            
//         }
//
//         public void Close()
//         {
//             
//         }
//
//         public UniTask SendAsync(ExternalMessage message, CancellationToken cancellationToken = default)
//         {
//            
//         }
//
//         public event Action<ConnectionState> OnStateChanged;
//         public event Action<ExternalMessage> OnMessageReceived;
//         public event Action<Exception> OnError;
//     }
// }