using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using ProgCloud.Models;

namespace ProgCloud.DataAccess
{
    public class PubSubRepository
    {
        TopicName topicName;
        Topic topic;
        public PubSubRepository(string projectId)
        {
            topicName = TopicName.FromProjectTopic(projectId, "transcribe");

            if (topicName == null)
            {
                PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();

                try
                {
                    topicName = new TopicName(projectId, "transcribe");
                    topic = publisher.CreateTopic(topicName);
                }
                catch (Exception ex)
                {
                    //log
                    throw ex;
                }
            }
        }

        public async Task<string> PushMessage(string videoName)
        {

            PublisherClient publisher = await PublisherClient.CreateAsync(topicName);

            var pubsubMessage = new PubsubMessage
            {
                // The data is any arbitrary ByteString. Here, we're using text.
                Data = ByteString.CopyFromUtf8(JsonConvert.SerializeObject(videoName)),
                // The attributes provide metadata in a string-to-string dictionary.
                Attributes =
                {
                    { "priority", "normal" }
                }
            };
            string message = await publisher.PublishAsync(pubsubMessage);
            return message;
        }
    }
}
