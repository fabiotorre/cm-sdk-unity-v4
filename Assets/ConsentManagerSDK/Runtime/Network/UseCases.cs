namespace ConsentManagerSDK
{
    /// <summary>
    /// CMP use cases that determine URL parameters and behavior
    /// Matches native SDK use cases exactly
    /// </summary>
    public enum UseCase
    {
        /// <summary>
        /// Initial check and open consent layer if needed
        /// </summary>
        VerifyConsentOnInitialize,
        
        /// <summary>
        /// Force open the consent layer
        /// </summary>
        OpenConsent,
        
        /// <summary>
        /// Check consent status only
        /// </summary>
        CheckConsent,
        
        /// <summary>
        /// Dry check without cookies
        /// </summary>
        PerformDryCheckConsent,
        
        /// <summary>
        /// Dry check with cache
        /// </summary>
        PerformDryCheckWithCache,
        
        /// <summary>
        /// Import consent from string
        /// </summary>
        ImportConsent,
        
        /// <summary>
        /// Enable specific purposes
        /// </summary>
        EnableConsentPurposes,
        
        /// <summary>
        /// Disable specific purposes
        /// </summary>
        DisableConsentPurposes,
        
        /// <summary>
        /// Enable specific vendors
        /// </summary>
        EnableConsentVendors,
        
        /// <summary>
        /// Disable specific vendors
        /// </summary>
        DisableConsentVendors,
        
        /// <summary>
        /// Accept all consents
        /// </summary>
        AcceptAllConsent,
        
        /// <summary>
        /// Reject all consents
        /// </summary>
        RejectAllConsent,
        
        /// <summary>
        /// Reset consent settings
        /// </summary>
        ResetConsentSettings
    }
}

