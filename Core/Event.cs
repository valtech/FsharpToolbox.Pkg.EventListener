namespace FsharpToolbox.Pkg.Communication.Core
{
    public class Event
    {
        public string Version { get; set; }
        public string EventName { get; set; }
        public string Payload { get; set; }
        public string Recipient { get; set; }
    }
}