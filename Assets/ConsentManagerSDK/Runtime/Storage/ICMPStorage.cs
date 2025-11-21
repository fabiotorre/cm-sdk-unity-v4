namespace ConsentManagerSDK
{
    /// <summary>
    /// Interface for platform-specific consent data storage
    /// Matches native SDK storage locations for data compatibility
    /// </summary>
    public interface ICMPStorage
    {
        /// <summary>
        /// Saves consent data to platform-specific storage
        /// </summary>
        /// <param name="json">JSON representation of consent model</param>
        /// <param name="consentString">Base64-encoded consent string</param>
        /// <param name="metadata">Metadata array for individual key storage</param>
        void SaveConsentData(string json, string consentString, CMPMetadata[] metadata);

        /// <summary>
        /// Retrieves consent data from platform-specific storage
        /// </summary>
        /// <returns>Tuple of JSON, consent string, and metadata</returns>
        (string json, string consentString, CMPMetadata[] metadata) GetConsentData();

        /// <summary>
        /// Resets all consent management data
        /// </summary>
        void ResetConsentData();

        /// <summary>
        /// Gets a specific metadata value by key
        /// </summary>
        /// <param name="key">Metadata key</param>
        /// <returns>Metadata value (string or int)</returns>
        object GetMetadataValue(string key);
    }
}

