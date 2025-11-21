using System;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Consent status for a specific purpose or vendor
    /// </summary>
    public enum ConsentStatus
    {
        /// <summary>
        /// No consent choice has been made yet
        /// </summary>
        ChoiceDoesntExist = 0,
        
        /// <summary>
        /// Consent has been granted
        /// </summary>
        Granted = 1,
        
        /// <summary>
        /// Consent has been denied
        /// </summary>
        Denied = 2
    }

    /// <summary>
    /// Overall user choice status
    /// </summary>
    public enum UserChoiceStatus
    {
        /// <summary>
        /// User has made a consent choice
        /// </summary>
        ChoiceExists,
        
        /// <summary>
        /// User has not made any consent choice yet
        /// </summary>
        ChoiceDoesntExist
    }

    /// <summary>
    /// Google Consent Mode types
    /// </summary>
    public enum ConsentType
    {
        /// <summary>
        /// Analytics storage consent
        /// </summary>
        AnalyticsStorage,
        
        /// <summary>
        /// Ad storage consent
        /// </summary>
        AdStorage,
        
        /// <summary>
        /// Ad user data consent
        /// </summary>
        AdUserData,
        
        /// <summary>
        /// Ad personalization consent
        /// </summary>
        AdPersonalization
    }

    /// <summary>
    /// Google Consent Mode status
    /// </summary>
    public enum ConsentModeStatus
    {
        /// <summary>
        /// Consent is granted
        /// </summary>
        Granted,
        
        /// <summary>
        /// Consent is denied
        /// </summary>
        Denied
    }
}

