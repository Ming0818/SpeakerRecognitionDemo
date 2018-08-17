using System;
using Azure.CognitiveServices.SpeakerRecognition.Enums;
using Newtonsoft.Json;

namespace Azure.CognitiveServices.SpeakerRecognition.Dtos
{
    public class OperationStatus
    {
        [JsonProperty("status")]
        public OperationStatusEnum Status { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTime CreateDate { get; set; }

        [JsonProperty("lastActionDateTime")]
        public DateTime LastActionDate { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("processingResult")]
        public ProcessingResultType ProcessingResult { get; set; }

        public class ProcessingResultType
        {
            [JsonProperty("enrollmentStatus")]
            public EnrollmentStatus Status { get; set; }

            [JsonProperty("remainingEnrollmentSpeechTime")]
            public float NeededDuration { get; set; }

            [JsonProperty("speechTime")]
            public float OperationParsedDuration { get; set; }

            [JsonProperty("enrollmentSpeechTime")]
            public float ProfileParsedDuration { get; set; }

            [JsonProperty("identifiedProfileId")]
            public Guid ProfileId { get; set; }

            [JsonProperty("confidence")]
            public ConfidenceLevel Confidence { get; set; }
        }
    }
}