using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.CognitiveServices.SpeakerRecognition.Dtos;
using Newtonsoft.Json;

namespace Azure.CognitiveServices.SpeakerRecognition
{
    public class ApiClient
    {
        private const string ApiUrlBase = "https://westus.api.cognitive.microsoft.com/spid/v1.0";
        private readonly ClientConfig _config;
        private static readonly HttpClient Client = new HttpClient();


        public ApiClient(ClientConfig config)
        {
            _config = config;
        }

        public async Task<Guid> CreateNewProfileAsync()
        {
            var requestUri = $"{ApiUrlBase}/identificationProfiles";
            var result = await MakeRequestAsync<IdentificationProfile>(
                    HttpMethod.Post,
                    requestUri,
                    new {locale = "en-us"});

            return result.Id;
        }

        public async Task<IdentificationProfile> GetProfileAsync(Guid id)
        {
            var requestUri = $"{ApiUrlBase}/identificationProfiles/{id}";
            var profiles = await MakeRequestAsync<IdentificationProfile>(HttpMethod.Get, requestUri);
            return profiles;
        }

        public async Task<List<IdentificationProfile>> GetProfilesAsync()
        {
            var requestUri = $"{ApiUrlBase}/identificationProfiles";
            var profiles = await MakeRequestAsync<List<IdentificationProfile>>(HttpMethod.Get, requestUri);
            return profiles;
        }

        public async Task DeleteProfileAsync(Guid id)
        {
            var requestUri = $"{ApiUrlBase}/identificationProfiles/{id}";
            await MakeRequestAsync(HttpMethod.Delete, requestUri);
        }

        public Task<Guid> EnrollAsync(Guid id, byte[] audioData)
        {
            var requestUri = $"{ApiUrlBase}/identificationProfiles/{id}/enroll";

            return UploadAudio(audioData, requestUri);
        }

        public Task<Guid> IdentifyAsync(IEnumerable<Guid> profileId, byte[] audioData, bool isShort)
        {
            var requestUri = $"{ApiUrlBase}/identify?identificationProfileIds={string.Join(",",profileId)}&shortAudio={isShort}";
            return UploadAudio(audioData, requestUri);
        }

        private async Task<Guid> UploadAudio(byte[] audioData, string requestUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Ocp-Apim-Subscription-Key", _config.Key1);
            request.Content = new ByteArrayContent(audioData);

            using (var response = await Client.SendAsync(request))
            {
                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    var operationUrl = response.Headers.GetValues("operation-location").Single();
                    Regex regex = new Regex(".*\\/operations\\/(.*)");
                    var match = regex.Match(operationUrl);
                    var operationId = match.Groups[1].Value;
                    return Guid.Parse(operationId);
                }

                throw new Exception($"Enrolment failed: status code = {response.StatusCode}");
            }
        }

        public Task<OperationStatus> GetOperationStatus(Guid operationId)
        {
            var requestUrl = $"{ApiUrlBase}/operations/{operationId}";
            return MakeRequestAsync<OperationStatus>(HttpMethod.Get, requestUrl);
        }

        private Task MakeRequestAsync(HttpMethod method, string requestUri, object parameters = null)
        {
            return MakeRequestAsync<object>(method, requestUri, parameters);
        }

        private async Task<T> MakeRequestAsync<T>(HttpMethod method, string requestUri, object parameters = null)
        {
            var serializer = new JsonSerializer();
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Ocp-Apim-Subscription-Key", _config.Key1);

            if (parameters != null)
            {
                var json = JsonConvert.SerializeObject(parameters);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using (var response = await Client.SendAsync(request))
            {
                if (typeof(T) == typeof(object))
                {
                    return default(T);
                }

                //using (var stream = await response.Content.ReadAsStreamAsync())
                //using (var sr = new StreamReader(stream))
                //using (var jsonTextReader = new JsonTextReader(sr))
                //{
                //    return serializer.Deserialize<T>(jsonTextReader);
                //}

                var jsonString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"{DateTime.Now:s}: {jsonString}");
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
        }
    }
}