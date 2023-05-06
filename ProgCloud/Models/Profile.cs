using Google.Cloud.Firestore;
using Google.Type;
using Microsoft.AspNetCore.Http;

namespace ProgCloud.Models
{
    [FirestoreData]
    public class Profile
    {
        [FirestoreProperty]
        public string VideoStorageName { get; set; }
        [FirestoreProperty]

        public string DateTime { get; set; }
        [FirestoreProperty]

        public string Uploader { get; set; }
        [FirestoreProperty]

        public string BucketName { get; set; }
        [FirestoreProperty]

        public string Status { get; set; }
        [FirestoreProperty]

        public string ImageUrl { get; set; }
        [FirestoreProperty]
        public string VideoUrl { get; set; }
    }
}
