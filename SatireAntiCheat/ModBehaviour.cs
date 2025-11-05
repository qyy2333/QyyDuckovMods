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
        private float scanDelay = 5f; // 进入游戏5秒后开始"扫描"
        private float elapsedTime = 0f;
        private GameObject warningPanel;

        // 可疑文件名列表(纯恶搞)
        private readonly string[] suspiciousNames = new string[]
        {
            "cheat", "trainer", "hack", "modifier", "editor",
            "修改器", "作弊", "辅助", "外挂",
            "CheatEngine", "WeMod", "FLiNG", "风灵月影"
        };

        // 讽刺性警告文本
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

            // 等待一段时间后执行"扫描"
            if (elapsedTime >= scanDelay)
            {
                Debug.Log("[SatireAntiCheat] 开始执行假扫描...");
                PerformFakeScan();
                hasShownWarning = true;
            }
        }

        /// <summary>
        /// 执行假扫描(只检查桌面文件名,不涉及隐私)
        /// </summary>
        private void PerformFakeScan()
        {
            try
            {
                // 只扫描桌面 - 这是用户可见的公开区域
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                Debug.Log($"[SatireAntiCheat] 桌面路径: {desktopPath}");

                if (!Directory.Exists(desktopPath))
                {
                    Debug.Log("[SatireAntiCheat] 无法访问桌面,显示默认消息");
                    ShowRandomSatiricalMessage();
                    return;
                }

                List<string> foundFiles = new List<string>();

                // 只获取文件名,不读取内容
                string[] files = Directory.GetFiles(desktopPath);
                Debug.Log($"[SatireAntiCheat] 桌面文件数: {files.Length}");

                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileName(filePath).ToLower();

                    foreach (string suspicious in suspiciousNames)
                    {
                        if (fileName.Contains(suspicious.ToLower()))
                        {
                            foundFiles.Add(Path.GetFileName(filePath));
                            Debug.Log($"[SatireAntiCheat] 找到'可疑'文件: {Path.GetFileName(filePath)}");
                            break;
                        }
                    }
                }

                // 显示讽刺性警告
                if (foundFiles.Count > 0)
                {
                    Debug.Log($"[SatireAntiCheat] 共找到 {foundFiles.Count} 个'可疑'文件,显示警告");
                    ShowSatiricalWarning(foundFiles);
                }
                else
                {
                    // 没找到也显示消息(降低概率检查)
                    Debug.Log("[SatireAntiCheat] 未找到可疑文件,显示随机消息");
                    ShowRandomSatiricalMessage();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SatireAntiCheat] 扫描出错: {e.Message}");
                Debug.LogWarning("[SatireAntiCheat] 显示备用消息");
                ShowRandomSatiricalMessage();
            }
        }

        /// <summary>
        /// 显示讽刺性警告对话框
        /// </summary>
        private void ShowSatiricalWarning(List<string> foundFiles)
        {
            string fileList = string.Join("\n• ", foundFiles);
            string warning = satiricalWarnings[UnityEngine.Random.Range(0, satiricalWarnings.Length)];

            // 如果警告文本包含占位符,则填入文件名
            if (warning.Contains("{0}"))
            {
                warning = string.Format(warning, fileList);
            }

            ShowDialog("『反作弊系统™』检测报告", warning);
        }

        /// <summary>
        /// 显示随机讽刺消息
        /// </summary>
        private void ShowRandomSatiricalMessage()
        {
            string[] messages = new string[]
            {
                "恭喜!您的电脑很干净!\n\n但我们还是扫描了您的:\n• 系统进程\n• 内存数据\n• 注册表\n• 您的灵魂\n\n开玩笑的,我们什么都没扫描\n不像某个游戏...",

                "『反作弊系统』运行正常\n\n已完成:\n[✓] 假装扫描硬盘\n[✓] 假装收集数据\n[✓] 假装上传信息\n[✓] 实际上啥也没干\n\n这才是正确的做法!",

                "提示:\n\n本游戏的反作弊系统:\n• 不扫描您的文件\n• 不收集您的数据\n• 不侵犯您的隐私\n• 不像某些'AAA'游戏\n\n请放心游玩!",

                "系统通知:\n\n检测到您正在运行:\n• Steam\n• Discord  \n• Chrome浏览器\n• 这个游戏\n\n这些都是正常的!\n不像某些反作弊会把这些当作威胁"
            };

            string message = messages[UnityEngine.Random.Range(0, messages.Length)];
            ShowDialog("反作弊系统™", message);
        }

        /// <summary>
        /// 显示对话框 - 使用Canvas和TextMeshPro
        /// </summary>
        private void ShowDialog(string title, string message)
        {
            Debug.Log($"[SatireAntiCheat] 显示对话框: {title}");

            // 创建Canvas
            var canvasObj = new GameObject("SatireAntiCheatCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // 确保在最上层

            var canvasScaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // 半透明背景
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0, 0, 0, 0.85f);

            // 警告面板
            var panelObj = new GameObject("WarningPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800, 500);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = panelObj.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.98f);

            // 标题
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = new Vector2(-40, 80);
            titleRect.anchoredPosition = new Vector2(0, -40);

            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            // titleText.text = "⚠️ " + title;
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(1f, 0.3f, 0.3f);
            titleText.alignment = TextAlignmentOptions.Center;

            // 消息内容
            var messageObj = new GameObject("Message");
            messageObj.transform.SetParent(panelObj.transform, false);
            var messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0, 0);
            messageRect.anchorMax = new Vector2(1, 1);
            messageRect.sizeDelta = new Vector2(-80, -200);
            messageRect.anchoredPosition = new Vector2(0, -20);

            var messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = message;
            messageText.fontSize = 20;
            messageText.color = Color.white;
            messageText.alignment = TextAlignmentOptions.Center;

            // 说明文字
            var noteObj = new GameObject("Note");
            noteObj.transform.SetParent(panelObj.transform, false);
            var noteRect = noteObj.AddComponent<RectTransform>();
            noteRect.anchorMin = new Vector2(0, 0);
            noteRect.anchorMax = new Vector2(1, 0);
            noteRect.sizeDelta = new Vector2(-40, 40);
            noteRect.anchoredPosition = new Vector2(0, 80);

            var noteText = noteObj.AddComponent<TextMeshProUGUI>();
            noteText.text = "(这是一个讽刺mod,不会有任何实际影响)";
            noteText.fontSize = 14;
            noteText.fontStyle = FontStyles.Italic;
            noteText.color = new Color(0.7f, 0.7f, 0.7f);
            noteText.alignment = TextAlignmentOptions.Center;

            // 关闭按钮
            var buttonObj = new GameObject("CloseButton");
            buttonObj.transform.SetParent(panelObj.transform, false);
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0);
            buttonRect.anchorMax = new Vector2(0.5f, 0);
            buttonRect.sizeDelta = new Vector2(300, 50);
            buttonRect.anchoredPosition = new Vector2(0, 30);

            var button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            var buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f);

            var buttonTextObj = new GameObject("ButtonText");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            var buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;

            var buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "我知道了 (关闭)";
            buttonText.fontSize = 20;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;

            // 按钮点击事件
            button.onClick.AddListener(() =>
            {
                Debug.Log("[SatireAntiCheat] 用户关闭对话框");
                Destroy(canvasObj);
            });

            // 5秒后自动关闭
            Destroy(canvasObj, 30f);

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