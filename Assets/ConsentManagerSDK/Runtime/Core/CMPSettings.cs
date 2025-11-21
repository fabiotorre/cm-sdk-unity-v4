using System;
using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// ScriptableObject container for CMP configuration values.
    /// Stored in Resources so it can be loaded before any scene runs.
    /// </summary>
    [CreateAssetMenu(fileName = "CMPSettings", menuName = "Consent Manager/CMP Settings")]
    public class CMPSettings : ScriptableObject
    {
        internal const string ResourcePath = "CMPSettings";
        private const string DefaultCmpId = "your-cmp-id";
        private const string DefaultDomain = "delivery.consentmanager.net";
        private const string DefaultLanguage = "EN";
        private const string DefaultAppName = "Unity Demo App";

        [Header("CMP Configuration")]
        [SerializeField] private string cmpId = DefaultCmpId;
        [SerializeField] private string cmpDomain = DefaultDomain;
        [SerializeField] private string language = DefaultLanguage;
        [SerializeField] private string appName = DefaultAppName;
        [SerializeField] private bool respectAppName = true;

        [Header("UI Settings")]
        [SerializeField] private CMPLayoutStyle layoutStyle = CMPLayoutStyle.BottomHalf;
        [SerializeField] private bool darkMode;

        [Header("Initialization")]
        [SerializeField] private bool autoInitialize = true;

        public bool AutoInitialize => autoInitialize;

        public CMPConfig ToConfig()
        {
            var resolvedAppName = respectAppName ? Application.productName : appName;
            return new CMPConfig(
                cmpId,
                cmpDomain,
                language,
                string.IsNullOrEmpty(resolvedAppName) ? DefaultAppName : resolvedAppName
            );
        }

        public CMPUIConfig ToUIConfig()
        {
            return new CMPUIConfig(layoutStyle)
            {
                DarkMode = darkMode
            };
        }

        public bool HasValidConfiguration()
        {
            return !string.IsNullOrWhiteSpace(cmpId) &&
                   !string.Equals(cmpId, DefaultCmpId, StringComparison.Ordinal) &&
                   !string.IsNullOrWhiteSpace(cmpDomain) &&
                   !string.Equals(cmpDomain, DefaultDomain, StringComparison.Ordinal);
        }

        public static CMPSettings LoadDefault()
        {
            return Resources.Load<CMPSettings>(ResourcePath);
        }
    }
}
