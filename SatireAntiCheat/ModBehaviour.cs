using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Duckov.Modding;

namespace SatireAntiCheat
{
    /// <summary>
    /// 讽刺性反作弊系统 - 模仿某些游戏的过度反作弊行为
    /// 注意:这是一个恶搞mod,不会真的扫描整个硬盘或造成任何实际影响
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private bool hasShownWarning = false;
        private float scanDelay = 5f;
        private float elapsedTime = 0f;
        private GameObject warningPanel;

        private readonly string[] suspiciousNames = new string[]
        {
            "cheat", "trainer", "hack", "modifier", "editor",
            "修改器", "作弊", "辅助", "外挂",
            "CheatEngine", "WeMod", "FLiNG", "风灵月影"
        };

        private readonly string[] satiricalWarnings = new string[]
        {
            "检测到可疑软件!\n\n我们的『先进反作弊系统』已扫描您的:\n• 桌面文件\n• 回收站\n• 浏览器历史记录\n• 家庭相册\n• 您奶奶的电脑\n\n封号倒计时: 永不封号\n(因为这只是个玩笑)",
            "警告:检测到作弊工具!\n\n根据我们的『隐私侵犯协议』:\n✓ 已读取您的硬盘序列号\n✓ 已记录您的浏览历史\n✓ 已扫描您的桌面壁纸\n✓ 已分析您的文件夹命名习惯\n\n别担心,我们不会真的这么做\n(不像某些游戏)",
            "『反作弊系统』检测报告:\n\n可疑文件: {0}\n威胁等级: 9999/10\n建议措施: 立即卸载\n实际措施: 什么都不做\n\n——来自一个有良心的游戏mod",
            "您的电脑已被标记!\n\n检测到文件名包含敏感词汇\n我们已经:\n[█████████] 100% 没有上传您的数据\n[█████████] 100% 没有扫描其他文件\n[█████████] 100% 尊重您的隐私\n\n不像某个使用内核级反作弊的游戏"
        };

        protected override void OnAfterSetup()
        {
            Debug.Log("[SatireAntiCheat] 讽刺性反作弊mod已加载");
            Debug.Log("[SatireAntiCheat] 本mod不会真的扫描您的硬盘,只是为了讽刺某些游戏的行为");
            Debug.Log($"[SatireAntiCheat] 将在 {scanDelay} 秒后执行'扫描'");
        }

        private void Update()
        {
            if (hasShownWarning) return;

            elapsedTime += Time.deltaTime;

            if (elapsedTime >= scanDelay)
            {
                PerformFakeScan();
                hasShownWarning = true;
            }
        }

        private void PerformFakeScan()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (!Directory.Exists(desktopPath))
                {
                    ShowRandomSatiricalMessage();
                    return;
                }

                List<string> foundFiles = new List<string>();
                string[] files = Directory.GetFiles(desktopPath);

                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileName(filePath).ToLower();

