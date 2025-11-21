using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ConsentManagerSDK;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Demo controller showcasing CMP SDK usage
/// </summary>
public class CMPDemoController : MonoBehaviour
{
    [Header("CMP Configuration")]
    [SerializeField] private string cmpId = "your-cmp-id";
    [SerializeField] private string cmpDomain = "delivery.consentmanager.net";
    [SerializeField] private string language = "EN";
    [SerializeField] private string appName = "Unity Demo App";

    [Header("UI Configuration")]
    [SerializeField] private CMPLayoutStyle layoutStyle = CMPLayoutStyle.BottomHalf;
    [SerializeField] private bool darkMode = false;

    [Header("UI References (Optional)")]
    [SerializeField] private Text statusText;
    [SerializeField] private Text consentStatusText;
    [SerializeField] private Button checkAndOpenButton;
    [SerializeField] private Button forceOpenButton;
    [SerializeField] private Button acceptAllButton;
    [SerializeField] private Button rejectAllButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button exportButton;
    [SerializeField] private Button getStatusButton;

    private CMPManager _cmpManager;

    private void Start()
    {
        _cmpManager = CMPManager.Instance;

        EnsureDemoUI();

        // Subscribe to events
        _cmpManager.OnConsentReceived += OnConsentReceived;
        _cmpManager.OnConsentLayerOpened += OnConsentLayerOpened;
        _cmpManager.OnConsentLayerClosed += OnConsentLayerClosed;
        _cmpManager.OnError += OnError;

        Log("CMP ready. Use the buttons below to interact.");
    }

    #region Button Handlers

