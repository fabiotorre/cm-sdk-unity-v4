using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Main singleton manager for Consent Management Platform SDK
    /// Pure C# implementation using unity-webview
    /// </summary>
    public class CMPManager : MonoBehaviour
    {
        private static CMPManager _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static CMPManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            GameObject go = new GameObject("CMPManager");
                            _instance = go.AddComponent<CMPManager>();
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return _instance;
            }
        }

        #region Private Fields

        private CMPConfig _config;
        private CMPUIConfig _uiConfig;
        private ICMPStorage _storage;
        private CMPWebViewManager _webViewManager;
        private CMPWebViewController _webViewController;
        private int _attStatus = 0;
        private bool _isInitialized;

        #endregion

        #region Events

        /// <summary>
        /// Fired when consent is received from user
        /// </summary>
        public event Action<string, JObject> OnConsentReceived;

        /// <summary>
        /// Fired when consent layer is opened
        /// </summary>
        public event Action OnConsentLayerOpened;

        /// <summary>
        /// Fired when consent layer is closed
        /// </summary>
        public event Action OnConsentLayerClosed;

        /// <summary>
        /// Fired on error
        /// </summary>
        public event Action<string> OnError;

        #endregion

        public bool IsInitialized => _isInitialized;

        #region Initialization

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeStorage();
        }

        private void InitializeStorage()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _storage = new CMPStorageIOS();
            Debug.Log("[CMPManager] Initialized iOS storage");
#elif UNITY_ANDROID && !UNITY_EDITOR
            _storage = new CMPStorageAndroid();
            Debug.Log("[CMPManager] Initialized Android storage");
#else
            _storage = new CMPStorageIOS(); // Use PlayerPrefs-based storage for Editor
            Debug.Log("[CMPManager] Initialized Editor storage (PlayerPrefs)");
