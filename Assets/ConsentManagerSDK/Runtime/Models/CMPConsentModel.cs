using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ConsentManagerSDK
{
    /// <summary>
    /// Flexible ID that can be either string or integer
    /// </summary>
    [Serializable]
    public class FlexibleId
    {
        [JsonIgnore]
        public string StringValue { get; set; }
        
        [JsonIgnore]
        public int? IntValue { get; set; }

        [JsonConstructor]
        public FlexibleId() { }

        public FlexibleId(string value)
        {
            StringValue = value;
            IntValue = int.TryParse(value, out int result) ? result : (int?)null;
        }

        public FlexibleId(int value)
        {
            IntValue = value;
            StringValue = value.ToString();
        }
    }

    /// <summary>
    /// Flexible metadata value that can be string or integer
    /// </summary>
    [Serializable]
    public class MetadataValue
    {
        [JsonIgnore]
        public string StringValue { get; set; }
        
        [JsonIgnore]
        public int? IntValue { get; set; }

        [JsonConstructor]
        public MetadataValue() { }

        public MetadataValue(string value)
        {
            StringValue = value;
            IntValue = int.TryParse(value, out int result) ? result : (int?)null;
        }

        public MetadataValue(int value)
        {
            IntValue = value;
            StringValue = value.ToString();
        }
    }

    /// <summary>
    /// TCF section data
    /// </summary>
    [Serializable]
    public class TCFSection
    {
        [JsonProperty("Version")]
        public int Version { get; set; }
        
        [JsonProperty("ConsentLanguage")]
        public string ConsentLanguage { get; set; }
        
        [JsonProperty("PublisherCC")]
        public string PublisherCC { get; set; }
        
        [JsonProperty("IsServiceSpecific")]
        public bool IsServiceSpecific { get; set; }
        
        [JsonProperty("VendorConsent")]
        public List<string> VendorConsent { get; set; }
        
        [JsonProperty("TcfPolicyVersion")]
        public int TcfPolicyVersion { get; set; }
        
        [JsonProperty("CmpVersion")]
        public int CmpVersion { get; set; }
        
        [JsonProperty("VendorLegitimateInterest")]
        public List<string> VendorLegitimateInterest { get; set; }
        
        [JsonProperty("CmpId")]
        public int CmpId { get; set; }
        
        [JsonProperty("SpecialFeatureOptIns")]
        public List<string> SpecialFeatureOptIns { get; set; }
        
        [JsonProperty("LastUpdated")]
        public string LastUpdated { get; set; }
        
        [JsonProperty("Created")]
        public string Created { get; set; }
        
        [JsonProperty("VendorListVersion")]
        public int VendorListVersion { get; set; }
        
        [JsonProperty("PurposeOneTreatment")]
        public bool PurposeOneTreatment { get; set; }
        
        [JsonProperty("ConsentScreen")]
        public int ConsentScreen { get; set; }
        
        [JsonProperty("PurposesLITransparency")]
        public List<string> PurposesLITransparency { get; set; }
        
        [JsonProperty("PurposeConsent")]
        public List<string> PurposeConsent { get; set; }
        
        [JsonProperty("UseNonStandardStacks")]
        public bool UseNonStandardStacks { get; set; }
    }

    /// <summary>
    /// GPP data structure
    /// </summary>
    [Serializable]
    public class GPPData
    {
        [JsonProperty("applicableSections")]
        public List<int> ApplicableSections { get; set; }
        
        [JsonProperty("cmpDisplayStatus")]
        public string CmpDisplayStatus { get; set; }
        
        [JsonProperty("cmpId")]
        public int? CmpId { get; set; }
        
        [JsonProperty("cmpStatus")]
        public string CmpStatus { get; set; }
        
        [JsonProperty("gppString")]
        public string GppString { get; set; }
        
        [JsonProperty("gppVersion")]
        public string GppVersion { get; set; }
        
        [JsonProperty("parsedSections")]
        public Dictionary<string, List<TCFSection>> ParsedSections { get; set; }
        
        [JsonProperty("sectionList")]
        public List<int> SectionList { get; set; }
        
        [JsonProperty("signalStatus")]
        public string SignalStatus { get; set; }
        
        [JsonProperty("supportedAPIs")]
        public List<string> SupportedAPIs { get; set; }
    }

    /// <summary>
    /// Purpose information
    /// </summary>
    [Serializable]
    public class Purpose
    {
        [JsonProperty("id")]
        public FlexibleId Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Vendor information
    /// </summary>
    [Serializable]
    public class Vendor
    {
        [JsonProperty("googleid")]
        public int GoogleId { get; set; }
        
        [JsonProperty("iabid")]
        public int IabId { get; set; }
        
        [JsonProperty("id")]
        public FlexibleId Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("purposes")]
        public List<string> Purposes { get; set; }
        
        [JsonProperty("systemid")]
        public string SystemId { get; set; }
    }

    /// <summary>
    /// CMP metadata item
    /// </summary>
    [Serializable]
    public class CMPMetadata
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("value")]
        public MetadataValue Value { get; set; }
    }

    /// <summary>
    /// Complete consent data model matching native SDK structure
    /// </summary>
    [Serializable]
    public class CMPConsentModel
    {
        [JsonProperty("cmpString")]
        public string CmpStringBase64Encoded { get; set; }
        
        [JsonProperty("addtlConsent")]
        public string GoogleAdditionalConsent { get; set; }
        
        [JsonProperty("consentstring")]
        public string ConsentString { get; set; }
        
        [JsonProperty("gdprApplies")]
        public bool? GdprApplies { get; set; }
        
        [JsonProperty("googleVendorConsents")]
        public Dictionary<string, bool> GoogleVendorConsents { get; set; }
        
        [JsonProperty("hasGlobalScope")]
        public bool? HasGlobalScope { get; set; }
        
        [JsonProperty("publisherCC")]
        public string PublisherCC { get; set; }
        
        [JsonProperty("regulation")]
        public int? Regulation { get; set; }
        
        [JsonProperty("regulationKey")]
        public string RegulationKey { get; set; }
        
        [JsonProperty("tcfcompliant")]
        public bool? TcfCompliant { get; set; }
        
        [JsonProperty("tcfversion")]
        public int? TcfVersion { get; set; }
        
        [JsonProperty("lastButtonEvent")]
        public int? LastButtonEvent { get; set; }
        
        [JsonProperty("tcfcaversion")]
        public int? TcfcaVersion { get; set; }
        
        [JsonProperty("gppversions")]
        public List<string> GppVersions { get; set; }
        
        [JsonProperty("uspstring")]
        public string UspString { get; set; }
        
        [JsonProperty("vendorsList")]
        public List<Vendor> VendorsList { get; set; }
        
        [JsonProperty("purposesList")]
        public List<Purpose> PurposesList { get; set; }
        
        [JsonProperty("purposeLI")]
        public Dictionary<string, bool> PurposeLI { get; set; }
        
        [JsonProperty("vendorLI")]
        public Dictionary<string, bool> VendorLI { get; set; }
        
        [JsonProperty("vendorConsents")]
        public Dictionary<string, bool> VendorConsents { get; set; }
        
        [JsonProperty("purposeConsents")]
        public Dictionary<string, bool> PurposeConsents { get; set; }
        
        [JsonProperty("metadata")]
        public List<CMPMetadata> Metadata { get; set; }
        
        [JsonProperty("userChoiceExists")]
        public bool? UserChoiceExists { get; set; }
        
        [JsonProperty("purModeActive")]
        public bool? PurModeActive { get; set; }
        
        [JsonProperty("purModeLoggedIn")]
        public bool? PurModeLoggedIn { get; set; }
        
        [JsonProperty("purModeLogic")]
        public int? PurModeLogic { get; set; }
        
        [JsonProperty("consentExists")]
        public bool? ConsentExists { get; set; }
        
        [JsonProperty("consentmode")]
        public Dictionary<string, string> ConsentMode { get; set; }
        
        [JsonProperty("gppdata")]
        public GPPData Gppdata { get; set; }

        /// <summary>
        /// Gets consent status for a specific purpose
        /// </summary>
        public ConsentStatus GetStatusForPurpose(string id)
        {
            if (PurposeConsents == null)
                return ConsentStatus.ChoiceDoesntExist;

            string normalizedId = id.ToLower();
            
            if (PurposeConsents.TryGetValue(normalizedId, out bool hasConsent))
                return hasConsent ? ConsentStatus.Granted : ConsentStatus.Denied;
            
            return ConsentStatus.ChoiceDoesntExist;
        }

        /// <summary>
        /// Gets consent status for a specific vendor
        /// </summary>
        public ConsentStatus GetStatusForVendor(string id)
        {
            if (VendorConsents == null)
                return ConsentStatus.ChoiceDoesntExist;

            string normalizedId = id.ToLower();
            
            if (VendorConsents.TryGetValue(normalizedId, out bool hasConsent))
                return hasConsent ? ConsentStatus.Granted : ConsentStatus.Denied;
            
            return ConsentStatus.ChoiceDoesntExist;
        }

        /// <summary>
        /// Checks if user has made any consent choice
        /// </summary>
        public bool HasUserChoice()
        {
            return !string.IsNullOrEmpty(ConsentString);
        }

        /// <summary>
        /// Exports CMP consent string for storage or transfer
        /// </summary>
        public string ExportCMPInfo()
        {
            return CmpStringBase64Encoded ?? string.Empty;
        }

        /// <summary>
        /// Gets all purpose IDs
        /// </summary>
        public List<string> GetAllPurposesIDs()
        {
            return PurposesList?.Select(p => p.Id.StringValue).ToList() ?? new List<string>();
        }

        /// <summary>
        /// Gets all vendor IDs
        /// </summary>
        public List<string> GetAllVendorsIDs()
        {
            return VendorsList?.Select(v => v.Id.StringValue).ToList() ?? new List<string>();
        }

        /// <summary>
        /// Converts to UserStatusResponse
        /// </summary>
        public CMPUserStatusResponse ToUserStatusResponse()
        {
            var response = new CMPUserStatusResponse
            {
                Status = HasUserChoice() ? UserChoiceStatus.ChoiceExists : UserChoiceStatus.ChoiceDoesntExist,
                Tcf = ConsentString ?? string.Empty,
                AddtlConsent = GoogleAdditionalConsent ?? string.Empty,
                Regulation = RegulationKey ?? string.Empty
            };

            // Map vendors
            foreach (var vendorId in GetAllVendorsIDs())
            {
                response.Vendors[vendorId] = GetStatusForVendor(vendorId);
            }

            // Map purposes
            foreach (var purposeId in GetAllPurposesIDs())
            {
                response.Purposes[purposeId] = GetStatusForPurpose(purposeId);
            }

            return response;
        }

        /// <summary>
        /// Deserializes from JSON string
        /// </summary>
        public static CMPConsentModel FromJson(string json)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Converters = new List<JsonConverter> { new FlexibleIdConverter(), new MetadataValueConverter() }
                };
                
                return JsonConvert.DeserializeObject<CMPConsentModel>(json, settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse consent model: {e.Message}");
                return new CMPConsentModel();
            }
        }

        /// <summary>
        /// Serializes to JSON string
        /// </summary>
        public string ToJson()
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new List<JsonConverter> { new FlexibleIdConverter(), new MetadataValueConverter() }
                };
                
                return JsonConvert.SerializeObject(this, settings);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize consent model: {e.Message}");
                return "{}";
            }
        }
    }

    /// <summary>
    /// JSON converter for FlexibleId
    /// </summary>
    public class FlexibleIdConverter : JsonConverter<FlexibleId>
    {
        public override void WriteJson(JsonWriter writer, FlexibleId value, JsonSerializer serializer)
        {
            if (value.IntValue.HasValue)
                writer.WriteValue(value.IntValue.Value);
            else
                writer.WriteValue(value.StringValue);
        }

        public override FlexibleId ReadJson(JsonReader reader, Type objectType, FlexibleId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
                return new FlexibleId(Convert.ToInt32(reader.Value));
            else if (reader.TokenType == JsonToken.String)
                return new FlexibleId(reader.Value.ToString());
            
            return new FlexibleId(string.Empty);
        }
    }

    /// <summary>
    /// JSON converter for MetadataValue
    /// </summary>
    public class MetadataValueConverter : JsonConverter<MetadataValue>
    {
        public override void WriteJson(JsonWriter writer, MetadataValue value, JsonSerializer serializer)
        {
            if (value.IntValue.HasValue)
                writer.WriteValue(value.IntValue.Value);
            else
                writer.WriteValue(value.StringValue);
        }

        public override MetadataValue ReadJson(JsonReader reader, Type objectType, MetadataValue existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
                return new MetadataValue(Convert.ToInt32(reader.Value));
            else if (reader.TokenType == JsonToken.String)
                return new MetadataValue(reader.Value.ToString());
            
            return new MetadataValue(string.Empty);
        }
    }
}

