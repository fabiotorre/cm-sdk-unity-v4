using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// URL builder matching native SDK structure exactly
    /// Constructs CMP API URLs with appropriate parameters per use case
    /// </summary>
    public static class CMPUrlBuilder
    {
        private const string ApiPath = "/delivery/appsdk/v3/";
        private const string SdkVersion = "4.0.0";
        private const string Platform = "Unity";

        /// <summary>
        /// URL parameters for CMP requests
        /// </summary>
        public class CMPUrlParams
        {
            public string Id { get; set; }
            public string Domain { get; set; }
            public UseCase UseCase { get; set; }
            public string Consent { get; set; }
            public string Language { get; set; }
            public string AppName { get; set; }
            public string PackageName { get; set; }
            public string BundleID { get; set; }
            public bool IsDebugMode { get; set; }
            public int? DesignId { get; set; }
            public bool IsTV { get; set; }
            public bool JumpToSettingsPage { get; set; }
            public bool ForceOpen { get; set; }
            public bool AcceptAll { get; set; }
            public bool RejectAll { get; set; }
            public List<string> AddPurposes { get; set; } = new List<string>();
            public List<string> AddVendors { get; set; } = new List<string>();
            public bool UpdateVendors { get; set; }
            public int AttStatus { get; set; }
            public bool SkipCookies { get; set; }
            public bool DarkMode { get; set; }
            public bool NoHash { get; set; }

            public void Apply(Dictionary<string, object> additionalParams)
            {
                if (additionalParams == null) return;

                foreach (var kvp in additionalParams)
                {
                    switch (kvp.Key)
                    {
                        case "consent":
                            Consent = kvp.Value as string;
                            break;
                        case "language":
                            Language = kvp.Value as string;
                            break;
                        case "appName":
                            AppName = kvp.Value as string;
                            break;
                        case "packageName":
                            PackageName = kvp.Value as string;
                            break;
                        case "isDebugMode":
                            IsDebugMode = kvp.Value is bool debug && debug;
                            break;
                        case "designId":
                            DesignId = kvp.Value as int?;
                            break;
                        case "isTV":
                            IsTV = kvp.Value is bool tv && tv;
                            break;
                        case "jumpToSettings":
                        case "jumpToSettingsPage":
                            JumpToSettingsPage = kvp.Value is bool jump && jump;
                            break;
                        case "forceOpen":
                            ForceOpen = kvp.Value is bool force && force;
                            break;
                        case "acceptAll":
                            AcceptAll = kvp.Value is bool accept && accept;
                            break;
                        case "rejectAll":
                            RejectAll = kvp.Value is bool reject && reject;
                            break;
                        case "addPurposes":
                            if (kvp.Value is List<string> purposes)
                                AddPurposes = purposes;
                            else if (kvp.Value is string[] purposeArray)
                                AddPurposes = purposeArray.ToList();
                            break;
                        case "addVendors":
                            if (kvp.Value is List<string> vendors)
                                AddVendors = vendors;
                            else if (kvp.Value is string[] vendorArray)
                                AddVendors = vendorArray.ToList();
                            break;
                        case "updateVendors":
                            UpdateVendors = kvp.Value is bool update && update;
                            break;
                        case "attStatus":
                            if (kvp.Value is int att)
                                AttStatus = att;
                            break;
                        case "darkMode":
                            DarkMode = kvp.Value is bool dark && dark;
                            break;
                        case "noHash":
                            NoHash = kvp.Value is bool noHash && noHash;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Builds complete CMP URL with appropriate parameters
        /// </summary>
        public static string Build(CMPUrlParams parameters)
        {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append("https://");
            urlBuilder.Append(parameters.Domain);
            urlBuilder.Append(ApiPath);
            urlBuilder.Append("?cdid=");
            urlBuilder.Append(Uri.EscapeDataString(parameters.Id));

            // Add common parameters
            AppendCommonParameters(urlBuilder, parameters);

            // Add use case specific parameters
            AppendUseCaseParameters(urlBuilder, parameters);

            // Build URL string
            string url = urlBuilder.ToString();

            // Add consent as hash fragment if not noHash
            if (!string.IsNullOrEmpty(parameters.Consent) && !parameters.NoHash)
            {
                string timestamp = DateAndRandomNumberAsString();
                url += $"#cmpimport={Uri.EscapeDataString(parameters.Consent)}&zt={timestamp}";
            }

            return url;
        }

        private static void AppendCommonParameters(StringBuilder builder, CMPUrlParams parameters)
        {
            // App name
            if (!string.IsNullOrEmpty(parameters.AppName))
            {
                string appName = parameters.AppName.Replace(" ", "-");
                builder.Append("&appname=");
                builder.Append(Uri.EscapeDataString(appName));
            }

            // Language
            if (!string.IsNullOrEmpty(parameters.Language))
            {
                builder.Append("&cmplang=");
                builder.Append(Uri.EscapeDataString(parameters.Language));
            }

            // Bundle ID / Package Name
            string appId = !string.IsNullOrEmpty(parameters.BundleID) ? parameters.BundleID : parameters.PackageName;
            if (!string.IsNullOrEmpty(appId))
            {
                builder.Append("&appid=");
                builder.Append(Uri.EscapeDataString(appId));
            }

            // SDK version
            builder.Append("&sdkversion=");
            builder.Append(SdkVersion);

            // Platform
            builder.Append("&cmpplatform=");
            builder.Append(Platform);

            // ATT status
            builder.Append("&cmpatt=");
            builder.Append(parameters.AttStatus);

            // Dark mode
            builder.Append("&cmpdarkmode=");
            builder.Append(parameters.DarkMode ? "1" : "0");

            // Debug mode
            if (parameters.IsDebugMode)
            {
                builder.Append("&cmpdebug");
            }

            // TV SDK
            if (parameters.IsTV)
            {
                builder.Append("&tvsdk=1");
            }
        }

        private static void AppendUseCaseParameters(StringBuilder builder, CMPUrlParams parameters)
        {
            switch (parameters.UseCase)
            {
                case UseCase.VerifyConsentOnInitialize:
                case UseCase.CheckConsent:
                    AppendCheckConsentParameters(builder, parameters);
                    break;

                case UseCase.OpenConsent:
                    AppendOpenConsentParameters(builder, parameters);
                    break;

                case UseCase.PerformDryCheckConsent:
                case UseCase.PerformDryCheckWithCache:
                    builder.Append("&cmpskipcookies=1");
                    break;

                case UseCase.ImportConsent:
                    builder.Append("&cmpskipcookies=1");
                    string timestamp = DateAndRandomNumberAsString();
                    builder.Append("&zt=");
                    builder.Append(timestamp);
                    break;

                case UseCase.EnableConsentPurposes:
                    AppendEnablePurposesParameters(builder, parameters);
                    break;

                case UseCase.DisableConsentPurposes:
                    AppendDisablePurposesParameters(builder, parameters);
                    break;

                case UseCase.EnableConsentVendors:
                    AppendEnableVendorsParameters(builder, parameters);
                    break;

                case UseCase.DisableConsentVendors:
                    AppendDisableVendorsParameters(builder, parameters);
                    break;

                case UseCase.AcceptAllConsent:
                    builder.Append("&cmpautoaccept=1");
                    builder.Append("&cmpscreen");
                    break;

                case UseCase.RejectAllConsent:
                    builder.Append("&cmpautoreject");
                    builder.Append("&cmpscreen");
                    break;

                case UseCase.ResetConsentSettings:
                    // No additional parameters
                    break;
            }
        }

        private static void AppendCheckConsentParameters(StringBuilder builder, CMPUrlParams parameters)
        {
            if (parameters.DesignId.HasValue)
            {
                builder.Append("&usedesign=");
                builder.Append(parameters.DesignId.Value);
            }

            if (parameters.JumpToSettingsPage)
            {
                builder.Append("&cmpscreencustom");
            }
        }

        private static void AppendOpenConsentParameters(StringBuilder builder, CMPUrlParams parameters)
        {
            AppendCheckConsentParameters(builder, parameters);
            builder.Append("&cmpscreen");
        }

        private static void AppendEnablePurposesParameters(StringBuilder builder, CMPUrlParams parameters)
        {
            if (parameters.AddPurposes != null && parameters.AddPurposes.Count > 0)
            {
                builder.Append("&cmpsetpurposes=");
                builder.Append(string.Join("_", parameters.AddPurposes));
            }

            if (!parameters.UpdateVendors)
            {
                builder.Append("&cmpdontfixpurposes");
            }

            builder.Append("&cmpautoaccept=1");
            builder.Append("&cmpscreen");
        }

        private static void AppendDisablePurposesParameters(StringBuilder builder, CMPUrlParams parameters)
        {
            if (parameters.AddPurposes != null && parameters.AddPurposes.Count > 0)
            {
                builder.Append("&cmpsetpurposes=");
                builder.Append(string.Join("_", parameters.AddPurposes));
            }

            if (!parameters.UpdateVendors)
            {
                builder.Append("&cmpdontfixpurposes");
            }

            builder.Append("&cmpautoreject");
            builder.Append("&cmpscreen");
        }

        private static void AppendEnableVendorsParameters(StringBuilder builder, CMPUrlParams parameters)
        {
            if (parameters.AddVendors != null && parameters.AddVendors.Count > 0)
            {
                builder.Append("&cmpsetvendors=");
                builder.Append(string.Join("_", parameters.AddVendors));
                builder.Append("&cmpautoaccept=1");
                builder.Append("&cmpscreen");
            }
        }

        private static void AppendDisableVendorsParameters(StringBuilder builder, CMPUrlParams parameters)
        {
            if (parameters.AddVendors != null && parameters.AddVendors.Count > 0)
            {
                builder.Append("&cmpsetvendors=");
                builder.Append(string.Join("_", parameters.AddVendors));
                builder.Append("&cmpautoreject");
                builder.Append("&cmpscreen");
            }
        }

        private static string DateAndRandomNumberAsString()
        {
            string dateString = DateTime.Now.ToString("ddMMyyyyHHmmss", CultureInfo.InvariantCulture);
            int randomNumber = UnityEngine.Random.Range(0, 10000);
            return string.Format("{0}{1:D4}", dateString, randomNumber);
        }
    }
}

