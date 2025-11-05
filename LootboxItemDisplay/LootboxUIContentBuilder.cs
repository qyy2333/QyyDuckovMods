using ItemStatsSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LootboxItemDisplay
{
    /// <summary>
    /// UI内容构建器
    /// </summary>
    public class LootboxUIContentBuilder
    {
        private readonly Transform parent;

        public LootboxUIContentBuilder(Transform parent)
        {
            this.parent = parent;
        }

        #region 箱子区块

        public void BuildLootboxSection(InteractableLootbox lootbox, float distance, List<GameObject> itemList)
        {
            // 检查是否启用箱子物品显示
            if (!LootboxConfigManager.Config.showLootboxItems)
            {
                return; // 不显示箱子物品,直接返回
            }

            BuildLootboxTitle(lootbox.name, distance, itemList);

            var inv = lootbox.Inventory;
            int itemCount = 0;
            float totalWeight = 0f;
            float totalValue = 0f; //总价值

            foreach (var item in inv)
            {
                if (item == null) continue;
                BuildItemRow(item, itemList);
                itemCount++;
                totalWeight += item.TotalWeight;
                totalValue += item.GetTotalRawValue() / 2f; //累计价值
            }

            if (itemCount == 0)
                BuildEmptyText(itemList);
            else
                BuildStatsText(itemCount, totalWeight, totalValue, itemList); //传入价值
        }

        private void BuildLootboxTitle(string lootboxName, float distance, List<GameObject> itemList)
        {
            var titleObj = new GameObject("BoxTitle");
            titleObj.transform.SetParent(parent, false);

            var text = titleObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = LootboxConfigManager.Config.lootboxTitleFontSize;
            text.fontStyle = FontStyles.Bold;
            text.color = LootboxConfig.LootboxTitleColor;
            text.text = $" {lootboxName} ({distance:F1}m)";

            itemList.Add(titleObj);
        }

        private void BuildItemRow(Item item, List<GameObject> itemList)
        {
            var itemObj = new GameObject("ItemText");
            itemObj.transform.SetParent(parent, false);

            var text = itemObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = LootboxConfigManager.Config.itemFontSize;
            text.color = LootboxConfig.ItemTextColor;

            // 获取品质颜色
            Color qualityColor = ItemQualitySystem.GetQualityColor(item);
            string qualityColorHex = ItemQualitySystem.GetColorHex(qualityColor);

            // 名称 - 使用品质颜色
            text.text = $"  • <color=#{qualityColorHex}>{item.DisplayName}</color>";

            // 数量显示
            if (item.StackCount > 1)
                text.text += $" <color=#{ColorUtility.ToHtmlStringRGB(LootboxConfig.ItemCountColor)}>×{item.StackCount}</color>";

            // 重量显示
            if (item.TotalWeight > 0)
                text.text += $" <color=#{ColorUtility.ToHtmlStringRGB(LootboxConfig.ItemWeightColor)}>({item.TotalWeight:0.##}kg)</color>";

            // 价值显示
            float itemValue = item.GetTotalRawValue() / 2f;
            if (itemValue > 0)
            {
                text.text += $" <color=#{ColorUtility.ToHtmlStringRGB(LootboxConfig.ItemValueColor)}>[${itemValue:F0}]</color>";
            }

            itemList.Add(itemObj);
        }

        private void BuildEmptyText(List<GameObject> itemList)
        {
            var emptyObj = new GameObject("EmptyText");
            emptyObj.transform.SetParent(parent, false);

            var text = emptyObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = LootboxConfig.EmptyTextFontSize;
            text.color = LootboxConfig.EmptyTextColor;
            text.text = "  (空箱子)";
            text.fontStyle = FontStyles.Italic;

            itemList.Add(emptyObj);
        }

        private void BuildStatsText(int itemCount, float totalWeight, float totalValue, List<GameObject> itemList)
        {
            var statsObj = new GameObject("StatsText");
            statsObj.transform.SetParent(parent, false);

            var text = statsObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = LootboxConfigManager.Config.statsFontSize;
            text.color = LootboxConfig.StatsTextColor;

            // 加入总价值显示
            text.text = $"  共 {itemCount} 种 | {totalWeight:0.##}kg";

            if (totalValue > 0)
            {
                text.text += $" | <color=#{ColorUtility.ToHtmlStringRGB(LootboxConfig.ItemValueColor)}>${totalValue:F0}</color>";
            }

            itemList.Add(statsObj);
        }

        #endregion

        #region 掉落物区块

        public void BuildPickupSection(List<InteractablePickup> pickups, List<GameObject> itemList)
        {
            // 检查是否启用掉落物显示
            if (!LootboxConfigManager.Config.showPickupItems)
            {
                return; // 不显示掉落物,直接返回
            }

            if (pickups == null || pickups.Count == 0) return;

            // 标题
            var titleObj = new GameObject("PickupTitle");
            titleObj.transform.SetParent(parent, false);

            var text = titleObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = LootboxConfigManager.Config.lootboxTitleFontSize;
            text.fontStyle = FontStyles.Bold;
            text.color = LootboxConfig.PickupTitleColor;
            text.text = $" 附近掉落物";

            itemList.Add(titleObj);

            // 统计掉落物价值
            float totalPickupValue = 0f;

            foreach (var pickup in pickups)
            {
                if (pickup?.ItemAgent?.Item == null) continue;

                BuildPickupRow(pickup.ItemAgent.Item, itemList);
                totalPickupValue += pickup.ItemAgent.Item.GetTotalRawValue() / 2f;
            }

            // 显示掉落物总价值
            if (totalPickupValue > 0)
            {
                BuildPickupStatsText(pickups.Count, totalPickupValue, itemList);
            }

            BuildDivider(itemList);
        }

        private void BuildPickupRow(Item item, List<GameObject> itemList)
        {
            var itemObj = new GameObject("PickupItemText");
            itemObj.transform.SetParent(parent, false);

            var text = itemObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = LootboxConfigManager.Config.itemFontSize;
            text.color = LootboxConfig.ItemTextColor;

            // 获取品质颜色
            Color qualityColor = ItemQualitySystem.GetQualityColor(item);
            string qualityColorHex = ItemQualitySystem.GetColorHex(qualityColor);

            // 名称 - 使用品质颜色
            text.text = $"  • <color=#{qualityColorHex}>{item.DisplayName}</color>";

            if (item.StackCount > 1)
                text.text += $" <color=#{ColorUtility.ToHtmlStringRGB(LootboxConfig.ItemCountColor)}>×{item.StackCount}</color>";

            if (item.TotalWeight > 0)
                text.text += $" <color=#{ColorUtility.ToHtmlStringRGB(LootboxConfig.ItemWeightColor)}>({item.TotalWeight:0.##}kg)</color>";

            // 价值显示
            float itemValue = item.GetTotalRawValue() / 2f;
            if (itemValue > 0)
            {
                text.text += $" <color=#{ColorUtility.ToHtmlStringRGB(LootboxConfig.ItemValueColor)}>[${itemValue:F0}]</color>";
            }

            itemList.Add(itemObj);
        }

        // 掉落物统计文本
        private void BuildPickupStatsText(int pickupCount, float totalValue, List<GameObject> itemList)
        {
            var statsObj = new GameObject("PickupStatsText");
            statsObj.transform.SetParent(parent, false);

            var text = statsObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = LootboxConfigManager.Config.statsFontSize;
            text.color = LootboxConfig.StatsTextColor;
            text.text = $"  共 {pickupCount} 个";

            if (totalValue > 0)
            {
                text.text += $" | <color=#{ColorUtility.ToHtmlStringRGB(LootboxConfig.ItemValueColor)}>${totalValue:F0}</color>";
            }

            itemList.Add(statsObj);
        }

        #endregion

        #region 分隔线

        public void BuildDivider(List<GameObject> itemList)
        {
            var dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(parent, false);

            var text = dividerObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = LootboxConfig.DividerFontSize;
            text.color = LootboxConfig.DividerColor;
            text.text = "─────────────────";
            text.alignment = TextAlignmentOptions.Center;

            itemList.Add(dividerObj);
        }

        #endregion
    }
}