using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ConsentManagerSDK
{
    internal static class CMPBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            _ = BootstrapAsync();
        }

        private static async Task BootstrapAsync()
        {
            var settings = CMPSettings.LoadDefault();
            if (settings == null)
            {
                Debug.LogWarning("[CMPBootstrap] CMPSettings asset not found in Resources. Skipping auto-initialization.");
                return;
            }

            if (!settings.AutoInitialize)
            {
                return;
            }

            if (!settings.HasValidConfiguration())
            {
                Debug.LogWarning("[CMPBootstrap] CMPSettings contains placeholder values. Update the CMPSettings asset to enable auto-initialization.");
                return;
            }

            try
            {
                var manager = CMPManager.Instance;
                await manager.InitializeAsync(settings.ToConfig(), settings.ToUIConfig());
                await manager.CheckAndOpenAsync();
                Debug.Log("[CMPBootstrap] CMP initialized and consent check completed before scene load.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CMPBootstrap] Failed to auto-initialize CMP: {ex.Message}");
            }
        }
    }
}
