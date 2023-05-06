using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ProgCloud.Models;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Google.Cloud.Storage.V1;
using System.Diagnostics;
using System.IO;

namespace ProgCloud.DataAccess
{
    public class FirestoreVideosRepository
    {
        FirestoreDb db;
        public FirestoreVideosRepository(string project)
        {
            db = FirestoreDb.Create(project);
        }

        public async void AddVideo(Profile p)
        {
            await db.Collection("videos").Document().SetAsync(p);
        }

        public async void DownloadVideo(string videoName)
        {
            Query videosQuery = db.Collection("videos").WhereEqualTo("VideoUrl", videoName);
            QuerySnapshot videosQuerySnapshot = await videosQuery.GetSnapshotAsync();
            string docId = videosQuerySnapshot.Documents.First().Id;

            Dictionary<string, object> initialData = new Dictionary<string, object>
            {
            { "Timestamp", DateTime.Now.ToString()}
            };

            DocumentReference parentDocument = db.Collection("videos").Document(docId);
            await parentDocument.Collection("downloads").Document().SetAsync(initialData);
        }

        public async void TranscribeVideo(string videoName)
        {

        }

        public async Task<List<Profile>> GetVideos(string userName)
        {
            List<Profile> videos = new List<Profile>();
            Query allVideosQuery = db.Collection("videos");
            QuerySnapshot allVideosQuerySnapshot = await allVideosQuery.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in allVideosQuerySnapshot.Documents)
            {
                Profile p = documentSnapshot.ConvertTo<Profile>();
                if (p.Uploader == userName)
                    videos.Add(p);
            }

            return videos;
        }

        public async Task<Profile> GetVideo(string name)
        {
            Query videosQuery = db.Collection("videos").WhereEqualTo("VideoStorageName", name);
            QuerySnapshot videosQuerySnapshot = await videosQuery.GetSnapshotAsync();

            DocumentSnapshot documentSnapshot = videosQuerySnapshot.Documents.FirstOrDefault();
            if (documentSnapshot.Exists == false) return null;
            else
            {
                Profile result = documentSnapshot.ConvertTo<Profile>();
                return result;
            }
        }

        public void GenerateThumbnail(string videoBucket, string videoFilePath, string thumbnailFilePath)
        {
            // Generate the thumbnail image using FFmpeg
            string args = $"-i gs://{videoBucket}/{videoFilePath} -vf scale=320:-1 -vframes 1 -y {Path.GetTempPath()}/{Path.GetFileNameWithoutExtension(thumbnailFilePath)}.jpg";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();

            // Upload the thumbnail image to Cloud Storage
            var storage = StorageClient.Create();
            using (var fileStream = File.OpenRead($"{Path.GetTempPath()}/{Path.GetFileNameWithoutExtension(thumbnailFilePath)}.jpg"))
            {
                storage.UploadObject(videoBucket, thumbnailFilePath, null, fileStream);
            }
        }
    }
}
