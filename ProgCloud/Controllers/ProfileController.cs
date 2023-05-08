using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using ProgCloud.DataAccess;
using ProgCloud.Models;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace ProgCloud.Controllers
{
    public class ProfileController : Controller
    {
        ILogger<ProfileController> _logger;
        FirestoreVideosRepository _videoRepo;
        PubSubRepository _pubSub;
        private IWebHostEnvironment _host;

        public ProfileController(IWebHostEnvironment host, FirestoreVideosRepository videoRepo, PubSubRepository pubSub, ILogger<ProfileController> logger)
        {
            _logger = logger;
            _videoRepo = videoRepo;
            _host = host;
            _pubSub = pubSub;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var list = await _videoRepo.GetVideos(User.Identity.Name);
            return View(list);
        }


        [HttpPost]
        [Authorize]
        public IActionResult Create(IFormFile file, [FromServices] IConfiguration config)
        {
            _logger.LogInformation($"User {User.Identity.Name} is uploading video");

            if (ModelState.IsValid)
            {

                _logger.LogInformation($"Validators for upload are ok");
                try
                {
                    string bucketName = config["bucket"].ToString();
                    if (file != null)
                    {
                        //1. Upload the file in the cloud bucket
                        var storage = StorageClient.Create();
                        using var fileStream = file.OpenReadStream();

                        _logger.LogInformation($"File {file.FileName} is about to be uploaded");

                        var newFilename = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        _logger.LogInformation($"File {file.FileName} has been renamed to {newFilename}");

                        storage.UploadObject(bucketName, newFilename, null, fileStream);
                        _logger.LogInformation($"File {file.FileName} with new filename {newFilename} has been uploaded successfully");

                        var videoUrl = $"https://storage.googleapis.com/{bucketName}/{newFilename}";
                        _logger.LogInformation($"File {file.FileName} with new filename {newFilename} can be found here {videoUrl}");


                        //Thumbnail Creation & saving
                        string thumbnailName = newFilename + "_thumbnail.jpg";
                        var thumbnailUrl = $"https://storage.googleapis.com/{bucketName}/{thumbnailName}";

                        _logger.LogInformation($"Attempting to create and upload thumbnail");

                        // Upload the thumbnail image to Cloud Storage
                        //_videoRepo.GenerateThumbnail(bucketName, videoUrl, thumbnailName);

                        _logger.LogInformation($"Thumbnail {file.FileName} with new filename {thumbnailName} can be found here {thumbnailUrl}");

                        //2. save the link together with the rest of the textual data into the NoSql db
                        _logger.LogInformation($"Video information with {newFilename} will be saved to db");

                        string path = _host.ContentRootPath + "\\" + thumbnailName;

                        Profile p = new Profile
                        {
                            VideoStorageName = newFilename,
                            DateTime = DateTime.Now.ToString(),
                            Uploader = User.Identity.Name,
                            BucketName = bucketName,
                            Status = "READY", 
                            ImageUrl = thumbnailUrl,
                            VideoUrl = videoUrl
                        };


                        _videoRepo.AddVideo(p);

                        _logger.LogInformation($"Video information with {p.VideoStorageName} was saved to db");

                    }

                    TempData["success"] = "Video was added successfully to the cloud";
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"{User.Identity.Name} had an error while uploading a file");
                    TempData["error"] = "Video was not added in the database";
                }
            }
            else
            {
                string jsonWarnings = JsonConvert.SerializeObject(ModelState.Values);
                _logger.LogWarning($"Validation errors: {jsonWarnings}");
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Download(string videoName)
        {
            _videoRepo.DownloadVideo(videoName);
            return Redirect(videoName);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> TranscribeVideo(string videoName)
        {

            await _pubSub.PushMessage(videoName);


            return RedirectToAction("Index");

        }
    }
}
