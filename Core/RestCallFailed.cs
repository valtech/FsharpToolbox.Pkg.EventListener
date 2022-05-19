using System;
using System.Net;
using System.Net.Http;
using FsharpToolbox.Pkg.AspNetCore.Core;
using FsharpToolbox.Pkg.Pkg.Common.Core.Exceptions;

namespace FsharpToolbox.Pkg.Communication.Core
{
    public class RestCallFailed : ApplicationError
    {
        public ErrorDetails Error { get; }

        public RestCallFailed(HttpMethod method, string url, ErrorDetails error) 
            : base($"Failed to {method} {url}: {error.Titel}, {error.Detail}", (HttpStatusCode)error.Status, Parse(error.SeverityLevel, SeverityLevel.Error), null, error)
        {
            Error = error;
        }

        private static TEnum Parse<TEnum>(string severity, TEnum defaultValue = default(TEnum)) =>
            Enum.TryParse(typeof(TEnum), severity, out var res) 
                ? (TEnum) res 
                : defaultValue;

        public override string Code => Error.Code;
    }
}