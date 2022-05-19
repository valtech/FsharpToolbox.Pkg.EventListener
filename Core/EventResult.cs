namespace FsharpToolbox.Pkg.Communication.Core
{
    public abstract class EventResult { }

    public class DeadLetterResult : EventResult
    {
        public string Message { get; set; }
    }

    public class CompleteResult : EventResult
    {
    }

    public class AbandonResult : EventResult
    {
    }
}