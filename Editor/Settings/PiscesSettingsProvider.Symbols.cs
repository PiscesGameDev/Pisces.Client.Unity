using System.Linq;
using Pisces.Client.Network.Channel;
using UnityEditor;
using UnityEditor.Build;

namespace Pisces.Client.Editor.Settings
{
    /// <summary>
    /// PiscesSettingsProvider - 脚本宏定义管理
    /// </summary>
    public partial class PiscesSettingsProvider
    {
        #region Scripting Define Symbols

        /// <summary>
        /// 同步 WebSocket 宏定义
        /// </summary>
        private void SyncWebSocketDefineSymbol(ChannelType channelType)
        {
            if (channelType == ChannelType.WebSocket)
            {
                AddScriptingDefineSymbol(WebSocketDefineSymbol);
            }
            else
            {
                RemoveScriptingDefineSymbol(WebSocketDefineSymbol);
            }
        }

        /// <summary>
        /// 检查是否存在指定的脚本宏定义
        /// </summary>
        private static bool HasScriptingDefineSymbol(string symbol)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);

            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);
            return defines.Contains(symbol);
        }

        /// <summary>
        /// 添加脚本宏定义
        /// </summary>
        private static void AddScriptingDefineSymbol(string symbol)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);

            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);

            if (!defines.Contains(symbol))
            {
                var newDefines = defines.Append(symbol).ToArray();
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefines);
            }
        }

        /// <summary>
        /// 移除脚本宏定义
        /// </summary>
        private static void RemoveScriptingDefineSymbol(string symbol)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);

            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);

            if (defines.Contains(symbol))
            {
                var newDefines = defines.Where(d => d != symbol).ToArray();
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefines);
            }
        }

        #endregion
    }
}
