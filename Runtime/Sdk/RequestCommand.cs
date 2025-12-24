using System.Collections.Generic;
using Google.Protobuf;
using Pisces.Client.Utils;
using Pisces.Protocol;

namespace Pisces.Client.Sdk
{
    /// <summary>
    /// 表示一个网络请求命令，用于封装客户端向服务器发送的消息。
    /// </summary>
    public sealed class RequestCommand : IPoolable
    {
        private static readonly ByteString _emptyByteString = ByteString.Empty;

        /// <summary>
        /// 获取消息标记号。该字段由前端在发起请求时设置，
        /// 服务端在响应时会原样带回，用于匹配请求与响应。
        /// </summary>
        public int MsgId { get; private set; }

        /// <summary>
        /// 获取业务路由标识。采用合并编码方式：高16位表示主命令，低16位表示子命令。
        /// </summary>
        public int CmdMerge { get; private set; }

        /// <summary>
        /// 获取请求数据内容
        /// </summary>
        public ByteString Data { get; private set; }

        /// <summary>
        /// 获取请求命令类型，默认为业务类型（Business）。
        /// 可选值包括心跳（Heartbeat）和业务（Business）两种类型。
        /// </summary>
        public CommandType CommandType { get; private set; } = CommandType.Business;

        private void Initialize(int cmdMerge, ByteString data, CommandType commandType = CommandType.Business)
        {
            CmdMerge = cmdMerge;
            Data = data ?? _emptyByteString;
            CommandType = commandType;
            MsgId = commandType == CommandType.Heartbeat ? 0 : MsgIdManager.GenerateNextMsgId();
        }

        /// <summary>
        /// 重置当前请求命令的所有属性到初始状态。
        /// </summary>
        public void Reset()
        {
            MsgId = 0;
            CmdMerge = 0;
            Data = _emptyByteString;
            CommandType = CommandType.Business;
        }

        /// <summary>
        /// IPoolable: 从池中取出时调用
        /// </summary>
        public void OnSpawn() { }

        /// <summary>
        /// IPoolable: 归还到池中时调用
        /// </summary>
        public void OnDespawn() => Reset();

        #region 工厂方法

        /// <summary>
        /// 创建一个新的请求命令实例，并指定其业务路由标识、数据内容以及命令类型。
        /// </summary>
        public static RequestCommand Of(
            int cmdMerge,
            ByteString data,
            CommandType commandType = CommandType.Business
        )
        {
            var requestCommand = ReferencePool<RequestCommand>.Spawn();
            requestCommand.Initialize(cmdMerge, data, commandType);
            return requestCommand;
        }

        /// <summary>
        /// 创建一个新的请求命令实例，并指定其业务路由标识。
        /// 数据部分为空，命令类型默认为业务类型。
        /// </summary>
        public static RequestCommand Of(int cmdMerge) => Of(cmdMerge, _emptyByteString);

        /// <summary>
        /// 创建一个新的请求命令实例，并指定其业务路由标识及 Protobuf 消息数据。
        /// 若传入的消息为空，则数据部分将被设为空字符串。
        /// </summary>
        public static RequestCommand Of<T>(int cmdMerge, T message) where T : IMessage
        {
            var byteString = message?.ToByteString() ?? _emptyByteString;
            return Of(cmdMerge, byteString);
        }

        /// <summary>
        /// 创建一个专门用于心跳检测的请求命令实例。
        /// 路由标识为 0，数据为空，命令类型为心跳。
        /// </summary>
        public static RequestCommand Heartbeat() => Of(0, _emptyByteString, CommandType.Heartbeat);

        #region 基础类型重载（利用隐式转换）

        /// <summary>
        /// 创建一个包含整型数值的请求命令实例。
        /// 利用隐式转换 int → IntValue，简化调用。
        /// </summary>
        /// <example>RequestCommand.Of(cmdMerge, 100);</example>
        public static RequestCommand Of(int cmdMerge, int data) => Of(cmdMerge, (IntValue)data);

        /// <summary>
        /// 创建一个包含字符串值的请求命令实例。
        /// 利用隐式转换 string → StringValue，简化调用。
        /// </summary>
        /// <example>RequestCommand.Of(cmdMerge, "hello");</example>
        public static RequestCommand Of(int cmdMerge, string data) => Of(cmdMerge, (StringValue)data);

        /// <summary>
        /// 创建一个包含长整型数值的请求命令实例。
        /// 利用隐式转换 long → LongValue，简化调用。
        /// </summary>
        /// <example>RequestCommand.Of(cmdMerge, 999L);</example>
        public static RequestCommand Of(int cmdMerge, long data) => Of(cmdMerge, (LongValue)data);

        /// <summary>
        /// 创建一个包含布尔值的请求命令实例。
        /// 利用隐式转换 bool → BoolValue，简化调用。
        /// </summary>
        /// <example>RequestCommand.Of(cmdMerge, true);</example>
        public static RequestCommand Of(int cmdMerge, bool data) => Of(cmdMerge, (BoolValue)data);

        /// <summary>
        /// 创建一个包含 Vector2 的请求命令实例。
        /// 利用隐式转换 UnityEngine.Vector2 → Vector2，简化调用。
        /// </summary>
        /// <example>RequestCommand.Of(cmdMerge, new Vector2(1, 2));</example>
        public static RequestCommand Of(int cmdMerge, UnityEngine.Vector2 data) => Of(cmdMerge, (Vector2)data);

