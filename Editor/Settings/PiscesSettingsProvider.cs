using System.Collections.Generic;
using Pisces.Client.Network.Channel;
using Pisces.Client.Settings;
using Pisces.Client.Utils;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pisces.Client.Editor.Settings
{
    /// <summary>
    /// Pisces Client SDK Project Settings Provider
    /// 在 Edit -> Project Settings -> Pisces Client 中显示
    /// </summary>
    public partial class PiscesSettingsProvider : SettingsProvider
    {
        private const string SettingsPath = "Project/Pisces Client";
        private const string WebSocketDefineSymbol = "ENABLE_WEBSOCKET";

        private SerializedObject _serializedSettings;
        private PiscesSettings _settings;

        // Serialized Properties
        private SerializedProperty _serverEnvironments;
        private SerializedProperty _activeEnvironmentIndex;
        private SerializedProperty _channelType;
        private SerializedProperty _connectTimeoutMs;
        private SerializedProperty _requestTimeoutMs;
        private SerializedProperty _heartbeatIntervalSec;
        private SerializedProperty _heartbeatTimeoutCount;
        private SerializedProperty _autoReconnect;
        private SerializedProperty _reconnectIntervalSec;
        private SerializedProperty _maxReconnectCount;
        private SerializedProperty _receiveBufferSize;
        private SerializedProperty _sendBufferSize;
        private SerializedProperty _enableRateLimit;
        private SerializedProperty _maxSendRate;
        private SerializedProperty _maxBurstSize;
        private SerializedProperty _enableRequestDedup;
        private SerializedProperty _dedupExcludeList;
        private SerializedProperty _logLevel;

        // Foldout States
        private bool _serverFoldout = true;
        private bool _networkFoldout = true;
        private bool _heartbeatFoldout = true;
        private bool _reconnectFoldout = true;
        private bool _bufferFoldout;
        private bool _rateLimitFoldout = true;
        private bool _requestDedupFoldout = true;
        private bool _debugFoldout = true;

        // Styles
        private GUIStyle _headerStyle;
        private GUIStyle _environmentBoxStyle;
        private GUIStyle _activeEnvironmentBoxStyle;

        public PiscesSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope)
        {
            keywords = new HashSet<string>(new[]
            {
                "Pisces", "Network", "TCP", "UDP", "WebSocket", "Client", "Server",
                "Heartbeat", "Reconnect", "Timeout", "Buffer", "网络", "服务器", "心跳", "重连"
            });
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            _settings = PiscesSettings.Instance;
            if (_settings != null)
            {
                _serializedSettings = new SerializedObject(_settings);
                CacheSerializedProperties();
            }
        }

        private void CacheSerializedProperties()
        {
            _serverEnvironments = _serializedSettings.FindProperty("_serverEnvironments");
            _activeEnvironmentIndex = _serializedSettings.FindProperty("_activeEnvironmentIndex");
            _channelType = _serializedSettings.FindProperty("_channelType");
            _connectTimeoutMs = _serializedSettings.FindProperty("_connectTimeoutMs");
            _requestTimeoutMs = _serializedSettings.FindProperty("_requestTimeoutMs");
            _heartbeatIntervalSec = _serializedSettings.FindProperty("_heartbeatIntervalSec");
            _heartbeatTimeoutCount = _serializedSettings.FindProperty("_heartbeatTimeoutCount");
            _autoReconnect = _serializedSettings.FindProperty("_autoReconnect");
            _reconnectIntervalSec = _serializedSettings.FindProperty("_reconnectIntervalSec");
            _maxReconnectCount = _serializedSettings.FindProperty("_maxReconnectCount");
            _receiveBufferSize = _serializedSettings.FindProperty("_receiveBufferSize");
            _sendBufferSize = _serializedSettings.FindProperty("_sendBufferSize");
            _enableRateLimit = _serializedSettings.FindProperty("_enableRateLimit");
            _maxSendRate = _serializedSettings.FindProperty("_maxSendRate");
            _maxBurstSize = _serializedSettings.FindProperty("_maxBurstSize");
            _enableRequestDedup = _serializedSettings.FindProperty("_enableRequestDedup");
            _dedupExcludeList = _serializedSettings.FindProperty("_dedupExcludeList");
            _logLevel = _serializedSettings.FindProperty("_logLevel");
        }

        private void InitializeStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 13,
                    margin = new RectOffset(0, 0, 10, 5)
                };
            }

            if (_environmentBoxStyle == null)
            {
                _environmentBoxStyle = new GUIStyle("helpbox")
                {
                    padding = new RectOffset(10, 10, 8, 8),
                    margin = new RectOffset(0, 0, 5, 5)
                };
            }

            if (_activeEnvironmentBoxStyle == null)
            {
                _activeEnvironmentBoxStyle = new GUIStyle("helpbox")
                {
                    padding = new RectOffset(10, 10, 8, 8),
                    margin = new RectOffset(0, 0, 5, 5)
                };
            }
        }

        public override void OnGUI(string searchContext)
        {
            if (_serializedSettings == null || _settings == null)
            {
                InitializeSettings();
            }

            InitializeStyles();
            _serializedSettings.Update();

            EditorGUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("恢复默认设置", GUILayout.Width(120)))
                {
                    if (EditorUtility.DisplayDialog("恢复默认设置",
                        "确定要将所有设置恢复为默认值吗？",
                        "确定", "取消"))
                    {
                        _settings.ResetToDefaults();
                        EditorUtility.SetDirty(_settings);
                        _serializedSettings.Update();
                    }
                }
            }

            EditorGUILayout.Space(10);

            // Draw sections
            DrawServerEnvironmentSection();
            DrawNetworkSection();
            DrawHeartbeatSection();
            DrawReconnectSection();
            DrawBufferSection();
            DrawRateLimitSection();
            DrawRequestDedupSection();
            DrawDebugSection();

            // Validation
            DrawValidationSection();

            _serializedSettings.ApplyModifiedProperties();
        }

        #region Utility Methods

        private void DrawSliderWithInput(SerializedProperty property, int min, int max, string label, string tooltip)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent(label, tooltip), GUILayout.Width(180));
                property.intValue = EditorGUILayout.IntSlider(property.intValue, min, max);
            }
        }

        private static string FormatBytes(int bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{bytes / (1024f * 1024f):F1} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024f:F1} KB";
            return $"{bytes} B";
        }

        #endregion

        #region Settings Asset Management

        [SettingsProvider]
        public static SettingsProvider CreatePiscesSettingsProvider()
        {
            return new PiscesSettingsProvider(SettingsPath);
        }

        #endregion
    }
}
