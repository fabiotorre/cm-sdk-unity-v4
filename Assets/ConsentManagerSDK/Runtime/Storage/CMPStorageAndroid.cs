using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Android storage implementation using SharedPreferences
    /// Direct access to PreferenceManager.getDefaultSharedPreferences() for compatibility
    /// </summary>
    public class CMPStorageAndroid : ICMPStorage
    {
        private const string KeyConsentJson = "consentJson";
        private const string KeyConsentString = "consentString";

        private AndroidJavaObject _sharedPreferences;
        private AndroidJavaObject _editor;

        public CMPStorageAndroid()
        {
            InitializeSharedPreferences();
        }

        private void InitializeSharedPreferences()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                    
                    using (AndroidJavaClass preferenceManager = new AndroidJavaClass("android.preference.PreferenceManager"))
                    {
                        _sharedPreferences = preferenceManager.CallStatic<AndroidJavaObject>("getDefaultSharedPreferences", context);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize SharedPreferences: {e.Message}");
            }
#endif
        }

        public void SaveConsentData(string json, string consentString, CMPMetadata[] metadata)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_sharedPreferences == null)
            {
                Debug.LogError("SharedPreferences not initialized");
                return;
            }

            try
            {
                _editor = _sharedPreferences.Call<AndroidJavaObject>("edit");
                
                // Save main consent data
                _editor.Call<AndroidJavaObject>("putString", KeyConsentJson, json);
                _editor.Call<AndroidJavaObject>("putString", KeyConsentString, consentString);

                // Parse and save metadata values
                if (metadata != null && metadata.Length > 0)
                {
                    foreach (var item in metadata)
                    {
                        if (item.Type == "int" && item.Value.IntValue.HasValue)
                        {
                            _editor.Call<AndroidJavaObject>("putInt", item.Name, item.Value.IntValue.Value);
                        }
                        else
                        {
                            _editor.Call<AndroidJavaObject>("putString", item.Name, item.Value.StringValue);
                        }
                    }
                }

                // Also extract metadata from JSON if present
                try
                {
                    var jsonObject = JObject.Parse(json);
                    var metadataArray = jsonObject["metadata"] as JArray;
                    if (metadataArray != null)
                    {
                        foreach (var metadataItem in metadataArray)
                        {
                            string name = metadataItem["name"]?.ToString();
                            string type = metadataItem["type"]?.ToString();
                            var value = metadataItem["value"];

                            if (!string.IsNullOrEmpty(name) && value != null)
                            {
                                if (type == "int" && value.Type == JTokenType.Integer)
                                {
                                    _editor.Call<AndroidJavaObject>("putInt", name, value.ToObject<int>());
                                }
                                else
                                {
                                    _editor.Call<AndroidJavaObject>("putString", name, value.ToString());
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }

                // Commit changes immediately
                _editor.Call<bool>("commit");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save consent data: {e.Message}");
            }
            finally
            {
                if (_editor != null)
                {
                    _editor.Dispose();
                    _editor = null;
                }
            }
#endif
        }

        public (string json, string consentString, CMPMetadata[] metadata) GetConsentData()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_sharedPreferences == null)
            {
                Debug.LogError("SharedPreferences not initialized");
                return (null, null, null);
            }

            try
            {
                string json = _sharedPreferences.Call<string>("getString", KeyConsentJson, null);
                string consentString = _sharedPreferences.Call<string>("getString", KeyConsentString, null);
                
                // Metadata is embedded in JSON, extract if needed
                CMPMetadata[] metadata = null;
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var jsonObject = JObject.Parse(json);
                        var metadataArray = jsonObject["metadata"] as JArray;
                        if (metadataArray != null)
                        {
                            List<CMPMetadata> metadataList = new List<CMPMetadata>();
                            foreach (var item in metadataArray)
                            {
                                var metadataItem = new CMPMetadata
                                {
                                    Name = item["name"]?.ToString(),
                                    Type = item["type"]?.ToString()
                                };

                                var value = item["value"];
                                if (metadataItem.Type == "int" && value.Type == JTokenType.Integer)
                                {
                                    metadataItem.Value = new MetadataValue(value.ToObject<int>());
                                }
                                else
                                {
                                    metadataItem.Value = new MetadataValue(value.ToString());
                                }

                                metadataList.Add(metadataItem);
                            }
                            metadata = metadataList.ToArray();
                        }
                    }
                    catch
                    {
                        // Ignore parsing errors
                    }
                }

                return (json, consentString, metadata);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to get consent data: {e.Message}");
                return (null, null, null);
            }
#else
            return (null, null, null);
#endif
        }

        public void ResetConsentData()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_sharedPreferences == null)
            {
                Debug.LogError("SharedPreferences not initialized");
                return;
            }

            try
            {
                _editor = _sharedPreferences.Call<AndroidJavaObject>("edit");
                _editor.Call<AndroidJavaObject>("clear");
                _editor.Call<bool>("commit");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to reset consent data: {e.Message}");
            }
            finally
            {
                if (_editor != null)
                {
                    _editor.Dispose();
                    _editor = null;
                }
            }
#endif
        }

        public object GetMetadataValue(string key)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_sharedPreferences == null)
            {
                Debug.LogError("SharedPreferences not initialized");
                return null;
            }

            try
            {
                // Try to get as int first
                if (_sharedPreferences.Call<bool>("contains", key))
                {
                    try
                    {
                        return _sharedPreferences.Call<int>("getInt", key, -1);
                    }
                    catch
                    {
                        // Try as string
                        return _sharedPreferences.Call<string>("getString", key, null);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to get metadata value: {e.Message}");
            }
#endif
            return null;
        }

        ~CMPStorageAndroid()
        {
            if (_sharedPreferences != null)
            {
                _sharedPreferences.Dispose();
                _sharedPreferences = null;
            }
        }
    }
}

