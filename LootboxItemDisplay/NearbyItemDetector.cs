using ItemStatsSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LootboxItemDisplay
{
    /// <summary>
    /// 统一检测器:使用缓存和增量更新,同时追踪最高价值物品
    /// </summary>
    public class NearbyItemDetector
    {
        private float lastDetectionTime = 0f;
        private float lastFullScanTime = 0f;

        // 缓存所有场景中的对象
        private readonly HashSet<InteractableLootbox> allLootboxesCache = new HashSet<InteractableLootbox>();
        private readonly HashSet<InteractablePickup> allPickupsCache = new HashSet<InteractablePickup>();

        // 检测结果缓存
        private readonly List<InteractableLootbox> cachedLootboxes = new List<InteractableLootbox>();
        private readonly List<InteractablePickup> cachedPickups = new List<InteractablePickup>();

        // 临时列表复用
        private readonly List<InteractableLootbox> tempLootboxList = new List<InteractableLootbox>();
        private readonly List<InteractablePickup> tempPickupList = new List<InteractablePickup>();

        // 距离缓存
        private readonly Dictionary<Transform, float> distanceCache = new Dictionary<Transform, float>();

        // 配置缓存
        private bool showPetInventory;
        private bool showPlayerStorage;
        private float detectionRadius;
        private float detectionInterval;

        // 全量扫描间隔
        private const float FULL_SCAN_INTERVAL = 5f;

        // 最高价值物品信息
        public class HighValueItemInfo
        {
            public Item Item;
            public Vector3 Position;
            public float Distance;
            public float Value;
            public string LocationType; // "箱子" 或 "地面"
            public string ContainerName;
        }

        private HighValueItemInfo currentHighestValue = null;
        private List<HighValueItemInfo> topThreeItems = new List<HighValueItemInfo>();

        public event Action<List<InteractableLootbox>, List<InteractablePickup>> OnItemsChanged;
        public event Action<List<HighValueItemInfo>> OnHighestValueChanged;

        /// <summary>
        /// 每帧更新检测
        /// </summary>
        public void Update()
        {
            var player = CharacterMainControl.Main;
            if (player == null) return;

            float currentTime = Time.time;

            // 限制检测频率
            if (currentTime - lastDetectionTime < detectionInterval) return;
            lastDetectionTime = currentTime;

            // 定期全量扫描
            if (currentTime - lastFullScanTime > FULL_SCAN_INTERVAL)
            {
                RefreshObjectCache();
                lastFullScanTime = currentTime;
            }

            // 刷新配置缓存
            RefreshConfigCache();

            try
            {
                Vector3 playerPos = player.transform.position;
                Camera camera = Camera.main;

                // 清空距离缓存
                distanceCache.Clear();

                // 检测附近对象
                bool lootboxChanged = DetectNearbyLootboxes(playerPos, camera);
                bool pickupChanged = DetectNearbyPickups(playerPos, camera);

                // 查找全图最高价值物品(如果功能开启)
                if (LootboxConfigManager.Config.showHighestValueItem)
                {
                    FindHighestValueItem(playerPos);
                }

                // 只在有变化时触发事件
                if (lootboxChanged || pickupChanged)
                {
                    OnItemsChanged?.Invoke(cachedLootboxes, cachedPickups);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NearbyItemDetector] 检测错误: {e.Message}");
            }
        }

        /// <summary>
        /// 刷新配置缓存
        /// </summary>
        private void RefreshConfigCache()
        {
            var config = LootboxConfigManager.Config;
            showPetInventory = config.showPetInventory;
            showPlayerStorage = config.showPlayerStorage;
            detectionRadius = config.detectionRadius;
            detectionInterval = config.detectionInterval;
        }

        /// <summary>
        /// 全量扫描并刷新对象缓存
        /// </summary>
        private void RefreshObjectCache()
        {
            // 清理已销毁的对象
            allLootboxesCache.RemoveWhere(obj => obj == null);
            allPickupsCache.RemoveWhere(obj => obj == null);

            // 查找新对象
            var lootboxes = UnityEngine.Object.FindObjectsOfType<InteractableLootbox>();
            foreach (var lootbox in lootboxes)
            {
                if (lootbox != null && lootbox.Inventory != null)
                {
                    allLootboxesCache.Add(lootbox);
                }
            }

            var pickups = UnityEngine.Object.FindObjectsOfType<InteractablePickup>();
            foreach (var pickup in pickups)
            {
                if (pickup != null)
                {
                    allPickupsCache.Add(pickup);
                }
            }
        }

        /// <summary>
        /// 检测附近箱子
        /// </summary>
        private bool DetectNearbyLootboxes(Vector3 playerPos, Camera camera)
        {
            tempLootboxList.Clear();
            float radiusSqr = detectionRadius * detectionRadius;

            foreach (var lootbox in allLootboxesCache)
            {
                if (lootbox == null || lootbox.Inventory == null) continue;

                // 快速距离检查
                Vector3 offset = lootbox.transform.position - playerPos;
                float distanceSqr = offset.sqrMagnitude;
                if (distanceSqr > radiusSqr) continue;

                // 名称过滤
                if (!IsLootboxNameValid(lootbox)) continue;

                // 视锥剔除
                if (camera != null && !IsInViewport(lootbox.transform.position, camera))
                    continue;

                // 缓存实际距离
                float distance = Mathf.Sqrt(distanceSqr);
                distanceCache[lootbox.transform] = distance;

                tempLootboxList.Add(lootbox);
            }

            // 按距离排序
            tempLootboxList.Sort((a, b) =>
            {
                float distA = distanceCache.ContainsKey(a.transform) ? distanceCache[a.transform] : 0;
                float distB = distanceCache.ContainsKey(b.transform) ? distanceCache[b.transform] : 0;
                return distA.CompareTo(distB);
            });

            bool changed = HasListChanged(cachedLootboxes, tempLootboxList);

            if (changed)
            {
                cachedLootboxes.Clear();
                cachedLootboxes.AddRange(tempLootboxList);
            }

            return changed;
        }

        /// <summary>
        /// 检测附近掉落物
        /// </summary>
        private bool DetectNearbyPickups(Vector3 playerPos, Camera camera)
        {
            tempPickupList.Clear();
            float radiusSqr = detectionRadius * detectionRadius;

            foreach (var pickup in allPickupsCache)
            {
                if (pickup == null || pickup.ItemAgent?.Item == null) continue;

                // 快速距离检查
                Vector3 offset = pickup.transform.position - playerPos;
                float distanceSqr = offset.sqrMagnitude;
                if (distanceSqr > radiusSqr) continue;

                // 视锥剔除
                if (camera != null && !IsInViewport(pickup.transform.position, camera))
                    continue;

                // 缓存实际距离
                float distance = Mathf.Sqrt(distanceSqr);
                distanceCache[pickup.transform] = distance;

                tempPickupList.Add(pickup);
            }

            // 按距离排序
            tempPickupList.Sort((a, b) =>
            {
                float distA = distanceCache.ContainsKey(a.transform) ? distanceCache[a.transform] : 0;
                float distB = distanceCache.ContainsKey(b.transform) ? distanceCache[b.transform] : 0;
                return distA.CompareTo(distB);
            });

            bool changed = HasListChanged(cachedPickups, tempPickupList);

            if (changed)
            {
                cachedPickups.Clear();
                cachedPickups.AddRange(tempPickupList);
            }

            return changed;
        }

        /// <summary>
        /// 查找全图最高价值物品
        /// </summary>
        private void FindHighestValueItem(Vector3 playerPos)
        {
            float threshold = LootboxConfigManager.Config.highValueThreshold;
            List<HighValueItemInfo> highValueItems = new List<HighValueItemInfo>();

            // 遍历所有缓存的箱子
            foreach (var lootbox in allLootboxesCache)
            {
                if (lootbox == null || lootbox.Inventory == null) continue;

                // 最高价值搜索时,始终过滤掉玩家仓库和宠物
                string lootboxName = lootbox.name ?? string.Empty;
                if (lootboxName.IndexOf("PetProxy", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                if (lootboxName.IndexOf("PlayerStorage", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                float distance = Vector3.Distance(playerPos, lootbox.transform.position);

                // 遍历箱子中的物品
                foreach (var item in lootbox.Inventory)
                {
                    if (item == null) continue;

                    float value = item.GetTotalRawValue() / 2f;

                    if (value >= threshold)
                    {
                        highValueItems.Add(new HighValueItemInfo
                        {
                            Item = item,
                            Position = lootbox.transform.position,
                            Distance = distance,
                            Value = value,
                            LocationType = "箱子",
                            ContainerName = lootboxName
                        });
                    }
                }
            }

            // 遍历所有缓存的掉落物
            foreach (var pickup in allPickupsCache)
            {
                if (pickup == null || pickup.ItemAgent?.Item == null) continue;

                var item = pickup.ItemAgent.Item;
                float value = item.GetTotalRawValue() / 2f;

                if (value >= threshold)
                {
                    float distance = Vector3.Distance(playerPos, pickup.transform.position);
                    highValueItems.Add(new HighValueItemInfo
                    {
                        Item = item,
                        Position = pickup.transform.position,
                        Distance = distance,
                        Value = value,
                        LocationType = "地面",
                        ContainerName = null
                    });
                }
            }

            // 按价值排序,取前3名
            highValueItems.Sort((a, b) => b.Value.CompareTo(a.Value));
            var topThree = highValueItems.Count > 3 ? highValueItems.GetRange(0, 3) : highValueItems;

            // 检查是否变化
            bool changed = HasTopItemsChanged(topThree);

            if (changed)
            {
                topThreeItems = new List<HighValueItemInfo>(topThree);
                currentHighestValue = topThree.Count > 0 ? topThree[0] : null;

                // 触发事件,传递前3名列表
                OnHighestValueChanged?.Invoke(topThreeItems);

                if (topThree.Count > 0)
                {
                    Debug.Log($"[NearbyItemDetector] 前{topThree.Count}名高价值物品:");
                    for (int i = 0; i < topThree.Count; i++)
                    {
                        var item = topThree[i];
                        Debug.Log($"  #{i + 1}: {item.Item.DisplayName} " +
                                 $"价值${item.Value:F0} 距离{item.Distance:F1}m 位于{item.LocationType}");
                    }
                }
            }
        }

        /// <summary>
        /// 检查前几名物品是否变化
        /// </summary>
        private bool HasTopItemsChanged(List<HighValueItemInfo> newTopItems)
        {
            // 如果数量不同,肯定变化了
            if (topThreeItems.Count != newTopItems.Count)
                return true;

            // 如果都是空的,没变化
            if (topThreeItems.Count == 0 && newTopItems.Count == 0)
                return false;

            // 检查前三名是否都相同
            for (int i = 0; i < newTopItems.Count && i < topThreeItems.Count; i++)
            {
                if (topThreeItems[i].Item != newTopItems[i].Item ||
                    topThreeItems[i].Position != newTopItems[i].Position)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 验证箱子名称是否有效
        /// </summary>
        private readonly Dictionary<string, bool> nameValidityCache = new Dictionary<string, bool>();

        private bool IsLootboxNameValid(InteractableLootbox lootbox)
        {
            string name = lootbox.name;
            if (name == null) return true;

            if (nameValidityCache.TryGetValue(name, out bool cached))
                return cached;

            bool isValid = true;

            if (!showPetInventory && name.IndexOf("PetProxy", StringComparison.OrdinalIgnoreCase) >= 0)
                isValid = false;
            else if (!showPlayerStorage && name.IndexOf("PlayerStorage", StringComparison.OrdinalIgnoreCase) >= 0)
                isValid = false;

            nameValidityCache[name] = isValid;
            return isValid;
        }

        /// <summary>
        /// 视锥剔除检查
        /// </summary>
        private bool IsInViewport(Vector3 worldPos, Camera camera)
        {
            Vector3 viewport = camera.WorldToViewportPoint(worldPos);
            return viewport.z > 0 &&
                   viewport.x >= LootboxConfig.ViewportMinRange.x &&
                   viewport.x <= LootboxConfig.ViewportMaxRange.x &&
                   viewport.y >= LootboxConfig.ViewportMinRange.y &&
                   viewport.y <= LootboxConfig.ViewportMaxRange.y;
        }

        /// <summary>
        /// 检查列表是否有变化
        /// </summary>
        private bool HasListChanged<T>(List<T> oldList, List<T> newList) where T : UnityEngine.Object
        {
            if (oldList.Count != newList.Count) return true;

            for (int i = 0; i < oldList.Count; i++)
            {
                if (oldList[i] != newList[i]) return true;
            }

            return false;
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        public void Clear()
        {
            allLootboxesCache.Clear();
            allPickupsCache.Clear();
            cachedLootboxes.Clear();
            cachedPickups.Clear();
            tempLootboxList.Clear();
            tempPickupList.Clear();
            distanceCache.Clear();
            nameValidityCache.Clear();
            currentHighestValue = null;
            topThreeItems.Clear();
        }

        /// <summary>
        /// 获取缓存的距离
        /// </summary>
        public float GetCachedDistance(Transform transform)
        {
            return distanceCache.TryGetValue(transform, out float distance) ? distance : 0f;
        }

        /// <summary>
        /// 获取当前最高价值物品列表
        /// </summary>
        public List<HighValueItemInfo> GetTopValueItems()
        {
            return topThreeItems;
        }
    }
}