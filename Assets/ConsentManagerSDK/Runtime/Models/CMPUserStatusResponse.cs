using System;
using System.Collections.Generic;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Represents comprehensive user consent status including all vendors, purposes, and consent strings
    /// </summary>
    [Serializable]
    public class CMPUserStatusResponse
    {
        /// <summary>
        /// Overall user choice status
        /// </summary>
        public UserChoiceStatus Status { get; set; }
        
        /// <summary>
        /// Vendor consent statuses mapped by vendor ID
        /// </summary>
        public Dictionary<string, ConsentStatus> Vendors { get; set; }
        
        /// <summary>
        /// Purpose consent statuses mapped by purpose ID
        /// </summary>
        public Dictionary<string, ConsentStatus> Purposes { get; set; }
        
        /// <summary>
        /// TCF (Transparency & Consent Framework) consent string
        /// </summary>
        public string Tcf { get; set; }
        
        /// <summary>
        /// Additional consent string (e.g., Google)
        /// </summary>
        public string AddtlConsent { get; set; }
        
        /// <summary>
        /// Regulation key (e.g., "gdpr", "ccpa")
        /// </summary>
        public string Regulation { get; set; }

        public CMPUserStatusResponse()
        {
            Status = UserChoiceStatus.ChoiceDoesntExist;
            Vendors = new Dictionary<string, ConsentStatus>();
            Purposes = new Dictionary<string, ConsentStatus>();
            Tcf = string.Empty;
            AddtlConsent = string.Empty;
            Regulation = string.Empty;
        }
    }
}

