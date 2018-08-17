using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.CognitiveServices.SpeakerRecognition;
using Azure.CognitiveServices.SpeakerRecognition.Dtos;
using Azure.CognitiveServices.SpeakerRecognition.Enums;
using Web.Dal;
using Web.Models;

namespace Web.Managers
{
    public class ProfileManager
    {
        private readonly EfContext _dbContext;
        private readonly ApiClient _azureClient;

        public ProfileManager(EfContext dbContext, ApiClient azureClient)
        {
            _dbContext = dbContext;
            _azureClient = azureClient;
        }

        public async Task<UserProfile> CreateAsync(string description)
        {
            Guid id = await _azureClient.CreateNewProfileAsync();
            var azureProfile = await _azureClient.GetProfileAsync(id);
            var userProfile = new UserProfile
            {
                Id = id,
                AzureProfile = azureProfile,
                Description = description
            };
            _dbContext.Profiles.Add(userProfile);
            await _dbContext.SaveChangesAsync();

            return userProfile;
        }

        public async Task<List<UserProfile>> GetProfilesAsync()
        {
            var list = new List<UserProfile>();
            foreach (var identificationProfile in await _azureClient.GetProfilesAsync())
            {
                var userProfile = await _dbContext.Profiles.FindAsync(identificationProfile.Id);
                userProfile.AzureProfile = identificationProfile;
                list.Add(userProfile);
            }

            return list;
        }

        public async Task DeleteProfileAsync(Guid profileId)
        {
            var profile = await _dbContext.Profiles.FindAsync(profileId);
            _dbContext.Profiles.Remove(profile);
            await _dbContext.SaveChangesAsync();

            await _azureClient.DeleteProfileAsync(profileId);
        }

        public Task<Guid> EnrollAsync(Guid profileId, byte[] audioData)
        {
            return _azureClient.EnrollAsync(profileId, audioData);
        }

        public Task<OperationStatus> GetOperationStatus(Guid operationId)
        {
            return _azureClient.GetOperationStatus(operationId);
        }

        public async Task<string> IdentifyAsync(byte[] audioData)
        {
            try
            {
                var profiles = _dbContext.Profiles.ToDictionary(p => p.Id, p => p);
                var operationId = await _azureClient.IdentifyAsync(profiles.Keys, audioData, true);

                OperationStatus status = null;
                while (status == null || status.Status == OperationStatusEnum.Notstarted || status.Status == OperationStatusEnum.Running)
                {
                    await Task.Delay(1000);
                    status = await _azureClient.GetOperationStatus(operationId);
                }

                if (status.Status == OperationStatusEnum.Failed)
                    return "I don't know";

                var matchtedProfile = profiles[status.ProcessingResult.ProfileId];
                switch (status.ProcessingResult.Confidence)
                {
                    case ConfidenceLevel.High:
                        return $"It's for sure '{matchtedProfile.Description}'";
                    case ConfidenceLevel.Normal:
                        return $"Most likely it is '{matchtedProfile.Description}'";
                    default:
                        return $"It seems like it is '{matchtedProfile.Description}'";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
           
        }
    }
}