namespace IntegrationTestingEventGrid.Tests.Integration
{
    public class MyEvent
    {
        public string Type { get; set; }
        public string Version { get; set; }
        public string CorrelationId { get; set; }
    }
}
