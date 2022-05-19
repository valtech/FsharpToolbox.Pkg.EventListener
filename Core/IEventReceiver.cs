using System;
using System.Threading.Tasks;

namespace FsharpToolbox.Pkg.Communication.Core
{
    public enum ReceiverType
    {
        Queue,
        Topic
    }

    public static class ReceiverTypeExtensions
    {
        public static string ToString(this ReceiverType self)
        {
            return self switch
            {
                ReceiverType.Queue => "Queue",
                ReceiverType.Topic => "Topic",
                _ => throw new ArgumentOutOfRangeException(nameof(self), self, "Missing enum variant")
            };
        }
    }
    
    public interface IEventReceiver
    {
        void RegisterHandler(Func<Event, Task<EventResult>> onEvent, Func<Exception, Task> onException);
        Task CloseAsync();
        ReceiverType ReceiverType();
        string ReceiverChannelName();
    }
}