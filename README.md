# Consent Manager SDK v4 for Unity

Pure C# Consent Management Platform SDK for Unity with modern async/await API. Built on unity-webview, this SDK provides GDPR, CCPA, and privacy compliance for iOS and Android without native dependencies.

## Features

- ✨ **Pure C#** - No native iOS/Android bridges required
- 🚀 **Modern API** - Async/await throughout
- 📱 **Cross-Platform** - iOS and Android support
- 💾 **Storage Compatible** - Reads/writes same storage as native SDKs
- 🎨 **Customizable UI** - Flexible layout and styling options
- ⚡ **Offline-First** - Local consent queries without network calls
- 🔄 **Google Consent Mode v2** - Built-in support

## Requirements

- Unity 2020.3 or newer
- iOS 12.0+ (uses WKWebView)
- Android 5.0+ (API 21+)
- Newtonsoft.Json (Json.NET for Unity)

## Installation

### Option 1: Unity Package Manager (Recommended)

1. Open Unity Package Manager (Window > Package Manager)
2. Click "+" and select "Add package from disk"
3. Navigate to `Assets/ConsentManagerSDK/package.json`
4. Click "Open"

### Option 2: Direct Import

1. Copy the `Assets/ConsentManagerSDK` folder to your project's Assets directory
2. Ensure Newtonsoft.Json is installed (available on Unity Asset Store)

## Quick Start

### 1. Initialize the SDK

```csharp
using ConsentManagerSDK;

public class GameManager : MonoBehaviour
{
    async void Start()
    {
        // Configure CMP
        var config = new CMPConfig(
            id: "your-cmp-id",
            domain: "delivery.consentmanager.net",
            language: "EN",
            appName: "Your Game"
        );

        // Configure UI (optional)
        var uiConfig = CMPUIConfig.BottomHalf(); // Recommended for games

        // Initialize
        try
        {
            await CMPManager.Instance.InitializeAsync(config, uiConfig);
            Debug.Log("CMP initialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"CMP initialization failed: {e.Message}");
        }
    }
}
```

### 2. Check and Show Consent Layer

```csharp
// Check if consent is needed and show layer if required
await CMPManager.Instance.CheckAndOpenAsync();

// Or force open the consent layer
await CMPManager.Instance.ForceOpenAsync();
```

### 3. Access Consent Status (Offline)

```csharp
// Check specific purpose
ConsentStatus analyticsStatus = CMPManager.Instance.GetStatusForPurpose("analytics");
if (analyticsStatus == ConsentStatus.Granted)
{
    // Enable analytics
}

// Check specific vendor
ConsentStatus googleStatus = CMPManager.Instance.GetStatusForVendor("s2789");

// Get full user status
CMPUserStatusResponse userStatus = CMPManager.Instance.GetUserStatus();

// Get Google Consent Mode status
Dictionary<string, string> gcm = CMPManager.Instance.GetGoogleConsentModeStatus();
```

### 4. Programmatic Consent Management

```csharp
// Accept all consents
await CMPManager.Instance.AcceptAllAsync();

// Reject all consents
await CMPManager.Instance.RejectAllAsync();

// Accept specific purposes
await CMPManager.Instance.AcceptPurposesAsync(
    new[] { "analytics", "marketing" },
    updateVendors: true
);

// Accept specific vendors
await CMPManager.Instance.AcceptVendorsAsync(new[] { "s2789", "s2790" });
```

### 5. Listen to Events

```csharp
void Start()
{
    var cmp = CMPManager.Instance;
    
    cmp.OnConsentReceived += (consent, json) => 
    {
        Debug.Log("Consent received");
        // Update your analytics/tracking based on new consent
    };
    
    cmp.OnConsentLayerOpened += () => 
    {
        Debug.Log("Consent layer opened");
        // Pause game, etc.
    };
    
    cmp.OnConsentLayerClosed += () => 
    {
        Debug.Log("Consent layer closed");
        // Resume game, etc.
    };
    
    cmp.OnError += (error) => 
    {
        Debug.LogError($"CMP Error: {error}");
    };
}
```

## UI Configuration

### Layout Styles

```csharp
// Full screen (default for consent forms)
var uiConfig = CMPUIConfig.FullScreen();

// Top half of screen
var uiConfig = CMPUIConfig.TopHalf();

// Bottom half of screen (recommended for games)
var uiConfig = CMPUIConfig.BottomHalf();
```

### Custom Styling

```csharp
var uiConfig = new CMPUIConfig(CMPLayoutStyle.BottomHalf)
{
    Background = CMPBackgroundType.Dimmed,
    BackgroundColor = Color.black,
    BackgroundAlpha = 0.7f,
    CornerRadius = 16f,
    DarkMode = true,
    RespectsSafeArea = true,
    AllowsOrientationChanges = true
};
```

## Platform-Specific Configuration

### iOS

The SDK uses WKWebView and requires network permissions. The unity-webview plugin automatically handles `Info.plist` modifications.

For ATT (App Tracking Transparency) integration:

```csharp
#if UNITY_IOS
using UnityEngine.iOS;

// Get ATT status
var attStatus = (int)Device.RequestStoreReview(); // Use proper ATT framework
CMPManager.Instance.SetATTStatus(attStatus);
#endif
```

### Android

The SDK uses standard WebView with hardware acceleration. Ensure your `AndroidManifest.xml` has:

```xml
<application android:hardwareAccelerated="true">
```

For API 28+, cleartext traffic:

```xml
<application android:usesCleartextTraffic="true">
```