        /// <summary>
        /// 创建一个包含 Vector2Int 的请求命令实例。
        /// 利用隐式转换 UnityEngine.Vector2Int → Vector2Int，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, UnityEngine.Vector2Int data) => Of(cmdMerge, (Vector2Int)data);

        /// <summary>
        /// 创建一个包含 Vector3 的请求命令实例。
        /// 利用隐式转换 UnityEngine.Vector3 → Vector3，简化调用。
        /// </summary>
        /// <example>RequestCommand.Of(cmdMerge, transform.position);</example>
        public static RequestCommand Of(int cmdMerge, UnityEngine.Vector3 data) => Of(cmdMerge, (Vector3)data);

        /// <summary>
        /// 创建一个包含 Vector3Int 的请求命令实例。
        /// 利用隐式转换 UnityEngine.Vector3Int → Vector3Int，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, UnityEngine.Vector3Int data) => Of(cmdMerge, (Vector3Int)data);

        #endregion

        #region 列表类型重载（利用隐式转换）

        /// <summary>
        /// 创建一个包含整型列表的请求命令实例。
        /// 利用隐式转换 int[] → IntValueList，简化调用。
        /// </summary>
        /// <example>RequestCommand.Of(cmdMerge, new int[] { 1, 2, 3 });</example>
        public static RequestCommand Of(int cmdMerge, int[] data) => Of(cmdMerge, (IntValueList)data);

        /// <summary>
        /// 创建一个包含整型列表的请求命令实例。
        /// 利用隐式转换 List&lt;int&gt; → IntValueList，简化调用。
        /// </summary>
        /// <example>RequestCommand.Of(cmdMerge, myIntList);</example>
        public static RequestCommand Of(int cmdMerge, List<int> data) => Of(cmdMerge, (IntValueList)data);

        /// <summary>
        /// 创建一个包含长整型列表的请求命令实例。
        /// 利用隐式转换 long[] → LongValueList，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, long[] data) => Of(cmdMerge, (LongValueList)data);

        /// <summary>
        /// 创建一个包含长整型列表的请求命令实例。
        /// 利用隐式转换 List&lt;long&gt; → LongValueList，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, List<long> data) => Of(cmdMerge, (LongValueList)data);

        /// <summary>
        /// 创建一个包含字符串列表的请求命令实例。
        /// 利用隐式转换 string[] → StringValueList，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, string[] data) => Of(cmdMerge, (StringValueList)data);

        /// <summary>
        /// 创建一个包含字符串列表的请求命令实例。
        /// 利用隐式转换 List&lt;string&gt; → StringValueList，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, List<string> data) => Of(cmdMerge, (StringValueList)data);

        /// <summary>
        /// 创建一个包含布尔值列表的请求命令实例。
        /// 利用隐式转换 bool[] → BoolValueList，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, bool[] data) => Of(cmdMerge, (BoolValueList)data);

        /// <summary>
        /// 创建一个包含布尔值列表的请求命令实例。
        /// 利用隐式转换 List&lt;bool&gt; → BoolValueList，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, List<bool> data) => Of(cmdMerge, (BoolValueList)data);

        /// <summary>
        /// 创建一个包含 Vector2 列表的请求命令实例。
        /// 利用隐式转换 Vector2[] → Vector2List，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, UnityEngine.Vector2[] data) => Of(cmdMerge, (Vector2List)data);

        /// <summary>
        /// 创建一个包含 Vector2 列表的请求命令实例。
        /// 利用隐式转换 List&lt;Vector2&gt; → Vector2List，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, List<UnityEngine.Vector2> data) => Of(cmdMerge, (Vector2List)data);

        /// <summary>
        /// 创建一个包含 Vector2Int 列表的请求命令实例。
        /// 利用隐式转换 Vector2Int[] → Vector2IntList，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, UnityEngine.Vector2Int[] data) => Of(cmdMerge, (Vector2IntList)data);

        /// <summary>
        /// 创建一个包含 Vector2Int 列表的请求命令实例。
        /// 利用隐式转换 List&lt;Vector2Int&gt; → Vector2IntList，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, List<UnityEngine.Vector2Int> data) => Of(cmdMerge, (Vector2IntList)data);

        /// <summary>
        /// 创建一个包含 Vector3 列表的请求命令实例。
        /// 利用隐式转换 Vector3[] → Vector3List，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, UnityEngine.Vector3[] data) => Of(cmdMerge, (Vector3List)data);

        /// <summary>
        /// 创建一个包含 Vector3 列表的请求命令实例。
        /// 利用隐式转换 List&lt;Vector3&gt; → Vector3List，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, List<UnityEngine.Vector3> data) => Of(cmdMerge, (Vector3List)data);

        /// <summary>
        /// 创建一个包含 Vector3Int 列表的请求命令实例。
        /// 利用隐式转换 Vector3Int[] → Vector3IntList，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, UnityEngine.Vector3Int[] data) => Of(cmdMerge, (Vector3IntList)data);

        /// <summary>
        /// 创建一个包含 Vector3Int 列表的请求命令实例。
        /// 利用隐式转换 List&lt;Vector3Int&gt; → Vector3IntList，简化调用。
        /// </summary>
        public static RequestCommand Of(int cmdMerge, List<UnityEngine.Vector3Int> data) => Of(cmdMerge, (Vector3IntList)data);

        #endregion
        #endregion
    }
}
