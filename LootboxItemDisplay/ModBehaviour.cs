using Duckov.Modding;
using System.Collections.Generic;
using UnityEngine;

namespace LootboxItemDisplay
{
    /// <summary>
    /// Mod主入口 - 负责协调各个模块
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private NearbyItemDetector detector;
        private LootboxUIPanel uiPanel;
        private HighestValueUIPanel highestValueUI; // 最高价值UI
        protected override void OnAfterSetup()
        {
            Debug.Log("[LootboxItemDisplay] Mod 启动");

            // 初始化检测器
            detector = new NearbyItemDetector();
            detector.OnItemsChanged += OnItemsChanged;
            detector.OnHighestValueChanged += OnHighestValueChanged; // 订阅最高价值事件

            // 初始化UI面板
            uiPanel = new LootboxUIPanel();
            uiPanel.Create();

            // 初始化最高价值UI
            highestValueUI = new HighestValueUIPanel();
            highestValueUI.Create();

            Debug.Log("[LootboxItemDisplay] Mod 初始化完成");
        }

        private void OnEnable()
        {
            // 监听 ModConfig 激活事件
            ModManager.OnModActivated += OnModActivated;

            // 立即检查 ModConfig 是否可用
            if (ModConfigAPI.IsAvailable())
            {
                Debug.Log("[LootboxItemDisplay] ModConfig 已可用");
                LootboxConfigManager.SetupModConfig();
                LootboxConfigManager.LoadFromModConfig();
            }
        }

        private void OnDisable()
        {
            ModManager.OnModActivated -= OnModActivated;
            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(LootboxConfigManager.OnModConfigChanged);
        }

        private void OnModActivated(ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            if (info.name == ModConfigAPI.ModConfigName)
            {
                Debug.Log("[LootboxItemDisplay] ModConfig 已激活");
                LootboxConfigManager.SetupModConfig();
                LootboxConfigManager.LoadFromModConfig();

                // 添加配置变更监听
                ModConfigAPI.SafeAddOnOptionsChangedDelegate(LootboxConfigManager.OnModConfigChanged);
            }
        }

        protected override void OnBeforeDeactivate()
        {
            // 清理检测器
            if (detector != null)
            {
                detector.OnItemsChanged -= OnItemsChanged;
                detector.OnHighestValueChanged -= OnHighestValueChanged;
                detector.Clear();
                detector = null;
            }
            // 清理UI
            if (uiPanel != null)
            {
                uiPanel.Destroy();
                uiPanel = null;
            }

            if (highestValueUI != null)
            {
                highestValueUI.Destroy();
                highestValueUI = null;
            }
            Debug.Log("[LootboxItemDisplay] Mod 已卸载");
        }

        private void Update()
        {
            // 更新检测器
            detector?.Update();

            // 更新UI拖拽
            uiPanel?.Update();

            // 更新最高价值UI(实时距离)
            highestValueUI?.Update();
        }

        /// <summary>
        /// 当检测到的列表改变时
        /// </summary>
        private void OnItemsChanged(List<InteractableLootbox> lootboxes, List<InteractablePickup> pickups)
        {
            if ((lootboxes == null || lootboxes.Count == 0) &&
                (pickups == null || pickups.Count == 0))
            {
                uiPanel?.Hide();
            }
            else
            {
                uiPanel?.Show(lootboxes, pickups);
            }
        }


        /// <summary>
        /// 当最高价值物品改变时
        /// </summary>
        private void OnHighestValueChanged(List<NearbyItemDetector.HighValueItemInfo> topItems)
        {
            highestValueUI?.UpdateDisplay(topItems);
        }
    }
}