## Advanced Usage

### Import/Export Consent

```csharp
// Export consent string (for backup or transfer)
string consentString = CMPManager.Instance.ExportCMPInfo();
PlayerPrefs.SetString("consent_backup", consentString);

// Import consent string (restore from backup)
string savedConsent = PlayerPrefs.GetString("consent_backup");
if (!string.IsNullOrEmpty(savedConsent))
{
    await CMPManager.Instance.ImportCMPInfoAsync(savedConsent);
}
```

### Reset Consent

```csharp
// Clear all consent data
await CMPManager.Instance.ResetConsentManagementDataAsync();
```

### Google Consent Mode Integration

```csharp
// Get consent mode status for Firebase Analytics, Google Ads, etc.
var consentMode = CMPManager.Instance.GetGoogleConsentModeStatus();

// Keys: analytics_storage, ad_storage, ad_user_data, ad_personalization
// Values: "granted" or "denied"

// Update Firebase Analytics
#if UNITY_ANDROID || UNITY_IOS
Firebase.Analytics.FirebaseAnalytics.SetConsent(new Dictionary<string, string>
{
    { Firebase.Analytics.ConsentType.AnalyticsStorage.ToString(), 
      consentMode["analytics_storage"] },
    { Firebase.Analytics.ConsentType.AdStorage.ToString(), 
      consentMode["ad_storage"] }
});
#endif
```

## Storage Compatibility

This SDK uses the same storage keys and format as the native iOS and Android SDKs:

- **iOS**: `NSUserDefaults` (via `PlayerPrefs`)
  - Keys: `consentJson`, `consentString`, `consentMetadata`
- **Android**: `SharedPreferences` (default preferences)
  - Keys: `consentJson`, `consentString`, individual metadata keys

This means you can migrate from native SDKs to v4 without losing consent data.

## API Reference

### CMPManager

Main singleton manager for SDK operations.

#### Initialization

- `Task InitializeAsync(CMPConfig, CMPUIConfig)` - Initialize SDK

#### Network Operations (async)

- `Task CheckAndOpenAsync(bool jumpToSettings)` - Check and show if needed
- `Task ForceOpenAsync(bool jumpToSettings)` - Force show consent layer
- `Task AcceptAllAsync()` - Accept all consents
- `Task RejectAllAsync()` - Reject all consents
- `Task AcceptPurposesAsync(string[], bool)` - Accept specific purposes
- `Task RejectPurposesAsync(string[], bool)` - Reject specific purposes
- `Task AcceptVendorsAsync(string[])` - Accept specific vendors
- `Task RejectVendorsAsync(string[])` - Reject specific vendors
- `Task ImportCMPInfoAsync(string)` - Import consent string
- `Task ResetConsentManagementDataAsync()` - Clear all data

#### Offline Data Access (sync)

- `ConsentStatus GetStatusForPurpose(string)` - Get purpose consent status
- `ConsentStatus GetStatusForVendor(string)` - Get vendor consent status
- `CMPUserStatusResponse GetUserStatus()` - Get complete user status
- `Dictionary<string, string> GetGoogleConsentModeStatus()` - Get GCM status
- `string ExportCMPInfo()` - Export consent string

#### Configuration

- `void SetATTStatus(int)` - Set iOS ATT status
- `void UpdateUIConfig(CMPUIConfig)` - Update UI configuration

#### Events

- `event Action<string, JObject> OnConsentReceived` - Consent received
- `event Action OnConsentLayerOpened` - Layer opened
- `event Action OnConsentLayerClosed` - Layer closed
- `event Action<string> OnError` - Error occurred

## Troubleshooting

### WebView not showing

- Ensure you've called `InitializeAsync()` before other operations
- Check that `CMPConfig` is valid (ID, domain, language, appName)
- Verify network connectivity
- Check Unity console for errors

### Consent data not persisting

- On iOS: Ensure `PlayerPrefs.Save()` permissions
- On Android: Ensure storage permissions if needed
- Check that consent is actually being saved (listen to `OnConsentReceived`)

### Build errors

- Ensure Newtonsoft.Json is installed
- Verify unity-webview plugin files are present
- Check platform-specific build settings (iOS: WKWebView, Android: hardware acceleration)

## Migration from v3

If migrating from the native bridge SDK (v3):

1. Remove old native bridge files:
   - `Plugins/iOS/ConsentManagerBridge.*`
   - `Plugins/Android/ConsentManagerBridge.java`

2. Update initialization:
   ```csharp
   // Old v3
   ConsentManager.Instance.Initialize(id, domain, language, appName);
   
   // New v4
   await CMPManager.Instance.InitializeAsync(config, uiConfig);
   ```

3. Replace callbacks with async/await:
   ```csharp
   // Old v3
   ConsentManager.Instance.CheckAndOpen(jumpToSettings: false);
   
   // New v4
   await CMPManager.Instance.CheckAndOpenAsync(jumpToSettings: false);
   ```

4. Consent data is automatically preserved (same storage format)

## Support

- Documentation: [https://help.consentmanager.net](https://help.consentmanager.net)
- Dashboard: [https://www.consentmanager.net](https://www.consentmanager.net)
- Email: support@consentmanager.net

## License

Copyright © 2024 Consent Manager. All rights reserved.

## Changelog

### Version 4.0.0
- Initial release of pure C# SDK
- Modern async/await API
- Cross-platform support (iOS, Android)
- Storage compatibility with native SDKs
- Google Consent Mode v2 support
- Offline-first consent queries

