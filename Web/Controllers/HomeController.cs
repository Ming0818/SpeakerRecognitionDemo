using System;
using System.IO;
using System.Threading.Tasks;
using Azure.CognitiveServices.SpeakerRecognition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;
using Web.Managers;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProfileManager _manager;

        public HomeController(ProfileManager manager)
        {
            _manager = manager;
        }

        public async Task<IActionResult> Index()
        {
            var userProfiles = await _manager.GetProfilesAsync();
            return View(userProfiles);
        }

        public async Task<IActionResult> AddProfile(CreateProfileRequest request)
        {
            await _manager.CreateAsync(request.Description);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteProfile(Guid id)
        {
            await _manager.DeleteProfileAsync(id);
            return RedirectToAction("Index");
        }

        private byte[] ResampleAudio(Stream inputData, WaveFormat inputFormat, WaveFormat outputFormat)
        {
            if (inputData.CanSeek)
                inputData.Seek(0, SeekOrigin.Begin);

            using (var ms = new MemoryStream())
            {
                using (var reader = new RawSourceWaveStream(inputData, inputFormat))
                {
                    using (var resampler = new MediaFoundationResampler(reader, outputFormat))
                    {
                        resampler.ResamplerQuality = 60;
                        WaveFileWriter.WriteWavFileToStream(ms, resampler);
                        return ms.ToArray();
                    }
                }
            }
        }


        [HttpPost("enroll/{id}")]
        public async Task<JsonResult> Enroll(Guid id)
        {
            byte[] audioData = ExtractAudio(Request);

            System.IO.File.WriteAllBytes($"spy_{id}_{DateTime.Now:HH-mm-ss}.wav", audioData);

            var operationId = await _manager.EnrollAsync(id, audioData);
            return Json(new {operationId});
        }

        [HttpPost("identify")]
        public async Task<JsonResult> Identify()
        {
            byte[] audioData = ExtractAudio(Request);
            var lookupResult = await _manager.IdentifyAsync(audioData);
            return Json(new { lookupResult });
        }

        [HttpPost("operation-status/{id}")]
        public async Task<IActionResult> GetOperationStatus(Guid id)
        {
            var location = await _manager.GetOperationStatus(id);
            return Ok(new { location });
        }

        private byte[] ExtractAudio(HttpRequest request)
        {
            var formFile = request.Form.Files[0];
            using (var audioStream = new MemoryStream((int) formFile.Length))
            {
                formFile.CopyTo(audioStream);
                var inputFormat = new WaveFormat(48000, 16, 2);
                var outputFormat = new WaveFormat(sampleRate: 16000, channels: 1);
                var audioData = ResampleAudio(audioStream, inputFormat, outputFormat);
                return audioData;
            }
        }

        [HttpPost("UploadFiles")]
        public async Task<IActionResult> Post()
        {
            var formFile = this.Request.Form.Files[0];
            var size = formFile.Length;
            //var filePath = Path.GetTempFileName();

            var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePathBase = Path.Combine(folderPath, formFile.Name);
            var filePath = $"{filePathBase}_{DateTime.Now.Ticks}.wav";

            if (formFile.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }
            }


            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new {size, filePath});
        }
    }
}
