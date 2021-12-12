namespace asbe.receive
{
    public class ASBEReceiveConfig
    {
        public string SBConnectionString { get; set; }
        public string QueueName { get; set; }
        public bool AutoCompleteMessages { get; set; }
        public int MaxConcurrentCalls { get; set; }
        public int PrefetchCount { get; set; }

    }
}