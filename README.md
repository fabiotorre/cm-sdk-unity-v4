# Consentmanager Unity CMP SDK (v4)

Modern, pure C# implementation of the consentmanager CMP for Unity.  
This package embeds the same storage format and behavioural model as the native Android/iOS SDKs while exposing an idiomatic Unity API built on async/await.

---

## Table of Contents
1. [Requirements](#requirements)  
2. [Repository layout](#repository-layout)  
3. [Installation](#installation)  
4. [Configuration](#configuration)  
5. [Quick start](#quick-start)  
6. [Public API](#public-api)  
7. [Demo scene](#demo-scene)  
8. [Platform specifics](#platform-specifics)  
9. [Troubleshooting](#troubleshooting)  
10. [Migration from native SDKs](#migration-from-native-sdks)  
11. [Support](#support)  

---

## Requirements

- Unity **2020.3 LTS** or newer (tested with Unity 6.0.2f1)  
- iOS 12+ (WKWebView) / Android 5.0+ (API 21)  
- `com.unity.nuget.newtonsoft-json` 3.x (installed by default via `Packages/manifest.json`)  
- Internet permission on Android (`<uses-permission android:name="android.permission.INTERNET"/>`)  

---

## Repository layout

```
Assets/ConsentManagerSDK/
 ├── Runtime/             # CMP core, storage, webview manager, bootstrap
 ├── Plugins/             # unity-webview sources + Android/iOS native templates
 ├── Demo/                # Playable scene + controller showcasing every API
 ├── package.json         # Unity Package Manager metadata
 └── Editor/              # Build-time validators
```

---

## Installation

### Option A – Unity Package Manager (recommended)
1. Clone or copy this repository.  
2. In Unity, open **Window ▸ Package Manager**.  
3. Click the **+** button → **Add package from disk…**.  
4. Select `Assets/ConsentManagerSDK/package.json`.  

### Option B – Direct copy
1. Copy `Assets/ConsentManagerSDK` into your own project’s `Assets`.  
2. Ensure the Newtonsoft Json package is present (either via UPM or Asset Store).  

---

## Configuration

### CMPSettings asset
1. Locate `Assets/ConsentManagerSDK/Runtime/Resources/CMPSettings.asset`.  
2. Fill in your **CMP ID**, **domain**, **language**, and **app name**.  
3. Toggle *Respect App Name* if the Unity product name should be sent automatically.  
4. Adjust UI defaults (layout style, dark mode) and *Auto Initialize* as needed.  

During builds a validator ensures placeholders are not shipped. At runtime `CMPBootstrap`:
1. Creates/initializes `CMPManager`.  
2. Runs `CheckAndOpenAsync()` before any other SDKs can start (ensuring consent gating).  

---

## Quick start

```csharp
using ConsentManagerSDK;
using UnityEngine;

public class CMPExample : MonoBehaviour
{
    async void Start()
    {
        // Optional: override settings programmatically
        var config = new CMPConfig("your-id", "a.delivery.consentmanager.net", "EN", Application.productName);
        var ui    = CMPUIConfig.BottomHalf();

        try
        {
            await CMPManager.Instance.InitializeAsync(config, ui);
            await CMPManager.Instance.CheckAndOpenAsync();
            Debug.Log("CMP ready");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"CMP init failed: {ex.Message}");
        }
    }

    public async void ForceOpenConsent()
    {
        await CMPManager.Instance.ForceOpenAsync();
    }
}
```

Important:
- Access `CMPManager.Instance` only after initialization (or check `CMPManager.Instance.IsInitialized`).  
- Register to `OnConsentReceived`, `OnConsentLayerOpened/Closed`, and `OnError` for state changes.  

---

## Public API

All async methods return `Task`, so they can be awaited or combined with Unity `AsyncOperation` wrappers.  
Key methods:

| Method | Description |
| --- | --- |
| `InitializeAsync(CMPConfig, CMPUIConfig)` | Creates storage, webview, and JS bridge |
| `CheckAndOpenAsync(bool jumpToSettings)` | Verifies consent status and shows layer only if required |
| `ForceOpenAsync(bool jumpToSettings)` | Always opens the consent layer |
| `AcceptAllAsync()` / `RejectAllAsync()` | Convenience shortcuts for full opt-in/out |
| `AcceptPurposesAsync(string[], bool)` / `RejectPurposesAsync` | Batch manage purposes |
| `AcceptVendorsAsync(string[])` / `RejectVendorsAsync` | Batch manage vendors |
| `ImportCMPInfoAsync(string)` / `ExportCMPInfo()` | Move consent strings cross-platform |
| `ResetConsentManagementDataAsync()` | Clears consent storage |
| `GetUserStatus()` / `GetStatusForPurpose(string)` / `GetStatusForVendor(string)` | Offline snapshot queries |
| `GetGoogleConsentModeStatus()` | Returns a dictionary ready for GCM v2 |
| `SetATTStatus(int)` | iOS ATTrackingManager status (0–3) |

Events:
- `OnConsentReceived(string cmpString, JObject fullJson)`  
- `OnConsentLayerOpened` / `OnConsentLayerClosed`  
- `OnError(string message)`  

---

## Demo scene

`Assets/ConsentManagerSDK/Demo/Scenes/CMPDemoScene.unity` contains:
- Auto-generated full-screen UI with scrollable buttons for **every** public API call.  
- Live status panel showing consent summary and Google Consent Mode values.  

Use this scene to validate flows and to see sample code in `CMPDemoController.cs`.  

---

## Platform specifics

### iOS
- Uses WKWebView via `unity-webview`.  
- ATT helper button (`Cycle ATT Status`) toggles values 0–3 for testing.  
- All WebView interactions happen before other SDKs because `CheckAndOpenAsync` is invoked during bootstrap.  

### Android
- Requires internet permission in your manifest (added automatically for Unity Player).  
- The plugin copies `.aar` templates from `Assets/ConsentManagerSDK/Plugins/unity-webview/Android`.  
- On build the postprocessor injects AndroidX dependencies and manifest entries.  

---

## Troubleshooting

| Symptom | Fix |
| --- | --- |
| `net::ERR_CACHE_MISS` when forcing open repeatedly | Ensure internet permission is present and allow the bootstrap `CheckAndOpenAsync` to complete before another force open. |
| Buttons not visible in demo | Make sure you are using the provided demo scene; any legacy serialized references will be destroyed at runtime so the overlay can rebuild itself. |
| Build fails complaining about placeholders | Update `CMPSettings.asset` with your real CMP ID/domain. |
| First consent load is slow on iOS | Expected—the bootstrap warm-up creates the WebView and runs `CheckAndOpenAsync`. Subsequent calls use the warmed WebKit process. |

---

## Migration from native SDKs

- Storage keys (`consentJson`, `consentString`, etc.) match Android `SharedPreferences` and iOS `NSUserDefaults`.  
- Remove old native bridge plugins before importing this package.  
- Replace legacy callbacks with the async equivalents (see the “Quick start” section).  
- If you used custom layouts on native, port the configuration values into `CMPUIConfig` or adjust the native html.  

---

## Support

- Documentation & Help Center: **https://help.consentmanager.net**  
- Dashboard: **https://www.consentmanager.net**  
- Email: **support@consentmanager.net**  
- Issues & feature requests can be filed via your consentmanager support channel.  

© 2024 consentmanager. All rights reserved.
