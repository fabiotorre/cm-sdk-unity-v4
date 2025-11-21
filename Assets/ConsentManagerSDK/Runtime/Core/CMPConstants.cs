namespace ConsentManagerSDK
{
    /// <summary>
    /// Constants used throughout the SDK
    /// </summary>
    public static class CMPConstants
    {
        /// <summary>
        /// SDK version
        /// </summary>
        public const string SdkVersion = "4.0.0";

        /// <summary>
        /// Platform identifier
        /// </summary>
        public const string Platform = "Unity";

        /// <summary>
        /// API path for CMP endpoints
        /// </summary>
        public const string ApiPath = "/delivery/appsdk/v3/";

        /// <summary>
        /// Default operation timeout in seconds
        /// </summary>
        public const float DefaultTimeout = 10f;

        /// <summary>
        /// JavaScript code injected into WebView for SDK bridge
        /// </summary>
        public static class JavaScript
        {
            public const string JavaScriptToExecute = @"
(function() {
    // Perfect match for Android's cmpToSDK_sendStatus
    window.cmpToSDK_sendStatus = function(consent, jsonObject) {
        jsonObject.cmpString = consent;

        Unity.call(JSON.stringify({
            type: 'consent',
            data: jsonObject
        }));
    };

    // Perfect match for Android's cmpToSDK_showConsentLayer
    window.cmpToSDK_showConsentLayer = function() {
        Unity.call(JSON.stringify({
            type: 'open',
            data: { opened: true }
        }));
    };

    // Match Android's window.onerror shape
    window.onerror = function(message, source, lineno, colno, error) {
        Unity.call(JSON.stringify({
            type: 'error',
            data: {
                message: message,
                source: source,
                lineno: lineno,
                colno: colno,
                error: error ? error.toString() : null
            }
        }));
    };
})();
";

            public static string GetInitCMPScript(string jsonConfig)
            {
                if (string.IsNullOrEmpty(jsonConfig))
                {
                    return "window.initCMP && window.initCMP();";
                }
                return $"window.initCMP && window.initCMP({jsonConfig});";
            }

            public static string GetConsentImportScript(string consentValue)
            {
                return $"window.cmp_importconsent = '{consentValue}';";
            }
        }
    }
}
