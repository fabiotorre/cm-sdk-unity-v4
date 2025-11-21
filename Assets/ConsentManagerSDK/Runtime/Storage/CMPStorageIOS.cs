using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// iOS storage implementation using PlayerPrefs (maps to NSUserDefaults)
    /// Matches native iOS SDK storage keys for data compatibility
    /// </summary>
    public class CMPStorageIOS : ICMPStorage
    {
        private const string KeyConsentJson = "consentJson";
        private const string KeyConsentString = "consentString";
        private const string KeyConsentMetadata = "consentMetadata";

        public void SaveConsentData(string json, string consentString, CMPMetadata[] metadata)
        {
            // Save main consent data
            PlayerPrefs.SetString(KeyConsentJson, json);
            PlayerPrefs.SetString(KeyConsentString, consentString);

            // Clear existing metadata keys first
            if (PlayerPrefs.HasKey(KeyConsentMetadata))
            {
                string existingMetadataJson = PlayerPrefs.GetString(KeyConsentMetadata);
                try
                {
                    var existingMetadata = JsonConvert.DeserializeObject<CMPMetadata[]>(existingMetadataJson);
                    if (existingMetadata != null)
                    {
                        foreach (var item in existingMetadata)
                        {
                            PlayerPrefs.DeleteKey(item.Name);
                        }
                    }
                }
                catch
                {
                    // Ignore deserialization errors for old metadata
                }
            }

            // Save individual metadata values
            if (metadata != null && metadata.Length > 0)
            {
                foreach (var item in metadata)
                {
                    if (item.Value.IntValue.HasValue)
                    {
                        PlayerPrefs.SetInt(item.Name, item.Value.IntValue.Value);
                    }
                    else
                    {
                        PlayerPrefs.SetString(item.Name, item.Value.StringValue);
                    }
                }

                // Save metadata array for future reference
                string metadataJson = JsonConvert.SerializeObject(metadata);
                PlayerPrefs.SetString(KeyConsentMetadata, metadataJson);
            }
            else
            {
                PlayerPrefs.DeleteKey(KeyConsentMetadata);
            }

            // Ensure immediate write (matches iOS synchronize())
            PlayerPrefs.Save();
        }

        public (string json, string consentString, CMPMetadata[] metadata) GetConsentData()
        {
            string json = PlayerPrefs.GetString(KeyConsentJson, null);
            string consentString = PlayerPrefs.GetString(KeyConsentString, null);
            
            CMPMetadata[] metadata = null;
            if (PlayerPrefs.HasKey(KeyConsentMetadata))
            {
                string metadataJson = PlayerPrefs.GetString(KeyConsentMetadata);
                try
                {
                    metadata = JsonConvert.DeserializeObject<CMPMetadata[]>(metadataJson);
                }
                catch
                {
                    // Ignore deserialization errors
                }
            }

            return (json, consentString, metadata);
        }

        public void ResetConsentData()
        {
            // Get stored metadata keys to clean up
            List<string> keysToRemove = new List<string>
            {
                KeyConsentJson,
                KeyConsentString,
                KeyConsentMetadata
            };

            // Add individual metadata keys
            if (PlayerPrefs.HasKey(KeyConsentMetadata))
            {
                string metadataJson = PlayerPrefs.GetString(KeyConsentMetadata);
                try
                {
                    var metadata = JsonConvert.DeserializeObject<CMPMetadata[]>(metadataJson);
                    if (metadata != null)
                    {
                        foreach (var item in metadata)
                        {
                            keysToRemove.Add(item.Name);
                        }
                    }
                }
                catch
                {
                    // Ignore deserialization errors
                }
            }

            // Remove all consent-related keys
            foreach (string key in keysToRemove)
            {
                PlayerPrefs.DeleteKey(key);
            }

            PlayerPrefs.Save();
        }

        public object GetMetadataValue(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                // Try to get as int first, then as string
                try
                {
                    return PlayerPrefs.GetInt(key);
                }
                catch
                {
                    return PlayerPrefs.GetString(key, null);
                }
            }

            return null;
        }
    }
}