#endif
        }

        /// <summary>
        /// Initializes the SDK with configuration
        /// </summary>
        /// <param name="config">CMP configuration</param>
        /// <param name="uiConfig">UI configuration (optional, defaults to BottomHalf)</param>
        public async Task InitializeAsync(CMPConfig config, CMPUIConfig uiConfig = null)
        {
            if (config == null || !config.IsValid())
            {
                throw new ArgumentException("Invalid CMP configuration");
            }

            _config = config;
            _uiConfig = uiConfig ?? new CMPUIConfig();

            Debug.Log($"[CMPManager] Initializing with ID={config.Id}, Domain={config.Domain}");

            // Initialize WebView manager
            if (_webViewManager == null)
            {
                _webViewManager = new CMPWebViewManager(this);
                _webViewManager.OnConsentReceived += HandleConsentReceived;
                _webViewManager.OnOpenSignal += HandleOpenSignal;
                _webViewManager.OnCloseSignal += HandleCloseSignal;
                _webViewManager.OnError += HandleError;
            }

            _webViewManager.Initialize(_uiConfig, _config.JsonConfig);

            // Initialize view controller
            if (_webViewController == null)
            {
                _webViewController = new CMPWebViewController(_webViewManager, _uiConfig);
            }

            await Task.CompletedTask;
            _isInitialized = true;
            Debug.Log("[CMPManager] Initialization complete");
        }

        #endregion

        #region Public API - Offline-First Data Access

        /// <summary>
        /// Gets consent status for a specific purpose (offline-first)
        /// </summary>
        public ConsentStatus GetStatusForPurpose(string purposeId)
        {
            var model = GetConsentModel();
            return model?.GetStatusForPurpose(purposeId) ?? ConsentStatus.ChoiceDoesntExist;
        }

        /// <summary>
        /// Gets consent status for a specific vendor (offline-first)
        /// </summary>
        public ConsentStatus GetStatusForVendor(string vendorId)
        {
            var model = GetConsentModel();
            return model?.GetStatusForVendor(vendorId) ?? ConsentStatus.ChoiceDoesntExist;
        }

        /// <summary>
        /// Gets comprehensive user status
        /// </summary>
        public CMPUserStatusResponse GetUserStatus()
        {
            var model = GetConsentModel();
            return model?.ToUserStatusResponse() ?? new CMPUserStatusResponse();
        }

        /// <summary>
        /// Gets Google Consent Mode status
        /// </summary>
        public Dictionary<string, string> GetGoogleConsentModeStatus()
        {
            var model = GetConsentModel();
            var consentMode = model?.ConsentMode;

            var result = new Dictionary<string, string>
            {
                { "analytics_storage", "denied" },
                { "ad_storage", "denied" },
                { "ad_user_data", "denied" },
                { "ad_personalization", "denied" }
            };

            if (consentMode != null)
            {
                foreach (var kvp in consentMode)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Exports CMP consent string (offline-first)
        /// </summary>
        public string ExportCMPInfo()
        {
            var model = GetConsentModel();
            return model?.ExportCMPInfo() ?? string.Empty;
        }

        #endregion

        #region Public API - Network Operations

        /// <summary>
        /// Checks consent status and opens consent layer if needed
        /// </summary>
        public async Task CheckAndOpenAsync(bool jumpToSettings = false)
        {
            ValidateInitialization();

            var additionalParams = new Dictionary<string, object>
            {
                { "jumpToSettings", jumpToSettings }
            };

            await LoadConsentURLWithUseCaseAsync(UseCase.VerifyConsentOnInitialize, additionalParams);
        }

        /// <summary>
        /// Forces consent layer to open
        /// </summary>
        public async Task ForceOpenAsync(bool jumpToSettings = false)
        {
            ValidateInitialization();

            var additionalParams = new Dictionary<string, object>
            {
                { "jumpToSettings", jumpToSettings }
            };

            await LoadConsentURLWithUseCaseAsync(UseCase.OpenConsent, additionalParams);
        }

        /// <summary>
        /// Accepts specific vendors
        /// </summary>
        public async Task AcceptVendorsAsync(string[] vendors)
        {
            ValidateInitialization();

            var additionalParams = new Dictionary<string, object>
            {
                { "addVendors", new List<string>(vendors) }
            };

            await LoadConsentURLWithUseCaseAsync(UseCase.EnableConsentVendors, additionalParams);
        }

        /// <summary>
        /// Rejects specific vendors
        /// </summary>
        public async Task RejectVendorsAsync(string[] vendors)
        {
            ValidateInitialization();

            var additionalParams = new Dictionary<string, object>
            {
                { "addVendors", new List<string>(vendors) }
            };

            await LoadConsentURLWithUseCaseAsync(UseCase.DisableConsentVendors, additionalParams);
        }

        /// <summary>
        /// Accepts specific purposes
        /// </summary>
        public async Task AcceptPurposesAsync(string[] purposes, bool updateVendors = false)
        {
            ValidateInitialization();

            var additionalParams = new Dictionary<string, object>
            {
                { "addPurposes", new List<string>(purposes) },
                { "updateVendors", updateVendors }
            };

            await LoadConsentURLWithUseCaseAsync(UseCase.EnableConsentPurposes, additionalParams);
        }

        /// <summary>
        /// Rejects specific purposes
        /// </summary>
        public async Task RejectPurposesAsync(string[] purposes, bool updateVendors = false)
        {
            ValidateInitialization();

            var additionalParams = new Dictionary<string, object>
            {
                { "addPurposes", new List<string>(purposes) },
                { "updateVendors", updateVendors }
            };

            await LoadConsentURLWithUseCaseAsync(UseCase.DisableConsentPurposes, additionalParams);
        }

        /// <summary>
        /// Accepts all consents
        /// </summary>
        public async Task AcceptAllAsync()
        {
            ValidateInitialization();
            await LoadConsentURLWithUseCaseAsync(UseCase.AcceptAllConsent);
        }

        /// <summary>
        /// Rejects all consents
        /// </summary>
        public async Task RejectAllAsync()
        {
            ValidateInitialization();
            await LoadConsentURLWithUseCaseAsync(UseCase.RejectAllConsent);
        }

        /// <summary>
        /// Imports CMP consent string
        /// </summary>
        public async Task ImportCMPInfoAsync(string cmpString)
        {
            ValidateInitialization();

            var additionalParams = new Dictionary<string, object>
            {
                { "consent", cmpString }
            };

            await LoadConsentURLWithUseCaseAsync(UseCase.ImportConsent, additionalParams);
        }

        /// <summary>
        /// Resets all consent management data
        /// </summary>
        public Task ResetConsentManagementDataAsync()
        {
            _storage?.ResetConsentData();
            Debug.Log("[CMPManager] Consent data reset");
            return Task.CompletedTask;
        }

        #endregion

        #region Public API - Configuration

        /// <summary>
        /// Sets ATT (App Tracking Transparency) status for iOS
        /// </summary>
        public void SetATTStatus(int status)
        {
            _attStatus = status;
            Debug.Log($"[CMPManager] ATT status set to: {status}");
        }

        /// <summary>
        /// Updates UI configuration
        /// </summary>
        public void UpdateUIConfig(CMPUIConfig uiConfig)
        {
            _uiConfig = uiConfig ?? _uiConfig;
            _webViewController?.UpdateConfig(_uiConfig);
        }

        #endregion

        #region Internal Methods

        private void ValidateInitialization()
        {
            if (!_isInitialized || _config == null || _webViewManager == null)
            {
                throw new InvalidOperationException("CMPManager not initialized. Call InitializeAsync first.");
            }
        }

        private async Task LoadConsentURLWithUseCaseAsync(UseCase useCase, Dictionary<string, object> additionalParams = null)
        {
            var model = GetConsentModel();

            var urlParams = new CMPUrlBuilder.CMPUrlParams
            {
                Id = _config.Id,
                Domain = _config.Domain,
                UseCase = useCase,
                Consent = model?.ExportCMPInfo(),
                Language = _config.Language,
                AppName = _config.AppName,
                PackageName = Application.identifier,
                BundleID = Application.identifier,
                AttStatus = _attStatus,
                DarkMode = _uiConfig.DarkMode,
                NoHash = _config.NoHash
            };

            // Apply additional parameters
            if (additionalParams != null)
            {
                urlParams.Apply(additionalParams);
            }

            string url = CMPUrlBuilder.Build(urlParams);

            Debug.Log($"[CMPManager] Loading URL for use case: {useCase}");

            bool needsUI = await _webViewManager.LoadConsentURLAsync(
                url,
                useCase,
                urlParams.Consent,
                urlParams.NoHash
            );

            // Some use cases don't show UI
            if (!needsUI)
            {
                Debug.Log("[CMPManager] Operation completed without UI");
            }
        }

        private CMPConsentModel GetConsentModel()
        {
            var (json, consentString, metadata) = _storage.GetConsentData();
            
            if (string.IsNullOrEmpty(json))
                return null;

            return CMPConsentModel.FromJson(json);
        }

        private void HandleConsentReceived(string consentString, JObject jsonObject)
        {
            try
            {
                Debug.Log("[CMPManager] Processing received consent");

                // Parse consent model
                var model = CMPConsentModel.FromJson(jsonObject.ToString());
                
                if (model != null)
                {
                    // Extract metadata
                    var metadataList = new List<CMPMetadata>();
                    if (model.Metadata != null)
                    {
                        metadataList.AddRange(model.Metadata);
                    }

                    // Save to storage
                    _storage.SaveConsentData(
                        jsonObject.ToString(),
                        consentString,
                        metadataList.ToArray()
                    );

                    Debug.Log("[CMPManager] Consent saved successfully");
                }

                // Fire event
                OnConsentReceived?.Invoke(consentString, jsonObject);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CMPManager] Error processing consent: {e.Message}");
                OnError?.Invoke($"Consent processing error: {e.Message}");
            }
        }

        private void HandleOpenSignal()
        {
            Debug.Log("[CMPManager] Open signal received - showing consent layer");
            _webViewController?.Show();
            OnConsentLayerOpened?.Invoke();
        }

        private void HandleCloseSignal()
        {
            Debug.Log("[CMPManager] Close signal received - hiding consent layer");
            _webViewController?.Hide();
            OnConsentLayerClosed?.Invoke();
        }

        private void HandleError(string error)
        {
            Debug.LogError($"[CMPManager] Error: {error}");
            OnError?.Invoke(error);
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _webViewManager?.Destroy();
                _webViewController?.Destroy();
                _instance = null;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && _webViewManager != null)
            {
                // App resumed - reapply layout in case orientation changed
                _webViewManager.ApplyWebViewLayout();
            }
        }

        #endregion
    }
}
