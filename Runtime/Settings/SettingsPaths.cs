namespace Pisces.Client.Settings
{
    internal static class SettingsPaths
    {
        // 统一资源路径（Resources 文件夹下）
        public const string PiscesSettingsResourcePath = "PiscesSettings"; // 用 Resources.Load("PiscesSettings")
        // 若仍需 AssetDatabase 路径（Editor 使用），统一写一个位置：
        public const string PiscesSettingsAssetPath = "Assets/Resources/PiscesSettings.asset";
        // 文档链接
        public const string DocumentationURL = "https://github.com/PiscesGameDev/Pisces.Client.Unity#readme";
    }
}