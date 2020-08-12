using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using Xunit;

namespace IntegrationTestingEventGrid.Tests.Integration.EventGrid
{
    public class EventGridTests : TestsBase
    {
        private HttpClient _httpClient;
        private ManualResetEvent _pause;
        private QueueClient _queueClient;

        [Fact]
        public async void Can_Publish_Event_To_EventGrid()
        {
            //arrange 
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("aeg-sas-key", TopicSettings.Key);

            _pause = new ManualResetEvent(false);

            _queueClient = new QueueClient(ProxySettings.StorageAccountConnectionString, ProxySettings.QueueName);

            _queueClient.CreateIfNotExists();
            _queueClient.ClearMessages();

            var myEvent = new MyEvent()
            {
                Type = "TestEvent",
                Version = "V1.0",
                CorrelationId = Guid.NewGuid().ToString()
            };

            //act
            var result = await PublishEventAsync(myEvent);

            //assert
            Assert.True(result.IsSuccessStatusCode);

            PollStorageQueueUntilEventArrives();

            QueueMessage[] messages = _queueClient.ReceiveMessages();
            var firstMessageJson = Encoding.UTF8.GetString(Convert.FromBase64String(messages.First().MessageText));
            var cloudEvent = JsonConvert.DeserializeObject<CloudEvent<MyEvent>>(firstMessageJson);

            Assert.Equal(cloudEvent.Data.Type, myEvent.Type);
            Assert.Equal(cloudEvent.Data.Version, myEvent.Version);
            Assert.Equal(cloudEvent.Data.CorrelationId, myEvent.CorrelationId);
        }

        private Task<HttpResponseMessage> PublishEventAsync(MyEvent myEvent)
        {
            var cloudEvent = new CloudEvent<MyEvent>
            {
                Id = Guid.NewGuid().ToString(),
                SpecVersion = "1.0",
                Time = DateTime.Now.ToLongDateString(),
                Type = $"{myEvent.Type}_{myEvent.Version}",
                Source = TopicSettings.Path,
                Subject = myEvent.CorrelationId,
                Data = myEvent
            };

            var content = new StringContent(JsonConvert.SerializeObject(cloudEvent), Encoding.UTF8, "application/cloudevents+json");

            return _httpClient.PostAsync(TopicSettings.Address, content);
        }

        private void PollStorageQueueUntilEventArrives()
        {
            PeekedMessage[] peekedMessages = _queueClient.PeekMessages();
            if (peekedMessages.Length > 0)
                return;

            var retries = 0;
            while (!_pause.WaitOne(5000) && retries < 5)
            {
                peekedMessages = _queueClient.PeekMessages();
                if (peekedMessages.Length > 0)
                    _pause.Set();
                retries++;
            }
        }
    }
}
