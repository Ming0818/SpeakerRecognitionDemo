using System;
using Azure.CognitiveServices.SpeakerRecognition.Enums;
using Newtonsoft.Json;

namespace Azure.CognitiveServices.SpeakerRecognition.Dtos
{
    public class IdentificationProfile
    {
        [JsonProperty("identificationProfileId")]
        public Guid Id { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("enrollmentSpeechTime")]
        public float ParsedDuration { get; set; }

        [JsonProperty("remainingEnrollmentSpeechTime")]
        public float NeededDuration { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTime CreateDate { get; set; }

        [JsonProperty("lastActionDateTime")]
        public DateTime LastActionDate { get; set; }

        [JsonProperty("enrollmentStatus")]
        public EnrollmentStatus Status { get; set; }
    }
}