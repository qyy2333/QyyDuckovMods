using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LootboxItemDisplay
{
    public class HighestValueUIPanel
    {
        private GameObject canvasObj;
        private GameObject panelObject;
        private RectTransform panelRect;

        private Image panelImage; // 新增：保存面板图像引用
        private Image titleBarImage; // 新增：保存标题栏图像引用
        private Image[] slotImages = new Image[3]; // 新增：保存槽位图像引用
        private GameObject[] itemSlots = new GameObject[3];
        private Image[] itemIconImages = new Image[3]; // 新增：物品图标
        //private GameObject[] itemIconBgs = new GameObject[3]; // 新增：图标背景
        private TextMeshProUGUI[] itemNameTexts = new TextMeshProUGUI[3];
        private TextMeshProUGUI[] valueTexts = new TextMeshProUGUI[3];
        private TextMeshProUGUI[] locationTexts = new TextMeshProUGUI[3];
        private TextMeshProUGUI[] distanceTexts = new TextMeshProUGUI[3];
        private GameObject[] arrowIndicators = new GameObject[3];

        private List<NearbyItemDetector.HighValueItemInfo> currentTopItems = new List<NearbyItemDetector.HighValueItemInfo>();

        private bool isDragging = false;
        private Vector2 dragOffset;
        private bool isManuallyHidden = false;

        private KeyCode toggleKey = KeyCode.F4;
        private string lastToggleKeyString = null;

        public bool IsVisible => panelObject != null && panelObject.activeSelf;

        public void Create()
        {
            try
            {
                CreateCanvas();
                CreatePanel();
                CreateTitleBar();
                for (int i = 0; i < 3; i++)
                {
                    CreateItemSlot(i);
                    CreateArrowIndicator(i);
                }
                LoadToggleKey();
                panelObject.SetActive(false);
            }
            catch (Exception e)
            {
                Debug.LogError($"[HighestValueUI] 创建失败: {e}");
            }
        }

        private void CreateCanvas()
        {
            canvasObj = new GameObject("HighestValueCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = LootboxConfig.CanvasSortingOrder; // 修改：使用相同图层

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = LootboxConfig.ReferenceResolution;

            canvasObj.AddComponent<GraphicRaycaster>();
            UnityEngine.Object.DontDestroyOnLoad(canvasObj);
        }

        private void CreatePanel()
        {
            panelObject = new GameObject("HighestValuePanel");
            panelObject.transform.SetParent(canvasObj.transform, false);

            panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.anchoredPosition = new Vector2(-20, -20);
            panelRect.sizeDelta = new Vector2(380, 420);

            panelImage = panelObject.AddComponent<Image>();
            panelImage.color = LootboxConfig.PanelBackgroundColor;

            var outline = panelObject.AddComponent<Outline>();
            outline.effectColor = LootboxConfig.PanelOutlineColor;
            outline.effectDistance = new Vector2(2, -2);
        }

        private void CreateTitleBar()
        {
            var titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(panelObject.transform, false);

            var titleRect = titleBar.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(0, 35);

            titleBarImage = titleBar.AddComponent<Image>();
            titleBarImage.color = LootboxConfig.TitleBarColor;

            var titleTextObj = new GameObject("TitleText");
            titleTextObj.transform.SetParent(titleBar.transform, false);

            var titleTextRect = titleTextObj.AddComponent<RectTransform>();
            titleTextRect.anchorMin = Vector2.zero;
            titleTextRect.anchorMax = Vector2.one;
            titleTextRect.offsetMin = new Vector2(10, 0);
            titleTextRect.offsetMax = new Vector2(-10, 0);

            var titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "高价值物品 TOP3";
            titleText.fontSize = 16;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = LootboxConfig.TitleTextColor;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.verticalAlignment = VerticalAlignmentOptions.Middle;
        }

        private void CreateItemSlot(int index)
        {
            float yOffset = -45f - (index * 120f);

            var slotObj = new GameObject($"Slot{index + 1}");
            slotObj.transform.SetParent(panelObject.transform, false);

            var slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0, 1);
            slotRect.anchorMax = new Vector2(1, 1);
            slotRect.pivot = new Vector2(0.5f, 1);
            slotRect.anchoredPosition = new Vector2(0, yOffset);
            slotRect.sizeDelta = new Vector2(-20, 110);

            slotImages[index] = slotObj.AddComponent<Image>();
            slotImages[index].color = new Color(0.15f, 0.15f, 0.2f, 0.6f);

            // 排名标签
            var rankText = CreateText(slotObj.transform, "Rank", new Vector2(10, -5), new Vector2(30, 25),
                $"#{index + 1}", 18, GetRankColor(index), FontStyles.Bold, TextAlignmentOptions.Left);
            var rankRect = rankText.GetComponent<RectTransform>();
            rankRect.anchorMin = new Vector2(0, 1);
            rankRect.anchorMax = new Vector2(0, 1);
            rankRect.pivot = new Vector2(0, 1);

            // 创建物品图标
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform, false);

            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 1);
            iconRect.anchorMax = new Vector2(0, 1);
            iconRect.pivot = new Vector2(0, 1);
            iconRect.anchoredPosition = new Vector2(50, -10);
            iconRect.sizeDelta = new Vector2(50, 50);

            itemIconImages[index] = iconObj.AddComponent<Image>();
            itemIconImages[index].color = Color.white;

            // 给图标添加背景
            // var iconBg = new GameObject("IconBg");
            // iconBg.transform.SetParent(slotObj.transform, false);
            // var iconBgRect = iconBg.AddComponent<RectTransform>();
            // iconBgRect.anchorMin = new Vector2(0, 1);
            // iconBgRect.anchorMax = new Vector2(0, 1);
            // iconBgRect.pivot = new Vector2(0, 1);
            // iconBgRect.anchoredPosition = new Vector2(50, -10);
            // iconBgRect.sizeDelta = new Vector2(50, 50);
            // var iconBgImage = iconBg.AddComponent<Image>();
            // iconBgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            // iconBg.transform.SetAsFirstSibling(); // 放到图标后面
            // itemIconBgs[index] = iconBg; // 保存背景引用

            // 物品名称（右移以避开图标）
            itemNameTexts[index] = CreateText(slotObj.transform, "Name", new Vector2(110, -10),
                new Vector2(-130, 25), "", 15, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);
            var nameRect = itemNameTexts[index].GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.pivot = new Vector2(0, 1);

            // 价值
            valueTexts[index] = CreateText(slotObj.transform, "Value", new Vector2(110, -35),
                new Vector2(-130, 20), "", 14, LootboxConfig.ItemValueColor, FontStyles.Normal, TextAlignmentOptions.Left);
            var valueRect = valueTexts[index].GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0, 1);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.pivot = new Vector2(0, 1);

            // 位置
            locationTexts[index] = CreateText(slotObj.transform, "Location", new Vector2(0, -65),
                new Vector2(-20, 18), "", 13, LootboxConfig.ItemTextColor, FontStyles.Normal, TextAlignmentOptions.Center);

            // 距离
            distanceTexts[index] = CreateText(slotObj.transform, "Distance", new Vector2(0, -87),
                new Vector2(-20, 20), "", 14, LootboxConfig.ItemWeightColor, FontStyles.Bold, TextAlignmentOptions.Center);

            itemSlots[index] = slotObj;
            itemSlots[index].SetActive(false);
        }

        private void CreateArrowIndicator(int index)
        {
            var arrowObj = new GameObject($"Arrow{index + 1}");
            arrowObj.transform.SetParent(canvasObj.transform, false);

            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
            arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.sizeDelta = new Vector2(40, 40);

            var arrow = arrowObj.AddComponent<Image>();
            arrow.sprite = CreateArrowSprite();
            arrow.color = GetRankColor(index);

            var outline = arrowObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            arrowIndicators[index] = arrowObj;
            arrowIndicators[index].SetActive(false);
        }

        private Sprite CreateArrowSprite()
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float centerX = 32f;
                    float topY = 50f;
                    float bottomY = 14f;
                    float width = 30f;

                    bool inTriangle = y >= bottomY && y <= topY &&
                                     x >= centerX - width * (topY - y) / (topY - bottomY) &&
                                     x <= centerX + width * (topY - y) / (topY - bottomY);

                    pixels[y * 64 + x] = inTriangle ? Color.white : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, Vector2 pos, Vector2 size,
            string text, float fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;

            return tmp;
        }

        private Color GetRankColor(int rank)
        {
            return rank == 0 ? new Color(1f, 0.84f, 0f) :
                   rank == 1 ? new Color(0.75f, 0.75f, 0.75f) :
                   new Color(0.8f, 0.5f, 0.2f);
        }

        public void Update()
        {
            // 检查是否在主菜单，如果是则隐藏UI
            if (CharacterMainControl.Main == null && IsVisible)
            {
                Hide();
                return;
            }

            UpdateToggleKey();
            HandleToggle();
            UpdateDragging();

            // 实时检查配置并更新图标显示状态
            UpdateIconVisibility();

            // 实时更新透明度
            UpdateOpacity();

            if (IsVisible && currentTopItems.Count > 0)
            {
                var player = CharacterMainControl.Main;
                if (player != null)
                {
                    for (int i = 0; i < currentTopItems.Count; i++)
                    {
                        float dist = Vector3.Distance(player.transform.position, currentTopItems[i].Position);
                        distanceTexts[i].text = $"{dist:F1}m";
                        UpdateArrowIndicator(i, currentTopItems[i].Position);
                    }
                }
            }
            else
            {
                HideAllArrows();
            }

            if (!LootboxConfigManager.Config.showHighestValueItem && IsVisible)
                Hide();
        }

        /// <summary>
        /// 实时更新UI透明度
        /// </summary>
        private void UpdateOpacity()
        {
            if (!IsVisible) return;

            float opacity = LootboxConfigManager.Config.highValuePanelOpacity;

            // 更新主面板透明度
            if (panelImage != null)
            {
                Color c = panelImage.color;
                c.a = opacity;
                panelImage.color = c;
            }

            // 更新标题栏透明度
            if (titleBarImage != null)
            {
                Color c = titleBarImage.color;
                c.a = opacity;
                titleBarImage.color = c;
            }

            // 更新所有槽位透明度
            for (int i = 0; i < slotImages.Length; i++)
            {
                if (slotImages[i] != null)
                {
                    Color c = slotImages[i].color;
                    c.a = opacity * 0.8f; // 槽位稍微透明一些
                    slotImages[i].color = c;
                }
            }
        }

        /// <summary>
        /// 实时更新图标显示状态
        /// </summary>
        private void UpdateIconVisibility()
        {
            if (!IsVisible || currentTopItems.Count == 0) return;

            bool showIcons = LootboxConfigManager.Config.showItemIcons;

            for (int i = 0; i < 3; i++)
            {
                if (i < currentTopItems.Count)
                {
                    // 同步控制图标和背景的显示状态
                    if (itemIconImages[i] != null)
                    {
                        itemIconImages[i].enabled = showIcons && currentTopItems[i].Item.Icon != null;
                    }
                    // if (itemIconBgs[i] != null)
                    // {
                    //     itemIconBgs[i].SetActive(showIcons && currentTopItems[i].Item.Icon != null);
                    // }
                }
            }
        }

        private void UpdateArrowIndicator(int index, Vector3 targetPos)
        {
            var player = CharacterMainControl.Main;
            if (player == null || arrowIndicators[index] == null)
                return;

            var camera = Camera.main;
            if (camera == null)
                return;

            Vector3 screenPos = camera.WorldToScreenPoint(targetPos);
            bool isOnScreen = screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width &&
                             screenPos.y > 0 && screenPos.y < Screen.height;

            if (isOnScreen)
            {
                arrowIndicators[index].SetActive(true);
                var arrowRect = arrowIndicators[index].GetComponent<RectTransform>();
                arrowRect.position = screenPos;

                Vector3 direction = (targetPos - player.transform.position).normalized;
                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg - camera.transform.eulerAngles.y;
                arrowRect.rotation = Quaternion.Euler(0, 0, -angle);
            }
            else
            {
                arrowIndicators[index].SetActive(true);
                var arrowRect = arrowIndicators[index].GetComponent<RectTransform>();

                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                float margin = 50f;

                float angle = Mathf.Atan2(screenPos.y - screenCenter.y, screenPos.x - screenCenter.x);

                float maxX = Screen.width / 2f - margin;
                float maxY = Screen.height / 2f - margin;

                float tx = Mathf.Abs(maxX / Mathf.Cos(angle));
                float ty = Mathf.Abs(maxY / Mathf.Sin(angle));
                float t = Mathf.Min(tx, ty);

                Vector2 arrowPos = new Vector2(
                    screenCenter.x + t * Mathf.Cos(angle),
                    screenCenter.y + t * Mathf.Sin(angle)
                );

                arrowRect.position = arrowPos;
                arrowRect.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg - 90);
            }
        }

        public void UpdateDisplay(List<NearbyItemDetector.HighValueItemInfo> topItems)
        {
            if (!LootboxConfigManager.Config.showHighestValueItem)
            {
                if (IsVisible) Hide();
                return;
            }

            if (topItems == null || topItems.Count == 0)
            {
                Hide();
                isManuallyHidden = false;
                return;
            }

            bool hasNewTopItem = currentTopItems.Count == 0 ||
                                 topItems.Count == 0 ||
                                 currentTopItems[0].Item != topItems[0].Item;

            currentTopItems = new List<NearbyItemDetector.HighValueItemInfo>(topItems);

            for (int i = 0; i < 3; i++)
            {
                if (i < topItems.Count)
                {
                    UpdateItemSlot(i, topItems[i]);
                    itemSlots[i].SetActive(true);
                }
                else
                {
                    itemSlots[i].SetActive(false);
                    if (arrowIndicators[i] != null)
                        arrowIndicators[i].SetActive(false);
                }
            }

            if (hasNewTopItem)
            {
                isManuallyHidden = false;
                Show();
            }
            else if (!isManuallyHidden)
            {
                Show();
            }
        }

        private void UpdateItemSlot(int index, NearbyItemDetector.HighValueItemInfo info)
        {
            Color qualityColor = ItemQualitySystem.GetQualityColor(info.Item);
            string colorHex = ItemQualitySystem.GetColorHex(qualityColor);

            // 更新物品图标
            if (itemIconImages[index] != null && info.Item.Icon != null)
            {
                itemIconImages[index].sprite = info.Item.Icon;
                itemIconImages[index].enabled = true;
            }
            else if (itemIconImages[index] != null)
            {
                itemIconImages[index].enabled = false;
            }

            itemNameTexts[index].text = $"<color=#{colorHex}>{info.Item.DisplayName}</color>";
            valueTexts[index].text = $"${info.Value:F0}";

            if (info.LocationType == "箱子" && !string.IsNullOrEmpty(info.ContainerName))
            {
                string name = info.ContainerName.Length > 20 ?
                    info.ContainerName.Substring(0, 17) + "..." : info.ContainerName;
                locationTexts[index].text = $"{name}";
            }
            else
            {
                locationTexts[index].text = $"{info.LocationType}";
            }

            distanceTexts[index].text = $"{info.Distance:F1}m";
        }

        private void HandleToggle()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (IsVisible)
                {
                    Hide();
                    HideAllArrows();
                    isManuallyHidden = true;
                }
                else if (currentTopItems.Count > 0 && LootboxConfigManager.Config.showHighestValueItem)
                {
                    Show();
                    isManuallyHidden = false;
                }
            }
        }

        private void HideAllArrows()
        {
            for (int i = 0; i < arrowIndicators.Length; i++)
            {
                if (arrowIndicators[i] != null)
                    arrowIndicators[i].SetActive(false);
            }
        }

        private void UpdateDragging()
        {
            if (panelObject == null || !panelObject.activeSelf) return;

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                if (RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePos))
                {
                    isDragging = true;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasObj.GetComponent<RectTransform>(), mousePos, null, out Vector2 localPoint);
                    dragOffset = panelRect.anchoredPosition - localPoint;
                }
            }

            if (isDragging && Input.GetMouseButton(0))
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasObj.GetComponent<RectTransform>(), Input.mousePosition, null, out Vector2 localPoint);
                panelRect.anchoredPosition = localPoint + dragOffset;
            }

            if (Input.GetMouseButtonUp(0))
                isDragging = false;
        }

        private void LoadToggleKey()
        {
            try
            {
                var cfg = LootboxConfigManager.Config;
                if (cfg == null) return;

                var field = cfg.GetType().GetField("highValueToggleKey", BindingFlags.Public | BindingFlags.Instance);
                if (field == null) return;

                string keyString = field.GetValue(cfg) as string;
                if (!string.IsNullOrEmpty(keyString) && Enum.TryParse(keyString, true, out KeyCode key))
                {
                    toggleKey = key;
                    lastToggleKeyString = keyString;
                }
            }
            catch { }
        }

        private void UpdateToggleKey()
        {
            try
            {
                var cfg = LootboxConfigManager.Config;
                if (cfg == null) return;

                var field = cfg.GetType().GetField("highValueToggleKey", BindingFlags.Public | BindingFlags.Instance);
                if (field == null) return;

                string keyString = field.GetValue(cfg) as string;
                if (!string.IsNullOrEmpty(keyString) && keyString != lastToggleKeyString)
                {
                    lastToggleKeyString = keyString;
                    if (Enum.TryParse(keyString, true, out KeyCode key))
                        toggleKey = key;
                }
            }
            catch { }
        }

        private void Show()
        {
            if (panelObject != null)
                panelObject.SetActive(true);
        }

        public void Hide()
        {
            if (panelObject != null)
                panelObject.SetActive(false);
            HideAllArrows();
        }

        public void Destroy()
        {
            if (canvasObj != null)
            {
                UnityEngine.Object.Destroy(canvasObj);
                canvasObj = null;
            }
            panelObject = null;
            panelRect = null;
            currentTopItems.Clear();
        }
    }
}