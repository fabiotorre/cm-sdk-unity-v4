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

    private CMPManager _cmpManager;
    private Transform _buttonContent;
    private readonly string[] _sampleVendors = { "s2790", "s2791" };
    private readonly string[] _samplePurposes = { "c52", "c53" };
    private const string SampleImportString = "Q1FaRWVQQVFaRWVQQUFmYjRCRU5DQUZnQVBMQUFFTEFBQWlnRjV3QVFGNWdYbkFCQVhtQUFBI181MV81Ml81NF8jX3MxMDUyX3MxX3MyNl9zMjYxMl9zOTA1X3MxNDQ4X2M3MzczN19VXyMxLS0tIw";
    private int _currentAttStatus;

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
        if (_buttonContent != null)
            return;

        if (statusText != null)
        {
            Destroy(statusText.gameObject);
            statusText = null;
        }
        if (consentStatusText != null)
        {
            Destroy(consentStatusText.gameObject);
            consentStatusText = null;
        }

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
        rect.anchorMax = new Vector2(0.95f, 0.95f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.35f);

        var panelLayout = panel.AddComponent<VerticalLayoutGroup>();
        panelLayout.spacing = 20f;
        panelLayout.padding = new RectOffset(20, 20, 20, 20);
        panelLayout.childAlignment = TextAnchor.UpperLeft;
        panelLayout.childControlHeight = true;
        panelLayout.childControlWidth = true;
        panelLayout.childForceExpandHeight = true;

        var statusContainer = new GameObject("Status Container", typeof(RectTransform), typeof(VerticalLayoutGroup));
        statusContainer.transform.SetParent(panel.transform, false);
        var statusLayout = statusContainer.GetComponent<VerticalLayoutGroup>();
        statusLayout.spacing = 10f;
        statusLayout.childAlignment = TextAnchor.UpperLeft;
        statusLayout.childControlWidth = true;
        statusLayout.childControlHeight = true;
        statusLayout.childForceExpandHeight = false;
        var statusLayoutElement = statusContainer.AddComponent<LayoutElement>();
        statusLayoutElement.preferredHeight = 140f;

        statusText = CreateText(statusContainer.transform, "Status Text", "Status output will appear here");
        consentStatusText = CreateText(statusContainer.transform, "Consent Status Text", "Consent status will appear here");

        var buttonArea = new GameObject("Button Area", typeof(RectTransform));
        buttonArea.transform.SetParent(panel.transform, false);
        var buttonsLayoutElement = buttonArea.AddComponent<LayoutElement>();
        buttonsLayoutElement.preferredHeight = 500f;

        var scrollView = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollView.transform.SetParent(buttonArea.transform, false);
        var scrollRect = scrollView.GetComponent<ScrollRect>();
        var scrollRectTransform = scrollView.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        viewport.transform.SetParent(scrollView.transform, false);
        var viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        var contentLayout = content.GetComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 10f;
        contentLayout.padding = new RectOffset(10, 10, 10, 10);
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        var fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        _buttonContent = content.transform;
        AddDemoButtons();
    }

    private Text CreateText(Transform parent, string name, string initialValue)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var textComponent = go.GetComponent<Text>();
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = Mathf.RoundToInt(Mathf.Max(32f, Screen.width * 0.03f));
        textComponent.text = initialValue;
        textComponent.alignment = TextAnchor.UpperLeft;
        textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;
        textComponent.color = Color.white;
        var layout = go.AddComponent<LayoutElement>();
        layout.minHeight = 80f;
        layout.flexibleHeight = 1f;
        return textComponent;
    }

    private Button CreateButton(Transform parent, string label, UnityAction onClick)
    {
        var buttonGO = new GameObject($"{label} Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);

        var image = buttonGO.GetComponent<Image>();
        image.color = new Color(0.1f, 0.4f, 0.8f, 0.8f);
        var layout = buttonGO.AddComponent<LayoutElement>();
        layout.minHeight = 90f;
        layout.flexibleHeight = 0f;

        var textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(buttonGO.transform, false);
        var text = textGO.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = Mathf.RoundToInt(Mathf.Max(32f, Screen.width * 0.035f));
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

    private void AddDemoButtons()
    {
        CreateButton(_buttonContent, "Refresh Status", () => UpdateConsentStatus());
        CreateButton(_buttonContent, "Has Purpose ID c53?", CheckPurposeStatus);
        CreateButton(_buttonContent, "Has Vendor ID s2789?", CheckVendorStatus);
        CreateButton(_buttonContent, "Export CMP String", ExportConsent);
        CreateButton(_buttonContent, "Import CMP String", () => ImportConsentAsync());
        CreateButton(_buttonContent, "Check & Open", () => CheckAndOpenAsync());
        CreateButton(_buttonContent, "Force Open", () => ForceOpenAsync());
        CreateButton(_buttonContent, "Force Open (Jump to Settings)", () => ForceOpenAsync(true));
        CreateButton(_buttonContent, "Accept All", () => AcceptAllAsync());
        CreateButton(_buttonContent, "Reject All", () => RejectAllAsync());
        CreateButton(_buttonContent, "Enable Vendors s2790 / s2791", () => AcceptVendorsAsync());
        CreateButton(_buttonContent, "Disable Vendors s2790 / s2791", () => RejectVendorsAsync());
        CreateButton(_buttonContent, "Enable Purposes c52 / c53", () => AcceptPurposesAsync());
        CreateButton(_buttonContent, "Disable Purposes c52 / c53", () => RejectPurposesAsync());
        CreateButton(_buttonContent, "Reset Consent Data", () => ResetAsync());
        CreateButton(_buttonContent, "Google Consent Mode Status", ShowGoogleConsentMode);
        CreateButton(_buttonContent, "Inspect Stored Consent", InspectStoredData);
        CreateButton(_buttonContent, "Cycle ATT Status (iOS)", CycleAttStatus);
    }

    private async void AcceptVendorsAsync()
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Enabling vendors s2790 / s2791...");
            await _cmpManager.AcceptVendorsAsync(_sampleVendors);
            Log("✓ Vendors enabled.");
            UpdateConsentStatus();
        }
        catch (Exception e)
        {
            LogError($"✗ Failed to enable vendors: {e.Message}");
        }
    }

    private async void RejectVendorsAsync()
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Disabling vendors s2790 / s2791...");
            await _cmpManager.RejectVendorsAsync(_sampleVendors);
            Log("✓ Vendors disabled.");
            UpdateConsentStatus();
        }
        catch (Exception e)
        {
            LogError($"✗ Failed to disable vendors: {e.Message}");
        }
    }

    private async void AcceptPurposesAsync()
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Enabling purposes c52 / c53...");
            await _cmpManager.AcceptPurposesAsync(_samplePurposes, true);
            Log("✓ Purposes enabled.");
            UpdateConsentStatus();
        }
        catch (Exception e)
        {
            LogError($"✗ Failed to enable purposes: {e.Message}");
        }
    }

    private async void RejectPurposesAsync()
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Disabling purposes c52 / c53...");
            await _cmpManager.RejectPurposesAsync(_samplePurposes, true);
            Log("✓ Purposes disabled.");
            UpdateConsentStatus();
        }
        catch (Exception e)
        {
            LogError($"✗ Failed to disable purposes: {e.Message}");
        }
    }

    private async void ImportConsentAsync()
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Importing CMP string...");
            await _cmpManager.ImportCMPInfoAsync(SampleImportString);
            Log("✓ CMP string imported.");
            UpdateConsentStatus();
        }
        catch (Exception e)
        {
            LogError($"✗ Import failed: {e.Message}");
        }
    }

    private void ShowGoogleConsentMode()
    {
        if (!CheckInitialized()) return;

        var mode = _cmpManager.GetGoogleConsentModeStatus();
        var summary = "Google Consent Mode:";
        foreach (var kvp in mode)
        {
            summary += $"\n{kvp.Key}: {kvp.Value}";
        }
        Log(summary);
    }

    private void InspectStoredData()
    {
        if (!CheckInitialized()) return;

        string storedJson = PlayerPrefs.GetString("consentJson", "<none>");
        string storedString = PlayerPrefs.GetString("consentString", "<none>");
        string metadata = PlayerPrefs.GetString("consentMetadata", "<none>");

        Log($"Stored consent snapshot:\njson: {Shorten(storedJson)}\nstring: {Shorten(storedString)}\nmetadata: {Shorten(metadata)}");
    }

    private void CheckPurposeStatus()
    {
        if (!CheckInitialized()) return;
        var status = _cmpManager.GetStatusForPurpose("c53");
        Log($"Purpose c53: {status}");
    }

    private void CheckVendorStatus()
    {
        if (!CheckInitialized()) return;
        var status = _cmpManager.GetStatusForVendor("s2789");
        Log($"Vendor s2789: {status}");
    }

    private void CycleAttStatus()
    {
        _currentAttStatus = (_currentAttStatus + 1) % 4;
        _cmpManager.SetATTStatus(_currentAttStatus);
        Log($"ATT status set to {_currentAttStatus}");
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

    private async void ForceOpenAsync(bool jumpToSettings = false)
    {
        if (!CheckInitialized()) return;

        try
        {
            Log("Opening consent layer...");
            await _cmpManager.ForceOpenAsync(jumpToSettings);
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
            UpdateConsentStatus();
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
            UpdateConsentStatus();
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

    private string Shorten(string value)
    {
        if (string.IsNullOrEmpty(value) || value == "<none>")
            return value;

        return value.Length <= 60 ? value : $"{value.Substring(0, 60)}...";
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
