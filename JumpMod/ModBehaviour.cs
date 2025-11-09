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

        private bool jumpRequested = false;

        private KeyCode jumpKey = KeyCode.Space; // 默认跳跃键
        private float jumpForce = 5f;            // 默认跳跃高度

        private string modName = "JumpMod";
        private string keyConfig = "jumpKey";
        private string forceConfig = "jumpForce";

        protected override void OnAfterSetup()
        {
            Debug.Log("[JumpMod] Mod 已加载，等待角色初始化...");

            if (ModConfigAPI.IsAvailable())
            {
                SetupModConfig();
            }

            // 添加 Mod 激活事件监听
            ModManager.OnModActivated += OnModActivated;
        }

        private void OnEnable()
        {
            // 同 OnAfterSetup 保证激活时也处理
            if (ModConfigAPI.IsAvailable())
            {
                SetupModConfig();
            }
        }

        private void OnDisable()
        {
            ModManager.OnModActivated -= OnModActivated;
            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(OnConfigChanged);
        }

        /// <summary>
        /// 在 ModConfig 可用时注册配置和监听事件
        /// </summary>
        private void SetupModConfig()
        {
            RegisterModConfig();
            LoadConfigFromModConfig();
            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnConfigChanged);
        }

        /// <summary>
        /// 注册 ModConfig 配置项
        /// </summary>
        private void RegisterModConfig()
        {
            // 跳跃键配置
            ModConfigAPI.SafeAddInputWithSlider(
                modName,
                keyConfig,
                "跳跃键 (Jump Key)",
                typeof(string),
                "Space"
            );

            // 跳跃高度配置
            ModConfigAPI.SafeAddInputWithSlider(
                modName,
                forceConfig,
                "跳跃高度 (Jump Force)",
                typeof(float),
                5f,
                new Vector2(1f, 20f)
            );
        }

        /// <summary>
        /// 从 ModConfig 读取配置
        /// </summary>
        private void LoadConfigFromModConfig()
        {
            string keyStr = ModConfigAPI.SafeLoad<string>(modName, keyConfig, "Space");
            if (!string.IsNullOrEmpty(keyStr) && Enum.TryParse(keyStr, true, out KeyCode key))
                jumpKey = key;

            jumpForce = ModConfigAPI.SafeLoad<float>(modName, forceConfig, 5f);

            Debug.Log($"[JumpMod] 配置加载完成: 跳跃键={jumpKey}, 跳跃高度={jumpForce}");
        }

        /// <summary>
        /// 配置变更回调
        /// </summary>
        private void OnConfigChanged(string changedKey)
        {
            if (!changedKey.EndsWith(keyConfig) && !changedKey.EndsWith(forceConfig))
                return;

            LoadConfigFromModConfig();
            Debug.Log("[JumpMod] 配置已更新");
        }

        private void Update()
        {
            if (player == null)
            {
                player = CharacterMainControl.Main;
                if (player != null)
                    Debug.Log("[JumpMod] 找到玩家对象！");
            }
            if (player == null) return;

            if (movement == null)
            {
                movement = player.GetComponent<Movement>();
                if (movement != null)
                    Debug.Log("[JumpMod] 找到 Movement 组件！");
            }
            if (movement == null) return;

            // 检测跳跃按键
            if (Input.GetKeyDown(jumpKey) && movement.IsOnGround)
                jumpRequested = true;

            if (jumpRequested)
            {
                PerformJump();
                jumpRequested = false;
            }
        }

        /// <summary>
        /// 执行跳跃
        /// </summary>
        private void PerformJump()
        {
            if (movement == null) return;

            var cm = movement.GetComponent<CharacterMovement>();
            if (cm == null) return;

            cm.PauseGroundConstraint(0.2f);

            Vector3 vel = cm.velocity;
            vel.y = jumpForce;
            cm.velocity = vel;

            Debug.Log($"[JumpMod] 跳跃成功！高度: {jumpForce}");
        }

        /// <summary>
        /// Mod 激活事件处理
        /// </summary>
        private void OnModActivated(ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            if (info.name == ModConfigAPI.ModConfigName)
            {
                Debug.Log("[JumpMod] ModConfig 已激活");

                // 注册配置和事件监听
                SetupModConfig();
            }
        }
    }
}
