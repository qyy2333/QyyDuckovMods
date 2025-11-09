using SodaCraft.Localizations;
using System;
using System.Linq;
using UnityEngine;

namespace LootboxItemDisplay
{
    /// <summary>
    /// 箱子检测和UI配置
    /// </summary>
    public class LootboxConfig
    {
        // 检测设置 
        public float detectionRadius = 5f;          // 检测半径(米)
        public float detectionInterval = 0.2f;      // 检测间隔(秒)

        // 显示设置
        public bool showPetInventory = false;       // 是否显示宠物栏(PetProxy)
        public bool showPlayerStorage = true;       // 是否显示仓库栏(PlayerStorage)
        public bool showLootboxItems = true;        // 是否显示箱子物品
        public bool showPickupItems = true;         // 是否显示地面掉落物

        //按键设置
        public string toggleKey = "F3";   // 默认按 F3 打开/关闭UI
        public string highValueToggleKey = "F4";  // 最高价值UI切换键 (新增)

        // UI尺寸
        public float PanelMinWidth = 380f;
        public float PanelMaxHeight = 600f;
        public float highValuePanelOpacity = 0.9f;

        // UI文字大小
        public int titleFontSize = 20;
        public int lootboxTitleFontSize = 18;
        public int itemFontSize = 15;
        public int statsFontSize = 13;

        // 其他
        public float scrollSensitivity = 20f;
        public int layoutSpacing = 8;
        public bool debugMode = false;
        public bool showHighestValueItem = true;  // 是否显示最高价值物品
        public bool showItemIcons = true;         //是否显示图标
        public float highValueThreshold = 100f;   // 价值阈值

        // 静态常量
        public static Vector2 ViewportMinRange = new Vector2(0.2f, 0.2f);
        public static Vector2 ViewportMaxRange = new Vector2(0.8f, 0.8f);
        public static Vector2 PanelInitialPosition = new Vector2(20, 0);
        public static Vector2 ReferenceResolution = new Vector2(1920, 1080);

        // UI颜色常量
        public static Color PanelBackgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.92f);
        public static Color PanelOutlineColor = new Color(0.3f, 0.6f, 1f, 0.8f);
        public static Color PanelShadowColor = new Color(0f, 0f, 0f, 0.5f);
        public static Color TitleBarColor = new Color(0.2f, 0.3f, 0.5f, 0.8f);
        public static Color TitleTextColor = new Color(0.9f, 0.95f, 1f);
        public static Color LootboxTitleColor = new Color(0.4f, 0.8f, 1f);
        public static Color PickupTitleColor = new Color(0.4f, 0.8f, 1f);

        public static Color ItemValueColor = new Color(1f, 0.84f, 0f);

        public static Color ItemTextColor = new Color(0.9f, 0.9f, 0.9f);
        public static Color ItemCountColor = new Color(0.53f, 1f, 0.67f);
        public static Color ItemWeightColor = new Color(0.6f, 0.6f, 0.6f);
        public static Color EmptyTextColor = new Color(0.6f, 0.6f, 0.6f);
        public static Color StatsTextColor = new Color(0.7f, 0.7f, 0.7f);
        public static Color DividerColor = new Color(0.3f, 0.5f, 0.7f, 0.5f);
        public static Color ToggleButtonColor = new Color(0.3f, 0.5f, 0.7f, 0.8f);
        public static Color CloseButtonColor = new Color(0.7f, 0.3f, 0.3f, 0.8f);

        public static int CanvasSortingOrder = 10000;
        public static int EmptyTextFontSize = 14;
        public static int DividerFontSize = 14;
        public static RectOffset LayoutPadding = new RectOffset(10, 10, 10, 10);

