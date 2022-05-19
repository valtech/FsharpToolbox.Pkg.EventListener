using System.Net;
using System.Threading.Tasks;
using FsharpToolbox.Pkg.Pkg.Common.Result;

namespace FsharpToolbox.Pkg.Communication.Core
{
    public interface IRestCaller
    {
        Task<TModel> Get<TModel>(string path, params (string key, object value)[] parameters)
            where TModel : class;

        Task<TModel> Post<TModel>(string path, object requestModel, params (string key, object value)[] parameters)
            where TModel : class;

        Task<(string body, HttpStatusCode status)> GetRaw(string path, params (string key, object value)[] parameters);

        Task<(string body, HttpStatusCode status)> PostRaw<TRequestModel>(string path, TRequestModel requestModel,
            params (string key, object value)[] parameters)
            where TRequestModel : class;

        Task<byte[]> PostByteArray<TRequestModel>(string path, TRequestModel requestModel,
            params (string key, object value)[] parameters) where TRequestModel : class;

        Task<Result<TModel, TError>> GetResult<TModel, TError>(string path, params (string key, object value)[] parameters)
            where TModel : class;

        Task<Result<TModel, TError>> PostResult<TModel, TError>(string path, object requestModel, params (string key, object value)[] parameters)
            where TModel : class;
    }
}