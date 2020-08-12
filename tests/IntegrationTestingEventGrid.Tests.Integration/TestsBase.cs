using Microsoft.Extensions.Configuration;

namespace IntegrationTestingEventGrid.Tests.Integration
{
    public class TestsBase
    {
        public EventGridTopicSettings TopicSettings;
        public StorageQueueTopicProxySettings ProxySettings;

        protected TestsBase()
        {
            var config = InitConfiguration();

            TopicSettings = new EventGridTopicSettings
            {
                Address = config.GetSection("EventGrid.MyTopic")["Address"],
                Key = config.GetSection("EventGrid.MyTopic")["Key"],
                Path = config.GetSection("EventGrid.MyTopic")["Path"]
            };

            ProxySettings = new StorageQueueTopicProxySettings()
            {
                StorageAccountConnectionString =
                    config.GetSection("StorageQueueTopicProxy")["StorageAccountConnectionString"],
                QueueName = config.GetSection("StorageQueueTopicProxy")["QueueName"]
            };
        }

        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json")
                .Build();
            return config;
        }
    }

    public class EventGridTopicSettings
    {
        public string Address { get; set; }
        public string Key { get; set; }
        public string Path { get; set; }
    }

    public class StorageQueueTopicProxySettings
    {
        public string StorageAccountConnectionString { get; set; }
        public string QueueName { get; set; }
    }
}