                    foreach (string suspicious in suspiciousNames)
                    {
                        if (fileName.Contains(suspicious.ToLower()))
                        {
                            foundFiles.Add(Path.GetFileName(filePath));
                            break;
                        }
                    }
                }

                if (foundFiles.Count > 0)
                {
                    ShowSatiricalWarning(foundFiles);
                }
                else
                {
                    ShowRandomSatiricalMessage();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SatireAntiCheat] 扫描出错: {e.Message}");
                ShowRandomSatiricalMessage();
            }
        }

        private void ShowSatiricalWarning(List<string> foundFiles)
        {
            string fileList = string.Join("\n• ", foundFiles);
            string warning = satiricalWarnings[UnityEngine.Random.Range(0, satiricalWarnings.Length)];

            if (warning.Contains("{0}"))
            {
                warning = string.Format(warning, foundFiles.Count);
            }

            ShowDialog("系统检测", warning);
        }

        private void ShowRandomSatiricalMessage()
        {
            string[] messages = new string[]
            {
                "恭喜!您的电脑很干净!\n\n但我们还是扫描了您的:\n• 系统进程\n• 内存数据\n• 注册表\n• 您的灵魂\n\n开玩笑的,我们什么都没扫描\n不像某个游戏...",
                "『反作弊系统』运行正常\n\n已完成:\n[✓] 假装扫描硬盘\n[✓] 假装收集数据\n[✓] 假装上传信息\n[✓] 实际上啥也没干\n\n这才是正确的做法!",
                "提示:\n\n本游戏的反作弊系统:\n• 不扫描您的文件\n• 不收集您的数据\n• 不侵犯您的隐私\n• 不像某些'AAA'游戏\n\n请放心游玩!",
                "系统通知:\n\n检测到您正在运行:\n• Steam\n• Discord\n• Chrome浏览器\n• 这个游戏\n\n这些都是正常的!\n不像某些反作弊会把这些当作威胁"
            };

            string message = messages[UnityEngine.Random.Range(0, messages.Length)];
            ShowDialog("反作弊系统™", message);
        }

        private static Color FromHex(string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out var color))
            {
                return color;
            }
            return Color.magenta;
        }

        private void ShowDialog(string title, string message)
        {
            Debug.Log($"[SatireAntiCheat] 显示对话框: {title}");

            // 创建Canvas
            var canvasObj = new GameObject("SatireAntiCheatCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var canvasScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // 大背景
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = FromHex("#0f161e");

            // 主对话框（顶满左右）
            var panelObj = new GameObject("DialogPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.5f);
            panelRect.anchorMax = new Vector2(1, 0.5f);
            panelRect.sizeDelta = new Vector2(0, 400);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = panelObj.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = FromHex("#192028");

            // 顶部泛光白线
            var topLineObj = new GameObject("TopGlowLine");
            topLineObj.transform.SetParent(panelObj.transform, false);
            var topLineRect = topLineObj.AddComponent<RectTransform>();
            topLineRect.anchorMin = new Vector2(0, 1);
            topLineRect.anchorMax = new Vector2(1, 1);
            topLineRect.sizeDelta = new Vector2(0, 2);
            topLineRect.anchoredPosition = new Vector2(0, 0);

            var topLineImage = topLineObj.AddComponent<UnityEngine.UI.Image>();
            topLineImage.color = FromHex("#58646e");

            // 消息文本区域
            var messageObj = new GameObject("MessageText");
            messageObj.transform.SetParent(panelObj.transform, false);
            var messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0, 0.2f);
            messageRect.anchorMax = new Vector2(1, 1);
            messageRect.sizeDelta = new Vector2(-100, -60);
            messageRect.anchoredPosition = new Vector2(0, -30);

            var messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = message;
            messageText.fontSize = 16;
            messageText.color = Color.white;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.verticalAlignment = VerticalAlignmentOptions.Middle;

            // 底部按钮区域
            var bottomBarObj = new GameObject("BottomBar");
            bottomBarObj.transform.SetParent(panelObj.transform, false);
            var bottomBarRect = bottomBarObj.AddComponent<RectTransform>();
            bottomBarRect.anchorMin = new Vector2(0, 0);
            bottomBarRect.anchorMax = new Vector2(1, 0);
            bottomBarRect.sizeDelta = new Vector2(0, 80);
            bottomBarRect.anchoredPosition = new Vector2(0, 40);

            var bottomBarImage = bottomBarObj.AddComponent<UnityEngine.UI.Image>();
            bottomBarImage.color = FromHex("#20272f");

            // 确定按钮背景
            var buttonBgObj = new GameObject("ButtonBackground");
            buttonBgObj.transform.SetParent(bottomBarObj.transform, false);
            var buttonBgRect = buttonBgObj.AddComponent<RectTransform>();
            buttonBgRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonBgRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonBgRect.sizeDelta = new Vector2(250, 50);
            buttonBgRect.anchoredPosition = Vector2.zero;

            var buttonBgImage = buttonBgObj.AddComponent<UnityEngine.UI.Image>();
            buttonBgImage.color = FromHex("#192024");

            // 确定按钮描边（上细下粗）
            // 上边描边（细）
            var topBorderObj = new GameObject("TopBorder");
            topBorderObj.transform.SetParent(buttonBgObj.transform, false);
            var topBorderRect = topBorderObj.AddComponent<RectTransform>();
            topBorderRect.anchorMin = new Vector2(0, 1);
            topBorderRect.anchorMax = new Vector2(1, 1);
            topBorderRect.sizeDelta = new Vector2(0, 1);
            topBorderRect.anchoredPosition = new Vector2(0, 0);

            var topBorderImage = topBorderObj.AddComponent<UnityEngine.UI.Image>();
            topBorderImage.color = FromHex("#179061");

            // 下边描边（粗）
            var bottomBorderObj = new GameObject("BottomBorder");
            bottomBorderObj.transform.SetParent(buttonBgObj.transform, false);
            var bottomBorderRect = bottomBorderObj.AddComponent<RectTransform>();
            bottomBorderRect.anchorMin = new Vector2(0, 0);
            bottomBorderRect.anchorMax = new Vector2(1, 0);
            bottomBorderRect.sizeDelta = new Vector2(0, 3);
            bottomBorderRect.anchoredPosition = new Vector2(0, 0);

            var bottomBorderImage = bottomBorderObj.AddComponent<UnityEngine.UI.Image>();
            bottomBorderImage.color = FromHex("#179061");

            // 左边描边
            var leftBorderObj = new GameObject("LeftBorder");
            leftBorderObj.transform.SetParent(buttonBgObj.transform, false);
            var leftBorderRect = leftBorderObj.AddComponent<RectTransform>();
            leftBorderRect.anchorMin = new Vector2(0, 0);
            leftBorderRect.anchorMax = new Vector2(0, 1);
            leftBorderRect.sizeDelta = new Vector2(1, 0);
            leftBorderRect.anchoredPosition = new Vector2(0.5f, 0);

            var leftBorderImage = leftBorderObj.AddComponent<UnityEngine.UI.Image>();
            leftBorderImage.color = FromHex("#179061");

            // 右边描边
            var rightBorderObj = new GameObject("RightBorder");
            rightBorderObj.transform.SetParent(buttonBgObj.transform, false);
            var rightBorderRect = rightBorderObj.AddComponent<RectTransform>();
            rightBorderRect.anchorMin = new Vector2(1, 0);
            rightBorderRect.anchorMax = new Vector2(1, 1);
            rightBorderRect.sizeDelta = new Vector2(1, 0);
            rightBorderRect.anchoredPosition = new Vector2(-0.5f, 0);

            var rightBorderImage = rightBorderObj.AddComponent<UnityEngine.UI.Image>();
            rightBorderImage.color = FromHex("#179061");

            // 确定按钮（透明，只用于点击）
            var buttonObj = new GameObject("ConfirmButton");
            buttonObj.transform.SetParent(buttonBgObj.transform, false);
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = Vector2.zero;
            buttonRect.anchorMax = Vector2.one;
            buttonRect.sizeDelta = Vector2.zero;

            var button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            var buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0, 0, 0, 0); // 透明

            // 按钮悬停效果
            var colors = button.colors;
            colors.normalColor = new Color(1, 1, 1, 0);
            colors.highlightedColor = new Color(1, 1, 1, 0.1f);
            colors.pressedColor = new Color(1, 1, 1, 0.2f);
            button.colors = colors;

            // 确定文字 RGB(22,188,120)
            var buttonTextObj = new GameObject("ButtonText");
            buttonTextObj.transform.SetParent(buttonBgObj.transform, false);
            var buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;

            var buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "确定";
            buttonText.fontSize = 14;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.color = FromHex("#20ab73");
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.verticalAlignment = VerticalAlignmentOptions.Middle;
            buttonText.raycastTarget = false; // 防止阻挡点击

            // 底部说明文字
            var noteObj = new GameObject("NoteText");
            noteObj.transform.SetParent(bottomBarObj.transform, false);
            var noteRect = noteObj.AddComponent<RectTransform>();
            noteRect.anchorMin = new Vector2(0, 0);
            noteRect.anchorMax = new Vector2(1, 0);
            noteRect.sizeDelta = new Vector2(0, 20);
            noteRect.anchoredPosition = new Vector2(0, -25);

            var noteText = noteObj.AddComponent<TextMeshProUGUI>();
            noteText.text = "(这是一个讽刺mod，不会有任何实际影响，点击关闭对话框)";
            noteText.fontSize = 14;
            noteText.fontStyle = FontStyles.Italic;
            noteText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            noteText.alignment = TextAlignmentOptions.Center;

            // 按钮点击事件
            button.onClick.AddListener(() =>
            {
                Debug.Log("[SatireAntiCheat] 用户关闭对话框");
                Destroy(canvasObj);
            });

            warningPanel = canvasObj;
            Debug.Log("[SatireAntiCheat] 对话框创建完成");
        }

        protected override void OnBeforeDeactivate()
        {
            if (warningPanel != null)
            {
                Destroy(warningPanel);
            }
            Debug.Log("[SatireAntiCheat] mod已卸载");
        }
    }
}