    private void EnsureDemoUI()
    {
        if (statusText != null && consentStatusText != null && checkAndOpenButton != null)
            return;

        var existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas == null)
        {
            var canvasGO = new GameObject("CMP Demo Canvas");
            existingCanvas = canvasGO.AddComponent<Canvas>();
            existingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        if (FindObjectOfType<EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        var panel = new GameObject("CMP Demo Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(existingCanvas.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.05f, 0.05f);
        rect.anchorMax = new Vector2(0.95f, 0.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.padding = new RectOffset(20, 20, 20, 20);

        statusText = CreateText(panel.transform, "Status Text", "Status output will appear here");
        consentStatusText = CreateText(panel.transform, "Consent Status Text", "Consent status will appear here");

        checkAndOpenButton = CreateButton(panel.transform, "Check & Open", () => CheckAndOpenAsync());
        forceOpenButton = CreateButton(panel.transform, "Force Open", () => ForceOpenAsync());
        acceptAllButton = CreateButton(panel.transform, "Accept All", () => AcceptAllAsync());
        rejectAllButton = CreateButton(panel.transform, "Reject All", () => RejectAllAsync());
        resetButton = CreateButton(panel.transform, "Reset Data", () => ResetAsync());
        exportButton = CreateButton(panel.transform, "Export Consent", () => ExportConsent());
        getStatusButton = CreateButton(panel.transform, "Get Status", () => GetUserStatus());
    }

    private Text CreateText(Transform parent, string name, string initialValue)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var textComponent = go.GetComponent<Text>();
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.text = initialValue;
        textComponent.color = Color.white;
        return textComponent;
    }

    private Button CreateButton(Transform parent, string label, UnityAction onClick)
    {
        var buttonGO = new GameObject($"{label} Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);

        var image = buttonGO.GetComponent<Image>();
        image.color = new Color(0.1f, 0.4f, 0.8f, 0.8f);

        var textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(buttonGO.transform, false);
        var text = textGO.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
        text.color = Color.white;

        var rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var button = buttonGO.GetComponent<Button>();
        button.onClick.AddListener(onClick);
        return button;
    }

    private async void CheckAndOpenAsync()
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Checking consent status...");
            await _cmpManager.CheckAndOpenAsync();
            Log("✓ Check complete");
        }
        catch (Exception e)
        {
            LogError($"✗ Check failed: {e.Message}");
        }
    }

    private async void ForceOpenAsync()
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Opening consent layer...");
            await _cmpManager.ForceOpenAsync();
            Log("✓ Consent layer opened");
        }
        catch (Exception e)
        {
            LogError($"✗ Failed to open: {e.Message}");
        }
    }

    private async void AcceptAllAsync()
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Accepting all consents...");
            await _cmpManager.AcceptAllAsync();
            Log("✓ All consents accepted");
        }
        catch (Exception e)
        {
            LogError($"✗ Accept all failed: {e.Message}");
        }
    }

    private async void RejectAllAsync()
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Rejecting all consents...");
            await _cmpManager.RejectAllAsync();
            Log("✓ All consents rejected");
        }
        catch (Exception e)
        {
            LogError($"✗ Reject all failed: {e.Message}");
        }
    }

    private async void ResetAsync()
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Resetting consent data...");
            await _cmpManager.ResetConsentManagementDataAsync();
            Log("✓ Consent data reset");
            UpdateConsentStatus();
        }
        catch (Exception e)
        {
            LogError($"✗ Reset failed: {e.Message}");
        }
    }

    private void ExportConsent()
    {
        if (!CheckInitialized()) return;

        string consentString = _cmpManager.ExportCMPInfo();
        if (!string.IsNullOrEmpty(consentString))
        {
            Log($"✓ Exported: {consentString.Substring(0, Math.Min(50, consentString.Length))}...");
        }
        else
        {
            Log("No consent data to export");
        }
    }

    private void GetUserStatus()
    {
        if (!CheckInitialized()) return;

        var userStatus = _cmpManager.GetUserStatus();
        
        string status = $"User Choice: {userStatus.Status}\n";
        status += $"Vendors: {userStatus.Vendors.Count}\n";
        status += $"Purposes: {userStatus.Purposes.Count}\n";
        status += $"Regulation: {userStatus.Regulation}";

        Log(status);
        UpdateConsentStatus();
    }

    #endregion

    #region Event Handlers

    private void OnConsentReceived(string consent, Newtonsoft.Json.Linq.JObject jsonObject)
    {
        Log($"✓ Consent received: {consent.Substring(0, Math.Min(30, consent.Length))}...");
        UpdateConsentStatus();
    }

    private void OnConsentLayerOpened()
    {
        Log("→ Consent layer opened");
    }

    private void OnConsentLayerClosed()
    {
        Log("← Consent layer closed");
        UpdateConsentStatus();
    }

    private void OnError(string error)
    {
        LogError($"✗ Error: {error}");
    }

    #endregion

    #region Helpers

    bool CheckInitialized()
    {
        if (_cmpManager == null || !_cmpManager.IsInitialized)
        {
            Log("⚠ CMP SDK is still initializing. Please wait...");
            return false;
        }
        return true;
    }

    void UpdateConsentStatus()
    {
        if (_cmpManager == null || !_cmpManager.IsInitialized || consentStatusText == null)
            return;

        var userStatus = _cmpManager.GetUserStatus();
        var gcmStatus = _cmpManager.GetGoogleConsentModeStatus();

        string status = $"<b>Consent Status:</b>\n";
        status += $"Choice Made: {userStatus.Status}\n";
        status += $"Vendors: {userStatus.Vendors.Count}\n";
        status += $"Purposes: {userStatus.Purposes.Count}\n\n";
        status += $"<b>Google Consent Mode:</b>\n";
        
        foreach (var kvp in gcmStatus)
        {
            status += $"{kvp.Key}: {kvp.Value}\n";
        }

        consentStatusText.text = status;
    }

    void Log(string message)
    {
        Debug.Log($"[CMP Demo] {message}");
        if (statusText != null)
        {
            statusText.text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }
    }

    void LogError(string message)
    {
        Debug.LogError($"[CMP Demo] {message}");
        if (statusText != null)
        {
            statusText.text = $"<color=red>{DateTime.Now:HH:mm:ss} - {message}</color>";
        }
    }

    #endregion

    void OnDestroy()
    {
        // Unsubscribe from events
        if (_cmpManager != null)
        {
            _cmpManager.OnConsentReceived -= OnConsentReceived;
            _cmpManager.OnConsentLayerOpened -= OnConsentLayerOpened;
            _cmpManager.OnConsentLayerClosed -= OnConsentLayerClosed;
            _cmpManager.OnError -= OnError;
        }
    }
}
