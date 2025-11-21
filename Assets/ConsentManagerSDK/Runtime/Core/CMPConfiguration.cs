using System;
using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Main configuration for CMP SDK
    /// </summary>
    [Serializable]
    public class CMPConfig
    {
        /// <summary>
        /// CMP configuration ID from your dashboard
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// CMP service domain (e.g., "delivery.consentmanager.net")
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Language code for consent layer (e.g., "EN", "DE", "FR")
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Application name for identification
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// Optional JSON configuration (for iubenda clients only)
        /// </summary>
        public string JsonConfig { get; set; }

        /// <summary>
        /// If true, suppresses hash parameter on URL and uses JS injection instead
        /// </summary>
        public bool NoHash { get; set; }

        public CMPConfig() { }

        public CMPConfig(string id, string domain, string language, string appName, string jsonConfig = null, bool noHash = false)
        {
            Id = id;
            Domain = domain;
            Language = language;
            AppName = appName;
            JsonConfig = jsonConfig;
            NoHash = noHash;
        }

        /// <summary>
        /// Validates configuration
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Id) &&
                   !string.IsNullOrEmpty(Domain) &&
                   !string.IsNullOrEmpty(Language) &&
                   !string.IsNullOrEmpty(AppName);
        }
    }
}

