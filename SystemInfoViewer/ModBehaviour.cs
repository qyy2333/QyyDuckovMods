using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace SystemInfoViewer
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private GameObject canvasObj;
        private GameObject panelObj;
        private RectTransform panelRect;
        private ScrollRect scrollRect;
        private RectTransform contentRect;
        private TextMeshProUGUI infoText;
        private KeyCode toggleKey = KeyCode.F5;

        void Awake()
        {
            Debug.Log("[SystemInfoViewer] Mod Loaded!");
            CreateCanvas();
            CreatePanel();
            CreateScrollView();
            ShowSystemInfo();
            panelObj.SetActive(true);
        }

        void OnDestroy()
        {
            if (canvasObj != null)
                UnityEngine.Object.Destroy(canvasObj);
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                panelObj.SetActive(!panelObj.activeSelf);
            }
        }

        private void CreateCanvas()
        {
            canvasObj = new GameObject("SystemInfoCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
            UnityEngine.Object.DontDestroyOnLoad(canvasObj);
        }

        private void CreatePanel()
        {
            panelObj = new GameObject("SystemInfoPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);

            panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(20, -20);
            panelRect.sizeDelta = new Vector2(600, 800);

            var image = panelObj.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.75f);

            var outline = panelObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.8f, 1f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            var shadow = panelObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(3, -3);
        }

        private void CreateScrollView()
        {
            // ScrollView 容器
            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(panelObj.transform, false);

            var scrollRectTransform = scrollObj.AddComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0, 0);
            scrollRectTransform.anchorMax = new Vector2(1, 1);
            scrollRectTransform.offsetMin = new Vector2(10, 10);
            scrollRectTransform.offsetMax = new Vector2(-10, -10);

            // 添加 Mask 组件防止文字溢出
            var mask = scrollObj.AddComponent<RectMask2D>();

            scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // 内容区
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);
            contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            var layout = contentObj.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.padding = new RectOffset(10, 10, 10, 10);

            var fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;

            // 文本对象
            var textObj = new GameObject("SystemInfoText");
            textObj.transform.SetParent(contentObj.transform, false);
            infoText = textObj.AddComponent<TextMeshProUGUI>();
            infoText.fontSize = 18;
            infoText.color = Color.cyan;
            infoText.alignment = TextAlignmentOptions.TopLeft;
            infoText.enableWordWrapping = true;
            infoText.overflowMode = TextOverflowModes.Overflow;
        }

        private void ShowSystemInfo()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<b><color=yellow>=== 系统信息 ===</color></b>");
            sb.AppendLine($"<b>设备名称：</b> {SystemInfo.deviceName}");
            sb.AppendLine($"<b>操作系统：</b> {SystemInfo.operatingSystem}");
            sb.AppendLine($"<b>CPU：</b> {SystemInfo.processorType} ×{SystemInfo.processorCount}");
            sb.AppendLine($"<b>内存：</b> {SystemInfo.systemMemorySize} MB");
            sb.AppendLine($"<b>显卡：</b> {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"<b>显存：</b> {SystemInfo.graphicsMemorySize} MB");
            sb.AppendLine($"<b>分辨率：</b> {Screen.currentResolution.width}×{Screen.currentResolution.height}\n");

            sb.AppendLine("<b><color=yellow>=== 网络信息 ===</color></b>");
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        sb.AppendLine($"网卡: {ni.Name}");
                        sb.AppendLine($"MAC: {ni.GetPhysicalAddress()}");
                        foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                sb.AppendLine($"IPv4: {ip.Address}");
                        }
                        sb.AppendLine();
                    }
                }
            }
            catch (Exception e)
            {
                sb.AppendLine($"[网络信息读取失败]: {e.Message}");
            }

            sb.AppendLine("<b><color=yellow>=== 磁盘扫描 ===</color></b>");
            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (!drive.IsReady) continue;
                    sb.AppendLine($"驱动器 {drive.Name} ({drive.DriveType})");
                    sb.AppendLine($"  格式: {drive.DriveFormat}");
                    sb.AppendLine($"  可用空间: {drive.AvailableFreeSpace / (1024 * 1024)} MB / 总计 {drive.TotalSize / (1024 * 1024)} MB");
                    sb.AppendLine($"  根目录下文件 (前10个):");
                    var files = drive.RootDirectory.GetFiles();
                    for (int i = 0; i < Mathf.Min(files.Length, 10); i++)
                    {
                        sb.AppendLine($"   • {files[i].Name}");
                    }
                    sb.AppendLine();
                }
            }
            catch (Exception e)
            {
                sb.AppendLine($"[磁盘扫描失败]: {e.Message}");
            }

            infoText.text = sb.ToString();
        }
    }
}