        // 便捷属性
        public Vector2 PanelSize => new Vector2(PanelMinWidth, PanelMaxHeight);


    }

    /// <summary>
    /// ModConfig管理器
    /// </summary>
    public static class LootboxConfigManager
    {
        public static string MOD_NAME = "LootboxItemDisplay";

        private static LootboxConfig config = new LootboxConfig();

        public static LootboxConfig Config => config;

        /// <summary>
        /// 设置ModConfig
        /// </summary>
        public static void SetupModConfig()
        {
            if (!ModConfigAPI.IsAvailable())
            {
                Debug.LogWarning("[LootboxConfig] ModConfig 不可用");
                return;
            }

            Debug.Log("[LootboxConfig] 开始注册 ModConfig");

            // 根据当前语言设置描述文字
            SystemLanguage[] chineseLanguages = {
                SystemLanguage.Chinese,
                SystemLanguage.ChineseSimplified,
                SystemLanguage.ChineseTraditional
            };

            bool isChinese = chineseLanguages.Contains(LocalizationManager.CurrentLanguage);

            // ===== 检测设置 =====
            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "detectionRadius",
                isChinese ? "检测半径(米)" : "Detection Radius (m)",
                typeof(float),
                config.detectionRadius,
                new Vector2(1, 20));

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "detectionInterval",
                isChinese ? "检测间隔(秒)" : "Detection Interval (s)",
                typeof(float),
                config.detectionInterval,
                new Vector2(0.05f, 1f));

            // ===== 显示设置 =====
            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "showPetInventory",
                isChinese ? "显示宠物栏" : "Show Pet Inventory",
                config.showPetInventory);

            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "showPlayerStorage",
                isChinese ? "显示仓库栏" : "Show Player Storage",
                config.showPlayerStorage);

            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "showLootboxItems",
                isChinese ? "显示箱子物品" : "show Lootbox Items",
                config.showLootboxItems);

            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "showPickupItems",
                isChinese ? "显示地面掉落物" : "show Pickup Items",
                config.showPickupItems);

            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "showHighestValueItem",
                isChinese ? "显示最高价值物品" : "show Highest ValueItem",
                config.showHighestValueItem);

            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "showItemIcons",
                isChinese ? "显示物品图标" : "show Item Icons",
                config.showItemIcons);
            // ===== 键位设置 =====
            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "toggleKey",
                isChinese ? "UI切换键(权宜之计)" : "UI Toggle Key",
                typeof(string),
                config.toggleKey,
                null);
            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "highValueToggleKey",
                isChinese ? "最高价值UI切换键" : "High Value UI Toggle Key",
                typeof(string),
                config.highValueToggleKey,
                null);

            // ===== UI 尺寸设置 =====
            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "panelWidth",
                isChinese ? "最小面板宽度" : "Panel Width",
                typeof(float),
                config.PanelMinWidth,
                new Vector2(200, 600));

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "panelHeight",
                isChinese ? "最大面板高度" : "Panel Height",
                typeof(float),
                config.PanelMaxHeight,
                new Vector2(300, 1500));

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "highValuePanelOpacity",
                isChinese ? "面板透明度" : "high Value Panel Opacity",
                typeof(float),
                config.highValuePanelOpacity,
                new Vector2(0, 1));

            // ===== UI 文字设置 =====
            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "titleFontSize",
                isChinese ? "标题字体大小" : "Title Font Size",
                typeof(int),
                config.titleFontSize,
                new Vector2(12, 40));

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "lootboxTitleFontSize",
                isChinese ? "箱子标题字体大小" : "Lootbox Title Font Size",
                typeof(int),
                config.lootboxTitleFontSize,
                new Vector2(12, 30));

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "itemFontSize",
                isChinese ? "物品字体大小" : "Item Font Size",
                typeof(int),
                config.itemFontSize,
                new Vector2(10, 30));

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "statsFontSize",
                isChinese ? "统计字体大小" : "Stats Font Size",
                typeof(int),
                config.statsFontSize,
                new Vector2(10, 25));

            // ===== 其他设置 =====
            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "scrollSensitivity",
                isChinese ? "滚动灵敏度" : "Scroll Sensitivity",
                typeof(float),
                config.scrollSensitivity,
                new Vector2(1, 50));

            ModConfigAPI.SafeAddInputWithSlider(
                MOD_NAME,
                "layoutSpacing",
                isChinese ? "内容间距" : "Layout Spacing",
                typeof(int),
                config.layoutSpacing,
                new Vector2(0, 20));

            // ===== 调试 =====
            ModConfigAPI.SafeAddBoolDropdownList(
                MOD_NAME,
                "debugMode",
                isChinese ? "调试模式" : "Debug Mode",
                config.debugMode);

            Debug.Log("[LootboxConfig] ModConfig 注册完成");
        }

        /// <summary>
        /// 从 ModConfig 加载配置
        /// </summary>
        public static void LoadFromModConfig()
        {
            if (!ModConfigAPI.IsAvailable())
            {
                Debug.LogWarning("[LootboxConfig] ModConfig 不可用,使用默认配置");
                return;
            }

            try
            {
                config.detectionRadius = ModConfigAPI.SafeLoad<float>(MOD_NAME, "detectionRadius", config.detectionRadius);
                config.detectionInterval = ModConfigAPI.SafeLoad<float>(MOD_NAME, "detectionInterval", config.detectionInterval);

                config.showPetInventory = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "showPetInventory", config.showPetInventory);
                config.showPlayerStorage = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "showPlayerStorage", config.showPlayerStorage);
                config.showLootboxItems = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "showLootboxItems", config.showLootboxItems);
                config.showPickupItems = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "showPickupItems", config.showPickupItems);
                config.showHighestValueItem = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "showHighestValueItem", config.showHighestValueItem);
                config.showItemIcons = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "showItemIcons", config.showItemIcons);

                config.toggleKey = ModConfigAPI.SafeLoad<string>(MOD_NAME, "toggleKey", config.toggleKey);
                config.highValueToggleKey = ModConfigAPI.SafeLoad<string>(MOD_NAME, "highValueToggleKey", config.highValueToggleKey);

                config.PanelMinWidth = ModConfigAPI.SafeLoad<float>(MOD_NAME, "panelWidth", config.PanelMinWidth);
                config.PanelMaxHeight = ModConfigAPI.SafeLoad<float>(MOD_NAME, "panelHeight", config.PanelMaxHeight);
                config.highValuePanelOpacity = ModConfigAPI.SafeLoad<float>(MOD_NAME, "highValuePanelOpacity", config.highValuePanelOpacity);

                config.titleFontSize = ModConfigAPI.SafeLoad<int>(MOD_NAME, "titleFontSize", config.titleFontSize);
                config.lootboxTitleFontSize = ModConfigAPI.SafeLoad<int>(MOD_NAME, "lootboxTitleFontSize", config.lootboxTitleFontSize);
                config.itemFontSize = ModConfigAPI.SafeLoad<int>(MOD_NAME, "itemFontSize", config.itemFontSize);
                config.statsFontSize = ModConfigAPI.SafeLoad<int>(MOD_NAME, "statsFontSize", config.statsFontSize);

                config.scrollSensitivity = ModConfigAPI.SafeLoad<float>(MOD_NAME, "scrollSensitivity", config.scrollSensitivity);
                config.layoutSpacing = ModConfigAPI.SafeLoad<int>(MOD_NAME, "layoutSpacing", config.layoutSpacing);

                config.debugMode = ModConfigAPI.SafeLoad<bool>(MOD_NAME, "debugMode", config.debugMode);

                if (config.debugMode)
                {
                    Debug.Log($"[LootboxConfig] 配置已加载: " +
                        $"DetectionRadius={config.detectionRadius}, " +
                        $"ShowPet={config.showPetInventory}, " +
                        $"ShowStorage={config.showPlayerStorage}, " +
                        $"PanelSize=({config.PanelMinWidth}, {config.PanelMaxHeight})");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LootboxConfig] 加载配置失败: {e.Message}");
            }
        }

        /// <summary>
        /// 配置变动回调
        /// </summary>
        public static void OnModConfigChanged(string key)
        {
            if (!key.StartsWith(MOD_NAME + "_"))
                return;

            LoadFromModConfig();

            if (config.debugMode)
            {
                Debug.Log($"[LootboxConfig] 配置已更新 -> {key}");
            }
        }
    }
}