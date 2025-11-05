using ItemStatsSystem;
using UnityEngine;

namespace LootboxItemDisplay
{
    // 品质系统类
    public static class ItemQualitySystem
    {
        // 品质颜色
        private static readonly Color[] QualityColors = new Color[6]
        {
            new Color(0.961f, 0.961f, 0.961f),  // 白色 - 普通
            new Color(0.298f, 0.686f, 0.314f),  // 绿色 - 优秀
            new Color(0.129f, 0.588f, 0.953f),  // 蓝色 - 稀有
            new Color(0.604f, 0.153f, 0.69f),   // 紫色 - 史诗
            new Color(1f, 0.757f, 0.027f),      // 橙色 - 传说
            new Color(0.957f, 0.263f, 0.212f)   // 红色 - 神话
        };

        // 品质名称
        private static readonly string[] QualityNames = new string[6]
        {
            "普通",    // 白色
            "优秀",    // 绿色
            "稀有",    // 蓝色
            "史诗",    // 紫色
            "传说",    // 橙色
            "神话"     // 红色
        };

        // 获取品质等级 (0-5)
        public static int GetQualityLevel(ItemStatsSystem.DisplayQuality displayQuality)
        {
            switch (displayQuality)
            {
                case ItemStatsSystem.DisplayQuality.None:
                case ItemStatsSystem.DisplayQuality.White:
                    return 0;
                case ItemStatsSystem.DisplayQuality.Green:
                    return 1;
                case ItemStatsSystem.DisplayQuality.Blue:
                    return 2;
                case ItemStatsSystem.DisplayQuality.Purple:
                    return 3;
                case ItemStatsSystem.DisplayQuality.Orange:
                    return 4;
                case ItemStatsSystem.DisplayQuality.Red:
                case ItemStatsSystem.DisplayQuality.Q7:
                case ItemStatsSystem.DisplayQuality.Q8:
                    return 5;
                default:
                    return 0;
            }
        }

        // 获取品质颜色
        public static Color GetQualityColor(Item item)
        {
            try
            {
                var displayQuality = item.DisplayQuality;
                int level = GetQualityLevel(displayQuality);
                return QualityColors[level];
            }
            catch
            {
                return QualityColors[0]; // 默认白色
            }
        }

        // 获取品质名称
        public static string GetQualityName(Item item)
        {
            try
            {
                var displayQuality = item.DisplayQuality;
                int level = GetQualityLevel(displayQuality);
                return QualityNames[level];
            }
            catch
            {
                return QualityNames[0];
            }
        }

        // 获取颜色的HTML代码
        public static string GetColorHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        // 获取所有品质颜色（用于UI显示）
        public static Color[] GetAllQualityColors()
        {
            return (Color[])QualityColors.Clone();
        }

        // 获取所有品质名称（用于UI显示）
        public static string[] GetAllQualityNames()
        {
            return (string[])QualityNames.Clone();
        }
    }
}