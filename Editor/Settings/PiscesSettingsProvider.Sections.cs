using Pisces.Client.Network.Channel;
using UnityEditor;
using UnityEngine;

namespace Pisces.Client.Editor.Settings
{
    /// <summary>
    /// PiscesSettingsProvider - 基础设置区域
    /// </summary>
    public partial class PiscesSettingsProvider
    {
        #region Server Environment Section

        private void DrawServerEnvironmentSection()
        {
            _serverFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_serverFoldout, "服务器环境");
            if (_serverFoldout)
            {
                EditorGUI.indentLevel++;

                // Environment selector
                if (_serverEnvironments is { arraySize: > 0 })
                {
                    var envNames = new string[_serverEnvironments.arraySize];
                    for (var i = 0; i < _serverEnvironments.arraySize; i++)
                    {
                        var env = _serverEnvironments.GetArrayElementAtIndex(i);
                        var name = env.FindPropertyRelative("Name").stringValue;
                        var host = env.FindPropertyRelative("Host").stringValue;
                        var port = env.FindPropertyRelative("Port").intValue;
                        envNames[i] = $"{name} ({host}:{port})";
                    }

                    EditorGUILayout.Space(5);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("当前环境", GUILayout.Width(150));
                        _activeEnvironmentIndex.intValue = EditorGUILayout.Popup(
                            _activeEnvironmentIndex.intValue, envNames);
                    }

                    // Show active environment details
                    if (_activeEnvironmentIndex.intValue >= 0 &&
                        _activeEnvironmentIndex.intValue < _serverEnvironments.arraySize)
                    {
                        var activeEnv = _serverEnvironments.GetArrayElementAtIndex(_activeEnvironmentIndex.intValue);
                        var desc = activeEnv.FindPropertyRelative("Description").stringValue;
                        if (!string.IsNullOrEmpty(desc))
                        {
                            EditorGUILayout.HelpBox(desc, MessageType.None);
                        }
                    }
                }

                EditorGUILayout.Space(10);

