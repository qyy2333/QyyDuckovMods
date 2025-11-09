using Duckov.Modding;
using UnityEngine;
using System;
using ECM2;

namespace JumpMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private Movement movement;
        private CharacterMainControl player;

        private bool jumpRequested;
        private float lastJumpTime;
        private const float JumpCooldown = 0.1f;

        private string jumpKeyString = "Space";
        private float jumpForce = 5f;
        private float gravityMultiplier = 1f;

        private Vector3 originalGravity;
        private bool gravityInitialized = false;

        private const string ModName = "JumpMod";
        private const string KeyConfig = "jumpKey";
        private const string ForceConfig = "jumpForce";
        private const string GravityConfig = "gravityMultiplier";

        protected override void OnAfterSetup()
        {
            Debug.Log("[JumpMod] 已加载，等待 ModConfig...");
            ModManager.OnModActivated += OnModActivated;

            if (ModConfigAPI.IsAvailable())
                SetupModConfig();
        }

        private void OnDisable()
        {
            ModManager.OnModActivated -= OnModActivated;
            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(OnConfigChanged);
        }

        private void OnModActivated(ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            if (info.name == ModConfigAPI.ModConfigName)
            {
                Debug.Log("[JumpMod] 检测到 ModConfig 已激活");
                SetupModConfig();
            }
        }

        private void SetupModConfig()
        {
            // 注册配置项
            ModConfigAPI.SafeAddInputWithSlider(ModName, KeyConfig, "跳跃键", typeof(string), "Space");
            ModConfigAPI.SafeAddInputWithSlider(ModName, ForceConfig, "跳跃高度", typeof(float), 5f, new Vector2(1f, 20f));
            ModConfigAPI.SafeAddInputWithSlider(ModName, GravityConfig, "重力倍数", typeof(float), 1f, new Vector2(0.1f, 3f));

            LoadConfig();
            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnConfigChanged);
        }

        private void LoadConfig()
        {
            jumpKeyString = ModConfigAPI.SafeLoad<string>(ModName, KeyConfig, "Space");
            jumpForce = ModConfigAPI.SafeLoad<float>(ModName, ForceConfig, 5f);
            gravityMultiplier = ModConfigAPI.SafeLoad<float>(ModName, GravityConfig, 1f);

            ApplyGravity();
            Debug.Log($"[JumpMod] 配置加载完成: 跳跃键={jumpKeyString}, 跳跃高度={jumpForce}, 重力倍数={gravityMultiplier}");
        }

        private void OnConfigChanged(string changedKey)
        {
            if (changedKey.EndsWith(KeyConfig) ||
                changedKey.EndsWith(ForceConfig) ||
                changedKey.EndsWith(GravityConfig))
                LoadConfig();
        }

        private void Update()
        {
            // 等待玩家对象
            if (player == null)
            {
                player = CharacterMainControl.Main;
                if (player != null)
                    Debug.Log("[JumpMod] 找到玩家对象");
            }
            if (player == null) return;

            // 初始化原始重力，只做一次
            if (!gravityInitialized)
            {
                originalGravity = Physics.gravity;
                gravityInitialized = true;
                Debug.Log($"[JumpMod] 原始全局重力: {originalGravity}");
                ApplyGravity();
            }

            // 找 Movement 组件
            if (movement == null)
            {
                movement = player.GetComponent<Movement>();
                if (movement != null)
                    Debug.Log("[JumpMod] 找到 Movement 组件");
            }
            if (movement == null) return;

            // 跳跃按键检测 - 支持特殊键
            if (IsJumpKeyPressed() && movement.IsOnGround && Time.time - lastJumpTime > JumpCooldown)
                jumpRequested = true;

            if (jumpRequested)
            {
                PerformJump();
                jumpRequested = false;
            }
        }

        /// <summary>
        /// 检测跳跃键是否按下（支持特殊键）
        /// </summary>
        private bool IsJumpKeyPressed()
        {
            if (string.IsNullOrEmpty(jumpKeyString))
                return false;

            // 处理特殊键
            string key = jumpKeyString.Trim();

            switch (key.ToLower())
            {
                case "leftctrl":
                case "left ctrl":
                case "lctrl":
                    return Input.GetKeyDown(KeyCode.LeftControl);

                case "rightctrl":
                case "right ctrl":
                case "rctrl":
                    return Input.GetKeyDown(KeyCode.RightControl);

                case "leftshift":
                case "left shift":
                case "lshift":
                    return Input.GetKeyDown(KeyCode.LeftShift);

                case "rightshift":
                case "right shift":
                case "rshift":
                    return Input.GetKeyDown(KeyCode.RightShift);

                case "leftalt":
                case "left alt":
                case "lalt":
                    return Input.GetKeyDown(KeyCode.LeftAlt);

                case "rightalt":
                case "right alt":
                case "ralt":
                    return Input.GetKeyDown(KeyCode.RightAlt);

                case "ctrl":
                case "control":
                    return Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);

                case "shift":
                    return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);

                case "alt":
                case "altgr":
                    return Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);

                default:
                    // 尝试解析为普通 KeyCode
                    if (Enum.TryParse(key, true, out KeyCode keyCode))
                        return Input.GetKeyDown(keyCode);
                    return false;
            }
        }

        private void ApplyGravity()
        {
            if (!gravityInitialized) return;
            Physics.gravity = originalGravity * gravityMultiplier;
            Debug.Log($"[JumpMod] 已应用全局重力倍数: {Physics.gravity}");
        }

        private void PerformJump()
        {
            var cm = movement.GetComponent<CharacterMovement>();
            if (cm == null) return;

            cm.PauseGroundConstraint(0.05f);
            Vector3 vel = cm.velocity;
            vel.y = jumpForce;
            cm.velocity = vel;

            lastJumpTime = Time.time;
            Debug.Log($"[JumpMod] 跳跃成功！高度: {jumpForce}");
        }
    }
}