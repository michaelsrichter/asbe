namespace asbe.send
{
    internal class MessageDetails
    {
        public MessageDetails()
        {
        }

        public string IterationId { get; set; }
        public string Type { get; set; }
        public int Thread { get; set; }
        public DateTimeOffset Sent { get; set; }
    }
}