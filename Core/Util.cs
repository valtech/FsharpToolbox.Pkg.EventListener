using System.Collections.Generic;

namespace FsharpToolbox.Pkg.Communication.Core
{
    public static class DictionaryExtension
    {
        public static TValue GetValueOrDefault<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var value) ? value : default;
        }
    }
}