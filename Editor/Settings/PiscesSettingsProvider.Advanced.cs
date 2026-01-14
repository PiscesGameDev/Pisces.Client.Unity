using Pisces.Protocol;
using UnityEditor;
using UnityEngine;
using MessageType = UnityEditor.MessageType;

namespace Pisces.Client.Editor.Settings
{
    /// <summary>
    /// PiscesSettingsProvider - 高级设置区域（限流、去重）
    /// </summary>
    public partial class PiscesSettingsProvider
    {
        #region Rate Limit Section

        private void DrawRateLimitSection()
        {
            EditorGUILayout.Space(5);
            _rateLimitFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_rateLimitFoldout, "流量控制");
            if (_rateLimitFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_enableRateLimit, new GUIContent("启用限流",
                    "是否启用发送消息的流量控制"));

                using (new EditorGUI.DisabledGroupScope(!_enableRateLimit.boolValue))
                {
                    DrawSliderWithInput(_maxSendRate, 10, 1000, "每秒最大发送数",
                        "每秒允许发送的最大消息数量");

                    DrawSliderWithInput(_maxBurstSize, 10, 200, "最大突发数量",
                        "令牌桶容量，允许短时间内突发发送的消息数量");

                    // 显示说明
                    EditorGUILayout.HelpBox(
                        $"令牌桶算法：每秒补充 {_maxSendRate.intValue} 个令牌，" +
                        $"桶容量 {_maxBurstSize.intValue}。\n" +
                        "超出限制的消息将返回 RateLimited 错误。",
                        MessageType.Info);
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion

        #region Request Dedup Section

        private void DrawRequestDedupSection()
        {
            EditorGUILayout.Space(5);
            _requestDedupFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_requestDedupFoldout, "请求去重");
            if (_requestDedupFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(_enableRequestDedup, new GUIContent("启用去重",
                    "是否启用请求去重，防止同一路由重复发送"));

                using (new EditorGUI.DisabledGroupScope(!_enableRequestDedup.boolValue))
                {
                    EditorGUILayout.HelpBox(
                        "启用后，同一路由在等待响应期间不能重复发送。\n" +
                        "重复请求将返回 RequestLocked 错误。",
                        MessageType.Info);

                    EditorGUILayout.Space(5);

                    // 排除列表
                    DrawDedupExcludeList();
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawDedupExcludeList()
        {
            EditorGUILayout.LabelField("排除列表", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("以下路由不参与去重检查，可以重复发送", MessageType.None);

            EditorGUILayout.Space(3);

            // 列表项
            for (var i = 0; i < _dedupExcludeList.arraySize; i++)
            {
                if (DrawDedupExcludeItem(i))
                {
                    break; // 如果删除了项，跳出循环避免索引错误
                }
            }

            // 添加按钮
            EditorGUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ 添加排除项", GUILayout.Width(120)))
                {
                    _dedupExcludeList.InsertArrayElementAtIndex(_dedupExcludeList.arraySize);
                    var newItem = _dedupExcludeList.GetArrayElementAtIndex(_dedupExcludeList.arraySize - 1);
                    newItem.intValue = 0; // 默认 MergeCmd = 0
                }
            }
        }

        /// <summary>
        /// 绘制单个排除项
        /// </summary>
        /// <returns>是否删除了该项</returns>
        private bool DrawDedupExcludeItem(int index)
        {
            var item = _dedupExcludeList.GetArrayElementAtIndex(index);
            var mergeCmd = item.intValue;

            // 从 MergeCmd 解析出 Cmd 和 SubCmd
            var cmdInfo = new CmdInfo(mergeCmd);
            var cmd = cmdInfo.Cmd;
            var subCmd = cmdInfo.SubCmd;

            using (new EditorGUILayout.HorizontalScope(_environmentBoxStyle))
            {
                // Cmd
                EditorGUILayout.LabelField("Cmd", GUILayout.Width(35));
                EditorGUI.BeginChangeCheck();
                cmd = EditorGUILayout.IntField(cmd, GUILayout.Width(60));
                var cmdChanged = EditorGUI.EndChangeCheck();

                // SubCmd
                EditorGUILayout.LabelField("SubCmd", GUILayout.Width(50));
                EditorGUI.BeginChangeCheck();
                subCmd = EditorGUILayout.IntField(subCmd, GUILayout.Width(60));
                var subCmdChanged = EditorGUI.EndChangeCheck();

                // 如果 Cmd 或 SubCmd 改变，重新计算 MergeCmd
                if (cmdChanged || subCmdChanged)
                {
                    item.intValue = CmdKit.Merge(cmd, subCmd);
                }

                // MergeCmd (只读)
                EditorGUILayout.LabelField("MergeCmd", GUILayout.Width(65));
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.IntField(item.intValue, GUILayout.Width(80));
                }

                // 删除按钮
                if (GUILayout.Button("×", GUILayout.Width(25)))
                {
                    _dedupExcludeList.DeleteArrayElementAtIndex(index);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
