using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Azure.CognitiveServices.SpeakerRecognition.Dtos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Web.Models
{
    public class UserProfile
    {
        [Key]
        public Guid Id { get; set; }

        public string Description { get; set; }

        [NotMapped]
        public IdentificationProfile AzureProfile { get; set; }
    }
}