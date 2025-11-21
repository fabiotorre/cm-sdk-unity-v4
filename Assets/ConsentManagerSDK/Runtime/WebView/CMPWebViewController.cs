using UnityEngine;
using UnityEngine.UI;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Controls WebView UI presentation with background overlay
    /// </summary>
    public class CMPWebViewController
    {
        private GameObject _canvasObject;
        private Canvas _canvas;
        private GameObject _backgroundObject;
        private Image _backgroundImage;
        private CMPUIConfig _config;
        private CMPWebViewManager _webViewManager;

        public CMPWebViewController(CMPWebViewManager webViewManager, CMPUIConfig config)
        {
            _webViewManager = webViewManager;
            _config = config ?? new CMPUIConfig();
        }

        /// <summary>
        /// Shows the consent layer with background overlay
        /// </summary>
        public void Show()
        {
            CreateCanvasIfNeeded();
            CreateBackgroundIfNeeded();
            ApplyBackgroundStyle();
            
            _canvasObject?.SetActive(true);
            _webViewManager?.Show();
            
            Debug.Log("[CMPWebViewController] Showing consent layer");
        }

        /// <summary>
        /// Hides the consent layer
        /// </summary>
        public void Hide()
        {
            _webViewManager?.Hide();
            if (_canvasObject != null)
            {
                _canvasObject.SetActive(false);
            }
            
            Debug.Log("[CMPWebViewController] Hiding consent layer");
        }

        /// <summary>
        /// Destroys the controller and cleans up resources
        /// </summary>
        public void Destroy()
        {
            if (_canvasObject != null)
            {
                Object.Destroy(_canvasObject);
                _canvasObject = null;
            }
            
            _canvas = null;
            _backgroundObject = null;
            _backgroundImage = null;
            
            Debug.Log("[CMPWebViewController] Destroyed");
        }

        /// <summary>
        /// Updates UI configuration
        /// </summary>
        public void UpdateConfig(CMPUIConfig newConfig)
        {
            _config = newConfig ?? _config;
            ApplyBackgroundStyle();
            _webViewManager?.ApplyWebViewLayout();
        }

        private void CreateCanvasIfNeeded()
        {
            if (_canvasObject != null)
                return;

            // Create Canvas
            _canvasObject = new GameObject("CMPCanvas");
            Object.DontDestroyOnLoad(_canvasObject);
            
            _canvas = _canvasObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 1000; // High sorting order to appear on top
            
            var canvasScaler = _canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            
            _canvasObject.AddComponent<GraphicRaycaster>();
            
            Debug.Log("[CMPWebViewController] Canvas created");
        }

        private void CreateBackgroundIfNeeded()
        {
            if (_backgroundObject != null)
                return;

            if (_canvas == null)
                return;

            // Create background panel
            _backgroundObject = new GameObject("CMPBackground");
            _backgroundObject.transform.SetParent(_canvasObject.transform, false);
            
            RectTransform rectTransform = _backgroundObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            _backgroundImage = _backgroundObject.AddComponent<Image>();
            
            // Make background non-interactive based on config
            var canvasGroup = _backgroundObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true; // Block touches to content behind
            
            Debug.Log("[CMPWebViewController] Background created");
        }

        private void ApplyBackgroundStyle()
        {
            if (_backgroundImage == null || _config == null)
                return;

            switch (_config.Background)
            {
                case CMPBackgroundType.Dimmed:
                    _backgroundImage.color = new Color(
                        _config.BackgroundColor.r,
                        _config.BackgroundColor.g,
                        _config.BackgroundColor.b,
                        _config.BackgroundAlpha
                    );
                    _backgroundObject.SetActive(true);
                    break;

                case CMPBackgroundType.Solid:
                    _backgroundImage.color = new Color(
                        _config.BackgroundColor.r,
                        _config.BackgroundColor.g,
                        _config.BackgroundColor.b,
                        1.0f
                    );
                    _backgroundObject.SetActive(true);
                    break;

                case CMPBackgroundType.None:
                    _backgroundObject.SetActive(false);
                    break;
            }
            
            Debug.Log($"[CMPWebViewController] Background style applied: {_config.Background}");
        }

        /// <summary>
        /// Handles orientation changes
        /// </summary>
        public void OnOrientationChanged()
        {
            if (_config.AllowsOrientationChanges)
            {
                _webViewManager?.ApplyWebViewLayout();
            }
        }
    }
}

