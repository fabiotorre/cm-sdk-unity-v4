using System;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Manages unity-webview lifecycle, URL loading, and JavaScript communication
    /// </summary>
    public class CMPWebViewManager
    {
        private WebViewObject _webView;
        private CMPJavaScriptBridge _jsBridge;
        private CMPUIConfig _uiConfig;
        private string _jsonConfig;
        private UseCase _currentUseCase;
        private TaskCompletionSource<bool> _operationCompletion;
        private Coroutine _timeoutCoroutine;
        private MonoBehaviour _coroutineRunner;

        /// <summary>
        /// Event fired when consent is received from WebView
        /// </summary>
        public event Action<string, JObject> OnConsentReceived;

        /// <summary>
        /// Event fired when WebView signals to open consent layer
        /// </summary>
        public event Action OnOpenSignal;

        /// <summary>
        /// Event fired when WebView signals to close consent layer
        /// </summary>
        public event Action OnCloseSignal;

        /// <summary>
        /// Event fired on WebView error
        /// </summary>
        public event Action<string> OnError;

        public CMPWebViewManager(MonoBehaviour coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
            _jsBridge = new CMPJavaScriptBridge();
            
            // Wire up JavaScript bridge events
            _jsBridge.OnConsentReceived += HandleConsentReceived;
            _jsBridge.OnOpenReceived += HandleOpenReceived;
            _jsBridge.OnErrorReceived += HandleErrorReceived;
        }

        /// <summary>
        /// Initializes WebView with configuration
        /// </summary>
        public void Initialize(CMPUIConfig uiConfig, string jsonConfig = null)
        {
            _uiConfig = uiConfig;
            _jsonConfig = jsonConfig;
            RecreateWebView();
        }

        private void EnsureWebView()
        {
            if (_webView == null)
            {
                RecreateWebView();
            }
        }

        private void RecreateWebView()
        {
            DisposeWebView();

            Debug.Log("[CMPWebViewManager] Creating WebView instance");

            _webView = new GameObject("CMPWebView").AddComponent<WebViewObject>();

            _webView.Init(
                cb: OnWebViewCallback,
                err: OnWebViewError,
                httpErr: OnWebViewHttpError,
                started: OnWebViewStarted,
                hooked: OnWebViewHooked,
                ld: OnWebViewLoaded,
                enableWKWebView: true,
                wkContentMode: 0,
                transparent: true,
                radius: Mathf.RoundToInt(_uiConfig?.CornerRadius ?? 0)
            );

            _webView.SetVisibility(false);
            ApplyWebViewLayout();
        }

        private void DisposeWebView()
        {
            if (_webView != null)
            {
                UnityEngine.Object.Destroy(_webView.gameObject);
                _webView = null;
            }
        }

        /// <summary>
        /// Sets JSON configuration for iubenda
        /// </summary>
        public void SetJsonConfig(string jsonConfig)
        {
            _jsonConfig = jsonConfig;
        }

        /// <summary>
        /// Loads URL with specified use case
        /// </summary>
        public Task<bool> LoadConsentURLAsync(string url, UseCase useCase, string consentValue = null, bool noHash = false)
        {
            EnsureWebView();
            _currentUseCase = useCase;
            _operationCompletion = new TaskCompletionSource<bool>();

            Debug.Log($"[CMPWebViewManager] Loading URL for use case: {useCase}");
            Debug.Log($"[CMPWebViewManager] URL: {url.Substring(0, Math.Min(100, url.Length))}...");

            // For import consent, inject variable before page load
            if (useCase == UseCase.ImportConsent && !string.IsNullOrEmpty(consentValue))
            {
                string importScript = CMPConstants.JavaScript.GetConsentImportScript(consentValue);
                Debug.Log("[CMPWebViewManager] Injecting consent import variable");
                _webView?.EvaluateJS(importScript);
            }

            // Start timeout
            if (_timeoutCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_timeoutCoroutine);
            }
            _timeoutCoroutine = _coroutineRunner.StartCoroutine(OperationTimeoutCoroutine());

            // Load URL
            _webView?.LoadURL(url);

            return _operationCompletion.Task;
        }

        /// <summary>
        /// Applies WebView layout based on UI configuration
        /// </summary>
        public void ApplyWebViewLayout()
        {
            if (_webView == null || _uiConfig == null)
                return;

            Rect webViewRect = CMPLayoutCalculator.CalculateWebViewRect(_uiConfig.Layout, _uiConfig.RespectsSafeArea);
            var margins = CMPLayoutCalculator.GetMarginsForRect(webViewRect);

            Debug.Log($"[CMPWebViewManager] Setting margins: L={margins.left}, T={margins.top}, R={margins.right}, B={margins.bottom}");

            _webView.SetMargins(margins.left, margins.top, margins.right, margins.bottom);
        }

        /// <summary>
        /// Shows WebView
        /// </summary>
        public void Show()
        {
            Debug.Log("[CMPWebViewManager] Showing WebView");
            _webView?.SetVisibility(true);
        }

        /// <summary>
        /// Hides WebView
        /// </summary>
        public void Hide()
        {
            Debug.Log("[CMPWebViewManager] Hiding WebView");
            _webView?.SetVisibility(false);
        }

        /// <summary>
        /// Destroys WebView
        /// </summary>
        public void Destroy()
        {
            Debug.Log("[CMPWebViewManager] Destroying WebView");
            
            if (_timeoutCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_timeoutCoroutine);
                _timeoutCoroutine = null;
            }

            DisposeWebView();
        }

        /// <summary>
        /// Cancels current operation
        /// </summary>
        public void CancelOperation()
        {
            Debug.Log("[CMPWebViewManager] Cancelling operation");
            
            if (_timeoutCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_timeoutCoroutine);
                _timeoutCoroutine = null;
            }

            if (_operationCompletion != null && !_operationCompletion.Task.IsCompleted)
            {
                _operationCompletion.TrySetCanceled();
            }
        }

        #region WebView Callbacks

        private void OnWebViewCallback(string message)
        {
            Debug.Log($"[CMPWebViewManager] Callback: {message}");
            _jsBridge?.HandleMessage(message);
        }

        private void OnWebViewError(string error)
        {
            Debug.LogError($"[CMPWebViewManager] WebView error: {error}");
            OnError?.Invoke(error);
            
            if (_operationCompletion != null && !_operationCompletion.Task.IsCompleted)
            {
                _operationCompletion.TrySetException(new Exception(error));
            }
        }

        private void OnWebViewHttpError(string error)
        {
            Debug.LogError($"[CMPWebViewManager] HTTP error: {error}");
            OnError?.Invoke($"HTTP error: {error}");
        }

        private void OnWebViewStarted(string url)
        {
            Debug.Log($"[CMPWebViewManager] Page started: {url}");
        }

        private void OnWebViewHooked(string message)
        {
            Debug.Log($"[CMPWebViewManager] Hooked: {message}");
        }

        private void OnWebViewLoaded(string url)
        {
            Debug.Log($"[CMPWebViewManager] Page loaded: {url}");
            
            // Inject JavaScript bridge
            InjectJavaScript();
            
            // Inject initCMP for iubenda
            if (!string.IsNullOrEmpty(_jsonConfig))
            {
                InjectInitCMP();
            }
        }

        #endregion

        #region JavaScript Injection

        private void InjectJavaScript()
        {
            if (_webView == null)
                return;

            Debug.Log("[CMPWebViewManager] Injecting JavaScript bridge");
            _webView.EvaluateJS(CMPConstants.JavaScript.JavaScriptToExecute);
        }

        private void InjectInitCMP()
        {
            if (_webView == null)
                return;

            string initScript = CMPConstants.JavaScript.GetInitCMPScript(_jsonConfig);
            Debug.Log("[CMPWebViewManager] Injecting initCMP");
            _webView.EvaluateJS(initScript);
        }

        #endregion

        #region Message Handlers

        private void HandleConsentReceived(string consentString, JObject data)
        {
            Debug.Log("[CMPWebViewManager] Consent received");
            
            // Stop timeout
            if (_timeoutCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_timeoutCoroutine);
                _timeoutCoroutine = null;
            }

            // Fire event
            OnConsentReceived?.Invoke(consentString, data);

            // Complete operation
            if (_operationCompletion != null && !_operationCompletion.Task.IsCompleted)
            {
                if (_currentUseCase == UseCase.PerformDryCheckConsent)
                {
                    _operationCompletion.TrySetResult(false);
                }
                else
                {
                    _operationCompletion.TrySetResult(true);
                }
            }

            // Close consent layer after receiving consent (except for dry check)
            if (_currentUseCase != UseCase.PerformDryCheckConsent)
            {
                OnCloseSignal?.Invoke();
            }
        }

        private void HandleOpenReceived()
        {
            Debug.Log("[CMPWebViewManager] Open signal received");
            
            // Stop timeout
            if (_timeoutCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_timeoutCoroutine);
                _timeoutCoroutine = null;
            }

            // For check/verify use cases, complete operation when open signal received
            if (_currentUseCase == UseCase.CheckConsent || 
                _currentUseCase == UseCase.PerformDryCheckConsent ||
                _currentUseCase == UseCase.VerifyConsentOnInitialize)
            {
                if (_operationCompletion != null && !_operationCompletion.Task.IsCompleted)
                {
                    _operationCompletion.TrySetResult(true);
                }
            }

            // Fire open signal to show UI
            OnOpenSignal?.Invoke();
        }

        private void HandleErrorReceived(string error)
        {
            Debug.LogError($"[CMPWebViewManager] Error from JavaScript: {error}");
            OnError?.Invoke(error);
            
            if (_operationCompletion != null && !_operationCompletion.Task.IsCompleted)
            {
                _operationCompletion.TrySetException(new Exception(error));
            }
        }

        #endregion

        #region Timeout Handling

        private IEnumerator OperationTimeoutCoroutine()
        {
            yield return new WaitForSeconds(CMPConstants.DefaultTimeout);
            
            Debug.LogWarning("[CMPWebViewManager] Operation timed out");
            
            if (_operationCompletion != null && !_operationCompletion.Task.IsCompleted)
            {
                _operationCompletion.TrySetException(new TimeoutException("Operation timed out"));
            }
            
            OnError?.Invoke("Operation timed out");
        }

        #endregion
    }
}
