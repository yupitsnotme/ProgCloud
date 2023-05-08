using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ProgCloud.Models;
using System.Linq;
using Google.Cloud.Storage.V1;
using System.IO;

namespace ProgCloud.DataAccess
{
    public class FirestoreVideosRepository
    {
        FirestoreDb db;
        VideoTranscriptionRepository _videoTransRepo;
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

    }
}
