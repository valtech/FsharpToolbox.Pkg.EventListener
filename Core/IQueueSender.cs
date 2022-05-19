using System.Threading.Tasks;

namespace FsharpToolbox.Pkg.Communication.Core
{
    public interface IQueueSender<in TModel>
    {
        Task Send(TModel model, string sessionId = null, string version = null, string eventName = null, string messageId = null);
        Task Close();
    }
}