                // Environment list
                EditorGUILayout.LabelField("环境列表", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                for (int i = 0; i < _serverEnvironments.arraySize; i++)
                {
                    DrawEnvironmentItem(i);
                }

                // Add button
                EditorGUILayout.Space(5);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+ 添加环境", GUILayout.Width(150)))
                    {
                        _serverEnvironments.InsertArrayElementAtIndex(_serverEnvironments.arraySize);
                        var newEnv = _serverEnvironments.GetArrayElementAtIndex(_serverEnvironments.arraySize - 1);
                        newEnv.FindPropertyRelative("Name").stringValue = "新环境";
                        newEnv.FindPropertyRelative("Host").stringValue = "localhost";
                        newEnv.FindPropertyRelative("Port").intValue = 9090;
                        newEnv.FindPropertyRelative("Description").stringValue = "";
                    }
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawEnvironmentItem(int index)
        {
            var env = _serverEnvironments.GetArrayElementAtIndex(index);
            var nameProp = env.FindPropertyRelative("Name");
            var hostProp = env.FindPropertyRelative("Host");
            var portProp = env.FindPropertyRelative("Port");
            var descProp = env.FindPropertyRelative("Description");

            var isActive = index == _activeEnvironmentIndex.intValue;

            // 使用不同背景色区分激活状态
            var originalBgColor = GUI.backgroundColor;
            if (isActive)
            {
                GUI.backgroundColor = new Color(0.5f, 0.8f, 0.5f, 1f);
            }

            using (new EditorGUILayout.VerticalScope(_environmentBoxStyle))
            {
                GUI.backgroundColor = originalBgColor;

                using (new EditorGUILayout.HorizontalScope())
                {
                    var displayName = isActive ? "★ " + nameProp.stringValue : nameProp.stringValue;
                    EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    if (!isActive && GUILayout.Button("设为当前", GUILayout.Width(80)))
                    {
                        _activeEnvironmentIndex.intValue = index;
                    }

                    if (_serverEnvironments.arraySize > 1)
                    {
                        if (GUILayout.Button("×", GUILayout.Width(25)))
                        {
                            if (EditorUtility.DisplayDialog("删除环境",
                                $"确定要删除环境 '{nameProp.stringValue}' 吗？",
                                "删除", "取消"))
                            {
                                _serverEnvironments.DeleteArrayElementAtIndex(index);
                                if (_activeEnvironmentIndex.intValue >= _serverEnvironments.arraySize)
                                {
                                    _activeEnvironmentIndex.intValue = _serverEnvironments.arraySize - 1;
                                }
                                return;
                            }
                        }
                    }
                }

                EditorGUILayout.PropertyField(nameProp, new GUIContent("环境名称"));
                EditorGUILayout.PropertyField(hostProp, new GUIContent("服务器地址"));
                EditorGUILayout.PropertyField(portProp, new GUIContent("端口"));
                EditorGUILayout.PropertyField(descProp, new GUIContent("描述"));
            }
        }

        #endregion

        #region Network Section

        private void DrawNetworkSection()
        {
            EditorGUILayout.Space(5);
            _networkFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_networkFoldout, "网络设置");
            if (_networkFoldout)
            {
                EditorGUI.indentLevel++;

                // 检测协议类型变化
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_channelType, new GUIContent("传输协议",
                    "传输协议类型（TCP、UDP 或 WebSocket）"));
                if (EditorGUI.EndChangeCheck())
                {
                    // 应用修改以获取最新值
                    _serializedSettings.ApplyModifiedProperties();

                    // 同步 WebSocket 宏定义
                    SyncWebSocketDefineSymbol(_settings.ChannelType);

                    // 重新加载序列化对象
                    _serializedSettings.Update();
                }

                // 显示当前宏状态
                var hasWebSocketDefine = HasScriptingDefineSymbol(WebSocketDefineSymbol);
                var currentChannelType = (ChannelType)_channelType.enumValueIndex;

                if (currentChannelType == ChannelType.WebSocket)
                {
                    if (hasWebSocketDefine)
                    {
                        EditorGUILayout.HelpBox($"已启用 {WebSocketDefineSymbol} 宏定义", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"需要 {WebSocketDefineSymbol} 宏定义，点击下方按钮添加", MessageType.Warning);
                        if (GUILayout.Button($"添加 {WebSocketDefineSymbol} 宏定义"))
                        {
                            AddScriptingDefineSymbol(WebSocketDefineSymbol);
                        }
                    }
                }
                else if (hasWebSocketDefine)
                {
                    EditorGUILayout.HelpBox($"当前未使用 WebSocket，但 {WebSocketDefineSymbol} 宏定义仍存在", MessageType.Info);
                    if (GUILayout.Button($"移除 {WebSocketDefineSymbol} 宏定义"))
                    {
                        RemoveScriptingDefineSymbol(WebSocketDefineSymbol);
                    }
                }

                EditorGUILayout.Space(5);

                DrawSliderWithInput(_connectTimeoutMs, 1000, 60000, "连接超时 (毫秒)",
                    "等待连接建立的最大时间");

                DrawSliderWithInput(_requestTimeoutMs, 1000, 120000, "请求超时 (毫秒)",
                    "等待服务器响应的最大时间");

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Heartbeat Section

        private void DrawHeartbeatSection()
        {
            EditorGUILayout.Space(5);
            _heartbeatFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_heartbeatFoldout, "心跳设置");
            if (_heartbeatFoldout)
            {
                EditorGUI.indentLevel++;

                DrawSliderWithInput(_heartbeatIntervalSec, 5, 120, "心跳间隔 (秒)",
                    "发送心跳包的时间间隔");

                DrawSliderWithInput(_heartbeatTimeoutCount, 1, 10, "超时次数",
                    "连续多少次心跳超时后认为连接断开");

                // 显示计算出的超时时间
                var totalTimeout = _heartbeatIntervalSec.intValue * _heartbeatTimeoutCount.intValue;
                EditorGUILayout.HelpBox($"连接超时时间: {totalTimeout} 秒 (心跳间隔 × 超时次数)", MessageType.Info);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Reconnect Section

        private void DrawReconnectSection()
        {
            EditorGUILayout.Space(5);
            _reconnectFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_reconnectFoldout, "重连设置");
            if (_reconnectFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_autoReconnect, new GUIContent("自动重连",
                    "连接断开后是否自动尝试重连"));

                using (new EditorGUI.DisabledGroupScope(!_autoReconnect.boolValue))
                {
                    DrawSliderWithInput(_reconnectIntervalSec, 1, 30, "重连间隔 (秒)",
                        "每次重连尝试之间的等待时间");

                    DrawSliderWithInput(_maxReconnectCount, 0, 100, "最大重连次数",
                        "最大重连尝试次数（0 = 无限重试）");

                    if (_maxReconnectCount.intValue == 0)
                    {
                        EditorGUILayout.HelpBox("将无限重试直到连接成功", MessageType.Info);
                    }
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Buffer Section

        private void DrawBufferSection()
        {
            EditorGUILayout.Space(5);
            _bufferFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_bufferFoldout, "缓冲区设置");
            if (_bufferFoldout)
            {
                EditorGUI.indentLevel++;

                // 接收缓冲区
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("接收缓冲区", "接收数据的缓冲区大小"),
                        GUILayout.Width(180));
                    _receiveBufferSize.intValue = EditorGUILayout.IntField(_receiveBufferSize.intValue);
                }
                EditorGUILayout.LabelField($"  ({FormatBytes(_receiveBufferSize.intValue)})",
                    EditorStyles.miniLabel);

                EditorGUILayout.Space(3);

                // 发送缓冲区
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("发送缓冲区", "发送数据的缓冲区大小"),
                        GUILayout.Width(180));
                    _sendBufferSize.intValue = EditorGUILayout.IntField(_sendBufferSize.intValue);
                }
                EditorGUILayout.LabelField($"  ({FormatBytes(_sendBufferSize.intValue)})",
                    EditorStyles.miniLabel);

                // 快捷设置按钮
                EditorGUILayout.Space(5);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("16 KB", GUILayout.Width(60)))
                    {
                        _receiveBufferSize.intValue = 16384;
                        _sendBufferSize.intValue = 16384;
                    }
                    if (GUILayout.Button("64 KB", GUILayout.Width(60)))
                    {
                        _receiveBufferSize.intValue = 65536;
                        _sendBufferSize.intValue = 65536;
                    }
                    if (GUILayout.Button("256 KB", GUILayout.Width(60)))
                    {
                        _receiveBufferSize.intValue = 262144;
                        _sendBufferSize.intValue = 262144;
                    }
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Debug Section

        private void DrawDebugSection()
        {
            EditorGUILayout.Space(5);
            _debugFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_debugFoldout, "调试设置");
            if (_debugFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_logLevel, new GUIContent("日志级别",
                    "控制网络模块的日志输出级别"));

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Validation Section

        private void DrawValidationSection()
        {
            // 验证配置
            if (_settings.Validate(out var errors))
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("配置验证通过", MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space(10);
                foreach (var error in errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }
        }

        #endregion
    }
}
