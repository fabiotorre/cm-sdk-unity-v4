using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Message types from WebView JavaScript
    /// </summary>
    public enum CMPMessageType
    {
        Consent,
        Open,
        Error,
        Unknown
    }

    /// <summary>
    /// Message data from WebView
    /// </summary>
    public class CMPMessage
    {
        public CMPMessageType Type { get; set; }
        public JObject Data { get; set; }
    }

    /// <summary>
    /// Handles JavaScript bridge communication between WebView and C#
    /// </summary>
    public class CMPJavaScriptBridge
    {
        /// <summary>
        /// Event fired when consent message is received
        /// </summary>
        public event Action<string, JObject> OnConsentReceived;

        /// <summary>
        /// Event fired when open message is received
        /// </summary>
        public event Action OnOpenReceived;

        /// <summary>
        /// Event fired when error message is received
        /// </summary>
        public event Action<string> OnErrorReceived;

        /// <summary>
        /// Parses message from WebView and fires appropriate event
        /// </summary>
        /// <param name="message">JSON message from WebView</param>
        public void HandleMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("[CMPJavaScriptBridge] Received empty message");
                return;
            }

            try
            {
                var messageObject = JObject.Parse(message);
                var typeString = messageObject["type"]?.ToString();
                var data = messageObject["data"] as JObject;

                CMPMessageType messageType = ParseMessageType(typeString);

                switch (messageType)
                {
                    case CMPMessageType.Consent:
                        HandleConsentMessage(data);
                        break;

                    case CMPMessageType.Open:
                        HandleOpenMessage(data);
                        break;

                    case CMPMessageType.Error:
                        HandleErrorMessage(data);
                        break;

                    default:
                        Debug.LogWarning($"[CMPJavaScriptBridge] Unknown message type: {typeString}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CMPJavaScriptBridge] Failed to parse message: {e.Message}");
                OnErrorReceived?.Invoke($"Message parsing error: {e.Message}");
            }
        }

        private CMPMessageType ParseMessageType(string typeString)
        {
            if (string.IsNullOrEmpty(typeString))
                return CMPMessageType.Unknown;

            switch (typeString.ToLower())
            {
                case "consent":
                    return CMPMessageType.Consent;
                case "open":
                    return CMPMessageType.Open;
                case "error":
                    return CMPMessageType.Error;
                default:
                    return CMPMessageType.Unknown;
            }
        }

        private void HandleConsentMessage(JObject data)
        {
            if (data == null)
            {
                Debug.LogError("[CMPJavaScriptBridge] Consent message has no data");
                return;
            }

            try
            {
                string consentString = data["cmpString"]?.ToString();
                
                if (string.IsNullOrEmpty(consentString))
                {
                    Debug.LogError("[CMPJavaScriptBridge] Consent message missing cmpString");
                    return;
                }

                Debug.Log($"[CMPJavaScriptBridge] Consent received: {consentString.Substring(0, Math.Min(50, consentString.Length))}...");
                OnConsentReceived?.Invoke(consentString, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CMPJavaScriptBridge] Failed to handle consent message: {e.Message}");
                OnErrorReceived?.Invoke($"Consent processing error: {e.Message}");
            }
        }

        private void HandleOpenMessage(JObject data)
        {
            Debug.Log("[CMPJavaScriptBridge] Open message received");
            OnOpenReceived?.Invoke();
        }

        private void HandleErrorMessage(JObject data)
        {
            string errorMessage = data?["message"]?.ToString() ?? "Unknown error";
            Debug.LogError($"[CMPJavaScriptBridge] Error from WebView: {errorMessage}");
            OnErrorReceived?.Invoke(errorMessage);
        }
    }
}

