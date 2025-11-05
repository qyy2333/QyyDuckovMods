using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LootboxItemDisplay
{
    public class LootboxUIPanel
    {
        private GameObject canvasObj;
        private GameObject panelObj;
        private RectTransform panelRect;
        private VerticalLayoutGroup contentLayout;

        private bool isDragging = false;
        private bool isManuallyHidden = false;
        private Vector2 dragOffset;

        private string lastToggleKeyString = null;
        private TextMeshProUGUI titleTextComponent;

        private readonly List<GameObject> contentItems = new List<GameObject>();

        private KeyCode toggleKey = KeyCode.F1;

        public bool IsVisible => panelObj != null && panelObj.activeSelf;

        public void Create()
        {
            try
            {
                CreateCanvas();
                CreatePanel();
                CreateTitleBar();
                CreateContent();
                LoadToggleKeyFromConfig();
                panelObj.SetActive(false);
                Debug.Log("[LootboxUIPanel] UI 创建成功");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LootboxUIPanel] UI 创建失败: {e.Message}\n{e.StackTrace}");
            }
        }

        public void Destroy()
        {
            if (canvasObj != null)
            {
                UnityEngine.Object.Destroy(canvasObj);
                canvasObj = null;
            }
            panelObj = null;
            panelRect = null;
            contentLayout = null;
            contentItems.Clear();
        }

        public void Update()
        {
            UpdateToggleKeyFromConfig();
            HandleToggleKey();
            UpdateDragging();

            if (panelRect != null && contentLayout != null && panelObj.activeSelf)
            {
                var contentRect = contentLayout.GetComponent<RectTransform>();
                float contentHeight = LayoutUtility.GetPreferredHeight(contentRect);
                float newHeight = Mathf.Min(contentHeight + 40, LootboxConfigManager.Config.PanelMaxHeight);
                panelRect.sizeDelta = new Vector2(LootboxConfigManager.Config.PanelMinWidth, newHeight);
            }
        }

        public void UpdateDragging()
        {
            if (panelObj == null || !panelObj.activeSelf) return;

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

        private void LoadToggleKeyFromConfig()
        {
            try
            {
                var cfg = LootboxConfigManager.Config;
                if (cfg == null) return;

                var field = cfg.GetType().GetField("toggleKey", BindingFlags.Public | BindingFlags.Instance);
                if (field == null) { toggleKey = KeyCode.F3; return; }

                string keyString = field.GetValue(cfg) as string;
                if (string.IsNullOrEmpty(keyString)) { toggleKey = KeyCode.F3; return; }

                if (Enum.TryParse(keyString, true, out KeyCode parsedKey))
                    toggleKey = parsedKey;
                else
                    toggleKey = KeyCode.F3;
            }
            catch
            {
                toggleKey = KeyCode.F3;
            }
        }

        private void HandleToggleKey()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (IsVisible)
                {
                    Hide();
                    isManuallyHidden = true;
                }
                else
                {
                    panelObj.SetActive(true);
                    isManuallyHidden = false;
                }
            }
        }

        public void Show(List<InteractableLootbox> lootboxes, List<InteractablePickup> pickups = null)
        {
            if (panelObj == null || contentLayout == null)
            {
                Debug.LogError("[LootboxUIPanel] UI 未初始化");
                return;
            }

            if (isManuallyHidden) return;

            ClearContent();

            if ((lootboxes == null || lootboxes.Count == 0) && (pickups == null || pickups.Count == 0))
            {
                Hide();
                return;
            }

            BuildContent(lootboxes, pickups);
            panelObj.SetActive(true);
        }

        public void Hide()
        {
            if (panelObj != null)
                panelObj.SetActive(false);
        }

        private void UpdateToggleKeyFromConfig()
        {
            try
            {
                var cfg = LootboxConfigManager.Config;
                if (cfg == null) return;

                var field = cfg.GetType().GetField("toggleKey", BindingFlags.Public | BindingFlags.Instance);
                if (field == null) return;

                string keyString = field.GetValue(cfg) as string;
                if (string.IsNullOrEmpty(keyString) || keyString == lastToggleKeyString) return;

                lastToggleKeyString = keyString;

                if (Enum.TryParse(keyString, true, out KeyCode parsedKey))
                {
                    toggleKey = parsedKey;
                    if (titleTextComponent != null)
                        titleTextComponent.text = $"附近的箱子 (按 {toggleKey} 显示/隐藏)";
                }
            }
            catch { }
        }

        private void CreateCanvas()
        {
            canvasObj = new GameObject("LootboxDisplayCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = LootboxConfig.CanvasSortingOrder;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = LootboxConfig.ReferenceResolution;

            canvasObj.AddComponent<GraphicRaycaster>();
            UnityEngine.Object.DontDestroyOnLoad(canvasObj);
        }

        private void CreatePanel()
        {
            panelObj = new GameObject("LootboxPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);

            panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.5f);
            panelRect.anchorMax = new Vector2(0, 0.5f);
            panelRect.pivot = new Vector2(0, 0.5f);
            panelRect.anchoredPosition = LootboxConfig.PanelInitialPosition;
            panelRect.sizeDelta = new Vector2(LootboxConfigManager.Config.PanelMinWidth, 0);

            var image = panelObj.AddComponent<Image>();
            image.color = LootboxConfig.PanelBackgroundColor;

            var outline = panelObj.AddComponent<Outline>();
            outline.effectColor = LootboxConfig.PanelOutlineColor;
            outline.effectDistance = new Vector2(3, -3);

            var shadow = panelObj.AddComponent<Shadow>();
            shadow.effectColor = LootboxConfig.PanelShadowColor;
            shadow.effectDistance = new Vector2(5, -5);
        }

        private void CreateTitleBar()
        {
            var titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(panelObj.transform, false);
            var titleRect = titleBar.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(0, 40);

            var titleBg = titleBar.AddComponent<Image>();
            titleBg.color = LootboxConfig.TitleBarColor;

            var titleTextObj = new GameObject("TitleText");
            titleTextObj.transform.SetParent(titleBar.transform, false);
            var titleTextRect = titleTextObj.AddComponent<RectTransform>();
            titleTextRect.anchorMin = new Vector2(0, 0);
            titleTextRect.anchorMax = new Vector2(1, 1);
            titleTextRect.offsetMin = new Vector2(10, 0);
            titleTextRect.offsetMax = new Vector2(-10, 0);

            titleTextComponent = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleTextComponent.text = $"附近的箱子 (按 {toggleKey} 显示/隐藏)";
            titleTextComponent.fontSize = LootboxConfigManager.Config.titleFontSize;
            titleTextComponent.fontStyle = FontStyles.Bold;
            titleTextComponent.color = LootboxConfig.TitleTextColor;
            titleTextComponent.alignment = TextAlignmentOptions.Left;
            titleTextComponent.verticalAlignment = VerticalAlignmentOptions.Middle;
        }

        private void CreateContent()
        {
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(panelObj.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(5, 5);
            contentRect.offsetMax = new Vector2(-5, -45);

            // 添加 Mask 防止内容溢出
            contentObj.AddComponent<RectMask2D>();

            contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = LootboxConfigManager.Config.layoutSpacing;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.padding = LootboxConfig.LayoutPadding;
            contentLayout.childAlignment = TextAnchor.UpperCenter;

            var contentFitter = contentObj.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void ClearContent()
        {
            foreach (var item in contentItems)
            {
                if (item != null)
                    UnityEngine.Object.Destroy(item);
            }
            contentItems.Clear();
        }

        private void BuildContent(List<InteractableLootbox> lootboxes, List<InteractablePickup> pickups)
        {
            var player = CharacterMainControl.Main;
            if (player == null) return;

            var contentBuilder = new LootboxUIContentBuilder(contentLayout.transform);

            if (lootboxes != null)
            {
                for (int i = 0; i < lootboxes.Count; i++)
                {
                    var lootbox = lootboxes[i];
                    float distance = Vector3.Distance(player.transform.position, lootbox.transform.position);
                    contentBuilder.BuildLootboxSection(lootbox, distance, contentItems);

                    if (i < lootboxes.Count - 1)
                        contentBuilder.BuildDivider(contentItems);
                }
            }

            if (pickups != null && pickups.Count > 0)
            {
                contentBuilder.BuildDivider(contentItems);
                contentBuilder.BuildPickupSection(pickups, contentItems);
            }
        }
    }
}