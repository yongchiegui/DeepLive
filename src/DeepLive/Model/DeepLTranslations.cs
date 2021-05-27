using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeepLive.Model
{
    public class DeepLTranslations
    {
        [JsonProperty("translations")]
        public ICollection<DeepLTranslation> Translations { get; set; }
    }

    public class DeepLTranslation
    {
        [JsonProperty("detected_source_language")]
        public string DetectedSourceLanguage { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
